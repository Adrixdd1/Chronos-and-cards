using UnityEngine;
using ChronosAndCards.Interfaces;
using ChronosAndCards.Core;
using ChronosAndCards.Data;

namespace ChronosAndCards.Gameplay.Board
{
    /// <summary>
    /// Casilla que otorga un objeto consumible aleatorio desde el mazo (IItemDeck) al jugador que aterrice en ella.
    /// </summary>
    public class ItemTile : BaseTile
    {
        private readonly IItemDeck _itemDeck;

        public override TileType Type => TileType.Item;

        /// <summary>Acceso al mazo asignado a esta casilla.</summary>
        public IItemDeck ItemDeck => _itemDeck;

        /// <summary>
        /// Constructor que inicializa la casilla con el mazo de consumibles en el índice especificado.
        /// </summary>
        public ItemTile(int index, IItemDeck itemDeck) : base(index)
        {
            _itemDeck = itemDeck;
        }

        public override void OnPlayerLanded(IPlayer player)
        {
            if (_itemDeck != null && !_itemDeck.IsEmpty)
            {
                IItem item = _itemDeck.DrawItem();
                if (item != null)
                {
                    player.AddItem(item);
                    Debug.Log($"ItemTile[{Index}]: {player.PlayerName} obtuvo el objeto '{item.Name}'.");
                    GameEvents.OnInventoryChanged?.Invoke(player, item, InventoryAction.Added);
                }
                else
                {
                    Debug.LogWarning($"ItemTile[{Index}]: El mazo de objetos retornó un item nulo.");
                }
            }
            else
            {
                Debug.LogWarning($"ItemTile[{Index}]: Mazo de objetos agotado o no configurado. No se otorga objeto.");
            }

            EmitTileEffect(player);
        }
    }
}
