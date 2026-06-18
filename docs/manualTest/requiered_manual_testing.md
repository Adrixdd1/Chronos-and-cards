# Especificación de Pruebas Manuales Requeridas (Required Manual Testing) - Chronos & Cards

Este documento detalla los escenarios de prueba manuales y los criterios de aceptación para verificar el correcto funcionamiento del **Sistema de Tablero (Épica 2)** y su integración con el **Core FSM (Épica 1)** en el Unity Editor y en runtime.

---

## 📋 Escenarios de Prueba

### 1. Validación de Configuración del Tablero (HU-2.1)
* **Objetivo:** Verificar que el ScriptableObject `BoardConfig` valida correctamente las propiedades ingresadas en el Inspector.
* **Pasos:**
  1. En el Unity Editor, crea un nuevo preset de configuración haciendo clic derecho en la carpeta de assets y seleccionando `Create > Chronos > BoardConfig`.
  2. Intenta ingresar valores inválidos:
     - `TargetQuestionCount` = 5 (menor que el rango permitido de 8-50).
     - `TileTypeWeights` con 4 o 6 elementos (deben ser exactamente 5).
     - `TileTypeWeights` cuyos pesos sumen 1.5 o 0.7 (deben sumar 1.0).
     - `MinBranches` = 3 y `MaxBranches` = 1 (Min debe ser menor o igual a Max).
  3. Presiona Enter o haz clic fuera del Inspector para gatillar la validación de Unity (`OnValidate`).
* **Resultado Esperado:**
  - `TargetQuestionCount` debe autotajarse al valor mínimo permitido de 8.
  - Se debe emitir una advertencia en la Consola (`Debug.LogWarning`) si los pesos no suman 1.0, y el sistema debe autotemperarlos o normalizarlos para que sumen 1.0.
  - El Inspector debe corregir `MinBranches` para que no sea mayor que `MaxBranches`.

---

### 2. Inicialización y Generación del Tablero Lineal (HU-2.7 & HU-2.9)
* **Objetivo:** Verificar la generación secuencial del tablero de tipo "Oca".
* **Pasos:**
  1. Selecciona o crea un asset `BoardConfig` con `Mode = Linear` y una semilla aleatoria fija (ej. `RandomSeed = 12345`).
  2. Asigna este config al componente `BoardManager` en una escena de prueba y ejecuta la inicialización (`Initialize()`).
  3. Inspecciona en los logs de la consola el listado de casillas generadas.
* **Resultado Esperado:**
  - El tablero debe constar exactamente de `EstimatedTotalTiles` casillas.
  - La primera casilla (Índice 0) debe ser de tipo `Neutral`.
  - La última casilla (Meta) debe ser de tipo `Neutral`.
  - Todas las casillas intermedias deben poseer tipos distribuidos aleatoriamente conforme a los pesos configurados (`TileTypeWeights`).
  - Todas las casillas deben iniciar con `IsDiscovered = true` (completamente visibles en el tablero).
  - Al reinicializar con la misma semilla (`RandomSeed = 12345`), la secuencia de tipos de casilla debe ser exactamente idéntica.

---

### 3. Generación del Tablero de Exploración - DAG y Conectividad (HU-2.8)
* **Objetivo:** Verificar la generación de la red de nodos en modo Exploración, asegurando que no existan ciclos y que el mapa se revele progresivamente.
* **Pasos:**
  1. Configura un `BoardConfig` con `Mode = Exploration`, `MinBranches = 1`, `MaxBranches = 2`.
  2. Ejecuta la inicialización en el `BoardManager`.
  3. Revisa la consola y las estructuras de datos instanciadas del grafo (`BoardGraph`).
* **Resultado Esperado:**
  - La primera capa debe constar de 1 solo nodo de Inicio (`Neutral`, `IsDiscovered = true`).
  - Las casillas adyacentes conectadas directamente al Inicio deben marcarse inmediatamente como `IsDiscovered = true`. El resto de las casillas debe iniciar con `IsDiscovered = false`.
  - Todas las casillas intermedias deben tener al menos una conexión de entrada y una de salida.
  - Todos los caminos deben converger eventualmente en un único nodo de Meta (`Neutral`).
  - No debe haber bucles de retroceso (verificar que no haya caminos que lleven a un nodo con un índice de capa inferior).

