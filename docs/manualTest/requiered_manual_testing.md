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

---

### 7. Lanzamiento y Animación Física del Dado (HU-3.3 & HU-3.4)
* **Objetivo:** Verificar que el dado 3D físico simula el lanzamiento en la escena y se orienta suavemente a la cara predeterminada por la lógica RNG.
* **Pasos:**
  1. En una escena con gravedad y colisionadores de suelo (`DiceFloor`), lanza el dado invocando `AnimateToResult(targetValue)` para cada valor del 1 al 6.
  2. Observa el impulso de fuerza vertical y el torque aleatorio aplicados en el inicio.
  3. Observa cómo el dado desacelera por rozamiento físico y, al cruzar el umbral `StopThreshold`, realiza el acomodo (`Slerp`) a la orientación de la cara objetivo.
* **Resultado Esperado:**
  - El dado debe experimentar fuerzas físicas (saltar y rotar) en cada lanzamiento.
  - Al detenerse, el dado debe quedar orientado con la cara especificada (`targetValue`) apuntando hacia arriba.
  - La interpolación de la rotación hacia la cara correcta debe ser fluida e imperceptible, evitando snaps toscos.
  - Se debe disparar el evento `OnDiceAnimationComplete` cuando la rotación final se asiente.

---

### 8. Activación de Timeout de Físicas (HU-3.3)
* **Objetivo:** Verificar la existencia de la red de seguridad (`AnimationTimeout`) si el dado físico queda atascado o vibra indefinidamente.
* **Pasos:**
  1. Abre el preset `DiceConfig_Default` y ajusta `MaxRollDuration` a un valor bajo (ej. 1 segundo) y `AnimationTimeout` a 3 segundos.
  2. Lanza el dado en un espacio cerrado donde no pueda dejar de rebotar (ej. atrapado entre colliders estrechos).
  3. Monitorea el tiempo transcurrido desde el lanzamiento.
* **Resultado Esperado:**
  - Si el dado no se detiene físicamente tras 1 segundo (superando `MaxRollDuration`), el sistema debe forzar el settle desactivando las físicas del rigidbody (`isKinematic = true`).
  - Si el settle se bloquea por completo, a los 3 segundos (`AnimationTimeout`) el sistema debe forzar la orientación final del dado de forma instantánea y gatillar `OnDiceAnimationComplete` de manera segura, evitando bloquear el ciclo de juego.

---

### 9. Mapeo de Dificultad Dinámica del Dado (HU-3.5)
* **Objetivo:** Verificar que el `DifficultyMapper` mapea correctamente los números del dado a niveles de dificultad, respetando los presets personalizados del inspector.
* **Pasos:**
  1. En el Unity Editor, crea un asset `DifficultyMapConfig` alternativo (ej. `DifficultyMap_Easy.asset`) y configúralo con el mapa `{ 1, 1, 2, 3, 4, 5 }` (sesgando los dados 1 y 2 a dificultad de carta nivel 1).
  2. Asigna este config al `DifficultyMapper` y simula lanzamientos de dado del 1 al 6.
  3. Ejecuta los mismos pasos con el config por defecto `{ 1, 2, 3, 4, 5, 6 }`.
* **Resultado Esperado:**
  - Con el preset fácil: un dado con valor 1 debe mapear a dificultad 1, un dado con valor 2 debe mapear a dificultad 1, y un dado con valor 6 a dificultad 5.
  - Con el preset por defecto: el mapeo debe ser directo e idéntico 1:1 (dado 1 -> dificultad 1, ..., dado 6 -> dificultad 6).
  - Al ingresar un dado fuera del rango 1-6 (ej. 0 o 7), se debe lanzar un error `ArgumentOutOfRangeException`.

---

