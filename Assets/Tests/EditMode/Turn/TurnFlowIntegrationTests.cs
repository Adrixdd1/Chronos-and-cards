using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Core.States;
using ChronosAndCards.Data;
using ChronosAndCards.Gameplay;
using ChronosAndCards.Gameplay.Dice;
using ChronosAndCards.Gameplay.Board;

namespace ChronosAndCards.Tests.Turn
{
    [TestFixture]
    public class TurnFlowIntegrationTests
    {
        private GameObject _gameObject;
        private GameManager _gameManager;
        private BoardManager _boardManager;
        private ContentManager _contentManager;
        private DicePhysics _dicePhysics;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("IntegrationTestObject");
            _gameManager = _gameObject.AddComponent<GameManager>();
            _boardManager = _gameObject.AddComponent<BoardManager>();
            _contentManager = _gameObject.AddComponent<ContentManager>();
            _dicePhysics = _gameObject.AddComponent<DicePhysics>();

            // Configurar DiceConfig en el DicePhysics mediante reflexión
            var diceConfig = ScriptableObject.CreateInstance<DiceConfig>();
            diceConfig.AnimationTimeout = 2f;
            diceConfig.LaunchHeight = 5f;
            diceConfig.LaunchForce = 5f;
            diceConfig.TorqueForce = 5f;
            diceConfig.MaxRollDuration = 1f;
            diceConfig.SettleSpeed = 10f;
            diceConfig.StopThreshold = 0.1f;

            typeof(DicePhysics).GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_dicePhysics, diceConfig);
            var rb = _gameObject.AddComponent<Rigidbody>();
            typeof(DicePhysics).GetField("_rigidbody", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_dicePhysics, rb);

            // Asignar managers en el GameManager mediante reflexión
            typeof(GameManager).GetField("_boardManager", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_gameManager, _boardManager);
            typeof(GameManager).GetField("_contentManager", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_gameManager, _contentManager);
            typeof(GameManager).GetField("_dicePhysics", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_gameManager, _dicePhysics);

            // Forzar el Awake de GameManager para que cree las instancias de lógica
            MethodInfo awakeMethod = typeof(GameManager).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awakeMethod.Invoke(_gameManager, null);

            // Añadir un jugador de prueba
            var player = new Gameplay.Player("TestPlayer", 0, 3);
            _gameManager.Players.Add(player);

