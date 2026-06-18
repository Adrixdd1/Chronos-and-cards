using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Data;

namespace ChronosAndCards.Tests
{
    [TestFixture]
    public class GameManagerTests
    {
        private GameObject _gameObject;
        private GameManager _gameManager;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("GameManagerTestObject");
            _gameManager = _gameObject.AddComponent<GameManager>();
            
            // Limpiar eventos antes de cada test para evitar colisiones
            GameEvents.ClearAll();
        }

        [TearDown]
        public void TearDown()
        {
            GameEvents.ClearAll();
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void TransitionTo_CallsExitOnCurrentAndEnterOnNew()
        {
            // Arrange
            var state1 = new MockState();
            var state2 = new MockState();

            // Act
            _gameManager.TransitionTo(state1);
            Assert.IsTrue(state1.EnterCalled);
            Assert.IsFalse(state1.ExitCalled);

            _gameManager.TransitionTo(state2);

            // Assert
            Assert.IsTrue(state1.ExitCalled, "Exit debe ser llamado en el estado anterior al salir.");
            Assert.IsTrue(state2.EnterCalled, "Enter debe ser llamado en el nuevo estado al entrar.");
            Assert.AreEqual(state2, _gameManager.CurrentState, "El estado actual de la FSM debe ser el nuevo estado.");
        }

        [Test]
        public void PushState_StacksCurrentAndEntersInterrupt()
        {
            // Arrange
            var state1 = new MockState();
            var interruptState = new MockState();

            _gameManager.TransitionTo(state1);

            // Act
            _gameManager.PushState(interruptState);

            // Assert
            Assert.AreEqual(interruptState, _gameManager.CurrentState, "El estado actual debe ser el de interrupción.");
            Assert.AreEqual(1, _gameManager.StateStack.Count, "El stack debe tener guardado el estado original.");
            Assert.AreEqual(state1, _gameManager.StateStack.Peek(), "El estado guardado en el stack debe ser state1.");
            Assert.IsTrue(interruptState.EnterCalled, "El estado de interrupción debe ejecutar su Enter.");
            Assert.IsFalse(state1.ExitCalled, "El estado pausado NO debe ejecutar su Exit (se retoma donde quedó).");
        }

        [Test]
        public void PopState_ExitsInterruptAndRestoresPrevious()
        {
            // Arrange
            var state1 = new MockState();
            var interruptState = new MockState();

            _gameManager.TransitionTo(state1);
            _gameManager.PushState(interruptState);

            // Act
            _gameManager.PopState();

            // Assert
            Assert.AreEqual(state1, _gameManager.CurrentState, "El estado actual debe haber retornado a state1.");
            Assert.AreEqual(0, _gameManager.StateStack.Count, "El stack debe quedar vacío tras el pop.");
            Assert.IsTrue(interruptState.ExitCalled, "El estado de interrupción debe ejecutar su Exit al ser removido.");
            Assert.IsFalse(state1.EnterCalled && state1.ExitCalled, "El estado retornado no debe llamar a Enter() de nuevo.");
        }

        [Test]
        public void Tick_DelegatesToCurrentState()
        {
            // Arrange
            var state1 = new MockState();
            _gameManager.TransitionTo(state1);

            // Act
            // Invocar el método privado Update() de GameManager usando reflexión
            MethodInfo updateMethod = typeof(GameManager).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(updateMethod, "El método Update debe existir en GameManager.");
            updateMethod.Invoke(_gameManager, null);

            // Assert
            Assert.IsTrue(state1.TickCalled, "Update del GameManager debe delegar el Tick al estado activo.");
        }

        [Test]
        public void TransitionTo_EmitsOnStateChanged()
        {
            // Arrange
            var state1 = new MockState();
            IGameState emittedState = null;
            GameEvents.OnStateChanged += (state) => emittedState = state;

            // Act
            _gameManager.TransitionTo(state1);

            // Assert
            Assert.AreEqual(state1, emittedState, "El evento OnStateChanged debe emitir el estado ingresado.");
        }

        // --- Mock State Helper ---
        private class MockState : IGameState
        {
            public bool EnterCalled { get; private set; }
            public bool ExitCalled { get; private set; }
            public bool TickCalled { get; private set; }

            public void Enter() => EnterCalled = true;
            public void Tick() => TickCalled = true;
            public void Exit() => ExitCalled = true;
        }
    }
}
