using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using ChronosAndCards.Gameplay.Board;
using ChronosAndCards.Core;
using ChronosAndCards.Interfaces;
using ChronosAndCards.Data;

namespace ChronosAndCards.Tests
{
    [TestFixture]
    public class BoardManagerTests
    {
        private GameObject _gameObject;
        private BoardManager _boardManager;
        private BoardConfig _boardConfig;
        private List<TileEventConfig> _eventPool;
        private TestPlayer _player;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("BoardManagerTestObject");
            _boardManager = _gameObject.AddComponent<BoardManager>();

            _boardConfig = ScriptableObject.CreateInstance<BoardConfig>();
            _eventPool = new List<TileEventConfig>();
            _player = new TestPlayer("Test Player", 0);

            // Inyectar campos serializados privados vía reflexión
            typeof(BoardManager).GetField("_boardConfig", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_boardManager, _boardConfig);
            typeof(BoardManager).GetField("_eventPool", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_boardManager, _eventPool);

            GameEvents.ClearAll();
        }

        [TearDown]
        public void TearDown()
        {
            GameEvents.ClearAll();
            Object.DestroyImmediate(_gameObject);
            Object.DestroyImmediate(_boardConfig);
        }

        [Test]
        public void Initialize_LinearMode_GeneratesLinearBoard()
        {
            // Arrange
            _boardConfig.Mode = BoardMode.Linear;
            _boardConfig.TargetQuestionCount = 10;

            // Act
            _boardManager.Initialize();

            // Assert
            Assert.AreEqual(BoardMode.Linear, _boardManager.CurrentMode);
            Assert.Greater(_boardManager.TotalTiles, 0);
            Assert.IsNull(_boardManager.BoardGraph);
        }

        [Test]
        public void Initialize_ExplorationMode_GeneratesGraph()
        {
            // Arrange
            _boardConfig.Mode = BoardMode.Exploration;
            _boardConfig.TargetQuestionCount = 12;

            // Act
            _boardManager.Initialize();

            // Assert
            Assert.AreEqual(BoardMode.Exploration, _boardManager.CurrentMode);
            Assert.Greater(_boardManager.TotalTiles, 0);
            Assert.IsNotNull(_boardManager.BoardGraph);
        }

        [Test]
        public void RegisterPlayer_PlacesAtStart()
        {
            // Arrange
            _boardConfig.Mode = BoardMode.Linear;
            _boardConfig.TargetQuestionCount = 10;
            _boardManager.Initialize();

            // Act
            _boardManager.RegisterPlayer(_player);

            // Assert
            Assert.AreEqual(0, _player.Position);
            Assert.AreEqual(_boardManager.GetTileAt(0), _boardManager.GetPlayerTile(_player));
        }

        [Test]
        public void MovePlayer_Linear_UpdatesPosition()
        {
            // Arrange
            _boardConfig.Mode = BoardMode.Linear;
            _boardConfig.TargetQuestionCount = 10;
            _boardManager.Initialize();
            _boardManager.RegisterPlayer(_player);

            bool moveEventFired = false;
            GameEvents.OnPlayerMoved += (p, from, to) =>
            {
                if (p == _player && from == 0 && to == 4) moveEventFired = true;
            };

            // Act
            _boardManager.MovePlayer(_player, 4);

            // Assert
            Assert.AreEqual(4, _player.Position);
            Assert.AreEqual(_boardManager.GetTileAt(4), _boardManager.GetPlayerTile(_player));
            Assert.IsTrue(moveEventFired);
        }

        [Test]
        public void MovePlayer_Linear_ClampsToMeta()
        {
            // Arrange
            _boardConfig.Mode = BoardMode.Linear;
            _boardConfig.TargetQuestionCount = 8; // ~21 casillas
            _boardManager.Initialize();
            _boardManager.RegisterPlayer(_player);

            int lastIndex = _boardManager.TotalTiles - 1;

            // Act (movimiento masivo para forzar clamp)
            _boardManager.MovePlayer(_player, 99);

            // Assert
            Assert.AreEqual(lastIndex, _player.Position, "La posición final debe quedar acotada al índice de la meta.");
            Assert.AreEqual(_boardManager.GetTileAt(lastIndex), _boardManager.GetPlayerTile(_player));
        }

