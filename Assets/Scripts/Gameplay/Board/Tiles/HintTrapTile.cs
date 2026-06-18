using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Data;

namespace ChronosAndCards.Gameplay.Board
{
    /// <summary>
    /// Casilla trampa que reduce en uno (-1 Pista) el saldo de pistas del jugador que cae en ella.
    /// El saldo final no puede ser menor a 0.
    /// </summary>
    public class HintTrapTile : BaseTile
    {
        public override TileType Type => TileType.HintTrap;

        /// <summary>Constructor para inicializar la casilla en el índice especificado.</summary>
        public HintTrapTile(int index) : base(index)
        {
        }

        public override void OnPlayerLanded(IPlayer player)
        {
            player.AddHint(-1);
            
            Debug.Log($"HintTrapTile[{Index}]: {player.PlayerName} perdió -1 pista. Total: {player.HintCount}");
            
            GameEvents.OnHintChanged?.Invoke(player, player.HintCount);
            EmitTileEffect(player);
        }
    }
}
