---
name: arquitecto-computo-azure
description: Skill para seleccionar y dimensionar servicios de cómputo en Azure (VMs, VMSS). Ayuda a comparar familias, tamaños y precios para optimizar costo y rendimiento.
---

# 🖥️ Arquitecto de Cómputo Azure

## Descripción General
Esta skill permite tomar decisiones informadas sobre qué recursos de cómputo utilizar en Azure para diferentes cargas de trabajo (web, bases de datos, ML, etc.). Se enfoca en la selección de máquinas virtuales (VM) y conjuntos de escala (VMSS).

## Escenarios de Uso
- Elegir entre una VM individual o un conjunto de escala (VMSS).
- Recomendar tamaños de VM basados en la carga (CPU, RAM, IOPS).
- Comparar familias de VMs (General Purpose, Compute Optimized, Memory Optimized).
- Estimar costos sin necesidad de una cuenta activa.
- Configurar autoescalado y alta disponibilidad.

## Flujo de Trabajo (Workflow)
1. **Recopilar Requisitos**: Determinar carga (Web, DB, Batch), SSOO, y necesidades de persistencia.
2. **VM vs VMSS**: Usar VM para cargas estables; VMSS para escalabilidad horizontal y alta disponibilidad.
3. **Seleccionar Familia**:
   - `D-Series`: Propósito general.
   - `F-Series`: Optimizado para cómputo.
   - `E/M-Series`: Optimizado para memoria.
4. **Verificación de Precios**: Usar herramientas de búsqueda para comparar Tiers (Spot, Reservado, Pay-as-you-go).

## Regla Crítica
**SIEMPRE** verificar con la documentación en vivo de `learn.microsoft.com` antes de hacer recomendaciones finales, ya que los precios y tamaños cambian frecuentemente.