---

### 4. Navegación en Casillas y Efectos de Recursos (HU-2.3 & HU-2.4)
* **Objetivo:** Verificar que el paso de los jugadores por las casillas de recursos (Pista+ y Pista-) altera sus inventarios de forma legal.
* **Pasos:**
  1. Inicializa una partida simulada con dos jugadores (`Player 1` y `Player 2`).
  2. Fuerza al `Player 1` a aterrizar en una casilla `HintBoostTile`. Revisa el log de la consola y su saldo de pistas.
  3. Fuerza al `Player 1` a aterrizar en una casilla `HintTrapTile`. Revisa el log de la consola y su saldo de pistas.
  4. Agota las pistas de `Player 1` a 0 e intenta forzarlo a caer nuevamente en una casilla `HintTrapTile`.
* **Resultado Esperado:**
  - Al caer en `HintBoostTile`, las pistas de `Player 1` aumentan en +1, y se dispara el evento `OnHintChanged`.
  - Al caer en `HintTrapTile`, las pistas disminuyen en -1, disparando `OnHintChanged`.
  - Al caer en `HintTrapTile` teniendo 0 pistas, el saldo se mantiene en 0 (no debe bajar a valores negativos) y se previene cualquier desbordamiento.
  - En todos los casos se debe gatillar y loguear el evento de efecto aplicado (`OnTileEffectApplied`).

---

### 5. Bifurcaciones y Selección de Ruta (HU-2.10)
* **Objetivo:** Verificar que el sistema pausa el movimiento y solicita la decisión del jugador en bifurcaciones en modo Exploración.
* **Pasos:**
  1. Carga el tablero en modo `Exploration`.
  2. Ubica al jugador en un nodo que conecte a múltiples casillas de salida (bifurcación).
  3. Ejecuta `MovePlayer` con un valor de movimiento que sobrepase la bifurcación (ej. 3 casillas).
  4. Observa el estado de la FSM y los eventos disparados.
* **Resultado Esperado:**
  - El movimiento automático debe pausarse en la bifurcación.
  - Se debe disparar el evento `GameEvents.OnPathChoiceRequired` enviando la lista de casillas de destino disponibles.
  - Al simular que el jugador selecciona una de las casillas mediante el evento `GameEvents.OnPathChoiceSelected`, el movimiento debe reanudarse y el jugador debe avanzar por la ruta elegida.
  - Las casillas adyacentes al nuevo nodo alcanzado deben revelarse (`IsDiscovered = true`) y disparar `GameEvents.OnTileDiscovered`.

---

### 6. Intercambio de Casillas (HU-2.10)
* **Objetivo:** Verificar que el método de manipulación del tablero `SwapTiles` opera bajo las restricciones físicas correctas.
* **Pasos:**
  1. Con el tablero lineal generado, intenta invocar `SwapTiles` para intercambiar:
     - Casilla 0 (Inicio) y Casilla 1.
     - Casilla Meta y Casilla Meta-1.
     - Dos casillas normales separadas por 1 casilla intermedia (distancia = 2).
     - Dos casillas normales separadas por 5 casillas (distancia = 6).
  2. Verifica que las posiciones internas de los jugadores que estaban en dichas casillas se actualizan.
* **Resultado Esperado:**
  - El intercambio que involucra la casilla de Inicio o Meta debe ser **rechazado** con un log de error/advertencia.
  - El intercambio a distancias mayores a las permitidas (distancia > 2 en lineal, o no adyacentes en exploración) debe ser **rechazado**.
  - Los intercambios válidos deben completarse, actualizar sus índices internos, disparar el evento `GameEvents.OnBoardModified` y reposicionar a cualquier jugador presente en los casilleros modificados.
