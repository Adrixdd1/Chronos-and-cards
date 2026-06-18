using UnityEngine;

namespace ChronosAndCards.Data
{
    /// <summary>
    /// Configuración de mapeo personalizado para la dificultad de las cartas extraídas a partir del dado.
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultyMap_Default", menuName = "Chronos/Difficulty Map Config")]
    public class DifficultyMapConfig : ScriptableObject
    {
        [Tooltip("Mapeo personalizado: índice 0 = dado 1, ..., índice 5 = dado 6. Valor = nivel de dificultad.")]
        public int[] CustomMapping = { 1, 2, 3, 4, 5, 6 };

        private void OnValidate()
        {
            if (CustomMapping == null || CustomMapping.Length != 6)
            {
                Debug.LogWarning("DifficultyMapConfig: CustomMapping debe tener exactamente 6 elementos. Restableciendo al mapeo por defecto.");
                CustomMapping = new int[] { 1, 2, 3, 4, 5, 6 };
            }
            else
            {
                for (int i = 0; i < CustomMapping.Length; i++)
                {
                    if (CustomMapping[i] < 1 || CustomMapping[i] > 6)
                    {
                        Debug.LogWarning($"DifficultyMapConfig: El valor en el índice {i} ({CustomMapping[i]}) está fuera del rango 1–6. Se limitará al rango permitido.");
                        CustomMapping[i] = Mathf.Clamp(CustomMapping[i], 1, 6);
                    }
                }
            }
        }
    }
}
