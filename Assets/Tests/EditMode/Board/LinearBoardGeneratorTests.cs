using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ChronosAndCards.Gameplay.Board;
using ChronosAndCards.Interfaces;
using ChronosAndCards.Data;

namespace ChronosAndCards.Tests
{
    [TestFixture]
    public class LinearBoardGeneratorTests
    {
        private List<TileEventConfig> _eventPool;
        private IItemDeck _itemDeck;

        [SetUp]
        public void SetUp()
        {
            _eventPool = new List<TileEventConfig>();
            _itemDeck = new MockItemDeck();
        }

        [Test]
        public void Generate_CorrectTotalTiles_ForTargetQuestions()
        {
            // Arrange
            var config = ScriptableObject.CreateInstance<BoardConfig>();
            config.Mode = BoardMode.Linear;
            config.TargetQuestionCount = 10;
            // EstimatedTotalTiles: 10 * 3.5 * 0.75 = 26.25 -> 26
            int expectedTiles = config.EstimatedTotalTiles;

            var generator = new LinearBoardGenerator(_eventPool, _itemDeck);

            // Act
            var board = generator.GenerateBoard(config);

            // Assert
            Assert.AreEqual(expectedTiles, board.Count, "La longitud del tablero debe coincidir con el estimado de la configuración.");
        }

        [Test]
        public void Generate_FirstTileIsNeutral_LastTileIsNeutral()
        {
            // Arrange
            var config = ScriptableObject.CreateInstance<BoardConfig>();
            config.Mode = BoardMode.Linear;
            config.TargetQuestionCount = 12;

            var generator = new LinearBoardGenerator(_eventPool, _itemDeck);

            // Act
            var board = generator.GenerateBoard(config);

            // Assert
            Assert.AreEqual(TileType.Neutral, board[0].Type, "La primera casilla debe ser siempre Neutral.");
            Assert.AreEqual(TileType.Neutral, board[board.Count - 1].Type, "La última casilla (Meta) debe ser siempre Neutral.");
        }

        [Test]
        public void Generate_SameSeed_SameResult()
        {
            // Arrange
            var config1 = ScriptableObject.CreateInstance<BoardConfig>();
            config1.Mode = BoardMode.Linear;
            config1.RandomSeed = 999;

            var config2 = ScriptableObject.CreateInstance<BoardConfig>();
            config2.Mode = BoardMode.Linear;
            config2.RandomSeed = 999;

            var generator = new LinearBoardGenerator(_eventPool, _itemDeck);

            // Act
            var board1 = generator.GenerateBoard(config1);
            var board2 = generator.GenerateBoard(config2);

            // Assert
            Assert.AreEqual(board1.Count, board2.Count);
            for (int i = 0; i < board1.Count; i++)
            {
                Assert.AreEqual(board1[i].Type, board2[i].Type, $"La casilla en la posición {i} debería tener el mismo tipo con la misma semilla.");
            }
        }

        [Test]
        public void Generate_DifferentSeed_DifferentResult()
        {
            // Arrange
            var config1 = ScriptableObject.CreateInstance<BoardConfig>();
            config1.Mode = BoardMode.Linear;
            config1.RandomSeed = 1111;

            var config2 = ScriptableObject.CreateInstance<BoardConfig>();
            config2.Mode = BoardMode.Linear;
            config2.RandomSeed = 2222;

            var generator = new LinearBoardGenerator(_eventPool, _itemDeck);

            // Act
            var board1 = generator.GenerateBoard(config1);
            var board2 = generator.GenerateBoard(config2);

            // Assert
            bool differs = false;
            for (int i = 0; i < Mathf.Min(board1.Count, board2.Count); i++)
            {
                if (board1[i].Type != board2[i].Type)
                {
                    differs = true;
                    break;
                }
            }

            Assert.IsTrue(differs, "Tableros generados con diferentes semillas deberían diferir en la distribución de casillas.");
        }

        [Test]
        public void Generate_TileDistribution_ApproximatelyMatchesWeights()
        {
            // Arrange
            var config = ScriptableObject.CreateInstance<BoardConfig>();
            config.Mode = BoardMode.Linear;
            config.TargetQuestionCount = 400; // Generar una muestra grande (~1050 casillas)
            config.TileTypeWeights = new float[] { 0.40f, 0.20f, 0.10f, 0.15f, 0.15f };
            config.RandomSeed = 42;

            var generator = new LinearBoardGenerator(_eventPool, _itemDeck);

            // Act
            var board = generator.GenerateBoard(config);

            // Contar tipos de casillas intermedias (excluyendo inicio y fin que son siempre neutrales)
            int neutral = 0, hintBoost = 0, hintTrap = 0, eventTiles = 0, itemTiles = 0;
            for (int i = 1; i < board.Count - 1; i++)
            {
                switch (board[i].Type)
                {
                    case TileType.Neutral: neutral++; break;
                    case TileType.HintBoost: hintBoost++; break;
                    case TileType.HintTrap: hintTrap++; break;
                    case TileType.Event: eventTiles++; break;
                    case TileType.Item: itemTiles++; break;
                }
            }

            int totalIntermediates = board.Count - 2;

            float pNeutral = (float)neutral / totalIntermediates;
            float pHintBoost = (float)hintBoost / totalIntermediates;
            float pHintTrap = (float)hintTrap / totalIntermediates;
            float pEvent = (float)eventTiles / totalIntermediates;
            float pItem = (float)itemTiles / totalIntermediates;

            // Assert (tolerancia del 5% debido al azar controlado)
            Assert.AreEqual(0.40f, pNeutral, 0.05f, "La proporción de Neutrales debe aproximar al peso del 40%.");
            Assert.AreEqual(0.20f, pHintBoost, 0.05f, "La proporción de HintBoost debe aproximar al peso del 20%.");
            Assert.AreEqual(0.10f, pHintTrap, 0.05f, "La proporción de HintTrap debe aproximar al peso del 10%.");
            Assert.AreEqual(0.15f, pEvent, 0.05f, "La proporción de Eventos debe aproximar al peso del 15%.");
            Assert.AreEqual(0.15f, pItem, 0.05f, "La proporción de Items debe aproximar al peso del 15%.");
        }

        // --- Mock Classes ---
        private class MockItemDeck : IItemDeck
        {
            public IItem DrawItem() => null;
            public int RemainingItems => 10;
            public bool IsEmpty => false;
        }
    }
}
