using UnityEngine;

namespace ChronosAndCards.Core.States
{
    /// <summary>
    /// Estado final del juego. Detiene el game loop, emite el evento de finalización
    /// con el ganador y espera la interacción del usuario para reiniciar.
    /// </summary>
    public class GameOverState : IGameState
    {
        private readonly GameManager _gameManager;
        private readonly IPlayer _winner;

        /// <summary>Constructor que inyecta el GameManager y el jugador ganador.</summary>
        public GameOverState(GameManager gameManager, IPlayer winner)
        {
            _gameManager = gameManager;
            _winner = winner;
        }

        public void Enter()
        {
            Debug.Log($"GameOverState: Enter. El ganador es {_winner.PlayerName}");
            GameEvents.OnGameOver?.Invoke(_winner);
        }

        public void Tick()
        {
            // Espera input del usuario para reiniciar (en producción).
            // Para la Épica 1, permanece en este estado de forma pasiva.
        }

        public void Exit()
        {
            Debug.Log("GameOverState: Exit");
            // Limpieza de estado de la partida
        }
    }
}
