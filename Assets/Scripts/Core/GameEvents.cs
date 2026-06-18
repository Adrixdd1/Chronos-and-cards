using System;
using ChronosAndCards.Data;
using ChronosAndCards.Interfaces;

namespace ChronosAndCards.Core
{
    /// <summary>
    /// Bus de eventos centralizado y estático que actúa como intermediario desacoplado
    /// entre el Core del juego, la UI y los diferentes subsistemas.
    /// </summary>
    public static class GameEvents
    {
        // === FSM ===
        /// <summary>Se dispara al cambiar de estado en la FSM.</summary>
        public static Action<IGameState> OnStateChanged;

        // === Turno ===
        /// <summary>Se dispara al iniciar el turno de un jugador.</summary>
        public static Action<IPlayer> OnTurnStarted;

        /// <summary>Se dispara al finalizar el turno de un jugador.</summary>
        public static Action<IPlayer> OnTurnEnded;

        // === Dado ===
        /// <summary>Se dispara al obtener el resultado del dado.</summary>
        public static Action<int> OnDiceRolled;

        // === Cartas ===
        /// <summary>Se dispara al extraer una carta del mazo.</summary>
        public static Action<CardData> OnCardDrawn;

        /// <summary>Se dispara al resolver la respuesta del jugador.</summary>
        public static Action<IPlayer, PerformanceMultiplier> OnQuestionResolved;

        // === Movimiento ===
        /// <summary>Se dispara al mover un jugador en el tablero. Parámetros: jugador, posición origen, posición destino.</summary>
        public static Action<IPlayer, int, int> OnPlayerMoved;

        /// <summary>Se dispara al aplicar el efecto de una casilla.</summary>
        public static Action<IPlayer, TileType> OnTileEffectApplied;

        // === Pistas ===
        /// <summary>Se dispara al modificar la cantidad de pistas de un jugador. Parámetros: jugador, nueva cantidad.</summary>
        public static Action<IPlayer, int> OnHintChanged;

        /// <summary>Se dispara al revelar el texto de una pista.</summary>
        public static Action<string> OnHintRevealed;

        // === Inventario ===
        /// <summary>Se dispara al modificar el inventario de un jugador. Parámetros: jugador, item, tipo de acción.</summary>
        public static Action<IPlayer, IItem, InventoryAction> OnInventoryChanged;

        /// <summary>Se dispara al activar un objeto consumible.</summary>
        public static Action<IPlayer, IItemEffect> OnItemActivated;

        /// <summary>Se dispara cuando un objeto es bloqueado (ej. Parry, Inmunidad).</summary>
        public static Action<IPlayer, IItemEffect> OnItemBlocked;

        // === Desafío del GM ===
        /// <summary>Se dispara al iniciar el desafío del GM.</summary>
        public static Action<CardData> OnGmChallengeStarted;

        /// <summary>Se dispara al finalizar el desafío del GM. Parámetro: ganador (null si nadie ganó).</summary>
        public static Action<IPlayer> OnGmChallengeEnded;

        // === Duelo ===
        /// <summary>Se dispara al iniciar un duelo entre jugadores. Parámetros: atacante, defensor.</summary>
        public static Action<IPlayer, IPlayer> OnDuelStarted;

        /// <summary>Se dispara al finalizar un duelo. Parámetro: ganador.</summary>
        public static Action<IPlayer> OnDuelEnded;

        // === Partida ===
        /// <summary>Se dispara cuando la partida ha sido configurada y está lista para comenzar.</summary>
        public static Action OnGameStarted;

        /// <summary>Se dispara al terminar la partida y determinar el ganador. Parámetro: ganador.</summary>
        public static Action<IPlayer> OnGameOver;

        /// <summary>
        /// Limpia todos los suscriptores suscritos a los eventos para evitar fugas de memoria (memory leaks)
        /// al cambiar de escena o reiniciar la sesión de juego.
        /// </summary>
        public static void ClearAll()
        {
            OnStateChanged = null;
            OnTurnStarted = null;
            OnTurnEnded = null;
            OnDiceRolled = null;
            OnCardDrawn = null;
            OnQuestionResolved = null;
            OnPlayerMoved = null;
            OnTileEffectApplied = null;
            OnHintChanged = null;
            OnHintRevealed = null;
            OnInventoryChanged = null;
            OnItemActivated = null;
            OnItemBlocked = null;
            OnGmChallengeStarted = null;
            OnGmChallengeEnded = null;
            OnDuelStarted = null;
            OnDuelEnded = null;
            OnGameStarted = null;
            OnGameOver = null;
        }
    }
}
