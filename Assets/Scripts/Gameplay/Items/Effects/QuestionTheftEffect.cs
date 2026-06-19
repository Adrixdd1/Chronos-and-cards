using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Data;
using ChronosAndCards.Interfaces;

namespace ChronosAndCards.Gameplay.Items.Effects
{
    /// <summary>
    /// Efecto Ofensivo: Roba la pregunta del rival.
    /// El rival pierde su turno actual, y el usuario debe responder la misma pregunta.
    /// </summary>
    public class QuestionTheftEffect : IItemEffect
    {
        public ItemActivationPhase ActivationPhase => ItemActivationPhase.RivalTurn;

        public bool IsOffensive => true;

        public bool CanBeActivated(IPlayer owner, GameContext context)
        {
            // Solo en el turno de otro jugador, antes de que este responda.
            return context.ActivePlayer != owner &&
                   context.CurrentTurnContext.CurrentCard != null;
        }

        public void Execute(IPlayer owner, GameContext context)
        {
            var target = context.TargetPlayer ?? context.ActivePlayer;

            // El usuario ahora es el jugador activo para este turno
            context.ActivePlayer = owner;
            
            // Opcional: Reiniciar estado de ayudas si el rival había usado alguna
            context.CurrentTurnContext.UsedHint = false;
            context.CurrentTurnContext.RevealedOptions = false;
            context.CurrentTurnContext.IsSabotaged = false;

            // Aplicar Debuff UI (notifica que se robó la pregunta)
            GameEvents.OnDebuffApplied?.Invoke(owner, target, "Robo de Pregunta");

            // Solicitar reiniciar el estado de resolución para el nuevo jugador
            context.RequestStateTransition?.Invoke(GameStateType.Resolution);
        }
    }
}
