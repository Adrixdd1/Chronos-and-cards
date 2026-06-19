using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Core.States;
using ChronosAndCards.Data;
using ChronosAndCards.Interfaces;
using ChronosAndCards.Gameplay.GmChallenge;
using ChronosAndCards.Gameplay.GmChallenge.Rewards;

namespace ChronosAndCards.Tests.GmChallenge
{
    [TestFixture]
    public class GmChallengeStateTests
    {
        private GameManager _gameManager;
        private GameObject _go;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject();
            _gameManager = _go.AddComponent<GameManager>();
            // Since it requires ContentManager and FirstToPressManager internally,
            // we should ideally mock them or provide minimal setup.
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_go);
            GameEvents.ClearAll();
        }

        [Test]
        public void Enter_WithoutContentManager_SkipsChallenge()
        {
            var state = new GmChallengeState(_gameManager);
            
            bool skippedEventFired = false;
            GameEvents.OnGmChallengeSkipped += () => skippedEventFired = true;

            state.Enter();

            Assert.IsTrue(skippedEventFired);
        }
    }
}
