using System.Collections.Generic;
using NUnit.Framework;
using ChronosAndCards.Gameplay.StatusEffects;
using ChronosAndCards.Interfaces;
using ChronosAndCards.Core;
using ChronosAndCards.Data;

namespace ChronosAndCards.Tests.GmChallenge
{
    [TestFixture]
    public class StatusEffectManagerTests
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

        private class TestStatusEffect : IStatusEffect
        {
            public string Name { get; set; } = "Test";
            public string Description { get; set; } = "Test Desc";
            public StatusEffectDuration Duration { get; set; } = StatusEffectDuration.SingleUse;
            public bool IsActive { get; set; } = true;
            public bool IsExpired { get; set; } = false;

            public bool ShouldActivate(TurnContext context) => true;
            public void ApplyToTurn(TurnContext context) {}
        }

        [Test]
        public void AddEffect_AddsEffectToPlayerAndInvokesEvent()
        {
            var manager = new StatusEffectManager();
            var player = new TestPlayer();
            var effect = new TestStatusEffect();

            bool eventFired = false;
            manager.OnEffectApplied += (p, e) => { eventFired = true; };

            manager.AddEffect(player, effect);

            Assert.IsTrue(eventFired);
            Assert.AreEqual(1, player.ActiveEffects.Count);
            Assert.AreEqual(effect, player.ActiveEffects[0]);
        }

        [Test]
        public void CleanupExpiredEffects_RemovesOnlyExpiredEffects()
        {
            var manager = new StatusEffectManager();
            var player = new TestPlayer();
            var activeEffect = new TestStatusEffect { IsExpired = false };
            var expiredEffect = new TestStatusEffect { IsExpired = true };

            player.AddStatusEffect(activeEffect);
            player.AddStatusEffect(expiredEffect);

            manager.CleanupExpiredEffects(player);

            Assert.AreEqual(1, player.ActiveEffects.Count);
            Assert.AreEqual(activeEffect, player.ActiveEffects[0]);
        }
    }
}
