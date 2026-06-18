using UnityEngine;

namespace ChronosAndCards.Core.States
{
    /// <summary>
    /// Stub para el estado de Desafío del GM (fin de ronda). Se completará en la Épica 6.
    /// Para evitar bloquear el game loop, transiciona de inmediato al turno del siguiente jugador.
    /// </summary>
    public class GmChallengeState : IGameState
    {
        private readonly GameManager _gameManager;

        /// <summary>Constructor que inyecta el GameManager.</summary>
        public GmChallengeState(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public void Enter()
        {
            Debug.Log("GmChallengeState: Stub — implementación completa en Épica 6");
        }

        public void Tick()
        {
            // En el stub, transicionamos de inmediato para continuar el bucle del juego
            _gameManager.TransitionTo(new PlayerTurnState(_gameManager));
        }

        public void Exit()
        {
            Debug.Log("GmChallengeState: Exit");
        }
    }
}