        [Test]
        public void MovePlayer_Exploration_EmitsPathChoiceOnBifurcation()
        {
            // Arrange
            // Creamos un grafo de exploración controlado con semilla fija que tenga una bifurcación conocida
            _boardConfig.Mode = BoardMode.Exploration;
            _boardConfig.MinBranches = 2; // Asegura bifurcaciones
            _boardConfig.MaxBranches = 3;
            _boardConfig.TargetQuestionCount = 10;
            _boardConfig.RandomSeed = 12345; 

            _boardManager.Initialize();
            _boardManager.RegisterPlayer(_player);

            bool choiceRequiredFired = false;
            IReadOnlyList<ITile> availableChoices = null;

            GameEvents.OnPathChoiceRequired += (p, options) =>
            {
                if (p == _player)
                {
                    choiceRequiredFired = true;
                    availableChoices = options;
                }
            };

            // Act (moverse a partir de la casilla de inicio)
            // Dado que el nodo de inicio de exploración conecta a múltiples ramas (mínimo 2 debido a config),
            // el primer paso ya debería disparar la bifurcación
            _boardManager.MovePlayer(_player, 2);

            // Assert
            Assert.IsTrue(_boardManager.IsMovementPending, "El movimiento debe marcarse como pendiente de decisión.");
            Assert.IsTrue(choiceRequiredFired, "Debería gatillarse el requerimiento de selección de camino.");
            Assert.IsNotNull(availableChoices);
            Assert.GreaterOrEqual(availableChoices.Count, 2, "Deberían presentarse al menos 2 opciones de caminos.");

            // Act 2 (el jugador selecciona un camino)
            ITile selectedTile = availableChoices[0];
            int expectedIndex = ((BaseTile)selectedTile).Index;
            GameEvents.OnPathChoiceSelected?.Invoke(_player, selectedTile);

            // Assert 2 (se completa el movimiento)
            Assert.IsFalse(_boardManager.IsMovementPending, "El movimiento ya no debe estar pendiente.");
            Assert.AreEqual(expectedIndex, _player.Position);
        }

        [Test]
        public void SwapTiles_AdjacentTiles_SwapsCorrectly()
        {
            // Arrange
            _boardConfig.Mode = BoardMode.Linear;
            _boardConfig.TargetQuestionCount = 10;
            _boardManager.Initialize();

            ITile tile1 = _boardManager.GetTileAt(2); // Casilla intermedia
            ITile tile2 = _boardManager.GetTileAt(3); // Casilla intermedia (adyacente)

            TileType type1 = tile1.Type;
            TileType type2 = tile2.Type;

            bool swapEventFired = false;
            GameEvents.OnBoardModified += (a, b) =>
            {
                if ((a == tile1 && b == tile2) || (a == tile2 && b == tile1)) swapEventFired = true;
            };

            // Act
            _boardManager.SwapTiles(tile1, tile2);

            // Assert
            Assert.AreEqual(tile2, _boardManager.GetTileAt(2), "La casilla en índice 2 debería ser ahora tile2.");
            Assert.AreEqual(tile1, _boardManager.GetTileAt(3), "La casilla en índice 3 debería ser ahora tile1.");
            Assert.IsTrue(swapEventFired);
        }

        [Test]
        public void SwapTiles_StartOrEndTile_Rejected()
        {
            // Arrange
            _boardConfig.Mode = BoardMode.Linear;
            _boardConfig.TargetQuestionCount = 10;
            _boardManager.Initialize();

            ITile startTile = _boardManager.GetTileAt(0); // Inicio
            ITile intermediateTile = _boardManager.GetTileAt(1); // Casilla intermedia

            // Act
            _boardManager.SwapTiles(startTile, intermediateTile);

            // Assert
            Assert.AreEqual(startTile, _boardManager.GetTileAt(0), "La casilla de inicio no debe haberse intercambiado.");
            Assert.AreEqual(intermediateTile, _boardManager.GetTileAt(1));
        }
    }
}
