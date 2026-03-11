---
name: stock-system-key-points
description: Use when designing, architecting, or implementing stock management systems in .NET MAUI/Blazor. Ensures coverage of architecture, hardware integration, and traceability.
---

# 📦 Stock System Best Practices

## Objetivo
Implementar sistemas de gestión de stock robustos, seguros y eficientes, optimizados para entornos industriales y de ferretería, priorizando la precisión física y la auditoría.

## Core Principles
1. **Precisión Física vs. Teórica**: El sistema debe facilitar auditorías constantes para alinear ambos valores.
2. **Integridad de Hardware**: La comunicación con periféricos (balanzas, escáneres) es crítica y debe ser directa.
3. **Resiliencia de Datos**: El trabajo en depósito no puede detenerse por falta de internet (Offline-first).

## Instrucciones Clave

### 1. Arquitectura y Persistencia
- **Híbrid MAUI/Blazor**: Usar Razor Components con librerías como `MudBlazor` o `Radzen` para DataGrids potentes.
- **Modo Offline**: Implementar SQLite con EF Core para sincronización diferida.
- **Audit Trail**: Captura oculta de `Usuario`, `Terminal` y `Timestamp` para modificaciones críticas.
- **Gestión de Matrices**: Soporte para artículos "padre" con variantes "hijas" en formularios dinámicos.

### 2. Integración de Hardware
- **RS-232/USB**: Usar librerías nativas de .NET para captura directa de pesos de balanzas.
- **Escaneo**: Usar ráfagas de teclado (@onkeydown) o `ZXing.Net.Maui` / `Camera.MAUI`.
- **Impresión**: Generar comandos ZPL para etiquetado térmico directo.

### 3. Trazabilidad y Valoración
- **FEFO (First Expired, First Out)**: Prioritarios para químicos/selladores.
- **PMP (Precio Medio Ponderado)**: Algoritmo por defecto para valoración de capital.
- **Recuentos Cíclicos**: Prohibir la "limpieza manual" de stock; usar asientos de ajuste documentados.

### 4. UI/UX Industrial
- **Semáforo Visual**: Colores (Rojo/Amarillo/Verde) para niveles de reposición (MOQ/Punto de Pedido).
- **Notificaciones**: Usar `Microsoft.Maui.ApplicationModel.Notifications` para alertas push locales de quiebre de stock.
- **Reportes**: `QuestPDF` para PDF profesionales y `ClosedXML` para exportaciones Excel masivas.
- **Distribución**: Usar la API `Share` de MAUI para compartir PDFs por WhatsApp o correo.

## Red Flags (Prohibiciones)
- ❌ **NO** permitir edición directa de cantidad de stock sin un tipo de movimiento (Remito, Consumo, Merma).
- ❌ **NO** depender exclusivamente de internet para registrar movimientos físicos.
- ❌ **NO** usar placeholders para representaciones gráficas de hardware.
