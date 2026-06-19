using System;
using UnityEngine;
using ChronosAndCards.Data;
using ChronosAndCards.Gameplay;

namespace ChronosAndCards.Core.States
{
    /// <summary>
    /// Estado que presenta la pregunta al jugador y espera su respuesta.
    /// Evalúa la corrección (automática o mediante el GM) y el uso de pistas.
    /// </summary>
    public class ResolutionState : IGameState
    {
        private readonly GameManager _gameManager;
        private readonly TurnContext _turnContext;
        private PerformanceMultiplier? _submittedResult;
        private bool _awaitingAnswer;
        private bool _transitionStarted;
        private bool _waitingForGm;
        private float _gmTimer;
        private const float GmTimeoutDuration = 30f;

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
            _waitingForGm = false;
            _gmTimer = 0f;

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
            _waitingForGm = false;
        }

        private void OnAnswerReceived(string answer)
        {
            if (!_awaitingAnswer) return;

            Debug.Log($"ResolutionState: Respuesta recibida del jugador: {answer}");

            if (_turnContext != null)
            {
                _turnContext.PlayerAnswer = answer;
            }

            // Evaluar respuesta usando AnswerEvaluator
            EvaluationResult evalResult;
            if (_gameManager != null && _gameManager.AnswerEvaluator != null && _turnContext != null && _turnContext.CurrentCard.HasValue)
            {
                evalResult = _gameManager.AnswerEvaluator.Evaluate(answer, _turnContext.CurrentCard.Value, _turnContext);
            }
            else
            {
                // Fallback básico si faltan componentes
                evalResult = new EvaluationResult { IsCorrect = false, Multiplier = PerformanceMultiplier.Fail, RequiresGmJudgment = false };
            }

            if (evalResult.RequiresGmJudgment)
            {
                Debug.Log("ResolutionState: La pregunta requiere juicio manual del GM. Pausando flujo normal.");
                _awaitingAnswer = false;
                _waitingForGm = true;
                _gmTimer = 0f;

                // Suscribirse al veredicto del GM
                GameEvents.OnGmJudgmentSubmitted += OnGmJudgmentSubmitted;

                // Lanzar evento para que la UI muestre la respuesta al GM
                string criteria = (_turnContext != null && _turnContext.CurrentCard.HasValue) 
                    ? _turnContext.CurrentCard.Value.CorrectAnswer 
                    : "";
                
                GameEvents.OnGmJudgmentRequired?.Invoke(answer, criteria);
            }
            else
            {
                if (_turnContext != null)
                {
                    _turnContext.PerformanceResult = evalResult.Multiplier;
                }
                _submittedResult = evalResult.Multiplier;
                _awaitingAnswer = false;
            }
        }

        private void OnGmJudgmentSubmitted(bool isCorrect)
        {
            Debug.Log($"ResolutionState: Juicio del GM recibido. Correcto: {isCorrect}");
            
            // Desuscribirse inmediatamente
            GameEvents.OnGmJudgmentSubmitted -= OnGmJudgmentSubmitted;
            _waitingForGm = false;

            PerformanceMultiplier resultMultiplier;
            if (isCorrect)
            {
                if (_turnContext != null && (_turnContext.IsOverdriveActive || !_turnContext.UsedHint))
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
        }

        public void Tick()
        {
            if (_transitionStarted) return;

            // Si se está esperando la respuesta manual del GM, monitorear el timeout
            if (_waitingForGm)
            {
                _gmTimer += Time.deltaTime;
                if (_gmTimer >= GmTimeoutDuration)
                {
                    Debug.LogWarning($"ResolutionState: Se superó el tiempo límite de {GmTimeoutDuration}s para el juicio del GM. Calificando como incorrecta por defecto.");
                    OnGmJudgmentSubmitted(false);
                }
                return;
            }

            // Esperar a que se defina el resultado de la evaluación
            if (_submittedResult.HasValue)
            {
                _transitionStarted = true;
                PerformanceMultiplier result = _submittedResult.Value;

                // Almacenar resultado en el GameContext para flujos/UI retrocompatibles
                if (_gameManager != null && _gameManager.GameContext != null)
                {
                    _gameManager.GameContext.LastResult = result;
                }

                // Si falló, ofrecer ventana para objetos AfterFail o RivalTurn
                if (result == PerformanceMultiplier.Fail && _gameManager != null && _gameManager.GameContext != null)
                {
                    Debug.Log("ResolutionState: El jugador falló la pregunta. Habilitando ventana AfterFail.");
                    _gameManager.GameContext.CurrentPhase = ItemActivationPhase.AfterFail;
                    
                    if (_turnContext != null)
                    {
                        _turnContext.FailedThisTurn = true;
                    }
                }

                // Emitir evento de resolución
                if (_gameManager != null && _gameManager.GameContext != null)
                {
                    GameEvents.OnQuestionResolved?.Invoke(
                        _gameManager.GameContext.CurrentPlayer,
                        result
                    );
                }

                // Transicionar al estado de movimiento
                if (_gameManager != null)
                {
                    _gameManager.TransitionTo(new MovementState(_gameManager, _gameManager.AdvanceCalculator, _gameManager.BoardManager));
                }
            }
        }

        public void Exit()
        {
            Debug.Log("ResolutionState: Exit");
            // Limpieza de suscripciones
            GameEvents.OnAnswerSubmitted -= OnAnswerReceived;
            GameEvents.OnGmJudgmentSubmitted -= OnGmJudgmentSubmitted;
        }
    }
}