### 10. Integración de Flujo de Turno y Respuestas (HU-3.9)
* **Objetivo:** Verificar que la FSM avanza por todas las fases del turno al recibir inputs de respuesta desde la UI y aplica correctamente las fórmulas de avance.
* **Pasos:**
  1. Inicia la FSM de prueba.
  2. Cuando el flujo llegue a `ResolutionState`, el juego debe pausarse esperando respuesta.
  3. Simula la entrega de la respuesta correcta enviando la respuesta exacta mediante el evento `GameEvents.OnAnswerSubmitted` (o `SubmitAnswer` en el estado).
  4. Repite el flujo simulando:
     - Una respuesta correcta utilizando pistas (`UsedHint = true`).
     - Una respuesta incorrecta (`Fail`).
* **Resultado Esperado:**
  - Si la respuesta es correcta y no se usó ayuda, la fórmula calcula avance x1.0, el jugador se mueve en el tablero la distancia total del dado, y se transiciona a `TileEffectState`.
  - Si es correcta con ayuda, la fórmula calcula avance x0.5 (redondeando el avance del dado a la mitad) y se transiciona.
  - Si la respuesta es incorrecta, el avance es 0 (o retroceso si aplica una penalización), se habilita la ventana para usar items tras fallo, y se pasa al estado de efectos de casilla.

---

### 11. Carga de Archivo Markdown y Reporte de Errores (HU-4.1, HU-4.2 & HU-4.3)
* **Objetivo:** Verificar que el parser procesa archivos Markdown y reporta claramente errores y advertencias en el Inspector y consola.
* **Pasos:**
  1. Diseña un archivo `.md` de prueba malformado que contenga:
     - Una pregunta sin respuesta (`=`).
     - Una pregunta con dificultad no numérica o fuera de rango (ej. `[9]`).
     - Una línea no reconocida de texto plano.
  2. Carga este archivo en runtime usando el campo de ruta configurable del `ContentLoader` o a través del selector de archivos en el Editor.
  3. Revisa la Consola de Unity para el resumen del `ParseResult`.
* **Resultado Esperado:**
  - La consola debe imprimir un log de error detallando la línea exacta y la razón del fallo por cada pregunta descartada.
  - Las advertencias (warnings) deben indicar líneas con opciones duplicadas o líneas no reconocidas omitidas, pero permitir cargar el resto de preguntas válidas.
  - El sistema no debe cargar el mazo si el conteo de cartas utilizables es 0.

---

### 12. Barajado del Mazo y Fallback de Dificultad por Proximidad (HU-4.4 & HU-4.5)
* **Objetivo:** Verificar que el mazo de cartas se baraja y proporciona un fallback consistente si una dificultad específica se agota.
* **Pasos:**
  1. Genera un archivo `.md` con 2 cartas de nivel 1 y ninguna de nivel 2.
  2. Carga las preguntas y simula 3 extracciones sucesivas de nivel 2.
* **Resultado Esperado:**
  - Las primeras 2 extracciones de dificultad 2 deben retornar las cartas de nivel 1 (fallback por proximidad descendente N-1).
  - La consola debe loguear un Warning advirtiendo que el nivel 2 está agotado y se usó un fallback.
  - La tercera extracción (estando todo agotado) debe retornar la carta vacía de emergencia ("No hay más preguntas disponibles...").

---

### 13. Consumo de Pistas en Resolución de Preguntas (HU-4.6)
* **Objetivo:** Verificar que el uso de pistas reduce el multiplicador y altera los recursos del jugador activo.
* **Pasos:**
  1. Inicia una sesión donde el jugador activo posea 2 pistas y caiga en una carta con pista definida.
  2. Haz clic en "Usar Pista" durante el `ResolutionState`.
* **Resultado Esperado:**
  - El contador de pistas del jugador debe disminuir a 1.
  - Se debe disparar `GameEvents.OnHintRevealed` con el texto de la pista.
  - Si el jugador responde correctamente tras usar la pista, su avance debe ser multiplicado por x0.5 (multiplicador `WithHelp`), a menos que tenga activo el efecto de `IsOverdriveActive`.

---

