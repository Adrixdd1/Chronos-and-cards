using System;
using System.Collections.Generic;
using ChronosAndCards.Core;
using ChronosAndCards.Gameplay;
using ChronosAndCards.Gameplay.Items;

namespace ChronosAndCards.Data
{
    /// <summary>
    /// Encapsula el estado global de la sesión durante el turno actual para
    /// que los sistemas de ítems y casillas evalúen decisiones lógicas.
    /// </summary>
    public class GameContext
    {
        /// <summary>Jugador activo en el turno actual.</summary>
        public IPlayer ActivePlayer { get; set; }

        /// <summary>Alias por compatibilidad.</summary>
        public IPlayer CurrentPlayer { get => ActivePlayer; set => ActivePlayer = value; }

        /// <summary>Contexto de datos del turno actual.</summary>
        public TurnContext CurrentTurnContext { get; set; }

        /// <summary>Lista de jugadores (para Duelos y otros efectos globales).</summary>
        public List<IPlayer> Players { get; set; }

        /// <summary>Efecto ofensivo pendiente durante la ventana de reacción.</summary>
        public PendingOffensiveEffect PendingOffensiveEffect { get; set; }

        /// <summary>Delegado para solicitar una transición de estado a la FSM central.</summary>
        public Action<GameStateType> RequestStateTransition { get; set; }

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

        // --- Soporte de Sub-Turnos (Duelos / Robos) ---
        private readonly Stack<object> _subTurnStack = new();

        /// <summary>Empuja un contexto temporal (ej. DuelState).</summary>
        public void PushSubTurn(object subTurnContext)
        {
            _subTurnStack.Push(subTurnContext);
        }

        /// <summary>Saca el contexto temporal actual.</summary>
        public void PopSubTurn()
        {
            if (_subTurnStack.Count > 0)
                _subTurnStack.Pop();
        }

        /// <summary>Obtiene el subturno activo actual de tipo T.</summary>
        public T GetSubTurnContext<T>() where T : class
        {
            if (_subTurnStack.Count > 0)
                return _subTurnStack.Peek() as T;
            return null;
        }
    }
}
