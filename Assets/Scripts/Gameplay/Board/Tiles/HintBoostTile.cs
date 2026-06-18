using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Data;

namespace ChronosAndCards.Gameplay.Board
{
    /// <summary>
    /// Casilla que otorga una pista adicional (+1 Pista) al jugador que aterrice en ella.
    /// </summary>
    public class HintBoostTile : BaseTile
    {
        public override TileType Type => TileType.HintBoost;

        /// <summary>Constructor para inicializar la casilla en el índice especificado.</summary>
        public HintBoostTile(int index) : base(index)
        {
        }

        public override void OnPlayerLanded(IPlayer player)
        {
            player.AddHint(1);
            
            Debug.Log($"HintBoostTile[{Index}]: {player.PlayerName} ganó +1 pista. Total: {player.HintCount}");
            
            GameEvents.OnHintChanged?.Invoke(player, player.HintCount);
            EmitTileEffect(player);
        }
    }
}
