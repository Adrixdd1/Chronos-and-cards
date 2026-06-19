using System.Collections.Generic;
using UnityEngine;
using ChronosAndCards.Gameplay.GmChallenge;

namespace ChronosAndCards.Data
{
    /// <summary>
    /// Configuración de las recompensas del Desafío del GM.
    /// Define los tipos disponibles y sus pesos de probabilidad.
    /// </summary>
    [CreateAssetMenu(fileName = "GmRewardConfig", menuName = "ChronosAndCards/GmRewardConfig")]
    public class GmRewardConfig : ScriptableObject
    {
        /// <summary>Entradas de recompensas con sus pesos.</summary>
        [SerializeField] private List<GmRewardEntry> _rewardEntries = new();

        public IReadOnlyList<GmRewardEntry> RewardEntries => _rewardEntries.AsReadOnly();
    }

    /// <summary>Entrada de configuración para una recompensa del GM.</summary>
    [System.Serializable]
    public class GmRewardEntry
    {
        /// <summary>Tipo de recompensa.</summary>
        [SerializeField] private GmRewardType _rewardType;

        /// <summary>Peso de probabilidad (mayor = más probable).</summary>
        [SerializeField, Range(0.1f, 10f)] private float _weight = 1f;

        /// <summary>Descripción para la UI.</summary>
        [SerializeField, TextArea(1, 3)] private string _description;

        public GmRewardType RewardType => _rewardType;
        public float Weight => _weight;
        public string Description => _description;
    }
}
