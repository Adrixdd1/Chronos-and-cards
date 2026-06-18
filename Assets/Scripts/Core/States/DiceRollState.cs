using UnityEngine;
using ChronosAndCards.Data;

namespace ChronosAndCards.Core.States
{
    /// <summary>
    /// Estado que gestiona el lanzamiento del dado, registra el resultado obtenido
    /// y ofrece una ventana de interrupción para objetos activables durante el turno rival.
    /// </summary>
    public class DiceRollState : IGameState
    {
        private readonly GameManager _gameManager;
        private bool _rollCompleted;
        private bool _transitionStarted;

        /// <summary>Constructor que inyecta el GameManager.</summary>
        public DiceRollState(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public void Enter()
        {
            Debug.Log("DiceRollState: Enter");

            // Suscribirse al evento de resultado del dado
            if (_gameManager.DiceRoller != null)
            {
                _gameManager.DiceRoller.OnDiceResult += OnDiceRolled;
                _gameManager.DiceRoller.Roll();
            }
            else
            {
                Debug.LogError("DiceRollState: DiceRoller no está asignado en el GameManager.");
                // Simulación de seguridad para evitar bloqueos
                OnDiceRolled(UnityEngine.Random.Range(1, 7));
            }
        }

        public void Tick()
        {
            if (_transitionStarted) return;

            // Espera a que se complete la animación/rodaje del dado
            if (_rollCompleted)
            {
                _transitionStarted = true;
                
                // Ofrece ventana para objetos de fase RivalTurn (ej. Sabotaje)
                _gameManager.GameContext.CurrentPhase = ItemActivationPhase.RivalTurn;
                
                // Transiciona a la extracción de cartas
                _gameManager.TransitionTo(new CardDrawState(_gameManager));
            }
        }

        public void Exit()
        {
            Debug.Log("DiceRollState: Exit");
            if (_gameManager.DiceRoller != null)
            {
                _gameManager.DiceRoller.OnDiceResult -= OnDiceRolled;
            }
        }

        private void OnDiceRolled(int result)
        {
            Debug.Log($"DiceRollState: Resultado del dado recibido: {result}");
            
            // Almacenar resultado en el contexto
            _gameManager.GameContext.CurrentDiceValue = result;
            
            // Emitir evento
            GameEvents.OnDiceRolled?.Invoke(result);
            
            // Marcar rodaje como completado
            _rollCompleted = true;
        }
    }
}
