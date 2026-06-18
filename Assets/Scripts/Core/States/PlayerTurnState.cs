using UnityEngine;
using ChronosAndCards.Data;

namespace ChronosAndCards.Core.States
{
    /// <summary>
    /// Estado que inicia el turno del jugador activo. Evalúa penalizaciones y
    /// permite usar objetos que reemplazan el turno (ej. activar Duelo).
    /// </summary>
    public class PlayerTurnState : IGameState
    {
        private readonly GameManager _gameManager;
        private bool _isSkipped;
        private bool _transitionStarted;

        /// <summary>Constructor que inyecta el GameManager.</summary>
        public PlayerTurnState(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public void Enter()
        {
            Debug.Log("PlayerTurnState: Enter");

            if (_gameManager.Players == null || _gameManager.Players.Count == 0)
            {
                Debug.LogError("PlayerTurnState: No hay jugadores en el GameManager.");
                return;
            }

            // Determinar jugador activo usando round-robin
            IPlayer activePlayer = _gameManager.Players[_gameManager.CurrentPlayerIndex];

            // Comprobar penalización de perder turno
            if (activePlayer.IsSkipNextTurn)
            {
                Debug.Log($"PlayerTurnState: El jugador {activePlayer.PlayerName} pierde su turno.");
                activePlayer.SetSkipNextTurn(false);
                _isSkipped = true;
                return;
            }

            // Actualizar contexto
            _gameManager.GameContext.CurrentPlayer = activePlayer;
            _gameManager.GameContext.TargetPlayer = null;
            _gameManager.GameContext.CurrentCard = null;
            _gameManager.GameContext.IsPlayerTurn = true;
            _gameManager.GameContext.CurrentPhase = ItemActivationPhase.ReplaceTurn;

            // Emitir evento de inicio de turno
            GameEvents.OnTurnStarted?.Invoke(activePlayer);
        }

        public void Tick()
        {
            if (_transitionStarted) return;

            if (_isSkipped)
            {
                _transitionStarted = true;
                // Transiciona a NextPlayerState para evaluar condiciones de victoria y pasar al siguiente
                _gameManager.TransitionTo(new NextPlayerState(_gameManager));
                return;
            }

            // Ventana para usar ítems de fase ReplaceTurn (ej. Duelo)
            // Para la Épica 1, transicionamos directamente al estado del lanzamiento del dado (DiceRollState)
            _transitionStarted = true;
            _gameManager.TransitionTo(new DiceRollState(_gameManager));
        }

        public void Exit()
        {
            Debug.Log("PlayerTurnState: Exit");
        }
    }
}
