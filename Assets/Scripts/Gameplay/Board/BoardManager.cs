using System;
using System.Collections.Generic;
using UnityEngine;
using ChronosAndCards.Interfaces;
using ChronosAndCards.Core;
using ChronosAndCards.Data;

namespace ChronosAndCards.Gameplay.Board
{
    /// <summary>
    /// Fachada principal para el sistema de tablero. Gestiona la inicialización,
    /// el seguimiento de posiciones de jugadores, la revelación progresiva de casillas,
    /// el movimiento lineal y de exploración, y la manipulación del tablero (SwapTiles).
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private BoardConfig _boardConfig;
        [SerializeField] private List<TileEventConfig> _eventPool;

        private List<ITile> _tiles = new List<ITile>();
        private BoardGraph _boardGraph;
        private Dictionary<IPlayer, ITile> _playerPositions = new Dictionary<IPlayer, ITile>();
        private IItemDeck _itemDeck;

        // Estructura para registrar movimientos pendientes en bifurcaciones
        private struct PendingMovement
        {
            public IPlayer Player;
            public int RemainingSteps;
        }
        private PendingMovement _pendingMovement;

        /// <summary>Total de casillas en el tablero.</summary>
        public int TotalTiles => _tiles.Count;

        /// <summary>Modo actual del tablero (Linear o Exploration).</summary>
        public BoardMode CurrentMode => _boardConfig != null ? _boardConfig.Mode : BoardMode.Linear;

        /// <summary>Casillas que componen el tablero.</summary>
        public IReadOnlyList<ITile> Tiles => _tiles.AsReadOnly();

        /// <summary>Acceso al grafo del tablero (solo en modo Exploración).</summary>
        public BoardGraph BoardGraph => _boardGraph;

        /// <summary>Indica si hay un movimiento de bifurcación pendiente de decisión.</summary>
        public bool IsMovementPending => _pendingMovement.Player != null;

        private void OnEnable()
        {
            // Escuchar la respuesta del jugador en una bifurcación
            GameEvents.OnPathChoiceSelected += OnPathChoiceSelectedHandler;
        }

        private void OnDisable()
        {
            GameEvents.OnPathChoiceSelected -= OnPathChoiceSelectedHandler;
        }

        /// <summary>
        /// Inicializa el tablero instanciando los generadores de acuerdo al modo.
        /// </summary>
        /// <param name="itemDeck">Mazo de objetos a inyectar en las casillas de items.</param>
        public void Initialize(IItemDeck itemDeck = null)
        {
            _itemDeck = itemDeck ?? new MockItemDeck();
            _playerPositions.Clear();
            _pendingMovement = default;

            if (_boardConfig == null)
            {
                Debug.LogError("BoardManager: BoardConfig no está asignado.");
                return;
            }

            IBoardGenerator generator;

            if (_boardConfig.Mode == BoardMode.Linear)
            {
                generator = new LinearBoardGenerator(_eventPool, _itemDeck);
                _tiles = generator.GenerateBoard(_boardConfig);
                _boardGraph = null;
            }
            else
            {
                var expGenerator = new ExplorationBoardGenerator(_eventPool, _itemDeck);
                generator = expGenerator;
                _tiles = generator.GenerateBoard(_boardConfig);
                _boardGraph = expGenerator.GeneratedGraph;
            }

            Debug.Log($"BoardManager: Inicializado en modo {_boardConfig.Mode} con {_tiles.Count} casillas.");
        }

        /// <summary>
        /// Registra a un jugador en el tablero ubicándolo en la casilla de inicio.
        /// </summary>
        public void RegisterPlayer(IPlayer player)
        {
            if (_tiles.Count == 0)
            {
                Debug.LogError("BoardManager: No se puede registrar al jugador porque el tablero está vacío.");
                return;
            }

            ITile startTile = _tiles[0];
            _playerPositions[player] = startTile;
            player.MoveToPosition(0);
        }

        /// <summary>
        /// Retorna la casilla en la posición/índice especificado.
        /// </summary>
        public ITile GetTileAt(int position)
        {
            if (position < 0 || position >= _tiles.Count)
            {
                throw new IndexOutOfRangeException($"La posición {position} está fuera de los límites del tablero (0-{_tiles.Count - 1}).");
            }
            return _tiles[position];
        }

