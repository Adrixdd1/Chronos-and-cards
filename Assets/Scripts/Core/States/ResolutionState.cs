using UnityEngine;
using ChronosAndCards.Data;

namespace ChronosAndCards.Core.States
{
    /// <summary>
    /// Estado que presenta la pregunta al jugador y espera su respuesta.
    /// Evalúa la corrección y el uso de pistas para calcular el desempeño.
    /// </summary>
    public class ResolutionState : IGameState
    {
        private readonly GameManager _gameManager;
        private PerformanceMultiplier? _submittedResult;
        private bool _transitionStarted;

        /// <summary>Constructor que inyecta el GameManager.</summary>
        public ResolutionState(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public void Enter()
        {
            Debug.Log("ResolutionState: Enter");

            // Establecer fase de activación antes de responder
            _gameManager.GameContext.CurrentPhase = ItemActivationPhase.BeforeAnswer;

            // Esperar respuesta de la UI/Tests
            _submittedResult = null;
        }

        /// <summary>
        /// Método público para enviar el resultado de la respuesta (usado por la UI y los Tests).
        /// </summary>
        public void SubmitAnswer(PerformanceMultiplier result)
        {
            _submittedResult = result;
        }

        public void Tick()
        {
            if (_transitionStarted) return;

            // Esperar a que se envíe una respuesta
            if (_submittedResult.HasValue)
            {
                _transitionStarted = true;
                PerformanceMultiplier result = _submittedResult.Value;

                // Almacenar resultado en el contexto
                _gameManager.GameContext.LastResult = result;

                // Si falló, ofrecer ventana para objetos AfterFail o RivalTurn
                if (result == PerformanceMultiplier.Fail)
                {
                    Debug.Log("ResolutionState: El jugador falló la pregunta. Habilitando ventana AfterFail.");
                    _gameManager.GameContext.CurrentPhase = ItemActivationPhase.AfterFail;
                }

                // Emitir evento de resolución
                GameEvents.OnQuestionResolved?.Invoke(
                    _gameManager.GameContext.CurrentPlayer,
                    result
                );

                // Transicionar al estado de movimiento
                _gameManager.TransitionTo(new MovementState(_gameManager));
            }
        }

        public void Exit()
        {
            Debug.Log("ResolutionState: Exit");
        }
    }
}
