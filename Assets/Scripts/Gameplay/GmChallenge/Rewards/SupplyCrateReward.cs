using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Data;

namespace ChronosAndCards.Gameplay.GmChallenge.Rewards
{
    /// <summary>
    /// Recompensa "Cajón de Suministros" del Desafío del GM.
    /// Otorga al ganador +1 Pista y +1 Objeto aleatorio del mazo de ítems.
    /// Es una recompensa pasiva (no requiere interacción adicional).
    /// </summary>
    public class SupplyCrateReward : IGmReward
    {
        public string Name => "Cajón de Suministros";
        public string Description => "+1 Pista y +1 Objeto del mazo.";
        public GmRewardType RewardType => GmRewardType.SupplyCrate;
        public bool RequiresInteraction => false;

        /// <summary>
        /// Aplica la recompensa:
        /// 1. Añade 1 pista al jugador
        /// 2. Extrae 1 objeto del mazo de ítems y lo añade al inventario
        /// </summary>
        public void Apply(IPlayer winner, GameContext context)
        {
            // +1 Pista
            winner.AddHint(1);
            GameEvents.OnHintChanged?.Invoke(winner, winner.HintCount);

            Debug.Log($"SupplyCrateReward: {winner.PlayerName} recibió +1 Pista. Total pistas: {winner.HintCount}.");

            // +1 Objeto del mazo
            if (context.ItemDeck != null)
            {
                var drawnItem = context.ItemDeck.DrawItem();
                if (drawnItem != null)
                {
                    winner.AddItem(drawnItem);
                    GameEvents.OnItemObtained?.Invoke(winner, drawnItem);

                    Debug.Log($"SupplyCrateReward: {winner.PlayerName} recibió objeto '{drawnItem.Name}'.");
                }
                else
                {
                    Debug.LogWarning("SupplyCrateReward: El mazo de objetos está vacío. Solo se otorgó la pista.");
                }
            }
        }
    }
}
