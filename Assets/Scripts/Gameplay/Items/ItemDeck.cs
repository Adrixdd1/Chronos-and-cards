using System;
using System.Collections.Generic;
using UnityEngine;
using ChronosAndCards.Interfaces;
using ChronosAndCards.Data;

namespace ChronosAndCards.Gameplay.Items
{
    /// <summary>
    /// Mazo de objetos consumibles. Gestiona la distribución aleatoria de ítems
    /// cuando un jugador cae en una casilla de tipo Item.
    /// </summary>
    public class ItemDeck : IItemDeck
    {
        private readonly ItemDeckConfig _config;
        private List<IItem> _drawPile = new();
        private readonly List<IItem> _discardPile = new();

        /// <summary>Se dispara cuando el mazo se agota.</summary>
        public event Action OnDeckEmpty;

        /// <summary>Se dispara cuando el mazo se re-baraja.</summary>
        public event Action<int> OnDeckReshuffled; // totalCards

        /// <summary>Cartas restantes en el mazo.</summary>
        public int RemainingItems => _drawPile.Count;

        /// <summary>Indica si el mazo se encuentra vacío.</summary>
        public bool IsEmpty => _drawPile.Count == 0;

        public ItemDeck(ItemDeckConfig config)
        {
            _config = config;
            InitializeDeck();
        }

        /// <summary>Inicializa el mazo con las entradas de la configuración.</summary>
        private void InitializeDeck()
        {
            _drawPile.Clear();
            _discardPile.Clear();

            if (_config == null || _config.Entries == null)
            {
                Debug.LogWarning("ItemDeck: ItemDeckConfig no está asignado o no tiene entradas.");
                return;
            }

            foreach (var entry in _config.Entries)
            {
                for (int i = 0; i < entry.Copies; i++)
                {
                    // Instanciar una copia del ítem desde la definición
                    if (entry.ItemDefinition != null)
                    {
                        _drawPile.Add(entry.ItemDefinition);
                    }
                }
            }

            Shuffle(_drawPile);
            Debug.Log($"ItemDeck: Mazo inicializado con {_drawPile.Count} ítems.");
        }

        /// <summary>
        /// Extrae un ítem aleatorio del mazo.
        /// Si el mazo está vacío y reshuffleOnEmpty está activo, re-baraja.
        /// </summary>
        /// <returns>El ítem extraído, o null si el mazo está agotado y no se re-baraja.</returns>
        public IItem DrawItem()
        {
            if (_drawPile.Count == 0)
            {
                if (_config != null && _config.ReshuffleOnEmpty && _discardPile.Count > 0)
                {
                    ReshuffleDeck();
                }
                else
                {
                    Debug.LogWarning("ItemDeck: El mazo de objetos está agotado.");
                    OnDeckEmpty?.Invoke();
                    return null;
                }
            }

            if (_drawPile.Count == 0) return null;

            var item = _drawPile[0];
            _drawPile.RemoveAt(0);

            Debug.Log($"ItemDeck: Se extrajo '{item.Name}'. Restantes: {_drawPile.Count}");
            return item;
        }

        /// <summary>Re-baraja la pila de descarte y la convierte en el nuevo mazo.</summary>
        private void ReshuffleDeck()
        {
            _drawPile = new List<IItem>(_discardPile);
            _discardPile.Clear();
            Shuffle(_drawPile);
            OnDeckReshuffled?.Invoke(_drawPile.Count);
            Debug.Log($"ItemDeck: Mazo re-barajado con {_drawPile.Count} ítems.");
        }

        /// <summary>Envía un ítem usado a la pila de descarte.</summary>
        public void Discard(IItem item)
        {
            if (item != null)
            {
                _discardPile.Add(item);
            }
        }

        /// <summary>Fisher-Yates shuffle O(n).</summary>
        private void Shuffle<T>(List<T> list)
        {
            var rng = new System.Random();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
