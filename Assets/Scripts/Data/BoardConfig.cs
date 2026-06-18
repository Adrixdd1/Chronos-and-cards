using UnityEngine;
using ChronosAndCards.Data;

namespace ChronosAndCards.Data
{
    /// <summary>
    /// Configuración del tablero. Se implementará completamente en la Épica 2.
    /// </summary>
    [CreateAssetMenu(fileName = "BoardConfig", menuName = "Chronos/BoardConfig")]
    public class BoardConfig : ScriptableObject
    {
        /// <summary>Modo del tablero.</summary>
        public BoardMode Mode;
        
        /// <summary>Número objetivo de casillas/preguntas en el tablero.</summary>
        public int TargetQuestionCount;
        
        /// <summary>Pistas iniciales asignadas a los jugadores.</summary>
        public int InitialHints;
        
        /// <summary>Pesos de probabilidad para la distribución de casillas.</summary>
        public float[] TileTypeWeights;
    }
}
