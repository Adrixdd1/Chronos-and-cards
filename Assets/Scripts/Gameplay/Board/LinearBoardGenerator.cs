using System;
using System.Collections.Generic;
using UnityEngine;
using ChronosAndCards.Interfaces;
using ChronosAndCards.Data;

namespace ChronosAndCards.Gameplay.Board
{
    /// <summary>
    /// Generador que crea un tablero lineal secuencial (tipo Oca), distribuyendo
    /// las casillas de forma ponderada según la configuración del GM.
    /// </summary>
    public class LinearBoardGenerator : IBoardGenerator
    {
        private readonly IReadOnlyList<TileEventConfig> _eventPool;
        private readonly IItemDeck _itemDeck;

        /// <summary>
        /// Constructor que recibe el pool de eventos y el mazo de objetos a inyectar en las casillas.
        /// </summary>
        public LinearBoardGenerator(IReadOnlyList<TileEventConfig> eventPool, IItemDeck itemDeck)
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

            int totalTiles = config.EstimatedTotalTiles;
            if (totalTiles < 3) totalTiles = 3; // Mínimo: inicio, 1 intermedia, meta

            // Inicializar generador pseudo-aleatorio con semilla
            System.Random rng = config.RandomSeed != 0 ? new System.Random(config.RandomSeed) : new System.Random();

            List<ITile> board = new List<ITile>();
            int neutralCount = 0;
            int hintBoostCount = 0;
            int hintTrapCount = 0;
            int eventCount = 0;
            int itemCount = 0;

            for (int i = 0; i < totalTiles; i++)
            {
                BaseTile tile;

                // Casilla 0 (Inicio) y Casilla N-1 (Meta) siempre son Neutrales
                if (i == 0 || i == totalTiles - 1)
                {
                    tile = new NeutralTile(i);
                    neutralCount++;
                }
                else
                {
                    // Selección ponderada para las intermedias
                    double roll = rng.NextDouble();
                    TileType selectedType = SelectTileType(config.TileTypeWeights, roll);

                    switch (selectedType)
                    {
                        case TileType.HintBoost:
                            tile = new HintBoostTile(i);
                            hintBoostCount++;
                            break;
                        case TileType.HintTrap:
                            tile = new HintTrapTile(i);
                            hintTrapCount++;
                            break;
                        case TileType.Event:
                            TileEventConfig eventConfig = GetRandomEvent(rng);
                            tile = new EventTile(i, eventConfig);
                            eventCount++;
                            break;
                        case TileType.Item:
                            tile = new ItemTile(i, _itemDeck);
                            itemCount++;
                            break;
                        case TileType.Neutral:
                        default:
                            tile = new NeutralTile(i);
                            neutralCount++;
                            break;
                    }
                }

                // En el modo lineal, todas las casillas están descubiertas por defecto
                tile.IsDiscovered = true;
                board.Add(tile);
            }

            Debug.Log($"LinearBoardGenerator: Tablero lineal de {totalTiles} casillas generado con éxito. " +
                      $"Desglose -> Neutral: {neutralCount}, Pista+: {hintBoostCount}, Pista-: {hintTrapCount}, Evento: {eventCount}, Objeto: {itemCount}");

            return board;
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
            return TileType.Neutral; // Fallback por defecto
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
