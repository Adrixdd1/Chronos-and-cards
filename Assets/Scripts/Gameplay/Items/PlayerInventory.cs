using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Interfaces;

namespace ChronosAndCards.Gameplay.Items
{
    /// <summary>
    /// Gestiona el inventario de ítems de un jugador.
    /// Encapsula la lista de ítems y emite eventos de cambio.
    /// </summary>
    public class PlayerInventory
    {
        private readonly List<IItem> _items = new();
        private readonly int _maxCapacity;

        /// <summary>Evento disparado cuando el inventario cambia.</summary>
        public event Action<IPlayer, IItem, InventoryAction> OnInventoryChanged;

        /// <summary>Lista de lectura de los ítems actuales.</summary>
        public IReadOnlyList<IItem> Items => _items.AsReadOnly();

        /// <summary>Número de ítems en el inventario.</summary>
        public int Count => _items.Count;

        /// <summary>Indica si el inventario está lleno.</summary>
        public bool IsFull => _maxCapacity > 0 && _items.Count >= _maxCapacity;

        /// <param name="maxCapacity">Capacidad máxima. 0 o negativo = ilimitado.</param>
        public PlayerInventory(int maxCapacity = 0)
        {
            _maxCapacity = maxCapacity;
        }

        /// <summary>Añade un ítem al inventario si hay capacidad.</summary>
        /// <returns>true si se añadió, false si el inventario está lleno.</returns>
        public bool AddItem(IPlayer owner, IItem item)
        {
            if (IsFull)
            {
                Debug.LogWarning($"PlayerInventory: Inventario de {owner.PlayerName} lleno. No se puede añadir '{item.Name}'.");
                return false;
            }

            _items.Add(item);
            OnInventoryChanged?.Invoke(owner, item, InventoryAction.Added);
            Debug.Log($"PlayerInventory: {owner.PlayerName} obtuvo '{item.Name}'. Total: {_items.Count}");
            return true;
        }

        /// <summary>Remueve un ítem del inventario.</summary>
        /// <returns>true si se removió, false si no se encontró.</returns>
        public bool RemoveItem(IPlayer owner, IItem item)
        {
            if (!_items.Remove(item))
            {
                Debug.LogWarning($"PlayerInventory: '{item.Name}' no encontrado en inventario de {owner.PlayerName}.");
                return false;
            }

            OnInventoryChanged?.Invoke(owner, item, InventoryAction.Removed);
            Debug.Log($"PlayerInventory: {owner.PlayerName} perdió '{item.Name}'. Total: {_items.Count}");
            return true;
        }

        /// <summary>Verifica si el jugador tiene al menos un ítem de tipo T.</summary>
        public bool HasItem<T>() where T : IItem
        {
            return _items.Any(i => i is T);
        }

        /// <summary>Obtiene el primer ítem de tipo T, o null si no existe.</summary>
        public T GetItem<T>() where T : class, IItem
        {
            return _items.FirstOrDefault(i => i is T) as T;
        }

        /// <summary>Obtiene todos los ítems activables en la fase actual.</summary>
        public List<IItem> GetActivableItems(ItemActivationPhase currentPhase)
        {
            return _items.Where(i => i.Phase == currentPhase).ToList();
        }
    }
}
