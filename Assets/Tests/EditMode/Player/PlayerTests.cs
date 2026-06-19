using System.Collections.Generic;
using NUnit.Framework;
using ChronosAndCards.Core;
using ChronosAndCards.Interfaces;
using ChronosAndCards.Gameplay;

namespace ChronosAndCards.Tests.Player
{
    [TestFixture]
    public class PlayerTests
    {
        private class TestItem : IItem
        {
            public string Name { get; set; } = "TestItem";
            public string Description { get; set; } = "Test Description";
            public ItemActivationPhase Phase { get; set; } = ItemActivationPhase.None;
            public UnityEngine.Sprite Icon { get; set; } = null;
            public IItemEffect Effect { get; set; } = null;
        }

        private class TestStatusEffect : IStatusEffect
        {
            public string Name { get; set; } = "TestEffect";
            public int RemainingTurns { get; set; } = 3;
        }

        [Test]
        public void MoveForward_IncreasesPosition()
        {
            var player = new Gameplay.Player("Player1", 0, 3);
            Assert.AreEqual(0, player.Position);
            
            player.MoveForward(5);
            Assert.AreEqual(5, player.Position);
        }

        [Test]
        public void MoveForward_NegativeSteps_DoesNotGoBelowZero()
        {
            var player = new Gameplay.Player("Player1", 0, 3);
            player.MoveForward(5);
            
            player.MoveForward(-10);
            Assert.AreEqual(0, player.Position);
        }

        [Test]
        public void AddHint_PositiveAmount_IncreasesCount()
        {
            var player = new Gameplay.Player("Player1", 0, 3);
            player.AddHint(2);
            Assert.AreEqual(5, player.HintCount);
        }

        [Test]
        public void AddHint_NegativeAmount_DecreasesCount()
        {
            var player = new Gameplay.Player("Player1", 0, 3);
            player.AddHint(-2);
            Assert.AreEqual(1, player.HintCount);
        }

        [Test]
        public void AddHint_NegativeBeyondZero_ClampsToZero()
        {
            var player = new Gameplay.Player("Player1", 0, 3);
            player.AddHint(-5);
            Assert.AreEqual(0, player.HintCount);
        }

        [Test]
        public void AddItem_AddsToInventory()
        {
            var player = new Gameplay.Player("Player1", 0, 3);
            var item = new TestItem { Name = "Hourglass" };
            
            player.AddItem(item);
            
            Assert.AreEqual(1, player.Inventory.Count);
            Assert.AreEqual("Hourglass", player.Inventory.Items[0].Name);
        }

        [Test]
        public void RemoveItem_RemovesFromInventory()
        {
            var player = new Gameplay.Player("Player1", 0, 3);
            var item = new TestItem { Name = "Hourglass" };
            
            player.AddItem(item);
            Assert.AreEqual(1, player.Inventory.Count);
            
            player.RemoveItem(item);
            Assert.AreEqual(0, player.Inventory.Count);
        }

        [Test]
        public void Inventory_ReturnedAsReadOnly()
        {
            var player = new Gameplay.Player("Player1", 0, 3);
            Assert.IsInstanceOf<IReadOnlyList<IItem>>(player.Inventory);
        }

        [Test]
        public void StatusEffects_CanBeAddedAndRemoved()
        {
            var player = new Gameplay.Player("Player1", 0, 3);
            var effect = new TestStatusEffect { Name = "Stun" };

            Assert.IsFalse(player.HasStatusEffect<TestStatusEffect>());
            
            player.AddStatusEffect(effect);
            Assert.IsTrue(player.HasStatusEffect<TestStatusEffect>());
            Assert.AreEqual(1, player.ActiveEffects.Count);

            player.RemoveStatusEffect(effect);
            Assert.IsFalse(player.HasStatusEffect<TestStatusEffect>());
        }
    }
}