            GameEvents.ClearAll();
        }

        [TearDown]
        public void TearDown()
        {
            GameEvents.ClearAll();
            UnityEngine.Object.DestroyImmediate(_gameObject);
        }

        private void TickFSM()
        {
            MethodInfo updateMethod = typeof(GameManager).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
            updateMethod.Invoke(_gameManager, null);
        }

        [Test]
        public void FullTurn_DiceRollToTileEffect_CompletesSuccessfully()
        {
            // Registrar eventos para verificar su disparo
            bool diceRolledFired = false;
            bool cardDrawnFired = false;
            bool questionResolvedFired = false;
            bool playerMovedFired = false;

            GameEvents.OnDiceRolled += (res) => diceRolledFired = true;
            GameEvents.OnCardDrawn += (card) => cardDrawnFired = true;
            GameEvents.OnQuestionResolved += (p, mult) => questionResolvedFired = true;
            GameEvents.OnPlayerMoved += (p, orig, dest) => playerMovedFired = true;

            // 1. Inicializar FSM en PlayerTurnState
            _gameManager.TransitionTo(new PlayerTurnState(_gameManager));
            Assert.IsInstanceOf<PlayerTurnState>(_gameManager.CurrentState);

            // 2. Transicionar a DiceRollState
            TickFSM();
            Assert.IsInstanceOf<DiceRollState>(_gameManager.CurrentState);
            Assert.IsTrue(diceRolledFired, "Evento OnDiceRolled debería haberse disparado.");

            // Simulamos que la animación del dado se completa de inmediato
            // Para eso, como no tenemos simulación física de Rigidbody corriendo en EditMode,
            // podemos simular que el evento OnDiceAnimationComplete del DicePhysics se dispara
            var diceRollState = _gameManager.CurrentState as DiceRollState;
            typeof(DiceRollState).GetField("_animationComplete", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(diceRollState, true);

            // 3. Transicionar a CardDrawState
            TickFSM();
            Assert.IsInstanceOf<CardDrawState>(_gameManager.CurrentState);
            Assert.IsTrue(cardDrawnFired, "Evento OnCardDrawn debería haberse disparado.");

            // 4. Transicionar a ResolutionState
            TickFSM();
            Assert.IsInstanceOf<ResolutionState>(_gameManager.CurrentState);

            // 5. En ResolutionState, simular respuesta enviada vía evento
            GameEvents.OnAnswerSubmitted?.Invoke("Correcta");

            // 6. Transicionar a MovementState
            TickFSM();
            Assert.IsInstanceOf<MovementState>(_gameManager.CurrentState);
            Assert.IsTrue(questionResolvedFired, "Evento OnQuestionResolved debería haberse disparado.");
            Assert.IsTrue(playerMovedFired, "Evento OnPlayerMoved debería haberse disparado.");

            // 7. Transicionar a TileEffectState
            TickFSM();
            Assert.IsInstanceOf<TileEffectState>(_gameManager.CurrentState);
        }

        [Test]
        public void DiceRollState_Timeout_ForcesTransition()
        {
            // Forzar entrada a DiceRollState
            _gameManager.TransitionTo(new DiceRollState(_gameManager, _gameManager.DiceLogic, _gameManager.DicePhysics));
            Assert.IsInstanceOf<DiceRollState>(_gameManager.CurrentState);

            var diceRollState = _gameManager.CurrentState as DiceRollState;
            
            // Simular paso del tiempo configurando _elapsedTime a un valor superior al timeout
            float timeout = _gameManager.DicePhysics.AnimationTimeout;
            typeof(DiceRollState).GetField("_elapsedTime", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(diceRollState, timeout + 1f);

            // Tick de la FSM
            TickFSM();

            // Debe forzar la transición a CardDrawState debido al timeout
            Assert.IsInstanceOf<CardDrawState>(_gameManager.CurrentState);
        }

        [Test]
        public void ResolutionState_CorrectAnswer_PerfectMultiplier()
        {
            // Setup del turno
            _gameManager.TurnContext.Reset();
            _gameManager.TurnContext.CurrentCard = new CardData(1, "Pregunta?", "Pista", null, "RespuestaCorrecta");
            
            _gameManager.TransitionTo(new ResolutionState(_gameManager, _gameManager.TurnContext));
            Assert.IsInstanceOf<ResolutionState>(_gameManager.CurrentState);

            // Enviar respuesta correcta
            GameEvents.OnAnswerSubmitted?.Invoke("RespuestaCorrecta");
            
            Assert.AreEqual(PerformanceMultiplier.Perfect, _gameManager.TurnContext.PerformanceResult);
        }

        [Test]
        public void ResolutionState_CorrectWithHint_WithHelpMultiplier()
        {
            // Setup del turno con pista usada
            _gameManager.TurnContext.Reset();
            _gameManager.TurnContext.CurrentCard = new CardData(1, "Pregunta?", "Pista", null, "RespuestaCorrecta");
            _gameManager.TurnContext.UsedHint = true;
            
            _gameManager.TransitionTo(new ResolutionState(_gameManager, _gameManager.TurnContext));
            Assert.IsInstanceOf<ResolutionState>(_gameManager.CurrentState);

            // Enviar respuesta correcta
            GameEvents.OnAnswerSubmitted?.Invoke("  RespuestaCorrecta  "); // Con espacios para probar trim

            Assert.AreEqual(PerformanceMultiplier.WithHelp, _gameManager.TurnContext.PerformanceResult);
        }

        [Test]
        public void ResolutionState_WrongAnswer_FailMultiplier()
        {
            // Setup del turno
            _gameManager.TurnContext.Reset();
            _gameManager.TurnContext.CurrentCard = new CardData(1, "Pregunta?", "Pista", null, "RespuestaCorrecta");
            
            _gameManager.TransitionTo(new ResolutionState(_gameManager, _gameManager.TurnContext));
            Assert.IsInstanceOf<ResolutionState>(_gameManager.CurrentState);

            // Enviar respuesta incorrecta
            GameEvents.OnAnswerSubmitted?.Invoke("RespuestaIncorrecta");

            Assert.AreEqual(PerformanceMultiplier.Fail, _gameManager.TurnContext.PerformanceResult);
        }

        [Test]
        public void MovementState_CalculatesAndMovesCorrectly()
        {
            var player = _gameManager.Players[0];
            player.MoveToPosition(0);

            _gameManager.TurnContext.Reset();
            _gameManager.TurnContext.ActivePlayer = player;
            _gameManager.TurnContext.DiceValue = 4;
            _gameManager.TurnContext.PerformanceResult = PerformanceMultiplier.Perfect;

            // Transicionar a MovementState
            _gameManager.TransitionTo(new MovementState(_gameManager, _gameManager.AdvanceCalculator, _gameManager.BoardManager));
            Assert.IsInstanceOf<MovementState>(_gameManager.CurrentState);

            // Verificar que se calculó el movimiento correcto
            Assert.AreEqual(4, _gameManager.TurnContext.TilesToMove);
            Assert.AreEqual(4, player.Position);
        }
    }
}
