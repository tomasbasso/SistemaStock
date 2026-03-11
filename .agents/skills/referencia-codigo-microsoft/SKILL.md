---
name: referencia-codigo-microsoft
description: Skill especializada en encontrar, verificar y aplicar referencias de código oficiales de Microsoft. Úsala para validar firmas de métodos, namespaces, paquetes NuGet y ejemplos reales antes de implementar.
---

# 📚 Referencia de Código Microsoft (SDK & APIs)

## Descripción General
Esta skill complementa la consulta de documentación enfocándose en la **precisión técnica del código**. Su objetivo es evitar errores comunes como nombres de métodos incorrectos, versiones de SDK mezcladas o namespaces inexistentes.

## Flujo de Validación de Código
Antes de escribir código que use SDKs de Microsoft (Azure, .NET, MAUI), sigue este flujo:

1. **Confirmar Existencia**: Verifica que el método o paquete existe.
   - 🔍 `microsoft_docs_search(query: "[ClassName] [MethodName] [Namespace]")`
2. **Consultar Detalles**: Si el método tiene múltiples sobrecargas o parámetros complejos.
   - 📄 `microsoft_docs_fetch(url: "[URL_de_la_doc]")`
3. **Buscar Ejemplo Real**: Encuentra un patrón de trabajo oficial.
   - 💡 `microsoft_code_sample_search(query: "[tarea específica]", language: "csharp")`

## Cuándo usar esta Skill
- **Antes de escribir**: Para encontrar el patrón correcto (ej. cómo inicializar un cliente de base de datos).
- **Tras un error**: Para comparar tu implementación con un ejemplo que funciona.
- **Diferenciación de Versiones**: Vital para no mezclar SDKs antiguos (v11) con nuevos (v12/Azure.*).
- **Primera vez**: Siempre que uses una API o librería de Microsoft por primera vez.

## Búsquedas Precisas (Ejemplos)
- **Métodos**: `"BlobClient UploadAsync Azure.Storage.Blobs"`
- **Clases/Interfaces**: `"DefaultAzureCredential class Azure.Identity"`
- **Paquetes NuGet**: `"Azure Blob Storage NuGet package"` o `"Microsoft.EntityFrameworkCore.PostgreSQL package"`

## Reglas de Oro
- **Verifica siempre**: Si un nombre de método parece "demasiado conveniente" (ej. `UploadFile` vs el real `UploadAsync`), verifícalo.
- **Namespaces**: Incluye siempre el namespace en las búsquedas para mayor precisión.
- **Arquitectura**: Los ejemplos oficiales suelen seguir las mejores prácticas de arquitectura; tómalos como base.

## Resolución de Problemas
Si encuentras un error, busca específicamente:
- `"[Clase] methods [Namespace]"` para ver qué métodos están disponibles.
- `"[Tipo] migration vX to vY"` si estás trabajando con código heredado.
- `"[Servicio] troubleshooting"` para errores de configuración comunes.
