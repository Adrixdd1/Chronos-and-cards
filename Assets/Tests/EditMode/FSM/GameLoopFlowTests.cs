using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Core.States;
using ChronosAndCards.Data;
using ChronosAndCards.Gameplay;

namespace ChronosAndCards.Tests
{
    [TestFixture]
    public class GameLoopFlowTests
    {
        private GameObject _gameObject;
        private GameManager _gameManager;
        private BoardManager _boardManager;
        private ContentManager _contentManager;
        private DiceRoller _diceRoller;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("GameManagerTestObject");
            _gameManager = _gameObject.AddComponent<GameManager>();
            _boardManager = _gameObject.AddComponent<BoardManager>();
            _contentManager = _gameObject.AddComponent<ContentManager>();
            _diceRoller = _gameObject.AddComponent<DiceRoller>();

            // Asignar campos serializados privados vía reflexión
            typeof(GameManager).GetField("_boardManager", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_gameManager, _boardManager);
            typeof(GameManager).GetField("_contentManager", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_gameManager, _contentManager);
            typeof(GameManager).GetField("_diceRoller", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_gameManager, _diceRoller);

            GameEvents.ClearAll();
        }

        [TearDown]
        public void TearDown()
        {
            GameEvents.ClearAll();
            Object.DestroyImmediate(_gameObject);
        }

        private void TickFSM()
        {
            MethodInfo updateMethod = typeof(GameManager).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
            updateMethod.Invoke(_gameManager, null);
        }

        [Test]
        public void FullTurnFlow_SetupToNextPlayer_TransitionsInOrder()
        {
            // --- 1. Inicialización FSM (SetupState) ---
            // El GameManager inicializa en SetupState en el Awake/Start. Para forzar en EditMode:
            _gameManager.TransitionTo(new SetupState(_gameManager));
            Assert.IsInstanceOf<SetupState>(_gameManager.CurrentState);

            // --- 2. SetupState → PlayerTurnState ---
            TickFSM();
            Assert.IsInstanceOf<PlayerTurnState>(_gameManager.CurrentState);

            // --- 3. PlayerTurnState → DiceRollState ---
            TickFSM();
            Assert.IsInstanceOf<DiceRollState>(_gameManager.CurrentState);

            // --- 4. DiceRollState → CardDrawState (se lanza el dado automáticamente y se resuelve) ---
            TickFSM();
            Assert.IsInstanceOf<CardDrawState>(_gameManager.CurrentState);

            // --- 5. CardDrawState → ResolutionState ---
            TickFSM();
            Assert.IsInstanceOf<ResolutionState>(_gameManager.CurrentState);

            // --- 6. ResolutionState (espera input) ---
            // Simulamos el envío de la respuesta correcta perfecta (Perfect)
            var resolutionState = _gameManager.CurrentState as ResolutionState;
            Assert.IsNotNull(resolutionState);
            resolutionState.SubmitAnswer(PerformanceMultiplier.Perfect);

            // --- 7. ResolutionState → MovementState ---
            TickFSM();
            Assert.IsInstanceOf<MovementState>(_gameManager.CurrentState);

            // --- 8. MovementState → TileEffectState ---
            TickFSM();
            Assert.IsInstanceOf<TileEffectState>(_gameManager.CurrentState);

            // --- 9. TileEffectState → NextPlayerState ---
            TickFSM();
            Assert.IsInstanceOf<NextPlayerState>(_gameManager.CurrentState);
        }

        [Test]
        public void NextPlayer_PlayerReachesMeta_TransitionsToGameOver()
        {
            // Arrange
            _gameManager.TransitionTo(new SetupState(_gameManager));
            TickFSM(); // Entrar a PlayerTurnState
            
            IPlayer activePlayer = _gameManager.Players[_gameManager.CurrentPlayerIndex];
            // Mover el jugador hasta la casilla meta (posición >= 20)
            activePlayer.MoveToPosition(20);

            // Act
            _gameManager.TransitionTo(new NextPlayerState(_gameManager));

            // Assert
            Assert.IsInstanceOf<GameOverState>(_gameManager.CurrentState, "Debería transicionar a GameOverState al alcanzar la meta.");
        }

        [Test]
        public void NextPlayer_AllPlayersComplete_TransitionsToGmChallenge()
        {
            // Arrange
            _gameManager.TransitionTo(new SetupState(_gameManager));
            TickFSM(); // Entrar a PlayerTurnState

            // Establecer el índice en el último jugador de la lista
            _gameManager.CurrentPlayerIndex = _gameManager.Players.Count - 1;

            // Act
            _gameManager.TransitionTo(new NextPlayerState(_gameManager));

            // Assert
            Assert.IsInstanceOf<GmChallengeState>(_gameManager.CurrentState, "Debería transicionar a GmChallengeState al terminar la ronda.");
            Assert.AreEqual(0, _gameManager.CurrentPlayerIndex, "El índice del jugador activo debe resetearse a 0.");
        }

        [Test]
        public void DuelState_PushPop_RestoresNormalFlow()
        {
            // Arrange
            var normalState = new SetupState(_gameManager);
            _gameManager.TransitionTo(normalState);

            var attacker = new TestPlayer("Attacker", 0);
            var defender = new TestPlayer("Defender", 1);
            var duelState = new DuelState(_gameManager, attacker, defender);

            // Act (Push Duelo)
            _gameManager.PushState(duelState);
            Assert.IsInstanceOf<DuelState>(_gameManager.CurrentState, "Debería estar en DuelState.");

            // Resolver duelo (se ejecuta PopState en el Tick)
            TickFSM();

            // Assert (Restaurar el estado normal)
            Assert.IsInstanceOf<SetupState>(_gameManager.CurrentState, "Debería haber retornado al SetupState anterior.");
            Assert.AreEqual(0, _gameManager.StateStack.Count, "El stack de estados debería quedar vacío.");
        }
    }
}