### 14. Revelación de Opciones Múltiples (HU-4.7)
* **Objetivo:** Verificar que revelar las opciones disponibles en la UI penaliza el multiplicador del jugador.
* **Pasos:**
  1. Durante la pregunta en `ResolutionState`, haz clic en "Ver Opciones" para la carta actual.
  2. Responde la pregunta correctamente.
* **Resultado Esperado:**
  - Se debe gatillar `GameEvents.OnOptionsRevealed` enviando la lista de opciones.
  - El multiplicador final del turno debe computarse como `WithHelp` (x0.5), aplicando el redondeo a la baja o cercano en el movimiento final.

---

### 15. Evaluación Manual y Desafíos del Game Master (HU-4.8)
* **Objetivo:** Verificar que las preguntas abiertas detienen la evaluación automática y requieren la interacción manual del GM.
* **Pasos:**
  1. Extrae una carta abierta (dificultad 6 o sin opciones múltiples cuyo campo `=` empiece con `[Criterio del GM:`).
  2. Envía una respuesta por el jugador.
* **Resultado Esperado:**
  - La FSM de resolución debe quedar en pausa.
  - Se debe gatillar el evento `GameEvents.OnGmJudgmentRequired` enviando la respuesta y el criterio de evaluación.
  - Al pulsar ✅ Correcta o ❌ Incorrecta (gatillando `OnGmJudgmentSubmitted`), la FSM debe reanudarse y aplicar el multiplicador `Perfect` o `Fail` correspondiente.

---

### 16. Gestión de Inventario del Jugador (HU-5.1)
* **Objetivo:** Verificar que los ítems se añaden, eliminan y almacenan correctamente en el inventario del jugador, respetando la capacidad y emitiendo eventos.
* **Pasos:**
  1. Otorga un objeto al jugador usando un script de debug (ej. `player.Inventory.AddItem()`).
  2. Intenta añadir objetos más allá de la capacidad máxima (si está configurada).
  3. Ejecuta la eliminación de un ítem.
* **Resultado Esperado:**
  - `OnInventoryChanged` se dispara al añadir y remover, con la acción correspondiente (`Added` o `Removed`).
  - La UI del jugador (si está conectada a los eventos) refleja el ítem añadido/eliminado.
  - Al intentar sobrepasar la capacidad máxima, `AddItem` retorna false y emite un Warning en la Consola.

---

### 17. Configuración y Barajado del Mazo de Objetos (HU-5.2)
* **Objetivo:** Verificar que el mazo de objetos se instancia correctamente, reparte de forma aleatoria, y se re-baraja o agota según su configuración.
* **Pasos:**
  1. Configura un `ItemDeckConfig` con 3 objetos diferentes (con copias 2, 1, 1 respectivamente).
  2. Usa un script de pruebas para llamar a `ItemDeck.DrawItem()` 4 veces, luego descarta los objetos.
  3. Llama a `DrawItem()` nuevamente.
* **Resultado Esperado:**
  - El mazo debe contener exactamente 4 cartas inicialmente.
  - Las 4 primeras llamadas extraerán los objetos disponibles.
  - A la quinta llamada, si `ReshuffleOnEmpty` es true, el mazo debe re-barajar los Descartes y entregar un objeto. Si es false, debe retornar null y disparar `OnDeckEmpty`.
  - Se debe disparar `GameEvents.OnItemObtained` en cada extracción exitosa.

---

### 18. Restricciones de Activación del ItemEffectExecutor (HU-5.3 & HU-5.10)
* **Objetivo:** Verificar que el `ItemEffectExecutor` previene el uso de ítems en fases incorrectas del turno o si no se cumplen las condiciones.
* **Pasos:**
  1. Añade al jugador un "Overdrive" (fase `BeforeAnswer`) y un "Eco del Tiempo" (fase `AfterFail`).
  2. Intenta activar el Eco del Tiempo ANTES de lanzar el dado.
  3. Responde una pregunta e intenta activar el Overdrive.
