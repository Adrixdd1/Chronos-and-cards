using System;
using System.Collections.Generic;
using UnityEngine;
using ChronosAndCards.Interfaces;
using ChronosAndCards.Data;

namespace ChronosAndCards.Gameplay.Board
{
    /// <summary>
    /// Generador de tableros de exploración basado en un Grafo Dirigido Acíclico (DAG) por capas,
    /// asegurando que no existan bucles y que la meta sea siempre alcanzable.
    /// </summary>
    public class ExplorationBoardGenerator : IBoardGenerator
    {
        private readonly IReadOnlyList<TileEventConfig> _eventPool;
        private readonly IItemDeck _itemDeck;
        private const int MaxNodesPerLayer = 4;

        /// <summary>Grafo generado en la última llamada a GenerateBoard.</summary>
        public BoardGraph GeneratedGraph { get; private set; }

        /// <summary>
        /// Constructor que recibe el mazo y los eventos a inyectar en las casillas.
        /// </summary>
        public ExplorationBoardGenerator(IReadOnlyList<TileEventConfig> eventPool, IItemDeck itemDeck)
        {
            _eventPool = eventPool;
            _itemDeck = itemDeck;
        }

        public List<ITile> GenerateBoard(BoardConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config), "La configuración del tablero no puede ser nula.");
            }

            // Inicializar generador pseudo-aleatorio con semilla
            System.Random rng = config.RandomSeed != 0 ? new System.Random(config.RandomSeed) : new System.Random();

            // Estimar cantidad de capas intermedias
            int estimatedTiles = config.EstimatedTotalTiles;
            if (estimatedTiles < 5) estimatedTiles = 5;

            // Determinar cantidad de capas
            int intermediateLayersCount = Mathf.Clamp(estimatedTiles / 3, 3, 12);

            int currentIdx = 0;

            // 1. Crear nodo Start (Neutral)
            BaseTile startNode = new NeutralTile(currentIdx++);
            startNode.IsDiscovered = true; // El nodo de inicio siempre es descubierto

            List<List<BaseTile>> layers = new List<List<BaseTile>>();
            List<BaseTile> startLayer = new List<BaseTile> { startNode };
            layers.Add(startLayer);

            List<ITile> allNodesList = new List<ITile> { startNode };

            // 2. Crear capas intermedias
            for (int l = 1; l <= intermediateLayersCount; l++)
            {
                int numNodes = rng.Next(config.MinBranches, MaxNodesPerLayer + 1);
                List<BaseTile> currentLayer = new List<BaseTile>();

                for (int n = 0; n < numNodes; n++)
                {
                    TileType selectedType = SelectTileType(config.TileTypeWeights, rng.NextDouble());
                    BaseTile tile;

                    switch (selectedType)
                    {
                        case TileType.HintBoost:
                            tile = new HintBoostTile(currentIdx++);
                            break;
                        case TileType.HintTrap:
                            tile = new HintTrapTile(currentIdx++);
                            break;
                        case TileType.Event:
                            tile = new EventTile(currentIdx++, GetRandomEvent(rng));
                            break;
                        case TileType.Item:
                            tile = new ItemTile(currentIdx++, _itemDeck);
                            break;
                        case TileType.Neutral:
                        default:
                            tile = new NeutralTile(currentIdx++);
                            break;
                    }

                    // En la primera capa intermedia (adyacente al inicio), las casillas se descubren desde el inicio
                    if (l == 1)
                    {
                        tile.IsDiscovered = true;
                    }
                    else
                    {
                        tile.IsDiscovered = false;
                    }

                    currentLayer.Add(tile);
                    allNodesList.Add(tile);
                }
                layers.Add(currentLayer);
            }

            // 3. Crear nodo End (Meta, Neutral)
            BaseTile endNode = new NeutralTile(currentIdx++);
            endNode.IsDiscovered = false;
            layers.Add(new List<BaseTile> { endNode });
            allNodesList.Add(endNode);

            // Instanciar el grafo
            GeneratedGraph = new BoardGraph(startNode, endNode);

            // 4. Crear conexiones dirigidas entre capas sucesivas
            for (int l = 0; l < layers.Count - 1; l++)
            {
                List<BaseTile> currentLayer = layers[l];
                List<BaseTile> nextLayer = layers[l + 1];

                // Asegurar que cada nodo de la capa siguiente tenga al menos un padre
                foreach (BaseTile child in nextLayer)
                {
                    BaseTile parent = currentLayer[rng.Next(0, currentLayer.Count)];
                    GeneratedGraph.AddEdge(parent, child);
                }

                // Asegurar que cada nodo de la capa actual cumpla con sus requerimientos de branching
                foreach (BaseTile parent in currentLayer)
                {
                    int currentOutDegree = GeneratedGraph.GetOutDegree(parent);
                    int targetBranches = rng.Next(config.MinBranches, config.MaxBranches + 1);

                    // Añadir ramas adicionales si no cumple el mínimo
                    while (currentOutDegree < targetBranches && currentOutDegree < nextLayer.Count)
                    {
                        // Escoger un hijo que no esté ya conectado
                        BaseTile possibleChild = nextLayer[rng.Next(0, nextLayer.Count)];
                        GeneratedGraph.AddEdge(parent, possibleChild);
                        currentOutDegree = GeneratedGraph.GetOutDegree(parent);
                    }
                }
            }

            // Validar que el grafo sea conexo de inicio a fin
            if (!GeneratedGraph.HasPathToEnd())
            {
                Debug.LogWarning("ExplorationBoardGenerator: Grafo generado no tiene un camino válido al final. Forzando un camino directo.");
                // Forzar un camino lineal directo para garantizar conectividad
                for (int l = 0; l < layers.Count - 1; l++)
                {
                    GeneratedGraph.AddEdge(layers[l][0], layers[l + 1][0]);
                }
            }

            Debug.Log($"ExplorationBoardGenerator: Tablero de exploración generado con {allNodesList.Count} nodos repartidos en {layers.Count} capas (DAG).");

            return allNodesList;
        }

        private TileType SelectTileType(float[] weights, double roll)
        {
            double cumulative = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (roll <= cumulative)
                {
                    return (TileType)i;
                }
            }
            return TileType.Neutral;
        }

        private TileEventConfig GetRandomEvent(System.Random rng)
        {
            if (_eventPool != null && _eventPool.Count > 0)
            {
                int index = rng.Next(0, _eventPool.Count);
                return _eventPool[index];
            }
            return null;
        }
    }
}
