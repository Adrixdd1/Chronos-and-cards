# Architecture Design Document (ADD) - Chronos & Cards

## 1. Filosofía Arquitectónica (Antigravity & Clean Code)
Este proyecto se rige por principios de ingeniería de software estructurada. 
* **Desacoplamiento Estricto:** La lógica de negocio (reglas, turnos, validación de dados) no debe conocer detalles de la capa de presentación (animaciones, UI, partículas).
* **Single Responsibility Principle (SRP):** Cada `MonoBehaviour` o clase en C# debe tener una única razón para cambiar.
* **Composición sobre Herencia:** Fomentar el uso de interfaces pequeñas y componentes modulares en lugar de crear grandes árboles de herencia de clases abstractas.

## 2. Estructura de Directorios (Assets)
El agente debe mantener una jerarquía limpia y predecible en el directorio `Assets/`:
* `Assets/Scripts/Core`: Lógica principal del juego (GameManager, TurnStateMachine).
* `Assets/Scripts/Data`: ScriptableObjects, Parsers de Markdown, Modelos de datos estáticos.
* `Assets/Scripts/Gameplay`: Comportamientos interactivos (DiceRoller, PlayerMovement, ItemCaster).
* `Assets/Scripts/UI`: Controladores de interfaz que escuchan eventos del Core.
* `Assets/Scripts/Interfaces`: Contratos abstractos puros (ej. `IItem`, `IDamageable`).
* `Assets/Prefabs/`: Objetos preconfigurados listos para instanciar (Dado, Fichas, Nodos del Tablero).
* `Assets/Art/`: Modelos 3D, Texturas, Materiales y Shaders.

## 3. Comunicación entre Componentes
Para evitar el acoplamiento y la necesidad de frameworks externos de inyección de dependencias, se establecen las siguientes reglas de comunicación:

* **Inyección de Referencias (Inspector):** Todas las dependencias estáticas en la escena deben inyectarse a través de campos serializados (`[SerializeField] private Type _variableName;`). 
* **Prohibición Absoluta:** Queda estrictamente prohibido el uso de `GameObject.Find()`, `FindObjectOfType()`, o el patrón Singleton (a menos que esté expresamente justificado y aislado en un gestor de estado global inmutable).
* **Patrón Observer (Eventos):** La comunicación "Hacia arriba" (desde un script hijo hacia un gestor general) o entre dominios diferentes (Core hacia UI) debe hacerse exclusivamente mediante eventos. Utilizar `System.Action` para eventos internos rápidos, o `UnityEvent` si se requiere configuración en el Inspector.

## 4. Convenciones de Código (C#)
* **Interfaces:** Empiezan siempre con la letra `I` mayúscula (ej. `IDiceValidator`).
* **Campos Privados:** Deben usar el prefijo `_` y formato camelCase (ej. `[SerializeField] private Transform _playerTransform;`).
* **Propiedades Públicas:** Formato PascalCase (ej. `public int CurrentHealth { get; private set; }`).
* **Métodos:** Formato PascalCase y deben representar acciones claras (ej. `RollDice()`, `ParseMarkdown()`).

## 5. Gestión del Estado (Game Loop)
El flujo del juego se controla mediante una **Máquina de Estados Finitos (FSM)**.
* Existirá un `GameManager` que poseerá una referencia a un `BaseState` actual.
* Los estados (ej. `SetupState`, `PlayerTurnState`, `ResolutionState`, `GmChallengeState`) deben estar encapsulados en clases propias que implementen una interfaz común como `IGameState` (con métodos `Enter()`, `Tick()`, `Exit()`).
* Las transiciones de estado ocurren cuando un estado actual emite un evento de finalización o cuando el `GameManager` reacciona a una condición global.