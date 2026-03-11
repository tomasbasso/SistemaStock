---
name: facturacion-electronica-afip
description: Utiliza esta skill para implementar, depurar o revisar procesos de factura electrónica de AFIP (ARCA/WSFEv1/WSAA) en servicios backend.
---

# 🧾 Facturación Electrónica AFIP

## Objetivo
Guiar la implementación correcta de facturación electrónica en AFIP usando el esquema ARCA (WSAA + WSFEv1). Garantizar autenticación segura, numeración sincronizada y manejo completo de errores para evitar los 9 errores graves más comunes.

**Lee los recursos completos antes de tocar código:**
- `resources/flujo-tecnico.md` → Autenticación + solicitud de CAE
- `resources/validaciones.md` → Checklist de producción y errores comunes
- `resources/estructura-payloads.md` → Request/response WSFEv1

## Core Pattern: Arquitectura Backend-Only

```
Frontend (React)
      ↓ HTTP POST /api/sales
Backend (Node.js)
      ↓ 1. WSAA: Obtener token (cachear 12h)
AFIP WSAA
      ↓ 2. WSFEv1: Solicitar CAE
AFIP WSFEv1
      ↓ 3. Guardar CAE + logs en DB
PostgreSQL
```

⚠️ **NUNCA** llamar a AFIP desde el frontend. Siempre backend.

## Instrucciones

### 1. Setup Inicial (una sola vez)
1. Dar de alta en AFIP: Administrador de Relaciones → Adherir **WebService Facturación Electrónica**.
2. Generar certificado: ver `resources/flujo-tecnico.md` sección "Certificado Digital".
3. Configurar variables de entorno: `AFIP_CUIT`, `AFIP_CERT`, `AFIP_KEY`, `AFIP_PTO_VTA`, `AFIP_PRODUCTION`.
4. **La clave privada NUNCA va en el repo.** Usar `.env` o vault.

### 2. Flujo de Facturación
Lee `resources/flujo-tecnico.md` para el código paso a paso:
1. **WSAA** → Obtener/renovar token firmando XML con clave privada.
2. **FECompUltimoAutorizado** → Obtener último número de comprobante.
3. **FECAESolicitar** → Enviar request con datos de venta y obtener CAE.
4. **Persistir** → Guardar request + response + CAE + XML firmado en DB.

### 3. Manejo de Errores
- AFIP devuelve **Observaciones** (no bloquean) y **Errores** (bloquean CAE).
- Implementar retry con exponential backoff (ver `resources/validaciones.md`).
- Logear todo: request enviado, response completa, CAE, timestamp.

### 4. Módulos y Estructura de Archivos
```
services/afip/
  wsaa.js      → Autenticación y token
  wsfe.js      → CAE, FECompUltimoAutorizado, FECAESolicitar
  index.js     → Facade pública (generateInvoice, getCreditNote)
```
No mezclar lógica AFIP con lógica de stock o ventas.

## Bulletproofing (Rigor TDD)

### Tabla de Racionalizaciones
| Excusa | Realidad |
|--------|---------|
| "Voy a confiar en el número de mi DB" | SIEMPRE llamar a `FECompUltimoAutorizado` antes de cada factura. AFIP tiene la verdad. |
| "No hace falta guardar el request" | Sin logs, no hay auditoría. AFIP exige comprobantes por 10 años. |
| "Pruebo directo en producción" | Homologación OBLIGATORIA primero. Un error en prod puede generar comprobantes inválidos. |
| "La clave privada puede ir en el repo privado" | Nunca. Variables de entorno o vault, siempre. |
| "No necesito cachear el token" | El token dura 12h. Renovarlo en cada request genera throthling y delays. |

### Red Flags - STOP and Start Over
- Cualquier llamada a AFIP desde el frontend.
- Número de comprobante hardcodeado sin consultar `FECompUltimoAutorizado`.
- CAE no persistido en base de datos.
- Clave privada commiteada en cualquier repositorio.
- Ausencia de manejo de errores y observaciones de AFIP.

## Restricciones
- **NO** usar SOAP manualmente sin revisar primero `resources/estructura-payloads.md`.
- **NO** desplegar en producción sin completar el checklist en `resources/validaciones.md`.
- **NO** compartir token/sign entre usuarios o sesiones concurrentes sin sincronización.

## Ejemplos
- "¿Cómo facturo para un Responsable Inscripto?" → `CbteTipo: 1` (Factura A), `DocTipo: 80`.
- "¿Cómo facturo para consumidor final?" → `CbteTipo: 6` (Factura B), `DocTipo: 99`, `DocNro: 0`.
- "AFIP me devuelve error 10016" → Ver tabla de errores en `resources/validaciones.md`.
