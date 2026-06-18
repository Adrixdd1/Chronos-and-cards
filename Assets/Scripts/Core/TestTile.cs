using UnityEngine;
using ChronosAndCards.Interfaces;
using ChronosAndCards.Data;

namespace ChronosAndCards.Core
{
    /// <summary>
    /// Implementación concreta de prueba del contrato ITile para la Épica 1.
    /// </summary>
    public class TestTile : ITile
    {
        private readonly TileType _type;

        /// <summary>Constructor para inicializar una casilla de prueba.</summary>
        public TestTile(TileType type)
        {
            _type = type;
        }

        public TileType Type => _type;

        public void OnPlayerLanded(IPlayer player)
        {
            Debug.Log($"TestTile: El jugador {player.PlayerName} cayó en una casilla de tipo {_type}.");
            
            // Simular efectos básicos en pistas según la casilla
            if (_type == TileType.HintBoost)
            {
                player.AddHint(1);
            }
            else if (_type == TileType.HintTrap)
            {
                player.AddHint(-1);
            }
        }
    }
}
