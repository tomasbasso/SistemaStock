# Validaciones y Checklist AFIP

---

## Tabla de CbteTipo (Tipos de Comprobante)

| CbteTipo | Descripción           | Receptor |
|---------|-----------------------|---------|
| 1       | Factura A             | Responsable Inscripto |
| 6       | Factura B             | Consumidor Final / Monotributista |
| 11      | Factura C             | Exento / No categorizados |
| 3       | Nota de Crédito A     | RI |
| 8       | Nota de Crédito B     | CF / Mono |
| 13      | Nota de Crédito C     | Exento |

## Tabla de DocTipo

| DocTipo | Descripción |
|---------|------------|
| 80      | CUIT       |
| 96      | DNI        |
| 99      | Consumidor Final (DocNro = 0) |

---

## Errores Comunes AFIP y Soluciones

| Código | Error | Solución |
|--------|-------|---------|
| 10016  | Ya existe comprobante con ese número | Resincronizar: llamar `FECompUltimoAutorizado` |
| 10014  | Token vencido | Renovar token WSAA; revisar caché |
| 10041  | CUIT no autorizado para el servicio | Verificar alta en AFIP > Administrador Relaciones |
| 10004  | Fecha fuera de rango | Usar fecha del día en formato `YYYYMMDD` |
| 10048  | CbteFch inválido | No puede ser fecha futura ni demasiado antigua |
| 10079  | ImpTotal no coincide | `ImpTotal = ImpNeto + ImpIVA + ImpTrib + ImpOpEx` |
| 422    | XML firmado inválido | Revisar namespace y expiración del `LoginTicketRequest` |

---

## Manejo de Errores en Código

```javascript
const result = response.FECAESolicitar.FECAESolicitarResult;

// Errores bloqueantes (sin CAE)
if (result.Errors?.Err) {
    const errors = [].concat(result.Errors.Err);
    throw new Error(`AFIP Error ${errors[0].Code}: ${errors[0].Msg}`);
}

// Observaciones (informativas, no bloquean)
if (result.FeDetResp?.FECAEDetResponse?.Observaciones?.Obs) {
    const obs = [].concat(result.FeDetResp.FECAEDetResponse.Observaciones.Obs);
    console.warn('[AFIP Obs]', obs.map(o => `${o.Code}: ${o.Msg}`).join(' | '));
    // Guardar observaciones en la DB igual
}

const cae = result.FeDetResp.FECAEDetResponse.CAE;
const caeExpiry = result.FeDetResp.FECAEDetResponse.CAEFchVto; // YYYYMMDD
```

---

## Retry con Exponential Backoff

```javascript
async function withRetry(fn, retries = 3, delay = 2000) {
    for (let i = 0; i < retries; i++) {
        try {
            return await fn();
        } catch (err) {
            if (i === retries - 1) throw err;
            const wait = delay * Math.pow(2, i); // 2s, 4s, 8s
            console.warn(`[AFIP] Reintento ${i + 1}/${retries} en ${wait}ms`);
            await new Promise(r => setTimeout(r, wait));
        }
    }
}

// Uso:
const cae = await withRetry(() => wsfe.FECAESolicitar(payload));
```

---

## ✅ Checklist Pre-Producción

### Certificados
- [ ] Certificado X509 generado y subido a AFIP
- [ ] Clave privada en `.env` / vault (nunca en el repo)
- [ ] Certificado NO vencido (validar fecha de expiración)

### Autenticación
- [ ] Token WSAA cacheado correctamente (evita throttling)
- [ ] Renovación automática antes del vencimiento de 12h
- [ ] Hora UTC correcta en LoginTicketRequest

### Numeración
- [ ] `FECompUltimoAutorizado` consultado antes de CADA factura
- [ ] Número sincronizado, nunca hardcodeado

### Persistencia
- [ ] CAE guardado en DB inmediatamente tras aprobación
- [ ] Request completo guardado (auditoria)
- [ ] Response completa guardada (errores, observaciones)
- [ ] Fecha de vencimiento del CAE guardada

### Errores
- [ ] Errores y Observaciones de AFIP manejados y logueados
- [ ] Retry con backoff implementado
- [ ] Timeout controlado (evitar requests colgados)
- [ ] Rollback de la venta si AFIP rechaza (si es síncrono)

### Ambiente
- [ ] Pruebas completadas en homologación
- [ ] AFIP_PRODUCTION=true solo en producción
- [ ] Backup de certificados en lugar seguro
