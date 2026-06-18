using System;
using ChronosAndCards.Interfaces;

namespace ChronosAndCards.Gameplay.Dice
{
    /// <summary>
    /// Motor RNG del dado, encargado de generar y validar resultados desacoplado de la visualización.
    /// </summary>
    public class DiceLogic : IDiceRoller, IDiceValidator
    {
        /// <summary>Se dispara inmediatamente al generar el resultado del dado.</summary>
        public event Action<int> OnDiceResult;

        private readonly int _minValue = 1;
        private readonly int _maxValue = 6;
        private System.Random _random;

        /// <summary>Crea un DiceLogic con seed aleatorio.</summary>
        public DiceLogic() => _random = new System.Random();

        /// <summary>Crea un DiceLogic con seed fijo para reproducibilidad.</summary>
        public DiceLogic(int seed) => _random = new System.Random(seed);

        /// <summary>Genera un resultado aleatorio 1–6 y dispara OnDiceResult.</summary>
        public void Roll()
        {
            int result = _random.Next(_minValue, _maxValue + 1);
            OnDiceResult?.Invoke(result);
        }

        /// <summary>Valida que el resultado esté en el rango permitido (1–6).</summary>
        public bool IsValidResult(int result) => result >= _minValue && result <= _maxValue;

        /// <summary>Permite re-inicializar el seed (para testing o nuevas partidas).</summary>
        public void SetSeed(int seed) => _random = new System.Random(seed);
    }
}
