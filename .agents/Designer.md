# Art Design Document (ADD) - Chronos & Cards

## 1. Visión Estética y Dirección de Arte (Core Vibe)
**Tema Principal:** "Cozy Nature Tabletop" (Juego de mesa en la naturaleza).
**Sensación:** Familiar, relajante, clásico y tangible. 
**Regla de Oro:** **CERO ciencia ficción, CERO alta fantasía.** Evitar luces de neón, hologramas, magia brillante o metales industriales. Todo debe sentirse como objetos reales que podrías llevar en una mochila a un día de campo.

# Art Design Document (ADD) - Chronos & Cards (v1.1)

## 2. Entorno y Cámara (Micro-Mundo & Drone View)
* **Perspectiva del Jugador:** La cámara debe simular la visión de un pequeño dron volando a ras del suelo, unos centímetros por encima de las texturas físicas. 
* **Efecto Tilt-Shift (Macro):** Se requiere un uso agresivo del `Depth of Field` (Profundidad de Campo) en el Post-Processing de Unity. Solo la casilla actual y el dado deben estar en foco perfecto; el fondo y el primer plano extremo deben estar fuertemente desenfocados para dar la ilusión de miniaturización.
* **El Escenario "Vivo":** No hay un "tablero" como tal. Las casillas están dispersas orgánicamente sobre una superficie real. Una tarde cálida, iluminación de "Golden Hour" con destellos de luz solar filtrándose entre hojas grandes y cálidas, simulando la flora de un parque tropical o un jardín en una tarde soleada. 
* **Integración Orgánica:** Las briznas de hierba alta pueden ocultar parcialmente algunas rutas. La manta de picnic tiene pliegues y arrugas reales que sirven como colinas o valles en el mapa.

## 3. Materiales y Texturas (Diegetic Design)
* **Las Casillas:** Son objetos cotidianos perdidos o colocados en el pasto/manta. Chapas de botellas oxidadas, botones de madera de abrigos, hojas secas de diferentes colores, o monedas de cobre. 
* **El Dado (D6):** Un dado de madera pulida clásico. Cuando rueda, interactúa con la física del entorno (hace crujir una hoja seca o hunde ligeramente la tela de la manta).

## 4. Arquitectura de Cámara (Directriz para el Agente)
* **Cinemachine State-Driven Camera:** Implementar un sistema de cámara desacoplado. Utilizar `CinemachineVirtualCamera` para hacer transiciones suaves.
* **Controlador Antigravedad:** El script del "Dron/Cámara" (`DroneCameraController.cs`) no debe estar acoplado a la lógica del turno ni al movimiento de la ficha. Debe utilizar el Patrón Observer para escuchar el evento `OnPlayerMoved(Vector3 newPosition)` y flotar suavemente hacia esa coordenada utilizando interpolación (`Vector3.Lerp` o amortiguación de Cinemachine).
* **Colisiones Dinámicas:** El dron debe tener un *collider* suave para no atravesar briznas de hierba gigantes o rocas, sino esquivarlas fluidamente al moverse.

## 5. Prompts Base para Generación de Assets (Actualizado)
* **Entorno Macro:** `Macro photography of a tabletop game path made of scattered vintage wooden buttons and bottle caps resting deep in vibrant green grass. Sunbeams filtering through large tropical leaves. Tilt-shift effect, extremely shallow depth of field, miniature world, cozy warm golden hour lighting, Unreal Engine 5 render --ar 16:9`
* **El Dado en el Entorno:** `A small polished oak wood 6-sided die resting on a red and white checkered picnic blanket. The fabric has deep wrinkles acting like hills. A giant blade of grass is visible in the blurry foreground. Tilt-shift lens, highly realistic, cozy sunny afternoon --ar 1:1`