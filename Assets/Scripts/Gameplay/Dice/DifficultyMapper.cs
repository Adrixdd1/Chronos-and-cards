using System;
using ChronosAndCards.Data;

namespace ChronosAndCards.Gameplay.Dice
{
    /// <summary>
    /// Interfaz para traducir el resultado del dado a un nivel de dificultad de carta.
    /// </summary>
    public interface IDifficultyMapper
    {
        /// <summary>Convierte un resultado de dado a un nivel de dificultad de carta.</summary>
        int GetDifficulty(int diceValue);
    }

    /// <summary>
    /// Implementación concreta de IDifficultyMapper que soporta mapeo directo 1:1 o mapeos personalizados vía ScriptableObject.
    /// </summary>
    public class DifficultyMapper : IDifficultyMapper
    {
        private readonly DifficultyMapConfig _config;

        /// <summary>
        /// Constructor que recibe una configuración opcional de mapeo.
        /// </summary>
        public DifficultyMapper(DifficultyMapConfig config = null)
        {
            _config = config;
        }

        /// <summary>
        /// Convierte un resultado de dado (1–6) a un nivel de dificultad de carta.
        /// </summary>
        public int GetDifficulty(int diceValue)
        {
            if (diceValue < 1 || diceValue > 6)
            {
                throw new ArgumentOutOfRangeException(nameof(diceValue), "El resultado del dado debe estar en el rango de 1 a 6.");
            }

            if (_config != null && _config.CustomMapping != null)
            {
                return _config.CustomMapping[diceValue - 1];
            }

            return diceValue; // Mapeo directo por defecto: dado 1 = nivel 1, ..., dado 6 = nivel 6
        }
    }
}
