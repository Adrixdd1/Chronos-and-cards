using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Data;
using ChronosAndCards.Interfaces;

namespace ChronosAndCards.Gameplay.Items.Effects
{
    /// <summary>
    /// Efecto Defensivo: Bloquea un efecto ofensivo dirigido al usuario (Reaction Window).
    /// El agresor recibe una penalización.
    /// </summary>
    public class ParryEffect : IItemEffect
    {
        public ItemActivationPhase ActivationPhase => ItemActivationPhase.Reaction;

        public bool IsOffensive => false;

        public bool CanBeActivated(IPlayer owner, GameContext context)
        {
            // Solo se puede usar si hay un efecto ofensivo pendiente dirigido al dueño
            return context.PendingOffensiveEffect != null &&
                   context.PendingOffensiveEffect.Target == owner &&
                   !context.PendingOffensiveEffect.IsCountered;
        }

        public void Execute(IPlayer owner, GameContext context)
        {
            var pendingEffect = context.PendingOffensiveEffect;
            if (pendingEffect == null) return;

            // Bloquea el efecto pendiente
            pendingEffect.IsCountered = true;

            // Dispara el evento de contraataque para la UI y logs
            GameEvents.OnCounterActivated?.Invoke(owner, pendingEffect.Attacker, "Parry");

            // Penalización al agresor: pierde su siguiente turno
            pendingEffect.Attacker.SetSkipNextTurn(true);
            
            // Opcional: También se podría cancelar el turno actual del agresor si fuera su turno
            if (context.ActivePlayer == pendingEffect.Attacker)
            {
                context.CurrentTurnContext.TurnCancelled = true;
            }

            Debug.Log($"ParryEffect: {owner.PlayerName} bloqueó el ataque de {pendingEffect.Attacker.PlayerName}. El atacante pierde su siguiente turno.");
        }
    }
}
