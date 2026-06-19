using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Data;
using ChronosAndCards.Interfaces;

namespace ChronosAndCards.Gameplay.Items.Effects
{
    /// <summary>
    /// Efecto: Otorga una pista sin penalizar el multiplicador (Overdrive).
    /// </summary>
    public class OverdriveEffect : IItemEffect
    {
        public ItemActivationPhase ActivationPhase => ItemActivationPhase.BeforeAnswer;

        public bool IsOffensive => false;

        public bool CanBeActivated(IPlayer owner, GameContext context)
        {
            // Solo si es su turno, si la carta está visible, y si no ha usado pista aún.
            return context.ActivePlayer == owner &&
                   context.CurrentTurnContext.CurrentCard != null &&
                   !context.CurrentTurnContext.UsedHint &&
                   !context.CurrentTurnContext.HintsBlocked;
        }

        public void Execute(IPlayer owner, GameContext context)
        {
            // Otorga la pista
            GameEvents.OnHintRevealed?.Invoke(context.CurrentTurnContext.CurrentCard.Value.Hint);
            
            // Marca el Overdrive como activo para que ResolutionState no penalice
            context.CurrentTurnContext.IsOverdriveActive = true;
            
            // Aplicar Buff UI
            GameEvents.OnBuffApplied?.Invoke(owner, "Overdrive");
        }
    }
}
