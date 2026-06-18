using UnityEngine;
using ChronosAndCards.Data;
using ChronosAndCards.Gameplay.Dice;
using ChronosAndCards.Gameplay;

namespace ChronosAndCards.Core.States
{
    /// <summary>
    /// Estado que gestiona el lanzamiento del dado, registra el resultado obtenido
    /// y ofrece una ventana de interrupción para objetos activables durante el turno rival.
    /// </summary>
    public class DiceRollState : IGameState
    {
        private readonly GameManager _gameManager;
        private readonly DiceLogic _diceLogic;
        private readonly DicePhysics _dicePhysics;

        private bool _rollCompleted;
        private bool _animationComplete;
        private bool _transitionStarted;
        private float _elapsedTime;

        /// <summary>Constructor que inyecta el GameManager, DiceLogic y DicePhysics.</summary>
        public DiceRollState(GameManager gameManager, DiceLogic diceLogic, DicePhysics dicePhysics)
        {
            _gameManager = gameManager;
            _diceLogic = diceLogic;
            _dicePhysics = dicePhysics;
        }

        public void Enter()
        {
            Debug.Log("DiceRollState: Enter");
            _rollCompleted = false;
            _animationComplete = false;
            _transitionStarted = false;
            _elapsedTime = 0f;

            if (_diceLogic != null)
            {
                _diceLogic.OnDiceResult += OnDiceRolled;
            }
            if (_dicePhysics != null)
            {
                _dicePhysics.OnDiceAnimationComplete += OnDiceAnimationFinished;
            }

            if (_diceLogic != null)
            {
                _diceLogic.Roll();
            }
            else
            {
                Debug.LogError("DiceRollState: DiceLogic no está asignado.");
                // Simulación de seguridad para evitar bloqueos
                OnDiceRolled(UnityEngine.Random.Range(1, 7));
            }
        }

        public void Tick()
        {
            if (_transitionStarted) return;

            _elapsedTime += Time.deltaTime;

            float timeout = _dicePhysics != null ? _dicePhysics.AnimationTimeout : 6f;

            // Espera a que se complete la animación/rodaje del dado o que ocurra un timeout
            if (_animationComplete || _elapsedTime >= timeout)
            {
                if (_elapsedTime >= timeout && !_animationComplete)
                {
                    Debug.LogWarning("DiceRollState: Se detectó un timeout en la animación del dado. Forzando transición.");
                }

                _transitionStarted = true;
                
                // Ofrece ventana para objetos de fase RivalTurn (ej. Sabotaje)
                _gameManager.GameContext.CurrentPhase = ItemActivationPhase.RivalTurn;
                
                // Transiciona a la extracción de cartas
                _gameManager.TransitionTo(new CardDrawState(_gameManager, _gameManager.DifficultyMapper, _gameManager.ContentManager));
            }
        }

        public void Exit()
        {
            Debug.Log("DiceRollState: Exit");
            if (_diceLogic != null)
            {
                _diceLogic.OnDiceResult -= OnDiceRolled;
            }
            if (_dicePhysics != null)
            {
                _dicePhysics.OnDiceAnimationComplete -= OnDiceAnimationFinished;
            }
        }

        private void OnDiceRolled(int result)
        {
            Debug.Log($"DiceRollState: Resultado del dado recibido: {result}");
            
            // Almacenar resultado en el TurnContext
            if (_gameManager.TurnContext != null)
            {
                _gameManager.TurnContext.DiceValue = result;
            }

            // Almacenar resultado en el GameContext para retrocompatibilidad/flujos globales
            _gameManager.GameContext.CurrentDiceValue = result;
            
            // Emitir evento
            GameEvents.OnDiceRolled?.Invoke(result);
            
            // Marcar rodaje lógico como completado
            _rollCompleted = true;

            // Iniciar animación física
            if (_dicePhysics != null)
            {
                _dicePhysics.AnimateToResult(result);
            }
            else
            {
                Debug.LogWarning("DiceRollState: DicePhysics no asignado. Completando animación de inmediato.");
                OnDiceAnimationFinished();
            }
        }

        private void OnDiceAnimationFinished()
        {
            Debug.Log("DiceRollState: Animación física del dado terminada.");
            _animationComplete = true;
        }
    }
}
