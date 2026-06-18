using System;
using UnityEngine;
using ChronosAndCards.Interfaces;

namespace ChronosAndCards.Gameplay
{
    /// <summary>
    /// Componente físico/lógico del dado. Stub para compilación de la Épica 1.
    /// </summary>
    public class DiceRoller : MonoBehaviour, IDiceRoller
    {
        /// <summary>Se dispara cuando finaliza el rodaje del dado.</summary>
        public event Action<int> OnDiceResult;

        /// <summary>
        /// Realiza el lanzamiento del dado simularizando un valor aleatorio entre 1 y 6.
        /// </summary>
        public void Roll()
        {
            int result = UnityEngine.Random.Range(1, 7);
            Debug.Log($"DiceRoller: Dado lanzado, resultado: {result} (Stub).");
            OnDiceResult?.Invoke(result);
        }
    }
}
