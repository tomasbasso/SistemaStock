---
name: diseno-apps-moviles-sleek
description: Skill para utilizar sleek.design, una herramienta impulsada por IA para diseñar aplicaciones móviles y componentes UI premium. Genera vistas modernas, layouts de alta calidad y prototipos visuales directamente desde descripciones en lenguaje natural.
---

# 📱 Diseño de Apps Móviles con Sleek

## Descripción General
Esta skill utiliza la API de [sleek.design](https://sleek.design) para generar diseños de interfaces móviles de alta fidelidad. Es ideal para crear componentes premium, mockups de pantallas completas y sistemas de diseño modernos que puedan ser luego implementados en Blazor Hybrid o MAUI.

## Flujo de Trabajo con Peticiones de Usuario
Cuando el usuario solicita "diseñar una app de X" o "crear una pantalla de ajustes":
1. **Crear Proyecto**: Iniciar un proyecto en Sleek (si no existe uno relevante).
2. **Describir el Diseño**: Enviar un mensaje con la intención completa del usuario. Sleek interpreta lenguaje natural, así que puedes usar directamente las palabras del usuario.
3. **No es necesario descomponer**: Envía la intención completa y deja que Sleek decida qué pantallas crear.

## Regla de Entrega de Capturas (Visuals First)
**NUNCA** completes una tarea de diseño en silencio. Después de cada generación:
- **Nuevas Pantallas**: Entrega una captura individual por cada pantalla creada y una captura combinada de todo el proyecto.
- **Actualizaciones**: Entrega una captura de cada pantalla afectada por el cambio.
- **Fondo**: Usa `background: "transparent"` por defecto para facilitar la integración visual.

## Cuándo usar esta Skill
- **Fase de Prototipado**: Para mostrar al usuario cómo se vería la UI antes de codificarla en Blazor.
- **Diseño de Componentes**: Para obtener inspiración o assets visuales de alta calidad.
- **Modernización de UI**: Cuando se busca un look "premium" o "sleek" que se aleje de lo genérico.

## Seguridad
- La comunicación es exclusivamente vía HTTPS hacia `sleek.design`.
- Las claves de API (`SLEEK_API_KEY`) deben manejarse con cuidado y nunca exponerse.
