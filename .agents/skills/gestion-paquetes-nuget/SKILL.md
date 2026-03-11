---
name: gestion-paquetes-nuget
description: Gestión segura y consistente de paquetes NuGet en proyectos .NET. Prioriza el uso de la CLI de dotnet para mantener la integridad del proyecto y centralizar versiones.
---

# 📦 Gestión de Paquetes NuGet

## Descripción General
Esta skill garantiza una gestión consistente y segura de los paquetes NuGet en soluciones .NET. Prioriza el uso de la CLI de `dotnet` para mantener la integridad del proyecto y aplica un flujo de trabajo riguroso para la actualización de versiones.

## Reglas Principales
1. **NUNCA** edites directamente los archivos `.csproj` para añadir o eliminar paquetes. Usa siempre los comandos `dotnet add package` y `dotnet remove package`.
2. **EDICIÓN DIRECTA** solo está permitida para cambiar la versión de paquetes existentes.
3. **VERIFICACIÓN OBLIGATORIA**: Antes de actualizar, confirma que la versión existe en NuGet.
4. **RESTAURACIÓN INMEDIATA**: Ejecuta `dotnet restore` inmediatamente después de cualquier cambio manual de versión para verificar la compatibilidad.

## Flujos de Trabajo

### Añadir un Paquete
Usa el comando:
```powershell
dotnet add [<PROYECTO>] package <NOMBRE_PAQUETE> [--version <VERSION>]
```
*Ejemplo: `dotnet add src/PazSport.csproj package Newtonsoft.Json`*

### Eliminar un Paquete
Usa el comando:
```powershell
dotnet remove [<PROYECTO>] package <NOMBRE_PAQUETE>
```
*Ejemplo: `dotnet remove src/PazSport.csproj package Newtonsoft.Json`*

### Actualizar Versiones de Paquetes
1. **Verificar Existencia**:
   ```powershell
   (dotnet package search <NOMBRE_PAQUETE> --exact-match --format json | ConvertFrom-Json).searchResult.packages | Where-Object { $_.version -eq "<VERSION>" }
   ```
2. **Identificar Gestión de Versiones**:
   - Busca `Directory.Packages.props` en la raíz (gestión centralizada).
   - Si no existe, revisa los archivos individuales `.csproj`.
3. **Aplicar Cambio**: Modifica el archivo con la nueva cadena de versión.
4. **Verificar Estabilidad**: Ejecuta `dotnet restore`. Si hay errores, revierte y analiza.

## Ejemplos
- **Usuario**: "Añadí Serilog al proyecto" 
  -> Acción: `dotnet add src/Proyecto/Proyecto.csproj package Serilog`
- **Usuario**: "Actualiza Newtonsoft.Json a la 13.0.3"
  -> Acción: Verificar versión, editar `.csproj` o `Directory.Packages.props`, ejecutar `dotnet restore`.
