using System.Collections.Generic;
using UnityEngine;
using ChronosAndCards.Data;

namespace ChronosAndCards.Core.States
{
    /// <summary>
    /// Estado de Duelo: Gestiona la interrupción para un Duelo de Posiciones.
    /// Pide selección de rival, presenta la pregunta y evalúa las respuestas simultáneas.
    /// </summary>
    public class DuelState : IGameState
    {
        private readonly GameManager _gameManager;
        private IPlayer _attacker;
        private IPlayer _defender;
        private CardData _duelCard;
        
        private string _attackerAnswer;
        private string _defenderAnswer;
        private bool _isTimerActive;
        private float _timer;
        private const float DefaultTimer = 30f;
        
        private enum DuelPhase
        {
            SelectRival,
            AwaitingAnswers,
            Evaluation
        }
        private DuelPhase _phase;

        public DuelState(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public void Enter()
        {
            _attacker = _gameManager.GameContext.ActivePlayer;
            _phase = DuelPhase.SelectRival;
            
            GameEvents.OnRivalSelected += OnRivalSelected;
            GameEvents.OnDuelAnswerSubmitted += OnDuelAnswerSubmitted;

            // Solicitar a UI la selección del rival
            var possibleRivals = new List<IPlayer>(_gameManager.Players);
            possibleRivals.Remove(_attacker);
            GameEvents.OnRivalSelectionRequired?.Invoke(possibleRivals);
        }

        private void OnRivalSelected(IPlayer defender)
        {
            if (_phase != DuelPhase.SelectRival) return;
            
            _defender = defender;
            _gameManager.GameContext.TargetPlayer = defender;
            GameEvents.OnDuelInitiated?.Invoke(_attacker);

            // Obtener carta para el duelo
            if (_gameManager.ContentManager != null)
            {
                _duelCard = _gameManager.ContentManager.GetRandomCard(0); // Nivel básico por defecto para duelos
            }
            
            _phase = DuelPhase.AwaitingAnswers;
            _isTimerActive = true;
            _timer = DefaultTimer;

            GameEvents.OnDuelQuestionPresented?.Invoke(_attacker, _defender, _duelCard);
        }

        private void OnDuelAnswerSubmitted(IPlayer player, string answer)
        {
            if (_phase != DuelPhase.AwaitingAnswers) return;

            if (player == _attacker) _attackerAnswer = answer;
            else if (player == _defender) _defenderAnswer = answer;

            if (!string.IsNullOrEmpty(_attackerAnswer) && !string.IsNullOrEmpty(_defenderAnswer))
            {
                ResolveDuel();
            }
        }

        public void Tick()
        {
            if (_phase == DuelPhase.AwaitingAnswers && _isTimerActive)
            {
                _timer -= Time.deltaTime;
                if (_timer <= 0)
                {
                    ResolveDuel(); // Quien no respondió falla
                }
            }
        }

        private void ResolveDuel()
        {
            _phase = DuelPhase.Evaluation;
            _isTimerActive = false;

            bool attackerCorrect = false;
            bool defenderCorrect = false;

            if (_gameManager.AnswerEvaluator != null)
            {
                if (!string.IsNullOrEmpty(_attackerAnswer))
                    attackerCorrect = _gameManager.AnswerEvaluator.Evaluate(_attackerAnswer, _duelCard, null).IsCorrect;
                
                if (!string.IsNullOrEmpty(_defenderAnswer))
                    defenderCorrect = _gameManager.AnswerEvaluator.Evaluate(_defenderAnswer, _duelCard, null).IsCorrect;
            }

            DuelResult result;
            if (attackerCorrect && !defenderCorrect)
                result = DuelResult.AttackerWins;
            else if (defenderCorrect && !attackerCorrect)
                result = DuelResult.DefenderWins;
            else
                result = DuelResult.Draw; // Empate

            GameEvents.OnDuelResolved?.Invoke(_attacker, _defender, result);

            // Aplicar efectos
            if (result == DuelResult.AttackerWins)
            {
                int temp = _attacker.Position;
                _attacker.MoveToPosition(_defender.Position);
                _defender.MoveToPosition(temp);
                Debug.Log($"DuelState: {_attacker.PlayerName} gana y toma la posición {_attacker.Position}.");
            }
            else if (result == DuelResult.DefenderWins)
            {
                _attacker.MoveForward(-2);
                Debug.Log($"DuelState: {_defender.PlayerName} gana. Atacante retrocede a {_attacker.Position}.");
            }
            else
            {
                Debug.Log("DuelState: Empate. Nadie se mueve.");
            }

            _gameManager.GameContext.PopSubTurn();
            _gameManager.PopState();
        }

        public void Exit()
        {
            GameEvents.OnRivalSelected -= OnRivalSelected;
            GameEvents.OnDuelAnswerSubmitted -= OnDuelAnswerSubmitted;
            _gameManager.GameContext.TargetPlayer = null;
        }
    }
}
