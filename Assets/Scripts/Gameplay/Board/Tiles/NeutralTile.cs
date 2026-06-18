using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Data;

namespace ChronosAndCards.Gameplay.Board
{
    /// <summary>
    /// Casilla estándar del tablero que no aplica efectos sobre los recursos del jugador.
    /// </summary>
    public class NeutralTile : BaseTile
    {
        public override TileType Type => TileType.Neutral;

        /// <summary>Constructor que inicializa la casilla neutral en el índice especificado.</summary>
        public NeutralTile(int index) : base(index)
        {
        }

        public override void OnPlayerLanded(IPlayer player)
        {
            Debug.Log($"NeutralTile[{Index}]: {player.PlayerName} aterrizó. Sin efecto.");
            EmitTileEffect(player);
        }
    }
}
