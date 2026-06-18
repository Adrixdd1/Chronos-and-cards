using UnityEngine;
using ChronosAndCards.Data;
using ChronosAndCards.Gameplay.Board;
using ChronosAndCards.Gameplay;

namespace ChronosAndCards.Core.States
{
    /// <summary>
    /// Estado que calcula y ejecuta el movimiento de la ficha del jugador en el tablero
    /// en función de su desempeño.
    /// </summary>
    public class MovementState : IGameState
    {
        private readonly GameManager _gameManager;
        private readonly AdvanceCalculator _advanceCalculator;
        private readonly BoardManager _boardManager;

        /// <summary>Constructor que inyecta GameManager, AdvanceCalculator y BoardManager.</summary>
        public MovementState(GameManager gameManager, AdvanceCalculator advanceCalculator, BoardManager boardManager)
        {
            _gameManager = gameManager;
            _advanceCalculator = advanceCalculator;
            _boardManager = boardManager;
        }

        public void Enter()
        {
            Debug.Log("MovementState: Enter");

            TurnContext turnContext = _gameManager.TurnContext;
            IPlayer player = turnContext.ActivePlayer;

            // En caso de fallbacks/safety checks
            if (player == null)
            {
                player = _gameManager.GameContext.CurrentPlayer;
                turnContext.ActivePlayer = player;
            }

            int diceValue = turnContext.DiceValue;
            PerformanceMultiplier performance = turnContext.PerformanceResult;

            // Registrar posición de origen
            turnContext.OriginPosition = player.Position;

            // Calcular avance
            int tilesToMove = 0;
            if (_advanceCalculator != null)
            {
                tilesToMove = _advanceCalculator.Calculate(diceValue, performance);
            }
            else
            {
                tilesToMove = (diceValue * (int)performance) / 100;
            }

            turnContext.TilesToMove = tilesToMove;
            Debug.Log($"MovementState: {player.PlayerName} avanza {tilesToMove} casillas (Dado: {diceValue}, Desempeño: {performance}).");

            // Ejecutar movimiento en el BoardManager
            if (_boardManager != null)
            {
                _boardManager.MovePlayer(player, tilesToMove);
            }
            else
            {
                // Fallback si no hay BoardManager
                player.MoveForward(tilesToMove);
            }

            // Registrar posición de destino
            turnContext.DestinationPosition = player.Position;

            // Emitir evento
            GameEvents.OnPlayerMoved?.Invoke(player, turnContext.OriginPosition, turnContext.DestinationPosition);
        }

        public void Tick()
        {
            // Esperar si hay una bifurcación pendiente de decisión
            if (_boardManager == null || !_boardManager.IsMovementPending)
            {
                _gameManager.TransitionTo(new TileEffectState(_gameManager, _boardManager));
            }
        }

        public void Exit()
        {
            Debug.Log("MovementState: Exit");
        }
    }
}
