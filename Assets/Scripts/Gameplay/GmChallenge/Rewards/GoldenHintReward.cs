using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Data;
using ChronosAndCards.Gameplay.StatusEffects;

namespace ChronosAndCards.Gameplay.GmChallenge.Rewards
{
    /// <summary>
    /// Recompensa "Pista Dorada" del Desafío del GM.
    /// Otorga un token que permite responder una pregunta de Nivel 6
    /// sin penalización de multiplicador (similar a Overdrive pero para N6).
    /// Se implementa como un IStatusEffect temporal de 1 uso.
    /// </summary>
    public class GoldenHintReward : IGmReward
    {
        public string Name => "Pista Dorada";
        public string Description => "La próxima pregunta de Nivel 6 no reduce tu multiplicador.";
        public GmRewardType RewardType => GmRewardType.GoldenHint;
        public bool RequiresInteraction => false;

        /// <summary>
        /// Aplica la recompensa: añade un StatusEffect "GoldenHint" al jugador.
        /// El efecto se activará automáticamente la próxima vez que enfrente un N6.
        /// </summary>
        public void Apply(IPlayer winner, GameContext context)
        {
            var goldenHintEffect = new GoldenHintStatusEffect();
            winner.AddStatusEffect(goldenHintEffect);

            Debug.Log($"GoldenHintReward: {winner.PlayerName} recibió Pista Dorada. Se activará en su próxima pregunta de Nivel 6.");

            GameEvents.OnStatusEffectApplied?.Invoke(winner, goldenHintEffect);
        }
    }
}
