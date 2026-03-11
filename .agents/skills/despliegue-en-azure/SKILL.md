---
name: despliegue-en-azure
description: Skill para ejecutar el despliegue de aplicaciones en Azure. Gestiona comandos de azd (Azure Developer CLI), actualizaciones de infraestructura y pipelines de verificación post-despliegue.
---

# 🚀 Despliegue en Azure

## Descripción General
Esta skill se encarga de la ejecución técnica del despliegue de aplicaciones preparadas hacia la nube de Azure. Utiliza principalmente la **Azure Developer CLI (`azd`)** para sincronizar el código y la infraestructura con el entorno de producción.

## Cuándo activar esta Skill
- Para ejecutar el despliegue de una aplicación que ya tiene `azure.yaml` e infraestructura definida.
- Para subir actualizaciones a un despliegue de Azure existente.
- Para ejecutar comandos como `azd up`, `azd deploy` o despliegues mediante plantillas Bicep/ARM.
- Para desplegar aplicaciones que incluyen infraestructura de API Management (APIM).

## Reglas de Operación
1. **Validación Previa**: Solo ejecutar después de que el plan de despliegue haya sido validado.
2. **Checklist Pre-Despliegue**: Siempre revisar los requisitos mínimos antes de lanzar el comando de despliegue.
3. **Acciones Destructivas**: Requieren **SIEMPRE** la aprobación explícita del usuario (`ask_user`).
4. **Ámbito**: Esta skill es solo para ejecución; para crear la infraestructura inicial o preparar el proyecto, usa skills de preparación.

## Flujo de Trabajo (Comandos Core)
- `azd up`: Empaqueta, aprovisiona y despliega en un solo paso.
- `azd deploy`: Solo despliega el código de la aplicación.
- `az deployment`: Para despliegues directos a nivel de grupo de recursos o suscripción.

## Referencias para .NET / MAUI
- **Azure Identity**: Uso de `DefaultAzureCredential` para autenticación segura en la nube.
- **SQL & EF Core**: Configuración post-despliegue de bases de datos y migraciones.

## Herramientas MCP sugeridas
- `mcp_azure_mcp_subscription_list`: Para listar suscripciones disponibles.
- `mcp_azure_mcp_azd`: Para interactuar con la CLI de Azure Developer.

## Resolución de Problemas
- Consultar logs de Azure en caso de fallo en el aprovisionamiento.
- Verificar permisos de RBAC (Role-Based Access Control) si hay errores de acceso.