        /// <summary>
        /// Comprueba si la posición representa la última casilla (Meta).
        /// </summary>
        public bool IsLastTile(int position)
        {
            return position == TotalTiles - 1;
        }

        /// <summary>
        /// Retorna la casilla en la que se encuentra actualmente el jugador.
        /// </summary>
        public ITile GetPlayerTile(IPlayer player)
        {
            if (_playerPositions.TryGetValue(player, out ITile tile))
            {
                return tile;
            }
            return null;
        }

        /// <summary>
        /// Retorna las casillas vecinas de salida en modo Exploración.
        /// </summary>
        public IReadOnlyList<ITile> GetAdjacentTiles(ITile current)
        {
            if (_boardGraph != null)
            {
                return _boardGraph.GetNeighbors(current);
            }
            return new List<ITile>().AsReadOnly();
        }

        /// <summary>
        /// Retorna si el nodo actual tiene más de una conexión de salida (bifurcación).
        /// </summary>
        public bool HasMultiplePaths(ITile current)
        {
            if (_boardGraph != null)
            {
                return _boardGraph.GetOutDegree(current) > 1;
            }
            return false;
        }

        /// <summary>
        /// Descubre una casilla progresivamente y emite un evento a la UI.
        /// </summary>
        public void DiscoverTile(ITile tile)
        {
            if (tile is BaseTile baseTile && !baseTile.IsDiscovered)
            {
                baseTile.IsDiscovered = true;
                GameEvents.OnTileDiscovered?.Invoke(tile);
            }
        }

        /// <summary>
        /// Realiza el movimiento del jugador un número determinado de casillas.
        /// </summary>
        public void MovePlayer(IPlayer player, int steps)
        {
            if (CurrentMode == BoardMode.Linear)
            {
                MovePlayerLinear(player, steps);
            }
            else
            {
                MovePlayerExploration(player, steps);
            }
        }

        private void MovePlayerLinear(IPlayer player, int steps)
        {
            int oldPos = player.Position;
            int newPos = Mathf.Clamp(player.Position + steps, 0, TotalTiles - 1);

            _playerPositions[player] = _tiles[newPos];
            player.MoveToPosition(newPos);

            GameEvents.OnPlayerMoved?.Invoke(player, oldPos, newPos);
        }

        private void MovePlayerExploration(IPlayer player, int steps)
        {
            if (steps <= 0) return;

            ITile currentTile = GetPlayerTile(player);
            if (currentTile == _tiles[TotalTiles - 1])
            {
                Debug.Log($"{player.PlayerName} ya está en la Meta.");
                return;
            }

            ContinueExplorationMovement(player, steps);
        }

        private void ContinueExplorationMovement(IPlayer player, int steps)
        {
            ITile currentTile = GetPlayerTile(player);
            IReadOnlyList<ITile> neighbors = GetAdjacentTiles(currentTile);

            if (neighbors.Count == 0)
            {
                // Nodo meta alcanzado (sin salidas)
                return;
            }

            if (neighbors.Count == 1)
            {
                // Avance automático
                ITile nextTile = neighbors[0];
                int oldPos = player.Position;
                int newPos = ((BaseTile)nextTile).Index;

                _playerPositions[player] = nextTile;
                player.MoveToPosition(newPos);

                // Descubrir adyacentes al nuevo nodo
                DiscoverAdjacentTiles(nextTile);

                int remaining = steps - 1;
                if (remaining > 0 && nextTile != _tiles[TotalTiles - 1])
                {
                    ContinueExplorationMovement(player, remaining);
                }
                else
                {
                    GameEvents.OnPlayerMoved?.Invoke(player, oldPos, newPos);
                }
            }
            else
            {
                // Bifurcación: pausar movimiento y solicitar decisión
                _pendingMovement = new PendingMovement { Player = player, RemainingSteps = steps };
                GameEvents.OnPathChoiceRequired?.Invoke(player, neighbors);
            }
        }

