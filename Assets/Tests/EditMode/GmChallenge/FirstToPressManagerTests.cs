using NUnit.Framework;
using UnityEngine;
using ChronosAndCards.Gameplay.GmChallenge;
using ChronosAndCards.Data;
using ChronosAndCards.Interfaces;
using System.Collections.Generic;

namespace ChronosAndCards.Tests.GmChallenge
{
    [TestFixture]
    public class FirstToPressManagerTests
    {
        private class TestPlayer : IPlayer
        {
            public int PlayerIndex { get; set; } = 0;
            public string PlayerName { get; set; } = "P1";
            public int Position { get; set; }
            public int HintCount { get; set; }
            public IInventory Inventory { get; set; }
            public List<IStatusEffect> ActiveEffects { get; } = new List<IStatusEffect>();

            public void MoveForward(int steps) {}
            public void AddHint(int amount) {}
            public void AddItem(IItem item) {}
            public void RemoveItem(IItem item) {}
            public void AddStatusEffect(IStatusEffect effect) { ActiveEffects.Add(effect); }
            public void RemoveStatusEffect(IStatusEffect effect) { ActiveEffects.Remove(effect); }
            public bool HasStatusEffect<T>() where T : IStatusEffect => false;
            public bool SkipNextTurn { get; private set; }
            public void SetSkipNextTurn(bool skip) { SkipNextTurn = skip; }
        }

        [Test]
        public void NotifyPlayerPressed_TriggersEventAndStopsListening()
        {
            var go = new GameObject();
            var ftp = go.AddComponent<FirstToPressManager>();
            var config = ScriptableObject.CreateInstance<GmChallengeConfig>();

            var p1 = new TestPlayer { PlayerIndex = 0 };
            var p2 = new TestPlayer { PlayerIndex = 1 };

            ftp.StartListening(new List<IPlayer> { p1, p2 }, config);

            IPlayer pressed = null;
            ftp.OnFirstPlayerPressed += p => pressed = p;

            ftp.NotifyPlayerPressed(p1);

            Assert.IsNotNull(pressed);
            Assert.AreEqual(p1, pressed);

            // Should not trigger again because it stopped listening
            ftp.NotifyPlayerPressed(p2);
            Assert.AreEqual(p1, pressed); // Still p1

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(config);
        }
    }
}