* **Resultado Esperado:**
  - El primer intento debe fallar porque la fase actual no es `AfterFail`. Debe retornar false y emitir `OnItemBlocked`.
  - El segundo intento debe fallar porque el Overdrive solo sirve en la fase `BeforeAnswer` o si ya no hay pistas disponibles. Retorna false y emite `OnItemBlocked`.

---

### 19. Buffs: Overdrive y Eco del Tiempo (HU-5.4 & HU-5.5)
* **Objetivo:** Comprobar la lógica individual de los buff que alteran el multiplicador y la FSM.
* **Pasos:**
  1. **Overdrive:** Actívalo antes de usar una pista en una pregunta. Usa la pista y responde correctamente.
  2. **Eco del Tiempo:** Responde mal una pregunta. Activa el Eco. Intenta pedir una pista en el nuevo intento.
* **Resultado Esperado:**
  - **Overdrive:** El jugador avanza usando multiplicador de x1.0, ignorando la penalización de pistas de x0.5.
  - **Eco del Tiempo:** La FSM se interrumpe y retrocede a lanzar el dado. Durante la nueva pregunta, los botones de Pista y Revelar Opciones deben estar bloqueados.

---

### 20. Debuffs: Sabotaje y Robo de Pregunta (HU-5.6 & HU-5.7)
* **Objetivo:** Verificar las acciones ofensivas indirectas hacia otros jugadores.
* **Pasos:**
  1. **Sabotaje:** En el turno del Jugador 2, que el Jugador 1 active Sabotaje.
  2. **Robo:** En el turno del Jugador 2, espera a que responda incorrectamente. Activa Robo de Pregunta como Jugador 1.
* **Resultado Esperado:**
  - **Sabotaje:** Al mostrarse la pregunta del Jugador 2, sus opciones múltiples estarán invisibles (actuará como carta de nivel 6).
  - **Robo:** Tras el fallo del Jugador 2, se crea un "sub-turno" donde el Jugador 1 ve la misma pregunta. Si acierta, el Jugador 1 avanza los pasos correspondientes. Si falla, el Jugador 1 pierde 1 pista.

---

### 21. Efecto Especial: Duelo de Posiciones (HU-5.8)
* **Objetivo:** Comprobar la transición al estado `DuelState` y el intercambio de posiciones/robo de ítems.
* **Pasos:**
  1. Como Jugador 1, activa el Duelo de Posiciones al inicio de tu turno.
  2. Selecciona al Jugador 2 como objetivo.
  3. Ambos jugadores responden la misma pregunta. Simula que el Jugador 1 acierta y Jugador 2 falla.
  4. Repite el duelo, esta vez el Jugador 2 acierta y el Jugador 1 falla.
* **Resultado Esperado:**
  - El turno normal se suspende y entra al modo `DuelState`. Se pide una carta nueva al `ContentManager`.
  - **Caso Atacante gana:** Jugador 1 y 2 intercambian posiciones en el tablero.
  - **Caso Defensor gana:** Jugador 2 roba 1 pista o 1 objeto al Jugador 1. Si no tiene nada, el Jugador 1 se salta su próximo turno.

---

### 22. Ventana de Reacción y Parry (HU-5.9)
* **Objetivo:** Verificar la mecánica de Timer para las reacciones y la cancelación de efectos ofensivos.
* **Pasos:**
  1. Otorga al Jugador 2 un ítem ofensivo (ej. Sabotaje). Otorga al Jugador 1 un "Reflejo Perfecto" (Parry).
  2. El Jugador 2 activa el Sabotaje contra el Jugador 1.
  3. No hagas nada durante los segundos que dura el timer (ej. 5 seg).
  4. Repite el ataque, pero esta vez el Jugador 1 activa el Parry antes de que acabe el tiempo.
* **Resultado Esperado:**
  - En el primer caso (timeout), expira la ventana y el Sabotaje se aplica normalmente.
  - En el segundo caso (Parry), el Sabotaje se bloquea antes de aplicarse (`IsCountered = true`), no teniendo ningún efecto, y el turno actual del Jugador 2 (atacante) es cancelado de inmediato.

