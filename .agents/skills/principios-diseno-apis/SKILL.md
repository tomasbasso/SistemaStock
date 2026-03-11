---
name: principios-diseno-apis
description: Skill para diseñar APIs REST profesionales. Define estándares de nomenclatura, métodos HTTP, códigos de respuesta, paginación y seguridad.
---

# 🌐 Diseño Profesional de APIs REST

## Descripción General
Esta skill establece las pautas para crear APIs intuitivas, consistentes y seguras. Sigue los estándares de la industria para facilitar la integración por parte de terceros o de aplicaciones frontend/móviles.

## Reglas de Diseño (REST)
1. **Nomenclatura (Nouns over Verbs)**: 
   - ✅ `/productos`
   - ❌ `/getProductos`
2. **Jerarquía Lógica**: `/categorias/{id}/productos`
3. **Métodos HTTP**:
   - `GET`: Recuperar (Idempotente).
   - `POST`: Crear.
   - `PUT`: Reemplazo total.
   - `PATCH`: Modificación parcial.
   - `DELETE`: Eliminar.

## Códigos de Respuesta Estándar
- `200 OK`: Éxito.
- `201 Created`: Recurso creado.
- `400 Bad Request`: Error de validación/entrada.
- `401/403`: Problemas de autenticación/permisos.
- `404 Not Found`: No existe.
- `422 Unprocessable Entity`: Falló la lógica de negocio/validación.

## Paginación y Filtrado
Las consultas de listas deben soportar parámetros para evitar sobrecarga:
`GET /api/v1/productos?page=1&limit=20&sort=-precio`

## Formato de Error Consistente
```json
{
  "error": {
    "code": "STOCK_INSUFICIENTE",
    "message": "No hay suficientes artículos en inventario",
    "details": []
  }
}
```
