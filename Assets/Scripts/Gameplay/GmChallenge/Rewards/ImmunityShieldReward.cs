using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Data;
using ChronosAndCards.Gameplay.StatusEffects;

namespace ChronosAndCards.Gameplay.GmChallenge.Rewards
{
    /// <summary>
    /// Recompensa "Escudo de Inmunidad" del Desafío del GM.
    /// Otorga invulnerabilidad a Duelos, Robos y Sabotajes durante 1 ronda.
    /// Se implementa como un IStatusEffect temporal con duración de 1 ronda.
    /// </summary>
    public class ImmunityShieldReward : IGmReward
    {
        public string Name => "Escudo de Inmunidad";
        public string Description => "Invulnerable a efectos ofensivos durante la próxima ronda.";
        public GmRewardType RewardType => GmRewardType.ImmunityShield;
        public bool RequiresInteraction => false;

        /// <summary>
        /// Aplica la recompensa: añade un StatusEffect "ImmunityShield" al jugador.
        /// El efecto durará 1 ronda completa.
        /// </summary>
        public void Apply(IPlayer winner, GameContext context)
        {
            var shieldEffect = new ImmunityShieldStatusEffect(context.CurrentRound + 1);
            winner.AddStatusEffect(shieldEffect);

            Debug.Log($"ImmunityShieldReward: {winner.PlayerName} recibió Escudo de Inmunidad. Activo hasta el final de la ronda {context.CurrentRound + 1}.");

            GameEvents.OnStatusEffectApplied?.Invoke(winner, shieldEffect);
        }
    }
}
