using UnityEngine;
using ChronosAndCards.Data;

namespace ChronosAndCards.Gameplay
{
    /// <summary>
    /// Calculadora pura encargada de evaluar las casillas que avanzará un jugador según su desempeño y el dado.
    /// </summary>
    public class AdvanceCalculator
    {
        private readonly RoundingMode _roundingMode;

        /// <summary>
        /// Modos de redondeo soportados para fracciones (ej. avance de x0.5).
        /// </summary>
        public enum RoundingMode { Round, Floor, Ceil }

        /// <summary>
        /// Constructor que configura el modo de redondeo.
        /// </summary>
        /// <param name="roundingMode">Modo de redondeo (por defecto es Round).</param>
        public AdvanceCalculator(RoundingMode roundingMode = RoundingMode.Round)
        {
            _roundingMode = roundingMode;
        }

        /// <summary>
        /// Calcula las casillas a avanzar según la fórmula:
        /// tilesToMove = diceValue * (performanceMultiplier / 100)
        /// </summary>
        public int Calculate(int diceValue, PerformanceMultiplier multiplier)
        {
            float rawResult = diceValue * ((int)multiplier / 100f);
            return ApplyRounding(rawResult);
        }

        /// <summary>
        /// Sobrecarga que soporta aplicar un retroceso (retroceder casillas) en caso de fallo.
        /// </summary>
        public int Calculate(int diceValue, PerformanceMultiplier multiplier, int failPenalty)
        {
            if (multiplier == PerformanceMultiplier.Fail)
            {
                return -failPenalty; // Retorna penalización negativa
            }

            return Calculate(diceValue, multiplier);
        }

        private int ApplyRounding(float value)
        {
            return _roundingMode switch
            {
                RoundingMode.Floor => Mathf.FloorToInt(value),
                RoundingMode.Ceil  => Mathf.CeilToInt(value),
                _                  => Mathf.RoundToInt(value),
            };
        }
    }
}
