---
name: github-repo-uploader
description: Crea un nuevo repositorio en GitHub y sube el código fuente local actual a él de forma automatizada.
---

# GitHub Repo Uploader

## Objetivo
Inicializar un repositorio Git en el directorio de trabajo actual, crear un nuevo repositorio en GitHub usando las herramientas MCP (Model Context Protocol), y subir el código fuente local al nuevo repositorio remoto.

## Instrucciones
1. **Verificación de estado**: Confirma con el usuario en qué directorio se encuentra el código que desea subir y cuál debería ser el nombre del repositorio (y si debe ser público o privado).
2. **Crear el repositorio en GitHub**:
   - Utiliza exclusivamente tu herramienta `mcp_github-mcp-server_create_repository` proporcionada por el servidor MCP de GitHub (ej. pasando el parámetro `name` y `private`).
   - El resultado te dará un campo `clone_url` o `html_url`. Extrae la URL HTTPS del nuevo repositorio remoto. *(Ejemplo: `https://github.com/usuario/nombre-repo.git`)*
3. **Ejecutar el script de inicialización y empuje local**:
   - Llama al script incluido en esta skill para inicializar Git, hacer commit del código actual y hacer push hacia el nuevo remoto.
   - Ejecuta: `python "c:/Users/UD/Desktop/Creador de Skills/github-repo-uploader/scripts/init_and_push.py" "<URL_DEL_REPOSITORIO>"` (Sustituye la URL por la obtenida en el paso 2).
4. **Validación**:
   - Revisa la salida del comando. Si hubo advertencias sobre credenciales, indícale al usuario que necesita estar autenticado en Git localmente (ej. a través de Git Credential Manager ou SSH).
   - Informa al usuario del éxito y provéele el enlace directo a su nuevo repositorio en GitHub.

## Restricciones
- **NO** intentes inicializar git o agregar archivos manualmente usando `run_command` línea por línea. Usa siempre el script `init_and_push.py` para evitar problemas de plataforma y garantizar robustez.
- **NO** subas carpetas como `node_modules`, `venv`, o `.env` si puedes evitarlo. Aunque el script intenta crear un `.gitignore` genérico si no existe, ten cuidado.
- **NO** crees repositorios sin asegurarte del nombre del proyecto que desea el usuario.

## Ejemplos
### Caso de uso válido
Entrada del usuario: "Sube este proyecto en el que estamos trabajando a un nuevo repo privado en GitHub llamado 'mi-app-increible'".
Acción del agente:
1. Llamas al tool `mcp_github-mcp-server_create_repository` con `name="mi-app-increible"`, `private=true`.
2. Tomas la URL resultante (ej. https://github.com/user/mi-app-increible.git).
3. Usas `run_command`: `python "c:/Users/UD/Desktop/Creador de Skills/github-repo-uploader/scripts/init_and_push.py" "https://github.com/user/mi-app-increible.git"`.
4. Informas al usuario cuando termine correctamente.
