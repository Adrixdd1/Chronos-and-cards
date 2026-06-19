using System.Collections.Generic;
using ChronosAndCards.Interfaces;
using ChronosAndCards.Gameplay.Items;

namespace ChronosAndCards.Core
{
    /// <summary>
    /// Implementación concreta de prueba del contrato IPlayer para la Épica 1.
    /// </summary>
    public class TestPlayer : IPlayer
    {
        private string _playerName;
        private int _playerIndex;
        private int _position;
        private int _hintCount;
        private bool _isSkipNextTurn;

        /// <summary>Constructor para inicializar un jugador de prueba.</summary>
        public TestPlayer(string name, int index)
        {
            _playerName = name;
            _playerIndex = index;
            _position = 0;
            _hintCount = 0;
            _isSkipNextTurn = false;
            Inventory = new PlayerInventory(0);
        }

        public string PlayerName => _playerName;
        public int PlayerIndex => _playerIndex;
        public int Position => _position;
        public int HintCount => _hintCount;
        public PlayerInventory Inventory { get; private set; }
        public bool IsSkipNextTurn => _isSkipNextTurn;

        private List<IStatusEffect> _activeEffects = new List<IStatusEffect>();
        public IReadOnlyList<IStatusEffect> ActiveEffects => _activeEffects;

        public void AddStatusEffect(IStatusEffect effect)
        {
            _activeEffects.Add(effect);
        }

        public void RemoveStatusEffect(IStatusEffect effect)
        {
            _activeEffects.Remove(effect);
        }

        public bool HasStatusEffect<T>() where T : IStatusEffect
        {
            foreach (var effect in _activeEffects)
            {
                if (effect is T) return true;
            }
            return false;
        }

        public void MoveForward(int tiles)
        {
            int oldPos = _position;
            _position += tiles;
            GameEvents.OnPlayerMoved?.Invoke(this, oldPos, _position);
        }

        public void MoveToPosition(int pos)
        {
            int oldPos = _position;
            _position = pos;
            GameEvents.OnPlayerMoved?.Invoke(this, oldPos, _position);
        }

        public void AddHint(int amount)
        {
            _hintCount += amount;
            GameEvents.OnHintChanged?.Invoke(this, _hintCount);
        }

        public void AddItem(IItem item)
        {
            Inventory.AddItem(this, item);
        }

        public void RemoveItem(IItem item)
        {
            Inventory.RemoveItem(this, item);
        }

        public bool HasItem<T>() where T : IItem
        {
            return Inventory.HasItem<T>();
        }

        public void SetSkipNextTurn(bool skip)
        {
            _isSkipNextTurn = skip;
        }
    }
}
