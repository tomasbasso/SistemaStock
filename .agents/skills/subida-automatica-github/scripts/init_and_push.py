import os
import sys
import subprocess

def run_cmd(cmd, check=True):
    print(f"Ejecutando: {' '.join(cmd)}")
    result = subprocess.run(cmd, capture_output=True, text=True)
    if check and result.returncode != 0:
        print(f"ERROR al ejecutar: {' '.join(cmd)}")
        print(f"Salida de error: {result.stderr}")
        sys.exit(result.returncode)
    return result

def init_and_push(repo_url):
    print(f"Iniciando el proceso de subida a GitHub: {repo_url}")
    
    # 1. Comprobar si Git está instalado
    try:
        run_cmd(['git', '--version'])
    except FileNotFoundError:
        print("ERROR: 'git' no se reconoce como un comando interno o externo.")
        print("Asegúrate de tener Git instalado y agregado al PATH del sistema.")
        sys.exit(1)

    # 2. Inicializar repositorio si no lo está
    if not os.path.isdir('.git'):
        run_cmd(['git', 'init'])
        print("Repositorio Git inicializado localmente.")
    else:
        print("El directorio ya es un repositorio Git.")

    # 3. Crear un .gitignore básico si no existe para evitar subir basura
    if not os.path.isfile('.gitignore'):
        ignore_content = "node_modules/\n__pycache__/\n.env\n*.log\n.venv/\nvenv/\n"
        with open('.gitignore', 'w') as f:
            f.write(ignore_content)
        print("Se ha creado un archivo .gitignore básico ('node_modules', 'venv', '.env', etc).")

    # 4. Configurar la rama a main en lugar de master
    run_cmd(['git', 'checkout', '-b', 'main'], check=False) # Si ya existe o falla por otra razón lo ignora
    run_cmd(['git', 'branch', '-M', 'main'])

    # 5. Agregar archivos
    run_cmd(['git', 'add', '.'])
    
    # Comprobar si hay algo para comitear
    status = run_cmd(['git', 'status', '--porcelain'])
    if not status.stdout.strip():
        print("No hay archivos nuevos ni cambios para subir al repositorio.")
    else:
        # 6. Hacer commit
        run_cmd(['git', 'commit', '-m', 'Initial commit by Antigravity AI'])

    # 7. Configurar remoto
    remotes = run_cmd(['git', 'remote', '-v']).stdout
    if 'origin' in remotes:
        print("Actualizando URL del remoto 'origin' existente...")
        run_cmd(['git', 'remote', 'set-url', 'origin', repo_url])
    else:
        run_cmd(['git', 'remote', 'add', 'origin', repo_url])

    # 8. Empujar los cambios
    print("Empujando código a GitHub... (Esto puede requerir autenticación del usuario en el sistema)")
    run_cmd(['git', 'push', '-u', 'origin', 'main'])

    print("==================================================")
    print("✅ ¡Código subido exitosamente a GitHub!")
    print(f"✅ Repositorio disponible en: {repo_url}")
    print("==================================================")

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Uso: python init_and_push.py <URL_DEL_REPOSITORIO_GITHUB>")
        sys.exit(1)
        
    github_url = sys.argv[1]
    init_and_push(github_url)
