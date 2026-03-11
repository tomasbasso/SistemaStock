---
name: sistema-diseno-tailwind-v4
description: Skill para implementar sistemas de diseño modernos con Tailwind CSS v4. Enfoque en configuración CSS-first (@theme), tokens semánticos, modo oscuro nativo y componentes altamente reutilizables.
---

# 🎨 Sistema de Diseño Tailwind CSS (v4)

## Descripción General
Esta skill define los estándares para construir interfaces modernas, consistentes y de alto rendimiento utilizando **Tailwind CSS v4**. Prioriza la configuración "CSS-first" mediante `@theme`, el uso de colores OKLCH para una mejor percepción visual y una arquitectura de componentes basada en tokens semánticos.

## Principios de Configuración (v4 Core)
1. **Configuración CSS-first**: **NO** uses `tailwind.config.js`. Define todo dentro de bloques `@theme` en tu archivo CSS principal.
2. **Importación Directa**: Usa `@import "tailwindcss";` en lugar de las directivas `@tailwind` antiguas.
3. **Colores OKLCH**: Utiliza el formato `oklch()` para definir colores, ya que ofrece una uniformidad perceptual superior a HSL o RGB.

## Estructura de Tokens
Sigue la jerarquía de tokens para mantener la consistencia:
- **Tokens de Marca (Abstractos)**: Valores crudos (ej. `oklch(45% 0.2 260)`).
- **Tokens Semánticos (Propósito)**: Nombres basados en el uso (ej. `--color-primary`, `--color-background`).
- **Tokens de Componente (Específicos)**: Solo si es necesario para un componente único.

## Mejores Prácticas
- **Usa Clases Semánticas**: Prefiere `bg-primary` en lugar de `bg-blue-500`.
- **Atajos de Tamaño**: Usa `size-4` en lugar de `w-4 h-4`.
- **Modo Oscuro**: Implementa el modo oscuro mediante variables CSS dentro de selectores `.dark` o usando `@custom-variant dark`.
- **Accesibilidad**: No olvides los estados de foco (`focus:ring`) y los atributos ARIA.

## Cuándo usar esta Skill
- Al configurar o refactorizar el sistema de estilos de una aplicación web o híbrida.
- Al crear librerías de componentes UI reutilizables.
- Para implementar temas dinámicos o modo oscuro de forma profesional.

## Ejemplo de Configuración Pro (@theme)
```css
@import "tailwindcss";

@theme {
  --color-primary: oklch(14.5% 0.025 264);
  --color-primary-foreground: oklch(98% 0.01 264);
  --color-background: oklch(100% 0 0);
  
  --radius-lg: 0.5rem;
  
  --animate-fade-in: fade-in 0.2s ease-out;
  @keyframes fade-in {
    from { opacity: 0; }
    to { opacity: 1; }
  }
}
```
