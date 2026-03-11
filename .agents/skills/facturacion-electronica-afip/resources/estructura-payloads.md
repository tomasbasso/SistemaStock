# Estructura de Payloads AFIP

Referencia: https://docs.afipsdk.com/ | Manual ARCA COMPG v4.1

---

## FECAESolicitar – Request Completo

```json
{
  "FeCAEReq": {
    "FeCabReq": {
      "CantReg": 1,
      "PtoVta": 1,
      "CbteTipo": 6
    },
    "FeDetReq": {
      "FECAEDetRequest": [{
        "Concepto": 1,
        "DocTipo": 96,
        "DocNro": 12345678,
        "CbteDesde": 123,
        "CbteHasta": 123,
        "CbteFch": 20260226,
        "ImpTotal": 1210.00,
        "ImpTotConc": 0,
        "ImpNeto": 1000.00,
        "ImpOpEx": 0,
        "ImpIVA": 210.00,
        "ImpTrib": 0,
        "MonId": "PES",
        "MonCotiz": 1,
        "Iva": [
          { "Id": 5, "BaseImp": 1000.00, "Importe": 210.00 }
        ]
      }]
    }
  }
}
```

### Reglas de Importes
```
ImpTotal = ImpNeto + ImpIVA + ImpTrib + ImpOpEx + ImpTotConc
```

### Factura A (CbteTipo 1) – Para RI
```javascript
const impNeto = totalAmount / 1.21;   // Sin IVA
const impIVA  = totalAmount - impNeto;
```

### Factura B/C (CbteTipo 6/11) – Para CF y Mono
```javascript
// El ImpTotal ES el precio con IVA incluido
// ImpNeto = base imponible, ImpIVA = 21% sobre esa base
const impNeto = parseFloat((totalAmount / 1.21).toFixed(2));
const impIVA  = parseFloat((totalAmount - impNeto).toFixed(2));
```

---

## FECAESolicitar – Response Exitosa

```json
{
  "FECAESolicitarResult": {
    "FeCabResp": {
      "Cuit": 20409378472,
      "PtoVta": 1,
      "CbteTipo": 6,
      "FchProceso": "20260226120000",
      "CantReg": 1,
      "Resultado": "A"
    },
    "FeDetResp": {
      "FECAEDetResponse": {
        "Concepto": 1,
        "DocTipo": 96,
        "DocNro": 12345678,
        "CbteDesde": 123,
        "CbteHasta": 123,
        "CbteFch": 20260226,
        "Resultado": "A",
        "CAE": "74397350552670",
        "CAEFchVto": "20260308"
      }
    }
  }
}
```

### Significado de `Resultado`
| Valor | Significado |
|-------|------------|
| `A`   | Aprobado (tiene CAE) |
| `R`   | Rechazado (ver Errors) |
| `P`   | Parcial (algunas aprobadas, ver detalle) |

---

## FECompUltimoAutorizado – Response

```json
{
  "FECompUltimoAutorizadoResult": {
    "PtoVta": 1,
    "CbteTipo": 6,
    "CbteNro": 122,
    "Errors": null
  }
}
```
→ Próximo número = `CbteNro + 1`

---

## Nota de Crédito – Diferencias con Factura

```json
{
  "FeCabReq": {
    "CbteTipo": 8
  },
  "FECAEDetRequest": [{
    "CbtesAsoc": {
      "CbteAsoc": [{
        "Tipo": 6,
        "PtoVta": 1,
        "Nro": 122
      }]
    }
  }]
}
```
⚠️ Las notas de crédito deben referenciar el comprobante original con `CbtesAsoc`.