---

### 23. Desafío del Game Master y El Primero en Pulsar (HU-6.1 & HU-6.2)
* **Objetivo:** Verificar la transición al estado `GmChallengeState` y la resolución de concurrencia mediante `FirstToPressManager`.
* **Pasos:**
  1. Configura `GmChallengeProbability` a 1.0 (100%).
  2. Extrae una carta de Nivel 6.
  3. Espera a que se gatille el evento `OnGmChallengeStarted` y la UI del Desafío.
  4. Pulsa las teclas asignadas a Jugador 1 y Jugador 2 en diferentes tiempos. Luego pulsa simultáneamente (en el mismo frame).
* **Resultado Esperado:**
  - Si la carta es N6, se entra en `GmChallengeState`.
  - El primer jugador en pulsar es seleccionado y se dispara `OnGmChallengeContestantSelected`.
  - Si pulsan simultáneamente en el mismo frame (ej. Multi-Touch), el empate se resuelve al azar emitiendo un Warning en consola, y se escoge a un solo contestant.

---

### 24. Aprobación/Rechazo Manual y Recompensas Base (HU-6.3, HU-6.4 & HU-6.5)
* **Objetivo:** Verificar que el GM evalúa al contestant y el sistema castiga o recompensa en función del veredicto.
* **Pasos:**
  1. Inicia un Desafío GM, elige a un contestant.
  2. Simula que el GM rechaza (isCorrect = false).
  3. Repite el desafío y simula que el GM aprueba (isCorrect = true).
* **Resultado Esperado:**
  - Al rechazar, el contestant recibe el castigo (`SetSkipNextTurn(true)`) y se termina el desafío sin ganador (`OnGmChallengeEndedNoWinner`).
  - Al aprobar, el `GmRewardDistributor` le otorga una recompensa ponderada al azar (`GrantRandomReward`), lo registra en consola, y finaliza con ganador (`OnGmChallengeEnded`).

---

### 25. Efectos de Estado: Pista Dorada y Escudo de Inmunidad (HU-6.7 & HU-6.8)
* **Objetivo:** Comprobar el funcionamiento de los efectos de estado temporales.
* **Pasos:**
  1. **Pista Dorada:** Añade manualmente el `GoldenHintStatusEffect` a un jugador. En su turno, asegúrate que se enfrenta a una carta de Nivel 6 y pide una pista.
  2. **Escudo de Inmunidad:** Añade el `ImmunityShieldStatusEffect` al Jugador 1. Trata de que el Jugador 2 lance un Sabotaje (Ofensivo) contra el Jugador 1 en la misma ronda.
* **Resultado Esperado:**
  - **Pista Dorada:** El multiplicador NO se reduce por usar pista en esa carta N6, y el efecto se consume inmediatamente después del turno (`IsExpired = true`).
  - **Escudo de Inmunidad:** El `ItemEffectExecutor` detecta que el Jugador 1 tiene escudo y anula el Sabotaje del Jugador 2. El ítem de Sabotaje se consume sin efecto. Al cambiar de ronda superando la expiración, el escudo se remueve solo.

---

### 26. Recompensa: Manipulación del Tablero (HU-6.9)
* **Objetivo:** Verificar el flujo interactivo de intercambiar casillas.
* **Pasos:**
  1. Otorga al Jugador 1 la recompensa `BoardManipulationReward`.
  2. Al aplicarse la recompensa, observa la lista de `swappableTiles`.
  3. Dispara manualmente el evento `OnBoardManipulationCompleted` enviando dos casillas válidas.
* **Resultado Esperado:**
  - Las casillas Inicio y Meta nunca deben aparecer en `swappableTiles`.
  - Al completarse el evento, el `BoardManager` debe intercambiar ambas casillas, emitir `OnBoardModified` y reubicar las posiciones lógicas de los jugadores que estuvieran sobre ellas.
