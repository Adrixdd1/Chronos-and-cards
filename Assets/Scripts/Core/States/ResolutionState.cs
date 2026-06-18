using System;
using UnityEngine;
using ChronosAndCards.Data;
using ChronosAndCards.Gameplay;

namespace ChronosAndCards.Core.States
{
    /// <summary>
    /// Estado que presenta la pregunta al jugador y espera su respuesta.
    /// Evalúa la corrección y el uso de pistas para calcular el desempeño.
    /// </summary>
    public class ResolutionState : IGameState
    {
        private readonly GameManager _gameManager;
        private readonly TurnContext _turnContext;
        private PerformanceMultiplier? _submittedResult;
        private bool _awaitingAnswer;
        private bool _transitionStarted;

        /// <summary>Constructor que inyecta el GameManager y el TurnContext.</summary>
        public ResolutionState(GameManager gameManager, TurnContext turnContext)
        {
            _gameManager = gameManager;
            _turnContext = turnContext;
        }

        public void Enter()
        {
            Debug.Log("ResolutionState: Enter");

            // Establecer fase de activación antes de responder
            _gameManager.GameContext.CurrentPhase = ItemActivationPhase.BeforeAnswer;

            // Esperar respuesta de la UI/Tests
            _submittedResult = null;
            _awaitingAnswer = true;
            _transitionStarted = false;

            // Suscribirse al evento de respuesta enviada
            GameEvents.OnAnswerSubmitted += OnAnswerReceived;
        }

        /// <summary>
        /// Método público para enviar el resultado de la respuesta directamente (para retrocompatibilidad en tests).
        /// </summary>
        public void SubmitAnswer(PerformanceMultiplier result)
        {
            _submittedResult = result;
            _awaitingAnswer = false;
        }

        private void OnAnswerReceived(string answer)
        {
            if (!_awaitingAnswer) return;

            Debug.Log($"ResolutionState: Respuesta recibida del jugador: {answer}");

            if (_turnContext != null)
            {
                _turnContext.PlayerAnswer = answer;
            }

            // Comparación de respuesta (case insensitive, trim)
            bool isCorrect = false;
            if (_turnContext != null && _turnContext.CurrentCard.HasValue)
            {
                string correctAnswer = _turnContext.CurrentCard.Value.CorrectAnswer;
                if (correctAnswer != null && answer != null)
                {
                    isCorrect = string.Equals(correctAnswer.Trim(), answer.Trim(), StringComparison.OrdinalIgnoreCase);
                }
            }

            // Calcular PerformanceMultiplier
            PerformanceMultiplier resultMultiplier;
            if (isCorrect)
            {
                if (_turnContext != null && (_turnContext.IsOverdriveActive || (!_turnContext.UsedHint && !_turnContext.RevealedOptions)))
                {
                    resultMultiplier = PerformanceMultiplier.Perfect;
                }
                else
                {
                    resultMultiplier = PerformanceMultiplier.WithHelp;
                }
            }
            else
            {
                resultMultiplier = PerformanceMultiplier.Fail;
            }

            if (_turnContext != null)
            {
                _turnContext.PerformanceResult = resultMultiplier;
            }
            
            _submittedResult = resultMultiplier;
            _awaitingAnswer = false;
        }

        public void Tick()
        {
            if (_transitionStarted) return;

            // Esperar a que se envíe una respuesta
            if (_submittedResult.HasValue)
            {
                _transitionStarted = true;
                PerformanceMultiplier result = _submittedResult.Value;

                // Almacenar resultado en el GameContext para flujos/UI retrocompatibles
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
                _gameManager.TransitionTo(new MovementState(_gameManager, _gameManager.AdvanceCalculator, _gameManager.BoardManager));
            }
        }

        public void Exit()
        {
            Debug.Log("ResolutionState: Exit");
            // Desuscribirse del evento
            GameEvents.OnAnswerSubmitted -= OnAnswerReceived;
        }
    }
}
