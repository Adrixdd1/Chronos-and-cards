using System;
using System.Collections.Generic;
using ChronosAndCards.Data;
using ChronosAndCards.Interfaces;
using ChronosAndCards.Gameplay.Items;
using ChronosAndCards.Gameplay.GmChallenge;
using ChronosAndCards.Core.States;

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

        /// <summary>Se dispara cuando el jugador envía su respuesta desde la UI.</summary>
        public static Action<string> OnAnswerSubmitted;

        // === Contenido ===
        /// <summary>Se dispara cuando el contenido ha sido cargado y está listo.</summary>
        public static Action<int> OnContentLoaded; // totalCards

        /// <summary>Se dispara cuando un nivel de dificultad se queda sin cartas.</summary>
        public static Action<int> OnDeckLevelEmpty; // level

        // === Opciones ===
        /// <summary>Se dispara cuando el jugador revela las opciones múltiples.</summary>
        public static Action<List<string>> OnOptionsRevealed;

        // === Evaluación del GM ===
        /// <summary>Se dispara cuando se requiere la evaluación manual del GM.</summary>
        public static Action<string, string> OnGmJudgmentRequired; // playerAnswer, criteria

        /// <summary>Se dispara cuando el GM envía su veredicto.</summary>
        public static Action<bool> OnGmJudgmentSubmitted; // isCorrect

        // === Movimiento ===
        /// <summary>Se dispara al mover un jugador en el tablero. Parámetros: jugador, posición origen, posición destino.</summary>
        public static Action<IPlayer, int, int> OnPlayerMoved;

        /// <summary>Se dispara al aplicar el efecto de una casilla.</summary>
        public static Action<IPlayer, TileType> OnTileEffectApplied;

        // === Tablero ===
        /// <summary>Se dispara al requerir que un jugador elija ruta en una bifurcación. Parámetros: jugador, opciones disponibles.</summary>
        public static Action<IPlayer, IReadOnlyList<ITile>> OnPathChoiceRequired;

        /// <summary>Se dispara al seleccionar el jugador una de las casillas en la bifurcación. Parámetros: jugador, casilla seleccionada.</summary>
        public static Action<IPlayer, ITile> OnPathChoiceSelected;

        /// <summary>Se dispara al modificar/intercambiar dos casillas en el tablero. Parámetros: casilla A, casilla B.</summary>
        public static Action<ITile, ITile> OnBoardModified;

        /// <summary>Se dispara cuando una casilla es descubierta progresivamente.</summary>
        public static Action<ITile> OnTileDiscovered;

        // === Pistas ===
        /// <summary>Se dispara al modificar la cantidad de pistas de un jugador. Parámetros: jugador, nueva cantidad.</summary>
        public static Action<IPlayer, int> OnHintChanged;

        /// <summary>Se dispara al revelar el texto de una pista.</summary>
        public static Action<string> OnHintRevealed;

        // === Inventario ===
        /// <summary>Se dispara al modificar el inventario de un jugador. Parámetros: jugador, item, tipo de acción.</summary>
        public static Action<IPlayer, IItem, InventoryAction> OnInventoryChanged;

        /// <summary>Se dispara cuando un jugador obtiene un ítem del mazo.</summary>
        public static Action<IPlayer, IItem> OnItemObtained;

        /// <summary>Se dispara cuando un jugador usa un ítem exitosamente.</summary>
        public static Action<IPlayer, IItem> OnItemUsed;

        /// <summary>Se dispara cuando un ítem es bloqueado (no se puede usar).</summary>
        public static Action<IPlayer, IItem, string> OnItemBlocked; // reason

        // === Buffs / Debuffs ===
        /// <summary>Se dispara cuando un buff se aplica al jugador activo.</summary>
        public static Action<IPlayer, string> OnBuffApplied; // buffName

        /// <summary>Se dispara cuando un debuff se aplica a un rival.</summary>
        public static Action<IPlayer, IPlayer, string> OnDebuffApplied; // attacker, target, debuffName

        // === Desafío del GM ===
        /// <summary>Se dispara al iniciar el desafío del GM con la carta del reto.</summary>
        public static Action<CardData> OnGmChallengeStarted;

        /// <summary>Se dispara cuando se salta el desafío (sin retos disponibles).</summary>
        public static Action OnGmChallengeSkipped;

        /// <summary>Se dispara cuando un jugador es seleccionado como contestant.</summary>
        public static Action<IPlayer, CardData> OnGmChallengeContestantSelected;

        /// <summary>Se dispara cuando un contestant falla y es bloqueado.</summary>
        public static Action<IPlayer> OnGmChallengeContestantFailed;

        /// <summary>Se dispara al finalizar con ganador.</summary>
        public static Action<IPlayer, GmRewardType> OnGmChallengeEnded;

        /// <summary>Se dispara al finalizar sin ganador.</summary>
        public static Action OnGmChallengeEndedNoWinner;

        // === Recompensas del GM ===
        /// <summary>Se dispara cuando una recompensa es otorgada.</summary>
        public static Action<IPlayer, GmRewardType> OnGmRewardGranted;

        // === Manipulación del Tablero ===
        /// <summary>Se dispara al iniciar la selección de casillas.</summary>
        public static Action<IPlayer, List<ITile>> OnBoardManipulationStarted;

        /// <summary>Se dispara cuando se completa la selección de casillas.</summary>
        public static Action<ITile, ITile> OnBoardManipulationCompleted;

        // El evento OnBoardModified ya existe, se encuentra en la sección 'Tablero'.

        // === Efectos de Estado ===
        /// <summary>Se dispara cuando un efecto de estado se aplica a un jugador.</summary>
        public static Action<IPlayer, IStatusEffect> OnStatusEffectApplied;

        /// <summary>Se dispara cuando un efecto de estado expira.</summary>
        public static Action<IPlayer, IStatusEffect> OnStatusEffectExpired;

        // === Duelos ===
        /// <summary>Se dispara cuando se inicia un duelo de posiciones.</summary>
        public static Action<IPlayer> OnDuelInitiated;

        /// <summary>Se dispara cuando la UI debe mostrar selección de rival.</summary>
        public static Action<List<IPlayer>> OnRivalSelectionRequired;

        /// <summary>Se dispara cuando la UI confirma el rival seleccionado.</summary>
        public static Action<IPlayer> OnRivalSelected;

        /// <summary>Se dispara cuando un jugador envía su respuesta durante un duelo.</summary>
        public static Action<IPlayer, string> OnDuelAnswerSubmitted;

        /// <summary>Se dispara cuando la pregunta del duelo se presenta a ambos.</summary>
        public static Action<IPlayer, IPlayer, CardData> OnDuelQuestionPresented;

        /// <summary>Se dispara cuando el duelo se resuelve.</summary>
        public static Action<IPlayer, IPlayer, DuelResult> OnDuelResolved;

        // === Counters / Reacción ===
        /// <summary>Se dispara cuando un counter anula un efecto ofensivo.</summary>
        public static Action<IPlayer, IPlayer, string> OnCounterActivated; // defender, attacker, counterName

        /// <summary>Se dispara cuando se abre una ventana de reacción.</summary>
        public static Action<PendingOffensiveEffect, float> OnReactionWindowOpened;

        /// <summary>Se dispara cuando la ventana de reacción expira.</summary>
        public static Action<PendingOffensiveEffect> OnReactionWindowExpired;

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
            OnPathChoiceRequired = null;
            OnPathChoiceSelected = null;
            OnBoardModified = null;
            OnTileDiscovered = null;
            OnHintChanged = null;
            OnHintRevealed = null;
            OnInventoryChanged = null;
            OnItemObtained = null;
            OnItemUsed = null;
            OnItemBlocked = null;
            OnBuffApplied = null;
            OnDebuffApplied = null;
            OnGmChallengeStarted = null;
            OnGmChallengeSkipped = null;
            OnGmChallengeContestantSelected = null;
            OnGmChallengeContestantFailed = null;
            OnGmChallengeEnded = null;
            OnGmChallengeEndedNoWinner = null;
            OnGmRewardGranted = null;
            OnBoardManipulationStarted = null;
            OnBoardManipulationCompleted = null;
            OnStatusEffectApplied = null;
            OnStatusEffectExpired = null;
            OnDuelInitiated = null;
            OnRivalSelectionRequired = null;
            OnRivalSelected = null;
            OnDuelAnswerSubmitted = null;
            OnDuelQuestionPresented = null;
            OnDuelResolved = null;
            OnCounterActivated = null;
            OnReactionWindowOpened = null;
            OnReactionWindowExpired = null;
            OnGameStarted = null;
            OnGameOver = null;
            OnAnswerSubmitted = null;
            OnContentLoaded = null;
            OnDeckLevelEmpty = null;
            OnOptionsRevealed = null;
            OnGmJudgmentRequired = null;
            OnGmJudgmentSubmitted = null;
        }
    }
}
