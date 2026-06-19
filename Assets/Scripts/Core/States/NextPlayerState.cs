using UnityEngine;

namespace ChronosAndCards.Core.States
{
    /// <summary>
    /// Estado que finaliza el turno actual, evalúa la condición de victoria (meta en posición 20)
    /// y determina si se inicia una nueva ronda con el Desafío del GM o si se pasa al siguiente jugador.
    /// </summary>
    public class NextPlayerState : IGameState
    {
        private readonly GameManager _gameManager;
        private const int MetaPosition = 20;

        /// <summary>Constructor que inyecta el GameManager.</summary>
        public NextPlayerState(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public void Enter()
        {
            Debug.Log("NextPlayerState: Enter");

            IPlayer activePlayer = _gameManager.GameContext.CurrentPlayer;

            // Emitir evento de fin de turno
            if (activePlayer != null)
            {
                GameEvents.OnTurnEnded?.Invoke(activePlayer);
            }

            // 1. Evaluar condición de victoria
            if (activePlayer != null && activePlayer.Position >= MetaPosition)
            {
                Debug.Log($"NextPlayerState: ¡El jugador {activePlayer.PlayerName} alcanzó la meta!");
                _gameManager.TransitionTo(new GameOverState(_gameManager, activePlayer));

                return;
            }

            // 2. Evaluar fin de ronda
            int totalPlayers = _gameManager.Players.Count;
            if (_gameManager.CurrentPlayerIndex >= totalPlayers - 1)
            {
                // Todos los jugadores han tenido su turno en esta ronda
                Debug.Log("NextPlayerState: Fin de ronda. Transicionando a GmChallengeState.");
                // Resetear índice para la siguiente ronda
                _gameManager.CurrentPlayerIndex = 0;
                _gameManager.TransitionTo(new GmChallengeState(_gameManager));
            }
            else
            {
                // Quedan jugadores en esta ronda
                _gameManager.CurrentPlayerIndex++;
                Debug.Log($"NextPlayerState: Pasando al siguiente jugador. Nuevo índice: {_gameManager.CurrentPlayerIndex}");
                _gameManager.TransitionTo(new PlayerTurnState(_gameManager));
            }
        }

        public void Tick()
        {
            // La transición ya ocurre en Enter() de forma determinista para la Épica 1
        }

        public void Exit()
        {
            Debug.Log("NextPlayerState: Exit");
        }
    }
}
