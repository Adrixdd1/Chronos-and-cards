using UnityEngine;
using ChronosAndCards.Data;
using ChronosAndCards.Gameplay.Board;

namespace ChronosAndCards.Core.States
{
    /// <summary>
    /// Estado que calcula y ejecuta el movimiento de la ficha del jugador en el tablero
    /// en función de su desempeño.
    /// </summary>
    public class MovementState : IGameState
    {
        private readonly GameManager _gameManager;

        /// <summary>Constructor que inyecta el GameManager.</summary>
        public MovementState(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public void Enter()
        {
            Debug.Log("MovementState: Enter");

            IPlayer player = _gameManager.GameContext.CurrentPlayer;
            int diceValue = _gameManager.GameContext.CurrentDiceValue;
            PerformanceMultiplier multiplier = _gameManager.GameContext.LastResult;

            // Calcular avance según la fórmula: (dado * multiplicador) / 100
            int tilesToMove = (diceValue * (int)multiplier) / 100;
            Debug.Log($"MovementState: {player.PlayerName} avanza {tilesToMove} casillas (Dado: {diceValue}, Desempeño: {multiplier}).");

            int fromPosition = player.Position;

            // Mover en el BoardManager
            if (_gameManager.BoardManager != null)
            {
                _gameManager.BoardManager.MovePlayer(player, tilesToMove);
            }
            else
            {
                // Si no hay BoardManager, mover manualmente
                player.MoveForward(tilesToMove);
            }

            int toPosition = player.Position;

            // Emitir evento
            GameEvents.OnPlayerMoved?.Invoke(player, fromPosition, toPosition);
        }

        public void Tick()
        {
            // Esperar si hay una bifurcación pendiente de decisión
            if (_gameManager.BoardManager == null || !_gameManager.BoardManager.IsMovementPending)
            {
                _gameManager.TransitionTo(new TileEffectState(_gameManager));
            }
        }

        public void Exit()
        {
            Debug.Log("MovementState: Exit");
        }
    }
}
