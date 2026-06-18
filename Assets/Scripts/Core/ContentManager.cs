using UnityEngine;
using ChronosAndCards.Data;

namespace ChronosAndCards.Core
{
    /// <summary>
    /// Gestiona las cartas y preguntas del juego. Stub para compilación de la Épica 1.
    /// </summary>
    public class ContentManager : MonoBehaviour
    {
        /// <summary>
        /// Inicializa y carga el contenido de las cartas.
        /// </summary>
        public void LoadContent()
        {
            Debug.Log("ContentManager: Contenido cargado (Stub).");
        }

        /// <summary>
        /// Extrae una carta del mazo basada en el nivel de dificultad especificado.
        /// </summary>
        /// <param name="level">Nivel de dificultad (1-6).</param>
        /// <returns>Estructura CardData con la información de la pregunta.</returns>
        public CardData DrawCard(int level)
        {
            Debug.Log($"ContentManager: Carta extraída con dificultad {level} (Stub).");
            return new CardData(level, "Pregunta de prueba?", "Pista de prueba", null, "Correcta", false);
        }
    }
}
