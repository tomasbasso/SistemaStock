---
name: skill-creator
description: Use when creating new skills, editing existing skills, or designing agent capabilities following TDD standards.
---

# 🏛️ Unified Skill Architect

## Objetivo
Generar y mantener Agent Skills de alta calidad, combinando el **andamiaje automático** con el **Rigor TDD (Test-Driven Development)** aplicado a la documentación. Esta skill garantiza que cada habilidad sea estructurada, optimizada para la búsqueda (CSO) y verificada mediante pruebas de presión antes de su despliegue.

## Core Principle: No Skill without a Failing Test
**Escribir habilidades ES desarrollo dirigido por pruebas.** Si no has visto a un agente fallar sin la skill (RED), no sabes si la skill enseña lo correcto (GREEN).

## Instrucciones

### 1. Fase RED (Fallo Inicial)
Antes de crear cualquier archivo, define el **Escenario de Presión**:
- Ejecuta un subagente sin la skill y documenta su comportamiento.
- Identifica sus racionalizaciones y fallos (p. ej., "no sabía que debía borrar el código").
- Si no hay fallo basal, la skill no es necesaria o el escenario no es lo suficientemente estresante.

### 2. Fase GREEN (Andamiaje y Creación)
Usa la automatización para crear la estructura:
- Ejecuta: `python "c:/Users/UD/Desktop/Comercial Caiquen/.agent/skills/skill-architect/scripts/scaffold_skill.py" [name] "[description]" --path "c:/Users/UD/Desktop/Comercial Caiquen/.agent/skills"`
- **Popula `SKILL.md`**: Escribe instrucciones mínimas que solucionen los fallos detectados en RED.
- **Progressive Disclosure**: Mueve la lógica pesada a `scripts/` y los datos largos a `resources/`.

### 3. Fase REFACTOR (Cierre de Brechas)
- **Prueba con la Skill**: Ejecuta el subagente de nuevo. ¿Sigue racionalizando?
- **Cierra el Círculo**: Actualiza la **Tabla de Racionalizaciones** y la lista de **Red Flags** en el `SKILL.md` de la nueva habilidad para prohibir explícitamente los "atajos" detectados.

## Estándares de Calidad (CSO & Structure)
- **Description**: Debe empezar con "Use when...", describir síntomas/problemas, y **NUNCA** resumir el flujo de trabajo de la skill.
- **Strict Naming**: Exclusivamente `kebab-case`.
- **Minimalist**: Menos de 500 palabras por `SKILL.md`. Si es más largo, usa `resources/`.

## Restricciones
- **NO** despliegues una skill sin haber documentado el paso RED inicial.
- **NO** incluyas ejemplos narrativos de una sola vez; usa patrones generales reutilizables.
- **NO** ignores las racionalizaciones del agente; documéntalas y bloquéalas.

## Ejemplos de Flujo
1. **User**: "Crea una skill para optimizar consultas SQL".
2. **Agent (RED)**: Prueba a optimizar SQL sin la skill → El subagente olvida usar índices.
3. **Agent (GREEN)**: `python scaffold_skill.py sql-optimizer ...` → Escribe instrucciones sobre índices.
4. **Agent (REFACTOR)**: Prueba con la skill → El subagente intenta usar índices pero de forma redundante → Actualiza `SKILL.md` con prohibición de redundancia.
