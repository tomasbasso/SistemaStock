# Flujo Técnico AFIP: WSAA + WSFEv1

Documentación oficial: https://docs.afipsdk.com/ | https://www.afip.gob.ar/ws/documentacion/wsaa.asp

---

## 1. Certificado Digital (Setup Único)

```bash
# Generar clave privada RSA 2048 bits
openssl genrsa -out privada.key 2048

# Generar CSR (completar O y CN con tu empresa y servidor)
openssl req -new -key privada.key \
  -subj "/C=AR/O=COMERCIAL CAIQUEN/CN=caiquen-backend" \
  -out request.csr
```

1. Subir `request.csr` en AFIP → Administrador de Relaciones → Certificado Digital.
2. Descargar el `.crt` resultante.
3. Guardar `privada.key` y el `.crt` en variables de entorno, **nunca en el repo**.

---

## 2. Variables de Entorno Requeridas

```env
AFIP_CUIT=20409378472
AFIP_CERT=<contenido del .crt en base64 o ruta al archivo>
AFIP_KEY=<contenido de privada.key en base64 o ruta al archivo>
AFIP_PTO_VTA=1
AFIP_PRODUCTION=false   # "true" solo en producción
```

---

## 3. Paso 1 – Autenticación WSAA (Token)

El token dura **~12 horas**. Cachearlo para no pedir uno nuevo en cada factura.

### Flujo WSAA
```
1. Construir LoginTicketRequest XML con:
   - GenerationTime (UTC ahora - 10 min)
   - ExpirationTime  (UTC ahora + 12h)
   - UniqueId (timestamp unix)
   - Service = "wsfe"

2. Firmar el XML con clave privada (PKCS#7 / CMS sin detach)

3. POST SOAP a WSAA:
   - Homologación: https://wsaahomo.afip.gov.ar/ws/services/LoginCms
   - Producción:   https://wsaa.afip.gov.ar/ws/services/LoginCms

4. WSAA responde con { token, sign }
   → Guardar junto con la expiración en memoria o Redis
```

### Ejemplo de caché en Node.js
```javascript
let tokenCache = null;

async function getToken() {
    if (tokenCache && new Date() < tokenCache.expiresAt) {
        return tokenCache;
    }
    const { token, sign } = await wsaaLogin();
    tokenCache = { token, sign, expiresAt: new Date(Date.now() + 11 * 60 * 60 * 1000) };
    return tokenCache;
}
```

---

## 4. Paso 2 – Obtener Último Comprobante (Sincronización)

**SIEMPRE** hacer este paso antes de cada factura. Nunca confiar solo en la base de datos.

```javascript
// FECompUltimoAutorizado
const lastVoucher = await wsfe.call('FECompUltimoAutorizado', {
    Auth: { Token: token, Sign: sign, Cuit: CUIT },
    PtoVta: 1,
    CbteTipo: 1  // 1=A, 6=B, 11=C
});
const nextNumber = lastVoucher.FECompUltimoAutorizadoResult.CbteNro + 1;
```

---

## 5. Paso 3 – Solicitar CAE (FECAESolicitar)

```javascript
const response = await wsfe.call('FECAESolicitar', {
    Auth: { Token: token, Sign: sign, Cuit: CUIT },
    FeCAEReq: {
        FeCabReq: {
            CantReg: 1,
            PtoVta: 1,
            CbteTipo: cbteTipo  // 1=A, 6=B, 11=C
        },
        FeDetReq: {
            FECAEDetRequest: [{
                Concepto: 1,          // 1=Productos, 2=Servicios, 3=Ambos
                DocTipo: docTipo,     // 80=CUIT, 96=DNI, 99=Consumidor Final
                DocNro: docNro,
                CbteDesde: nextNumber,
                CbteHasta: nextNumber,
                CbteFch: 20260226,    // YYYYMMDD
                ImpTotal: 1210.00,
                ImpTotConc: 0,
                ImpNeto: 1000.00,
                ImpOpEx: 0,
                ImpIVA: 210.00,
                ImpTrib: 0,
                MonId: 'PES',
                MonCotiz: 1,
                Iva: [{ Id: 5, BaseImp: 1000, Importe: 210 }]  // Id 5 = 21%
            }]
        }
    }
});
```

### IDs de Alícuota IVA
| Id | Porcentaje |
|----|-----------|
| 3  | 0%        |
| 4  | 10.5%     |
| 5  | 21%       |
| 6  | 27%       |

---

## 6. Paso 4 – Persistir el Resultado

Guardar **siempre** en la base de datos:

```javascript
await db.query(`
  INSERT INTO fiscal_vouchers (
    reference_sale_id, voucher_type, status,
    pos_number, voucher_number, total_amount,
    net_taxed, net_exempt, cae, cae_expiration,
    afip_response, approved_at
  ) VALUES ($1,$2,'APPROVED',$3,$4,$5,$6,$7,$8,TO_DATE($9,'YYYYMMDD'),$10,NOW())
`, [saleId, vType, ptoVta, nextNumber, total, netTaxed, netExempt, cae, caeExpiry, fullResponse]);
```

---

## 7. URLs por Ambiente

| Servicio | Homologación | Producción |
|---------|-------------|-----------|
| WSAA    | `wsaahomo.afip.gov.ar/ws/services/LoginCms` | `wsaa.afip.gov.ar/ws/services/LoginCms` |
| WSFEv1  | `wswhomo.afip.gov.ar/wsfev1/service.asmx` | `servicios1.afip.gov.ar/wsfev1/service.asmx` |

⚠️ Cambiar `AFIP_PRODUCTION=true` solo cuando el ambiente de homologación esté 100% aprobado.
