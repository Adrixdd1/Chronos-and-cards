using UnityEngine;

namespace ChronosAndCards.Core.States
{
    /// <summary>
    /// Estado inicial de configuración de la partida.
    /// Inicializa a los jugadores, el tablero y el contenido.
    /// </summary>
    public class SetupState : IGameState
    {
        private readonly GameManager _gameManager;
        private bool _isSetupDone;

        /// <summary>Constructor que inyecta el GameManager.</summary>
        public SetupState(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public void Enter()
        {
            Debug.Log("SetupState: Enter");

            // Configurar el contexto inicial
            _gameManager.GameContext.CurrentPlayer = null;
            _gameManager.GameContext.TargetPlayer = null;
            _gameManager.GameContext.CurrentDiceValue = 0;
            _gameManager.GameContext.CurrentCard = null;
            _gameManager.GameContext.LastResult = Data.PerformanceMultiplier.Fail;
            _gameManager.GameContext.IsPlayerTurn = false;
            _gameManager.GameContext.CurrentPhase = Data.ItemActivationPhase.BeforeAnswer;

            // Inicializar managers
            _gameManager.BoardManager?.Initialize();
            _gameManager.ContentManager?.LoadContent();

            // Registrar jugadores de prueba para inicializar la partida
            _gameManager.Players.Clear();
            var p1 = new TestPlayer("Player 1", 0);
            var p2 = new TestPlayer("Player 2", 1);
            _gameManager.Players.Add(p1);
            _gameManager.Players.Add(p2);
            
            if (_gameManager.BoardManager != null)
            {
                _gameManager.BoardManager.RegisterPlayer(p1);
                _gameManager.BoardManager.RegisterPlayer(p2);
            }
            _gameManager.CurrentPlayerIndex = 0;

            _isSetupDone = true;
        }

        public void Tick()
        {
            // Transiciona al primer turno una vez que la inicialización se completa
            if (_isSetupDone)
            {
                _gameManager.TransitionTo(new PlayerTurnState(_gameManager));
            }
        }

        public void Exit()
        {
            Debug.Log("SetupState: Exit");
            // Notificar que el juego se ha iniciado
            GameEvents.OnGameStarted?.Invoke();
        }
    }
}
