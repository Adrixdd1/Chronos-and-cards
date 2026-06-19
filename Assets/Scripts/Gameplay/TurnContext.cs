using ChronosAndCards.Core;
using ChronosAndCards.Data;
using ChronosAndCards.Interfaces;

namespace ChronosAndCards.Gameplay
{
    /// <summary>
    /// Contenedor de datos mutables del turno actual. Se reinicia al inicio de cada turno.
    /// </summary>
    public class TurnContext
    {
        // --- Jugador ---
        /// <summary>Jugador activo en este turno.</summary>
        public IPlayer ActivePlayer { get; set; }

        // --- Dado ---
        /// <summary>Resultado del dado en este turno (1–6).</summary>
        public int DiceValue { get; set; }

        // --- Carta ---
        /// <summary>Nivel de dificultad determinado por el DifficultyMapper.</summary>
        public int DifficultyLevel { get; set; }

        /// <summary>Carta extraída para este turno.</summary>
        public CardData? CurrentCard { get; set; }

        // --- Resolución ---
        /// <summary>Respuesta del jugador (texto libre o índice de opción).</summary>
        public string PlayerAnswer { get; set; }

        /// <summary>Indica si el jugador usó una pista en este turno.</summary>
        public bool UsedHint { get; set; }

        /// <summary>Indica si el jugador reveló las opciones múltiples.</summary>
        public bool RevealedOptions { get; set; }

        /// <summary>Resultado de la evaluación de la respuesta.</summary>
        public PerformanceMultiplier PerformanceResult { get; set; }

        // --- Movimiento ---
        /// <summary>Casillas a avanzar calculadas por AdvanceCalculator.</summary>
        public int TilesToMove { get; set; }

        /// <summary>Posición de origen del jugador antes de moverse.</summary>
        public int OriginPosition { get; set; }

        /// <summary>Posición de destino tras el movimiento.</summary>
        public int DestinationPosition { get; set; }

        // --- Modificadores ---
        /// <summary>Indica si un efecto de Overdrive está activo (pista sin penalización).</summary>
        public bool IsOverdriveActive { get; set; }

        /// <summary>Indica si un efecto de Sabotaje fue aplicado al jugador (ocultar opciones).</summary>
        public bool IsSabotaged { get; set; }

        /// <summary>Indica si el jugador usó Eco del Tiempo en este turno.</summary>
        public bool UsedTimeEcho { get; set; }

        /// <summary>Bloquea el uso de pistas (ej. durante Eco del Tiempo).</summary>
        public bool HintsBlocked { get; set; }

        /// <summary>Bloquea la revelación de opciones (ej. durante Eco del Tiempo).</summary>
        public bool OptionsBlocked { get; set; }

        /// <summary>Referencia al jugador que saboteó este turno (para Parry).</summary>
        public IPlayer SabotagedBy { get; set; }

        /// <summary>Indica si el jugador falló la pregunta de este turno.</summary>
        public bool FailedThisTurn { get; set; }

        /// <summary>Indica si este turno fue cancelado (ej. por un Parry).</summary>
        public bool TurnCancelled { get; set; }

        /// <summary>Reinicia todos los campos para un nuevo turno.</summary>
        public void Reset()
        {
            ActivePlayer = null;
            DiceValue = 0;
            DifficultyLevel = 0;
            CurrentCard = null;
            PlayerAnswer = null;
            UsedHint = false;
            RevealedOptions = false;
            PerformanceResult = PerformanceMultiplier.Fail;
            TilesToMove = 0;
            OriginPosition = 0;
            DestinationPosition = 0;
            IsOverdriveActive = false;
            IsSabotaged = false;
            UsedTimeEcho = false;
            HintsBlocked = false;
            OptionsBlocked = false;
            SabotagedBy = null;
            FailedThisTurn = false;
            TurnCancelled = false;
        }
    }
}
