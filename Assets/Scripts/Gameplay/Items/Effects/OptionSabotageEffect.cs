using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Data;
using ChronosAndCards.Interfaces;

namespace ChronosAndCards.Gameplay.Items.Effects
{
    /// <summary>
    /// Efecto Ofensivo: Oculta las opciones múltiples de un rival en su turno.
    /// </summary>
    public class OptionSabotageEffect : IItemEffect
    {
        public ItemActivationPhase ActivationPhase => ItemActivationPhase.RivalTurn;

        public bool IsOffensive => true;

        public bool CanBeActivated(IPlayer owner, GameContext context)
        {
            // Solo en el turno de otro jugador, antes de que este responda o revele opciones.
            return context.ActivePlayer != owner &&
                   context.CurrentTurnContext.CurrentCard != null &&
                   !context.CurrentTurnContext.RevealedOptions &&
                   !context.CurrentTurnContext.IsSabotaged;
        }

        public void Execute(IPlayer owner, GameContext context)
        {
            var target = context.TargetPlayer ?? context.ActivePlayer; // Normalmente el active player

            // Bloquea las opciones y registra al saboteador
            context.CurrentTurnContext.OptionsBlocked = true;
            context.CurrentTurnContext.IsSabotaged = true;
            context.CurrentTurnContext.SabotagedBy = owner;

            // Aplicar Debuff UI
            GameEvents.OnDebuffApplied?.Invoke(owner, target, "Sabotaje de Opciones");
        }
    }
}