        private void OnPathChoiceSelectedHandler(IPlayer player, ITile selectedTile)
        {
            if (_pendingMovement.Player == player && _pendingMovement.RemainingSteps > 0)
            {
                int oldPos = player.Position;
                int newPos = ((BaseTile)selectedTile).Index;

                _playerPositions[player] = selectedTile;
                player.MoveToPosition(newPos);

                // Descubrir adyacentes al nuevo nodo
                DiscoverAdjacentTiles(selectedTile);

                int remaining = _pendingMovement.RemainingSteps - 1;
                _pendingMovement.RemainingSteps = remaining;

                if (remaining > 0 && selectedTile != _tiles[TotalTiles - 1])
                {
                    ContinueExplorationMovement(player, remaining);
                }
                else
                {
                    GameEvents.OnPlayerMoved?.Invoke(player, oldPos, newPos);
                    _pendingMovement = default;
                }
            }
        }

        private void DiscoverAdjacentTiles(ITile tile)
        {
            foreach (ITile neighbor in GetAdjacentTiles(tile))
            {
                DiscoverTile(neighbor);
            }
        }

        /// <summary>
        /// Intercambia las posiciones de dos casillas en el tablero bajo restricciones.
        /// </summary>
        public void SwapTiles(ITile a, ITile b)
        {
            if (a == null || b == null) return;

            BaseTile tileA = a as BaseTile;
            BaseTile tileB = b as BaseTile;

            if (tileA == null || tileB == null) return;

            // 1. Prohibir swap de Inicio (0) y Meta (Total-1)
            if (tileA.Index == 0 || tileA.Index == TotalTiles - 1 ||
                tileB.Index == 0 || tileB.Index == TotalTiles - 1)
            {
                Debug.LogWarning("BoardManager: No se puede hacer swap de la casilla de Inicio o de Meta.");
                return;
            }

            // 2. Validar restricciones de distancia/adyacencia
            if (CurrentMode == BoardMode.Linear)
            {
                if (Mathf.Abs(tileA.Index - tileB.Index) > 2)
                {
                    Debug.LogWarning("BoardManager: En modo lineal, los swaps sólo se permiten a una distancia <= 2.");
                    return;
                }
            }
            else
            {
                bool isAdjacent = false;
                foreach (var neighbor in GetAdjacentTiles(tileA))
                {
                    if (neighbor == tileB) { isAdjacent = true; break; }
                }
                if (!isAdjacent)
                {
                    foreach (var neighbor in GetAdjacentTiles(tileB))
                    {
                        if (neighbor == tileA) { isAdjacent = true; break; }
                    }
                }

                if (!isAdjacent)
                {
                    Debug.LogWarning("BoardManager: En modo exploración, los swaps sólo se permiten entre casillas adyacentes.");
                    return;
                }
            }

            // 3. Intercambiar casillas en la lista
            int idxA = _tiles.IndexOf(tileA);
            int idxB = _tiles.IndexOf(tileB);

            _tiles[idxA] = tileB;
            _tiles[idxB] = tileA;

            // 4. Actualizar tracking de jugadores que se encontraban en las casillas
            List<IPlayer> trackedPlayers = new List<IPlayer>(_playerPositions.Keys);
            foreach (IPlayer player in trackedPlayers)
            {
                if (_playerPositions[player] == tileA)
                {
                    _playerPositions[player] = tileB;
                    player.MoveToPosition(idxB);
                }
                else if (_playerPositions[player] == tileB)
                {
                    _playerPositions[player] = tileA;
                    player.MoveToPosition(idxA);
                }
            }

            // Emitir evento de modificación
            GameEvents.OnBoardModified?.Invoke(tileA, tileB);
            Debug.Log($"BoardManager: Intercambiadas casillas {tileA.Type} en index {idxA} y {tileB.Type} en index {idxB}.");
        }

        // --- Mock Item Deck Helper ---
        private class MockItemDeck : IItemDeck
        {
            private class MockItem : IItem
            {
                public string Name => "Mock Item";
                public string Description => "Item generado por stub de BoardManager.";
                public ChronosAndCards.Interfaces.ItemActivationPhase Phase => ChronosAndCards.Interfaces.ItemActivationPhase.BeforeAnswer;
                public UnityEngine.Sprite Icon => null;
                public ChronosAndCards.Interfaces.IItemEffect Effect => null;
            }

            public IItem DrawItem() => new MockItem();
            public int RemainingItems => 99;
            public bool IsEmpty => false;
        }
    }
}
