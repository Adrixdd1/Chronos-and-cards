# Formato de Preguntas (Markdown) - Chronos & Cards

Este documento describe el formato y la sintaxis que el Game Master (GM) debe emplear para definir preguntas personalizadas e importarlas de forma dinámica en **Chronos & Cards**.

---

## 📋 Sintaxis Básica

El archivo debe tener la extensión `.md` y guardarse codificado en **UTF-8** para asegurar que los caracteres especiales (acentos, ñ, signos de interrogación invertidos) se muestren correctamente.

Cada bloque de pregunta se compone de los siguientes delimitadores:

| Delimitador | Significado | Obligatorio | Descripción |
| :---: | :--- | :---: | :--- |
| `# [N]` | **Pregunta Normal** | Sí (para normales) | Inicia una pregunta con dificultad `N` (donde N es 1 a 6). |
| `## [GM]` | **Desafío del GM** | Sí (para GM) | Inicia un reto para el duelo del Game Master. |
| `>` | **Pista de ayuda** | No | Agrega texto de pista. Soporta múltiples líneas consecutivas. |
| `-` o `*` | **Opción Múltiple** | No | Define una opción visible. Soporta entre 2 y 6 opciones. |
| `=` | **Respuesta Correcta** | **Sí** | La solución exacta. O el criterio de juicio manual del GM. |

---

## 💡 Ejemplos por Tipo de Pregunta

### 1. Pregunta con Opciones Múltiples y Pista
```markdown
# [3] ¿Cuál es la velocidad de la luz en el vacío?
> Es aproximadamente 300,000 kilómetros por segundo.
- 150,000 km/s
* 300,000 km/s
- 450,000 km/s
= 300,000 km/s
```

### 2. Pregunta Abierta (Comparación Automática Exacta)
```markdown
# [1] ¿Cuál es el nombre del planeta en el que vivimos?
= Tierra
```

### 3. Pregunta Abierta con Criterio de Juicio Manual (Evaluación del GM)
Si no se proveen opciones múltiples (`-` o `*`) y la respuesta empieza estrictamente con `[Criterio del GM:`, el sistema detendrá la evaluación automática del jugador y le presentará al GM el criterio para que valide manualmente la validez de la respuesta.

```markdown
# [6] Explica en tus palabras la diferencia entre clase y objeto en POO.
= [Criterio del GM: El jugador debe mencionar que una clase es la plantilla o plano de diseño y el objeto es la instancia física en memoria.]
```

### 4. Pregunta del Desafío del Game Master (GM)
```markdown
## [GM] ¿Cuál es el lenguaje principal para desarrollar scripts en Unity?
- C++
* C#
- Python
= C#
```

---

## ⚠️ Advertencias y Errores Comunes

El parser genera un diagnóstico detallado ante problemas de formato. Ten cuidado con:

1. **Respuestas faltantes (Error Fatal)**: Cada pregunta debe tener una línea que empiece con `=` definiendo su respuesta. Si falta, la pregunta se descarta por completo.
2. **Dificultad fuera de rango (Error Fatal)**: El número entre corchetes debe ser estrictamente entre 1 y 6 (ej. `# [0]` o `# [7]` generará un error).
3. **Opción duplicada (Warning)**: Incluir la misma opción dos veces en el bloque de la misma pregunta.
4. **Mazo desbalanceado (Warning)**: Se recomienda tener al menos 3 preguntas por cada uno de los 6 niveles de dificultad para evitar problemas de agotamiento prematuro en partidas largas.
