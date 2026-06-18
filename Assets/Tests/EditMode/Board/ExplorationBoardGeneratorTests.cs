using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ChronosAndCards.Gameplay.Board;
using ChronosAndCards.Interfaces;
using ChronosAndCards.Data;

namespace ChronosAndCards.Tests
{
    [TestFixture]
    public class ExplorationBoardGeneratorTests
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
        public void Generate_HasPathFromStartToEnd()
        {
            // Arrange
            var config = ScriptableObject.CreateInstance<BoardConfig>();
            config.Mode = BoardMode.Exploration;
            config.TargetQuestionCount = 12;

            var generator = new ExplorationBoardGenerator(_eventPool, _itemDeck);

            // Act
            generator.GenerateBoard(config);
            var graph = generator.GeneratedGraph;

            // Assert
            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.HasPathToEnd(), "El grafo de exploración debe garantizar un camino viable desde Inicio hasta Meta.");
        }

        [Test]
        public void Generate_NoGraphCycles()
        {
            // Arrange
            var config = ScriptableObject.CreateInstance<BoardConfig>();
            config.Mode = BoardMode.Exploration;
            config.TargetQuestionCount = 15;

            var generator = new ExplorationBoardGenerator(_eventPool, _itemDeck);

            // Act
            generator.GenerateBoard(config);
            var graph = generator.GeneratedGraph;

            // Assert
            Assert.IsNotNull(graph);
            Assert.IsFalse(HasCycles(graph), "El grafo de exploración generado no debe contener ciclos (debe ser un DAG).");
        }

        [Test]
        public void Generate_BranchesWithinConfigRange()
        {
            // Arrange
            var config = ScriptableObject.CreateInstance<BoardConfig>();
            config.Mode = BoardMode.Exploration;
            config.MinBranches = 1;
            config.MaxBranches = 2;
            config.TargetQuestionCount = 15;

            var generator = new ExplorationBoardGenerator(_eventPool, _itemDeck);

            // Act
            generator.GenerateBoard(config);
            var graph = generator.GeneratedGraph;

            // Assert
            foreach (var node in graph.GetAllNodes())
            {
                // El nodo Meta (EndNode) tiene out-degree 0 por definición. El resto debe estar en rango
                if (node == graph.EndNode)
                {
                    Assert.AreEqual(0, graph.GetOutDegree(node));
                }
                else
                {
                    int outDegree = graph.GetOutDegree(node);
                    Assert.GreaterOrEqual(outDegree, config.MinBranches, $"El nodo {((BaseTile)node).Index} tiene menos ramas de salida que el mínimo configurado.");
                    Assert.LessOrEqual(outDegree, config.MaxBranches, $"El nodo {((BaseTile)node).Index} tiene más ramas de salida que el máximo configurado.");
                }
            }
        }

        [Test]
        public void Generate_SameSeed_SameResult()
        {
            // Arrange
            var config1 = ScriptableObject.CreateInstance<BoardConfig>();
            config1.Mode = BoardMode.Exploration;
            config1.RandomSeed = 777;

            var config2 = ScriptableObject.CreateInstance<BoardConfig>();
            config2.Mode = BoardMode.Exploration;
            config2.RandomSeed = 777;

            var generator = new ExplorationBoardGenerator(_eventPool, _itemDeck);

            // Act
            var board1 = generator.GenerateBoard(config1);
            var board2 = generator.GenerateBoard(config2);

            // Assert
            Assert.AreEqual(board1.Count, board2.Count);
            for (int i = 0; i < board1.Count; i++)
            {
                Assert.AreEqual(board1[i].Type, board2[i].Type, "El tipo de las casillas en la misma posición debería coincidir con la misma semilla.");
            }
        }

        [Test]
        public void Generate_OnlyStartAndAdjacentDiscovered()
        {
            // Arrange
            var config = ScriptableObject.CreateInstance<BoardConfig>();
            config.Mode = BoardMode.Exploration;
            config.TargetQuestionCount = 12;

            var generator = new ExplorationBoardGenerator(_eventPool, _itemDeck);

            // Act
            generator.GenerateBoard(config);
            var graph = generator.GeneratedGraph;

            // Assert
            var adjacentToStart = new HashSet<ITile>(graph.GetNeighbors(graph.StartNode));

            foreach (var node in graph.GetAllNodes())
            {
                BaseTile baseTile = node as BaseTile;
                Assert.IsNotNull(baseTile);

                if (node == graph.StartNode || adjacentToStart.Contains(node))
                {
                    Assert.IsTrue(baseTile.IsDiscovered, $"El nodo {baseTile.Index} (inicio o adyacente a inicio) debe estar descubierto por defecto.");
                }
                else
                {
                    Assert.IsFalse(baseTile.IsDiscovered, $"El nodo {baseTile.Index} (lejano al inicio) no debe estar descubierto inicialmente.");
                }
            }
        }

        // --- Helper de Detección de Ciclos (DFS) ---
        private bool HasCycles(BoardGraph graph)
        {
            var visited = new HashSet<ITile>();
            var recStack = new HashSet<ITile>();

            foreach (var node in graph.GetAllNodes())
            {
                if (HasCyclesDFS(node, graph, visited, recStack))
                    return true;
            }
            return false;
        }

        private bool HasCyclesDFS(ITile node, BoardGraph graph, HashSet<ITile> visited, HashSet<ITile> recStack)
        {
            if (recStack.Contains(node)) return true;
            if (visited.Contains(node)) return false;

            visited.Add(node);
            recStack.Add(node);

            foreach (var neighbor in graph.GetNeighbors(node))
            {
                if (HasCyclesDFS(neighbor, graph, visited, recStack))
                    return true;
            }

            recStack.Remove(node);
            return false;
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
