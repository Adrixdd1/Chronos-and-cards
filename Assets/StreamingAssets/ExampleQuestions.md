# [1] ¿Cuál es el valor por defecto de un tipo de dato booleano en C#?
- true
* false
= false

# [1] ¿Es C# un lenguaje compilado o puramente interpretado?
- Interpretado
* Compilado
= Compilado

# [1] ¿Cuál de los siguientes es un valor correcto para declarar un entero en C#?
- "12"
* 12
= 12

# [2] ¿Qué palabra clave se utiliza para heredar de una clase base en C#?
- extends
- inherits
* :
= :

# [2] ¿Qué tipo de datos se utiliza para almacenar un solo carácter en C#?
- string
* char
- byte
= char

# [2] ¿Qué método se ejecuta al instanciar un objeto de una clase?
- Main
* Constructor
- Destructor
= Constructor

# [3] ¿Qué palabra clave impide que una clase sea heredada en C#?
> Piensa en una clase que está "sellada".
- static
- abstract
* sealed
- readonly
= sealed

# [3] ¿Cuál es la complejidad temporal promedio de una búsqueda en un Dictionary en C#?
> Es una operación de tiempo constante debido a las tablas hash.
- O(N)
- O(log N)
* O(1)
- O(N log N)
= O(1)

# [3] ¿Qué tipo de dato de punto flotante tiene la mayor precisión en C#?
> Se utiliza principalmente en aplicaciones financieras debido a su alta precisión de 128 bits.
- float
- double
* decimal
- real
= decimal

# [4] ¿Qué patrón de diseño restringe la instanciación de una clase a un único objeto?
> Su nombre en inglés significa "único" o "soltero".
- Factory
* Singleton
- Observer
- Decorator
= Singleton

# [4] ¿Qué método se utiliza para forzar la recolección de basura en C#?
> La clase estática System.GC controla esto.
- GC.CollectAll()
* GC.Collect()
- GC.Free()
- GC.Dispose()
= GC.Collect()

# [4] ¿Qué significa la 'I' en los principios SOLID de diseño de software?
> Se refiere a no forzar a los clientes a depender de métodos que no usan.
- Inversión de Control
* Segregación de Interfaces
- Inyección de Dependencia
- Instanciación Única
= Segregación de Interfaces

# [5] ¿Qué palabra clave se usa para crear una corrutina en Unity?
> Comienza con "Start".
- Invoke
* StartCoroutine
- RunAsync
= StartCoroutine

# [5] ¿Cuál es el ciclo de vida del frame en Unity donde se deben aplicar las fuerzas físicas del Rigidbody?
> Ocurre antes del Update estándar y es de intervalo fijo.
- Update
- LateUpdate
* FixedUpdate
= FixedUpdate

# [5] ¿Qué componente se requiere para que un GameObject de Unity responda a fuerzas de gravedad?
> Añade físicas básicas de masa y colisión.
- BoxCollider
* Rigidbody
- Transform
= Rigidbody

# [6] Describe qué es y para qué sirve el principio de Responsabilidad Única (SRP) en SOLID.
= [Criterio del GM: El jugador debe explicar que una clase o componente debe tener una única razón para cambiar o realizar una única función principal.]

# [6] Explica qué es el acoplamiento y por qué se prefiere un bajo acoplamiento en ingeniería de software.
= [Criterio del GM: El jugador debe explicar que el acoplamiento mide la dependencia entre módulos y que un bajo acoplamiento facilita el mantenimiento y escalabilidad.]

# [6] ¿Qué es una fuga de memoria (memory leak) y cómo se puede prevenir en C# / Unity?
= [Criterio del GM: El jugador debe mencionar la acumulación de referencias no liberadas (ej. eventos sin desuscribir) y prevención mediante el desregistro de delegados en OnDestroy/Exit.]

## [GM] ¿Cuál es el resultado de la siguiente expresión: 10 + 5 * 2?
> Respeta la precedencia de operadores matemáticos.
- 30
* 20
= 20

## [GM] ¿Qué patrón de diseño define una dependencia de uno a muchos entre objetos para que cuando uno cambie de estado se notifique a todos los demás?
> El bus central de GameEvents usa este concepto.
- Singleton
* Observer
- Command
= Observer
