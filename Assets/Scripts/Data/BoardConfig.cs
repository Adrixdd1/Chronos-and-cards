using UnityEngine;

namespace ChronosAndCards.Data
{
    /// <summary>
    /// Configuración del tablero que define el modo de juego, dimensiones, economía y distribución de casillas.
    /// Valida automáticamente los rangos en el editor mediante OnValidate.
    /// </summary>
    [CreateAssetMenu(fileName = "BoardConfig", menuName = "Chronos/Board Config")]
    public class BoardConfig : ScriptableObject
    {
        [Header("Modo de Juego")]
        [Tooltip("Linear = ruta directa tipo Oca. Exploration = red de nodos multipath.")]
        public BoardMode Mode;

        [Header("Dimensiones del Tablero")]
        [Tooltip("Número estimado de preguntas para que un jugador termine la partida.")]
        [Range(8, 50)]
        public int TargetQuestionCount = 16;

        [Header("Economía")]
        [Tooltip("Pistas disponibles al inicio para cada jugador.")]
        [Range(0, 10)]
        public int InitialHints = 3;

        [Header("Distribución de Casillas")]
        [Tooltip("Pesos normalizados [Neutral, HintBoost, HintTrap, Event, Item]. Deben sumar 1.0.")]
        public float[] TileTypeWeights = { 0.40f, 0.15f, 0.15f, 0.15f, 0.15f };

        [Header("Exploración (solo aplica si Mode = Exploration)")]
        [Range(1, 3)]
        public int MinBranches = 1;
        [Range(1, 3)]
        public int MaxBranches = 3;

        [Header("Avanzado")]
        [Tooltip("Seed para generación procedural. 0 = aleatorio.")]
        public int RandomSeed = 0;

        /// <summary>
        /// Longitud estimada del tablero en casillas.
        /// Basado en avgDiceRoll(3.5) * avgMultiplier(0.75).
        /// </summary>
        public int EstimatedTotalTiles => Mathf.RoundToInt(TargetQuestionCount * 3.5f * 0.75f);

        private void OnValidate()
        {
            // Validar límites numéricos básicos
            if (TargetQuestionCount < 8) TargetQuestionCount = 8;
            if (TargetQuestionCount > 50) TargetQuestionCount = 50;

            if (InitialHints < 0) InitialHints = 0;
            if (InitialHints > 10) InitialHints = 10;

            // Validar array de pesos
            if (TileTypeWeights == null || TileTypeWeights.Length != 5)
            {
                Debug.LogWarning("BoardConfig: TileTypeWeights debe tener exactamente 5 elementos. Reinicializando a pesos estándar.");
                TileTypeWeights = new float[] { 0.40f, 0.15f, 0.15f, 0.15f, 0.15f };
            }
            else
            {
                float sum = 0f;
                for (int i = 0; i < 5; i++) sum += TileTypeWeights[i];

                if (Mathf.Abs(sum - 1.0f) > 0.01f)
                {
                    Debug.LogWarning($"BoardConfig: Los pesos de casillas suman {sum:F2} (deben sumar 1.0). Normalizando pesos.");
                    if (sum <= 0f)
                    {
                        TileTypeWeights = new float[] { 0.40f, 0.15f, 0.15f, 0.15f, 0.15f };
                    }
                    else
                    {
                        for (int i = 0; i < 5; i++) TileTypeWeights[i] /= sum;
                    }
                }
            }

            // Validar límites de ramas
            if (MinBranches < 1) MinBranches = 1;
            if (MinBranches > 3) MinBranches = 3;
            if (MaxBranches < 1) MaxBranches = 1;
            if (MaxBranches > 3) MaxBranches = 3;

            if (MinBranches > MaxBranches)
            {
                MinBranches = MaxBranches;
            }
        }
    }
}
