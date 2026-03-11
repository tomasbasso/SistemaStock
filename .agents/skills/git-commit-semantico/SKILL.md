---
name: git-commit-semantico
description: Skill para estandarizar el historial de cambios utilizando Conventional Commits. Mejora la trazabilidad, facilita el versionado automático y la generación de changelogs.
---

# 📝 Git Commit Semántico

## Descripción General
Esta skill implementa la especificación de **Conventional Commits** para mantener un historial de Git limpio, legible y profesional.

## Formato de Commit
`<tipo>[ámbito opcional]: <descripción corta>`

### Tipos Principales:
- `feat`: Nueva funcionalidad para el usuario.
- `fix`: Corrección de un error.
- `docs`: Cambios solo en la documentación.
- `style`: Cambios de formato (espacios, puntos y coma) no funcionales.
- `refactor`: Cambio de código que no arregla bug ni añade feature.
- `perf`: Mejora de rendimiento.
- `test`: Añadir o corregir pruebas.
- `chore`: Tareas de mantenimiento (build, dependencias).

## Reglas de Oro
- **Tiempo Presente**: Usar "add" no "added", "fix" no "fixed".
- **Modo Imperativo**: "add feature" no "adds feature".
- **Un cambio por commit**: No mezclar refactor con funcionalidad nueva.
- **Referencias**: Incluir número de ticket/tarea si existe (ej. `Closes #123`).

## Workflow de Seguridad
1. **Analizar Diff**: Revisar siempre `git diff --staged` antes de confirmar.
2. **Sin Secretos**: Nunca añadir archivos `.env` o credenciales.
3. **Mensaje Claro**: La descripción debe tener menos de 72 caracteres.
