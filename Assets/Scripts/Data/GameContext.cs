using ChronosAndCards.Core;

namespace ChronosAndCards.Data
{
    /// <summary>
    /// Encapsula el estado global de la sesión durante el turno actual para
    /// que los sistemas de ítems y casillas evalúen decisiones lógicas.
    /// </summary>
    public class GameContext
    {
        /// <summary>Jugador activo en el turno actual.</summary>
        public IPlayer CurrentPlayer { get; set; }

        /// <summary>Jugador objetivo para interacciones (puede ser null si no hay objetivo).</summary>
        public IPlayer TargetPlayer { get; set; }

        /// <summary>Valor obtenido en el último lanzamiento de dado en el turno.</summary>
        public int CurrentDiceValue { get; set; }

        /// <summary>Carta de pregunta que está siendo resuelta en el turno (null si no hay una activa).</summary>
        public CardData? CurrentCard { get; set; }

        /// <summary>Resultado de desempeño obtenido tras evaluar la respuesta.</summary>
        public PerformanceMultiplier LastResult { get; set; }

        /// <summary>Indica si actualmente se está ejecutando la fase de acción del jugador activo.</summary>
        public bool IsPlayerTurn { get; set; }

        /// <summary>Fase actual del ciclo de activación de ítems en la que se encuentra el juego.</summary>
        public ItemActivationPhase CurrentPhase { get; set; }
    }
}
