using UnityEngine;

namespace ChronosAndCards.Core.States
{
    /// <summary>
    /// Stub para el estado de Duelo (interrupción). Se completará en la Épica 5.
    /// Valida que el mecanismo Push/Pop de la FSM funciona correctamente.
    /// Al terminar, hace Pop para regresar al estado previo en la pila.
    /// </summary>
    public class DuelState : IGameState
    {
        private readonly GameManager _gameManager;
        private readonly IPlayer _attacker;
        private readonly IPlayer _defender;
        private bool _duelResolved;

        /// <summary>Constructor para inicializar el duelo con los contrincantes.</summary>
        public DuelState(GameManager gameManager, IPlayer attacker, IPlayer defender)
        {
            _gameManager = gameManager;
            _attacker = attacker;
            _defender = defender;
        }

        public void Enter()
        {
            Debug.Log($"DuelState: Enter. Duelo iniciado entre {_attacker.PlayerName} (atacante) y {_defender.PlayerName} (defensor).");
            GameEvents.OnDuelStarted?.Invoke(_attacker, _defender);
        }

        public void Tick()
        {
            if (!_duelResolved)
            {
                // Simulación de resolución instantánea en el stub: gana el atacante
                _duelResolved = true;
                Debug.Log($"DuelState: Duelo resuelto instantáneamente. Ganador: {_attacker.PlayerName}");
                
                // Salir del duelo y restaurar el estado anterior
                _gameManager.PopState();
            }
        }

        public void Exit()
        {
            Debug.Log("DuelState: Exit. Limpiando estado del duelo.");
            // Emitir evento de fin de duelo con el ganador
            GameEvents.OnDuelEnded?.Invoke(_attacker);
        }
    }
}
