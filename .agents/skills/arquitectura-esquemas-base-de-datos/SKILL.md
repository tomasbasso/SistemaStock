---
name: arquitectura-esquemas-base-de-datos
description: Skill maestra para el diseño de esquemas de bases de datos relacionales y no relacionales. Cubre normalización, diagramas ERD, estrategias de indexación y scripts de migración.
---

# 🗄️ Arquitectura y Diseño de Esquemas

## Descripción General
Esta skill proporciona un marco estructurado para diseñar bases de datos robustas y escalables. Complementa las skills específicas de motores (como PostgreSQL) con principios universales de diseño.

## Flujo de Diseño (Criterios Técnicos)
1. **Entidades y Atributos**: Identificar objetos del dominio y sus propiedades.
2. **Relaciones y Normalización**: 
   - Definir 1:1, 1:N, N:M.
   - Aplicar formas normales (1NF, 2NF, 3NF) para evitar redundancias.
3. **Estrategia de Indexación**:
   - Índices en llaves foráneas para evitar lentitud en JOINs.
   - Índices compuestos para consultas filtradas frecuentes.
4. **Restricciones y Triggers**: Asegurar la integridad de los datos a nivel de motor (Check, Unique, Not Null).
5. **Migraciones**: Diseñar scripts de cambio que permitan evolucionar el esquema sin pérdida de datos.

## Evitar Errores Comunes
- 🚫 **Problema N+1**: Diseño excesivamente fragmentado sin considerar el acceso de datos.
- 🚫 **JOINs Lentos**: Por falta de índices en columnas de relación.
- 🚫 **UUID vs Auto-increment**: Evaluar impacto en rendimiento de inserción vs facilidad de distribución.

## Formato de Entrega
Sugerir siempre el esquema acompañado de un diagrama **Mermaid ERD** para validación visual.
