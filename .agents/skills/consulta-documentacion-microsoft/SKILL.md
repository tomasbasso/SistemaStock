---
name: consulta-documentacion-microsoft
description: Skill para buscar y consultar eficazmente la documentación oficial de Microsoft (Azure, .NET, MAUI, etc.). Optimiza las consultas para obtener ejemplos de código y tutoriales precisos.
---

# 📚 Consulta de Documentación Microsoft

## Descripción General
Esta skill permite acceder de forma experta a la vasta documentación de Microsoft en `learn.microsoft.com`. Es fundamental para resolver dudas sobre .NET MAUI, Blazor Hybrid, C# y servicios de Azure.

## Herramientas Principales
1. **`microsoft_docs_search`**: Herramienta primaria para cualquier consulta sobre .NET, Windows y MAUI.
2. **`microsoft_code_sample_search`**: Específicamente para buscar ejemplos de código listos para usar en C#.
3. **`microsoft_docs_fetch`**: Úsala **después** de una búsqueda cuando necesites el tutorial completo, todas las opciones de configuración o cuando los fragmentos de la búsqueda parezcan incompletos.

## Reglas para Consultas Efectivas
**NO** uses términos vagos. Sé específico incluyendo versión, intención y lenguaje.

### Ejemplos de buenas consultas:
- ✅ `.NET MAUI Blazor Hybrid navigation patterns .NET 8`
- ✅ `Blazor component lifecycle events tutorial`
- ✅ `Entity Framework Core PostgreSQL configuration guide`
- ✅ `MAUI splash screen best practices for Android and iOS`

### Estructura recomendada:
- **Tecnología + Versión**: (ej. `.NET 9`, `MAUI 11`)
- **Intención**: (ej. `quickstart`, `API reference`, `troubleshooting`, `best practices`)
- **Lenguaje**: Siempre especificar `C#` o `Blazor` si aplica.

## Excepciones
Si la documentación que buscas no está en `learn.microsoft.com` (como herramientas muy nuevas o específicas de GitHub), utiliza herramientas de búsqueda web general, pero siempre prioriza el MCP de Microsoft Learn para fiabilidad técnica.
