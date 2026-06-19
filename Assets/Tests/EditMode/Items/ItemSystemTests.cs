using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Core.States;
using ChronosAndCards.Data;
using ChronosAndCards.Gameplay.Items;
using ChronosAndCards.Interfaces;

namespace ChronosAndCards.Tests.Items
{
    public class ItemSystemTests
    {
        private class DummyEffect : IItemEffect
        {
            public ItemActivationPhase ActivationPhase { get; set; } = ItemActivationPhase.BeforeAnswer;
            public bool IsOffensive { get; set; } = false;
            public bool CanBeActivated(IPlayer owner, GameContext context) => true;
            public void Execute(IPlayer owner, GameContext context) { }
        }

        private class DummyItem : IItem
        {
            public string Name { get; set; } = "Dummy Item";
            public string Description { get; set; } = "Test";
            public ItemActivationPhase Phase => Effect?.ActivationPhase ?? ItemActivationPhase.BeforeAnswer;
            public Sprite Icon { get; set; }
            public IItemEffect Effect { get; set; } = new DummyEffect();
        }

        [Test]
        public void PlayerInventory_AddItem_IncreasesCount()
        {
            var inventory = new PlayerInventory(2);
            var player = new TestPlayer("P1", 0);
            var item = new DummyItem();

            bool added = inventory.AddItem(player, item);

            Assert.IsTrue(added);
            Assert.AreEqual(1, inventory.Count);
            Assert.IsTrue(inventory.HasItem<DummyItem>());
        }

        [Test]
        public void PlayerInventory_CapacityLimit_BlocksAddition()
        {
            var inventory = new PlayerInventory(1);
            var player = new TestPlayer("P1", 0);
            var item1 = new DummyItem();
            var item2 = new DummyItem();

            Assert.IsTrue(inventory.AddItem(player, item1));
            Assert.IsFalse(inventory.AddItem(player, item2));
            Assert.AreEqual(1, inventory.Count);
        }

        [Test]
        public void ItemDeck_DrawItem_EmptiesAndReturnsNull_IfNoReshuffle()
        {
            var config = ScriptableObject.CreateInstance<ItemDeckConfig>();
            // No agregamos entradas, así que está vacío
            var deck = new ItemDeck(config);

            var item = deck.DrawItem();

            Assert.IsNull(item);
            Assert.IsTrue(deck.IsEmpty);
        }

        [Test]
        public void ItemEffectExecutor_RejectsWrongPhase()
        {
            var executor = new ItemEffectExecutor(null); // Sin ReactionWindowManager
            var item = new DummyItem { Effect = new DummyEffect { ActivationPhase = ItemActivationPhase.AfterFail } };
            var player = new TestPlayer("P1", 0);
            player.Inventory.AddItem(player, item);

            var context = new GameContext { CurrentPhase = ItemActivationPhase.BeforeAnswer }; // Fase distinta

            bool activated = executor.TryActivate(item, player, context);

            Assert.IsFalse(activated);
        }

        [Test]
        public void GameContext_PushPopSubTurn_WorksCorrectly()
        {
            var context = new GameContext();
            var dummyObject = new object();

            context.PushSubTurn(dummyObject);
            Assert.AreEqual(dummyObject, context.GetSubTurnContext<object>());

            context.PopSubTurn();
            Assert.IsNull(context.GetSubTurnContext<object>());
        }
    }
}
