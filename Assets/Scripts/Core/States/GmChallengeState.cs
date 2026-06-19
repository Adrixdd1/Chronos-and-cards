using System.Collections.Generic;
using UnityEngine;
using ChronosAndCards.Data;
using ChronosAndCards.Gameplay.GmChallenge;

namespace ChronosAndCards.Core.States
{
    /// <summary>
    /// Estado FSM: Maneja el flujo completo de un Desafío del Game Master.
    /// 1. Selecciona reto y presenta la UI.
    /// 2. Activa FirstToPressManager para decidir el contestant.
    /// 3. Inicia cuenta regresiva para la respuesta manual del GM.
    /// 4. Distribuye la recompensa si el GM aprueba.
    /// </summary>
    public class GmChallengeState : IGameState
    {
        private readonly GameManager _gameManager;
        private CardData? _currentChallenge;
        private IPlayer _contestant;
        private bool _isAwaitingGmJudgment;
        private float _timeRemaining;

        public GmChallengeState(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public void Enter()
        {
            Debug.Log("GmChallengeState: Ingresando...");
            _contestant = null;
            _isAwaitingGmJudgment = false;

            if (_gameManager.GameContext.ContentManager == null)
            {
                Debug.LogWarning("GmChallengeState: ContentManager es null. Saltando desafío.");
                GameEvents.OnGmChallengeSkipped?.Invoke();
                FinishState();
                return;
            }

            // 1. Extraer reto del mazo GM
            _currentChallenge = _gameManager.GameContext.ContentManager.DrawGmChallenge();

            if (_currentChallenge == null)
            {
                Debug.LogWarning("GmChallengeState: No hay retos GM disponibles. Saltando desafío.");
                GameEvents.OnGmChallengeSkipped?.Invoke();
                FinishState();
                return;
            }

            // Iniciar desafío
            GameEvents.OnGmChallengeStarted?.Invoke(_currentChallenge.Value);

            // 2. Iniciar FirstToPressManager
            if (_gameManager.FirstToPressManager != null)
            {
                _gameManager.FirstToPressManager.OnFirstPlayerPressed += OnContestantSelected;
                _gameManager.FirstToPressManager.StartListening(_gameManager.Players, _gameManager.GmChallengeConfig);
            }
            else
            {
                Debug.LogError("GmChallengeState: FirstToPressManager es null.");
                FinishState();
            }
        }

        private void OnContestantSelected(IPlayer contestant)
        {
            if (_gameManager.FirstToPressManager != null)
            {
                _gameManager.FirstToPressManager.OnFirstPlayerPressed -= OnContestantSelected;
            }

            _contestant = contestant;
            Debug.Log($"GmChallengeState: {contestant.PlayerName} fue el primero en pulsar.");
            
            GameEvents.OnGmChallengeContestantSelected?.Invoke(contestant, _currentChallenge.Value);

            // 3. Iniciar cuenta regresiva para respuesta
            _isAwaitingGmJudgment = true;
            _timeRemaining = _gameManager.GmChallengeConfig != null ? _gameManager.GmChallengeConfig.TimeLimit : 15f;

            // Suscribirse a la respuesta del GM
            GameEvents.OnGmJudgmentSubmitted += OnGmJudgmentReceived;
            
            // Emitir evento para abrir UI del GM
            GameEvents.OnGmJudgmentRequired?.Invoke("Respuesta en vivo (Oral)", _currentChallenge.Value.CorrectAnswer);
        }

        public void Tick()
        {
            if (_isAwaitingGmJudgment)
            {
                _timeRemaining -= Time.deltaTime;
                if (_timeRemaining <= 0)
                {
                    // Se agotó el tiempo
                    Debug.Log($"GmChallengeState: Tiempo agotado para {_contestant.PlayerName}.");
                    OnGmJudgmentReceived(false);
                }
            }
        }

        private void OnGmJudgmentReceived(bool isCorrect)
        {
            _isAwaitingGmJudgment = false;
            GameEvents.OnGmJudgmentSubmitted -= OnGmJudgmentReceived;

            if (isCorrect)
            {
                Debug.Log($"GmChallengeState: GM aprobó la respuesta de {_contestant.PlayerName}.");
                
                // Otorgar recompensa
                if (_gameManager.GmRewardDistributor != null)
                {
                    var rewardType = _gameManager.GmRewardDistributor.GrantRandomReward(_contestant, _gameManager.GameContext);
                    GameEvents.OnGmChallengeEnded?.Invoke(_contestant, rewardType);
                }
                else
                {
                    Debug.LogWarning("GmChallengeState: GmRewardDistributor es null. No se otorgó recompensa.");
                    GameEvents.OnGmChallengeEndedNoWinner?.Invoke();
                }

                FinishState();
            }
            else
            {
                Debug.Log($"GmChallengeState: GM rechazó la respuesta de {_contestant.PlayerName}.");
                
                // Penalizar: Bloqueo de turno
                _contestant.SetSkipNextTurn(true);
                GameEvents.OnGmChallengeContestantFailed?.Invoke(_contestant);
                GameEvents.OnGmChallengeEndedNoWinner?.Invoke();

                FinishState();
            }
        }

        private void FinishState()
        {
            _gameManager.TransitionTo(new PlayerTurnState(_gameManager));
        }

        public void Exit()
        {
            Debug.Log("GmChallengeState: Saliendo...");
            if (_gameManager.FirstToPressManager != null)
            {
                _gameManager.FirstToPressManager.StopListening();
                _gameManager.FirstToPressManager.OnFirstPlayerPressed -= OnContestantSelected;
            }
            GameEvents.OnGmJudgmentSubmitted -= OnGmJudgmentReceived;
        }
    }
}
