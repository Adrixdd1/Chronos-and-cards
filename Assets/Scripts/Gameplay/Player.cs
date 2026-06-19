using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Interfaces;
using ChronosAndCards.Gameplay.Items;

namespace ChronosAndCards.Gameplay
{
    /// <summary>
    /// Implementación concreta de un jugador (lógica pura sin MonoBehaviour).
    /// </summary>
    public class Player : IPlayer
    {
        /// <summary>Nombre del jugador.</summary>
        public string PlayerName { get; }

        /// <summary>Índice del jugador (0-based).</summary>
        public int PlayerIndex { get; }

        /// <summary>Posición actual en el tablero.</summary>
        public int Position { get; private set; }

        /// <summary>Cantidad de pistas disponibles del jugador.</summary>
        public int HintCount { get; private set; }

        /// <summary>Inventario del jugador.</summary>
        public PlayerInventory Inventory { get; }

        /// <summary>Indica si el jugador saltará su siguiente turno.</summary>
        public bool IsSkipNextTurn { get; private set; }

        /// <summary>Lista de efectos de estado activos sobre el jugador.</summary>
        public IReadOnlyList<IStatusEffect> ActiveEffects => _activeEffects.AsReadOnly();

        private readonly List<IStatusEffect> _activeEffects = new();

        /// <summary>
        /// Constructor para inicializar un jugador.
        /// </summary>
        /// <param name="name">Nombre del jugador.</param>
        /// <param name="index">Índice del jugador (0-based).</param>
        /// <param name="initialHints">Cantidad inicial de pistas.</param>
        public Player(string name, int index, int initialHints)
        {
            PlayerName = name;
            PlayerIndex = index;
            Position = 0;
            HintCount = initialHints;
            IsSkipNextTurn = false;
            Inventory = new PlayerInventory(0);
        }

        /// <summary>
        /// Mueve al jugador una cantidad relativa de casillas en el tablero.
        /// </summary>
        /// <param name="tiles">Cantidad de casillas a mover (puede ser negativo para retroceso).</param>
        public void MoveForward(int tiles)
        {
            Position += tiles;
            if (Position < 0) Position = 0;
        }

        /// <summary>
        /// Mueve al jugador a una posición absoluta en el tablero.
        /// </summary>
        /// <param name="pos">Posición absoluta (no negativa).</param>
        public void MoveToPosition(int pos)
        {
            Position = Mathf.Max(0, pos);
        }

        /// <summary>
        /// Modifica la cantidad de pistas del jugador, con protección contra valores negativos.
        /// </summary>
        /// <param name="amount">Cantidad de pistas a añadir/restar.</param>
        public void AddHint(int amount)
        {
            HintCount += amount;
            if (HintCount < 0) HintCount = 0;
        }

        /// <summary>
        /// Añade un objeto al inventario.
        /// </summary>
        public void AddItem(IItem item)
        {
            if (item != null)
            {
                Inventory.AddItem(this, item);
            }
        }

        /// <summary>
        /// Elimina un objeto del inventario.
        /// </summary>
        public void RemoveItem(IItem item)
        {
            if (item != null)
            {
                Inventory.RemoveItem(this, item);
            }
        }

        /// <summary>
        /// Comprueba si el jugador posee un ítem del tipo especificado.
        /// </summary>
        public bool HasItem<T>() where T : IItem
        {
            return Inventory.HasItem<T>();
        }

        /// <summary>
        /// Modifica la penalización de saltar turno.
        /// </summary>
        public void SetSkipNextTurn(bool skip)
        {
            IsSkipNextTurn = skip;
        }

        /// <summary>
        /// Añade un efecto de estado al jugador.
        /// </summary>
        public void AddStatusEffect(IStatusEffect effect)
        {
            if (effect != null)
            {
                _activeEffects.Add(effect);
            }
        }

        /// <summary>
        /// Elimina un efecto de estado del jugador.
        /// </summary>
        public void RemoveStatusEffect(IStatusEffect effect)
        {
            if (effect != null)
            {
                _activeEffects.Remove(effect);
            }
        }

        /// <summary>
        /// Comprueba si el jugador posee un efecto de estado activo del tipo especificado.
        /// </summary>
        public bool HasStatusEffect<T>() where T : IStatusEffect
        {
            return _activeEffects.Any(e => e is T);
        }
    }
}
