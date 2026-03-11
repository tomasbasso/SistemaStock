import os
import argparse
import sys

def scaffold_skill(base_path, skill_name, description):
    skill_dir = os.path.join(base_path, skill_name)
    
    if os.path.exists(skill_dir):
        print(f"Error: La carpeta de la skill '{skill_name}' ya existe en: {skill_dir}")
        sys.exit(1)

    try:
        os.makedirs(skill_dir)
        os.makedirs(os.path.join(skill_dir, 'scripts'))
        os.makedirs(os.path.join(skill_dir, 'resources'))
        os.makedirs(os.path.join(skill_dir, 'examples'))
    except Exception as e:
        print(f"Error al intentar crear directorios: {e}")
        sys.exit(1)

    skill_md_path = os.path.join(skill_dir, 'SKILL.md')
    template = f"""---
name: {skill_name}
description: {description}
---

# {skill_name.replace('-', ' ').title()}

## Objetivo
[** Skill Architect: Reemplaza este texto detallando el objetivo principal de la skill y lo que logrará en el sistema.]

## Instrucciones
1. [** Skill Architect: Define el paso a paso lógico.]
2. [Si existe lógica compleja, mueve ese trabajo a la carpeta `scripts/`].
3. [Si existen convenciones o datos largos, muévelos a `resources/` e indica al agente leerlos].

## Restricciones
- [** Skill Architect: Define reglas estrictas y limitaciones (lo que NO se puede hacer)].

## Ejemplos
- [** Skill Architect: Opcional. Incluye un ejemplo de cómo invocarla].
"""
    try:
        with open(skill_md_path, 'w', encoding='utf-8') as f:
            f.write(template)
            
        print(f"[OK] ¡Éxito! Se generó la estructura de la skill '{skill_name}' en: {os.path.abspath(skill_dir)}")
        print(f"   Estructura estandarizada ('scripts/', 'resources/', 'examples/') creada.")
        print(f"   Por favor, rellena el archivo {skill_md_path} según corresponda.")
    except Exception as e:
        print(f"Error al escribir el archivo SKILL.md: {e}")
        sys.exit(1)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description="Google Antigravity - Agent Skill Scaffolding Tool")
    parser.add_argument('name', help="El nombre identificador de la skill (en minúsculas y formato kebab-case)")
    parser.add_argument('description', help="La descripción extremadamente precisa que servirá de activador para la skill")
    parser.add_argument('--path', default='.', help="Ruta base del directorio de trabajo (donde aterrizará la carpeta de la skill)")
    
    args = parser.parse_args()
    
    if not args.name.islower() or ' ' in args.name:
         print("[!] Advertencia de Estilo: El nombre de la skill estandarizado debe ir en minúsculas y separado por guiones (kebab-case).")
    
    scaffold_skill(args.path, args.name, args.description)
