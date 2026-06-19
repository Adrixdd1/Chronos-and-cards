using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Data;
using ChronosAndCards.Gameplay.GmChallenge.Rewards;

namespace ChronosAndCards.Gameplay.GmChallenge
{
    /// <summary>
    /// Distribuye recompensas aleatorias al ganador del Desafío del GM.
    /// Selecciona una recompensa según pesos configurados y la aplica al jugador.
    /// </summary>
    public class GmRewardDistributor
    {
        private readonly GmRewardConfig _config;
        private readonly Dictionary<GmRewardType, IGmReward> _rewardImplementations;

        /// <summary>Se dispara cuando una recompensa es otorgada.</summary>
        public event Action<IPlayer, GmRewardType> OnRewardGranted;

        public GmRewardDistributor(
            GmRewardConfig config,
            Dictionary<GmRewardType, IGmReward> rewardImplementations)
        {
            _config = config;
            _rewardImplementations = rewardImplementations;
        }

        /// <summary>
        /// Selecciona una recompensa aleatoria (ponderada) y la aplica al jugador.
        /// </summary>
        /// <returns>El tipo de recompensa otorgada.</returns>
        public GmRewardType GrantRandomReward(IPlayer winner, GameContext context)
        {
            var rewardType = SelectWeightedRandom();

            if (_rewardImplementations.TryGetValue(rewardType, out var reward))
            {
                reward.Apply(winner, context);

                Debug.Log($"GmRewardDistributor: {winner.PlayerName} recibió recompensa '{rewardType}'.");

                OnRewardGranted?.Invoke(winner, rewardType);
                GameEvents.OnGmRewardGranted?.Invoke(winner, rewardType);
            }
            else
            {
                Debug.LogError($"GmRewardDistributor: No hay implementación para recompensa '{rewardType}'.");
            }

            return rewardType;
        }

        /// <summary>
        /// Selección aleatoria ponderada usando los pesos del config.
        /// </summary>
        private GmRewardType SelectWeightedRandom()
        {
            if (_config == null || _config.RewardEntries == null || _config.RewardEntries.Count == 0)
                return GmRewardType.SupplyCrate; // Fallback extremo

            var entries = _config.RewardEntries;
            float totalWeight = entries.Sum(e => e.Weight);
            float roll = UnityEngine.Random.Range(0f, totalWeight);

            float accumulated = 0f;
            foreach (var entry in entries)
            {
                accumulated += entry.Weight;
                if (roll <= accumulated)
                {
                    return entry.RewardType;
                }
            }

            // Fallback (nunca debería llegar aquí si los pesos son > 0)
            return entries[entries.Count - 1].RewardType;
        }
    }
}
