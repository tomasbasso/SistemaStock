---
name: skill-architect
description: Skill maestra encargada de diseñar, generar y estructurar nuevas Agent Skills siguiendo rigurosamente los estándares de Google Antigravity para el desarrollo de aplicaciones.
---

# 🏛️ Skill Architect

## Objetivo
Generar nuevas Agent Skills estructuradas, efímeras y modulares basándose en los estándares de Google Antigravity (Progressive Disclosure y YAML Frontmatter rigurosamente respetado). Esta meta-skill permite expandir las capacidades del agente de manera ordenada y dotarlo de habilidades potentes para el desarrollo de aplicaciones robustas.

## Instrucciones
Cuando el usuario te pida crear una nueva skill de desarrollo, sigue obligatoriamente este flujo de trabajo:

1. **Recopilación de requisitos (Prompt the User)**:
   - Si la solicitud inicial es ambigua, haz preguntas clarificadoras antes de empezar a crear archivos: 
     * ¿Cuál es el objetivo principal (Core Action)? 
     * ¿Qué herramientas, lenguajes o scripts específicos necesitará? 
     * ¿Tendrá plantillas de código fuente estáticas que deba inyectar? 
     * ¿Cuáles son sus restricciones inquebrantables (lo que NO debe hacer)?

2. **Ejecutar el Script de Andamiaje (Scaffolding)**:
   - Ejecuta el script de Python `scripts/scaffold_skill.py` (ubicado dentro de tu propia carpeta `skill-architect`) proporcionándole el nombre de la nueva skill (en `kebab-case`), la descripción (¡activador crítico del enrutador!) y la ruta destino (generalmente el directorio de trabajo del usuario).
   - Ejemplo: `python "c:/Users/UD/Desktop/Creador de Skills/skill-architect/scripts/scaffold_skill.py" "react-component-gen" "Genera componentes de React con tests unitarios" --path "c:/Users/UD/Desktop/Creador de Skills"`
   - Esto creará automáticamente la estructura base: `SKILL.md`, `scripts/`, `resources/`, `examples/`.

3. **Populación de Archivos (Aplicando Progressive Disclosure)**:
   - Escribe el contenido formativo de `[skill-name]/SKILL.md` (basándote en la plantilla si la necesitas, o rellenando el archivo base generado por el script). Deberás expandir los apartados de instrucciones.
   - **Vital:** Mueve toda lógica pesada o scripts complejos de bash/python a scripts dedicados dentro de `[skill-name]/scripts/`. No satures el contexto del enrutador.
   - Si la skill requiere de documentos largos, reglas de diseño de UI/UX, prompts de base, o código boilerplate, ubícalos como archivos `.md` o de texto bajo `[skill-name]/resources/`, e instruye al agente en el `SKILL.md` a leerlos mediante `view_file` cuando sea invocado.

4. **Confirmación**:
   - Notifica al usuario que la skill ha sido generada exitosamente y muestra un ejemplo rápido de cómo pedirle al agente que la invoque a partir de ahora.

## Restricciones
- **NO** crees la carpeta de la skill a ciegas sin asegurarte de tener una `description` extremadamente precisa (esto funge de "frase de activación" para el enrutador de modelos).
- **NO** escribas un archivo `SKILL.md` gigantesco o masivo. Aplica el principio de aislar la carga cognitiva pesada en `resources/` y `scripts/`.
- **NO** uses camelCase ni snake_case para el `name` del YAML. Exclusivamente en minúsculas y con guiones (`kebab-case`).

## Ejemplos
### Ejemplo: Crear una skill de "Despliegue de Docker"
1. Consultas detalles con el usuario.
2. Usas `run_command` y ejecutas: `python "c:/Users/UD/Desktop/Creador de Skills/skill-architect/scripts/scaffold_skill.py" "docker-deployer" "Despliega aplicaciones empaquetadas en Docker a un servidor remoto." --path "."`
3. Editas `docker-deployer/SKILL.md` con las instrucciones.
4. Creas `docker-deployer/scripts/deploy.sh` de bash con la lógica pura para que el agente simplemente deba llamarlo al usar la skill.
