using NUnit.Framework;
using UnityEngine;
using ChronosAndCards.Gameplay.Board;
using ChronosAndCards.Core;
using ChronosAndCards.Interfaces;
using ChronosAndCards.Data;

namespace ChronosAndCards.Tests
{
    [TestFixture]
    public class TileEffectTests
    {
        private TestPlayer _player;
        private MockItemDeck _itemDeck;

        [SetUp]
        public void SetUp()
        {
            _player = new TestPlayer("Test Player", 0);
            _itemDeck = new MockItemDeck();
            GameEvents.ClearAll();
        }

        [TearDown]
        public void TearDown()
        {
            GameEvents.ClearAll();
        }

        [Test]
        public void NeutralTile_OnPlayerLanded_NoSideEffect()
        {
            // Arrange
            var tile = new NeutralTile(3);
            bool eventFired = false;
            GameEvents.OnTileEffectApplied += (p, type) =>
            {
                if (p == _player && type == TileType.Neutral) eventFired = true;
            };

            // Act
            tile.OnPlayerLanded(_player);

            // Assert
            Assert.AreEqual(0, _player.HintCount, "El jugador no debe ganar pistas.");
            Assert.AreEqual(0, _player.Inventory.Count, "El jugador no debe ganar objetos.");
            Assert.IsTrue(eventFired, "Debería dispararse OnTileEffectApplied.");
        }

        [Test]
        public void HintBoostTile_OnPlayerLanded_IncreasesHintByOne()
        {
            // Arrange
            var tile = new HintBoostTile(5);
            bool eventFired = false;
            GameEvents.OnHintChanged += (p, count) =>
            {
                if (p == _player && count == 1) eventFired = true;
            };

            // Act
            tile.OnPlayerLanded(_player);

            // Assert
            Assert.AreEqual(1, _player.HintCount, "El saldo de pistas del jugador debería ser 1.");
            Assert.IsTrue(eventFired, "Debería dispararse OnHintChanged.");
        }

        [Test]
        public void HintTrapTile_OnPlayerLanded_DecreasesHintByOne()
        {
            // Arrange
            _player.AddHint(3); // Saldo inicial = 3
            var tile = new HintTrapTile(6);
            bool eventFired = false;
            GameEvents.OnHintChanged += (p, count) =>
            {
                if (p == _player && count == 2) eventFired = true;
            };

            // Act
            tile.OnPlayerLanded(_player);

            // Assert
            Assert.AreEqual(2, _player.HintCount, "El saldo de pistas del jugador debería ser 2.");
            Assert.IsTrue(eventFired, "Debería dispararse OnHintChanged.");
        }

        [Test]
        public void HintTrapTile_OnPlayerLanded_DoesNotGoBelowZero()
        {
            // Arrange
            var tile = new HintTrapTile(7);

            // Act
            tile.OnPlayerLanded(_player);

            // Assert
            Assert.AreEqual(0, _player.HintCount, "El saldo de pistas no debe ser menor a 0.");
        }

        [Test]
        public void ItemTile_OnPlayerLanded_AddsItemToInventory()
        {
            // Arrange
            _itemDeck.IsEmpty = false;
            var tile = new ItemTile(8, _itemDeck);

            bool inventoryEventFired = false;
            GameEvents.OnInventoryChanged += (p, item, action) =>
            {
                if (p == _player && item.Name == "Speed Card" && action == InventoryAction.Added)
                {
                    inventoryEventFired = true;
                }
            };

            // Act
            tile.OnPlayerLanded(_player);

            // Assert
            Assert.AreEqual(1, _player.Inventory.Count, "El jugador debe recibir 1 item en su inventario.");
            Assert.AreEqual("Speed Card", _player.Inventory.Items[0].Name);
            Assert.IsTrue(inventoryEventFired, "Debería dispararse OnInventoryChanged.");
        }

        [Test]
        public void ItemTile_OnPlayerLanded_EmptyDeck_NoItem()
        {
            // Arrange
            _itemDeck.IsEmpty = true;
            var tile = new ItemTile(9, _itemDeck);

            // Act
            tile.OnPlayerLanded(_player);

            // Assert
            Assert.AreEqual(0, _player.Inventory.Count, "El jugador no debe recibir ningún item si el mazo está vacío.");
        }

        [Test]
        public void EventTile_OnPlayerLanded_AppliesModifier()
        {
            // Arrange
            var eventConfig = ScriptableObject.CreateInstance<TileEventConfig>();
            eventConfig.EventName = "Brisa Favorable";
            eventConfig.Description = "Avanzas 3 casillas extra.";
            eventConfig.EventType = TileEventType.ExtraMovement;
            eventConfig.DurationInTurns = 0;
            eventConfig.ModifierValue = 3;

            var tile = new EventTile(10, eventConfig);

            bool eventFired = false;
            GameEvents.OnTileEffectApplied += (p, type) =>
            {
                if (p == _player && type == TileType.Event) eventFired = true;
            };

            // Act
            tile.OnPlayerLanded(_player);

            // Assert
            Assert.AreEqual(eventConfig, tile.EventConfig);
            Assert.IsTrue(eventFired, "Debería dispararse OnTileEffectApplied.");
        }

        // --- Mock Classes ---
        private class MockItemDeck : IItemDeck
        {
            public bool IsEmpty { get; set; }

            public IItem DrawItem()
            {
                if (IsEmpty) return null;
                return new MockItem();
            }

            public int RemainingItems => IsEmpty ? 0 : 5;
        }

        private class MockItem : IItem
        {
            public string Name => "Speed Card";
            public string Description => "Ganas movimiento extra.";
            public ItemActivationPhase Phase => ItemActivationPhase.None;
            public UnityEngine.Sprite Icon => null;
            public IItemEffect Effect => null;
        }
    }
}
