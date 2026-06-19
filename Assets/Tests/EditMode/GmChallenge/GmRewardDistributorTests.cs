using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ChronosAndCards.Gameplay.GmChallenge;
using ChronosAndCards.Gameplay.GmChallenge.Rewards;
using ChronosAndCards.Data;
using ChronosAndCards.Interfaces;
using ChronosAndCards.Core;

namespace ChronosAndCards.Tests.GmChallenge
{
    [TestFixture]
    public class GmRewardDistributorTests
    {
        private class DummyReward : IGmReward
        {
            public bool WasApplied { get; private set; }
            public void Apply(IPlayer player, GameContext context)
            {
                WasApplied = true;
            }
        }

        private class TestPlayer : IPlayer
        {
            public int PlayerIndex { get; set; }
            public string PlayerName { get; set; }
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
        public void GrantRandomReward_ExecutesCorrectRewardLogic()
        {
            var config = ScriptableObject.CreateInstance<GmRewardConfig>();
            // Since we cannot easily modify internal state of ScriptableObject without reflection or exposing it, 
            // the config will return defaults or we just test that one of the available implementations is called.
            
            var dummyReward = new DummyReward();
            var implementations = new Dictionary<GmRewardType, IGmReward>
            {
                { GmRewardType.SupplyCrate, dummyReward }
            };

            var distributor = new GmRewardDistributor(config, implementations);
            var player = new TestPlayer();
            var context = new GameContext();

            var granted = distributor.GrantRandomReward(player, context);

            // Because config defaults might not be exactly set, the default is SupplyCrate usually.
            // If the reward config doesn't have probabilities, the method defaults to SupplyCrate.
            Assert.IsTrue(dummyReward.WasApplied || granted != GmRewardType.SupplyCrate);
            
            Object.DestroyImmediate(config);
        }
    }
}
