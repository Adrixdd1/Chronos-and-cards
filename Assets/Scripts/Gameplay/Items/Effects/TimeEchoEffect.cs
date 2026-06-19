using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Data;
using ChronosAndCards.Interfaces;

namespace ChronosAndCards.Gameplay.Items.Effects
{
    /// <summary>
    /// Efecto: Si el jugador falla, puede repetir la tirada de dado.
    /// Bloquea el uso de ayudas (pistas/opciones) en el reintento.
    /// </summary>
    public class TimeEchoEffect : IItemEffect
    {
        public ItemActivationPhase ActivationPhase => ItemActivationPhase.AfterFail;

        public bool IsOffensive => false;

        public bool CanBeActivated(IPlayer owner, GameContext context)
        {
            // Solo se puede usar en el turno del dueño, y si acaba de fallar.
            return context.ActivePlayer == owner &&
                   context.CurrentTurnContext.FailedThisTurn &&
                   !context.CurrentTurnContext.UsedTimeEcho;
        }

        public void Execute(IPlayer owner, GameContext context)
        {
            // Bloquear ayudas
            context.CurrentTurnContext.HintsBlocked = true;
            context.CurrentTurnContext.OptionsBlocked = true;
            
            // Marcar uso para evitar bucles infinitos
            context.CurrentTurnContext.UsedTimeEcho = true;
            context.CurrentTurnContext.FailedThisTurn = false; // Reset de la bandera

            // Solicitar transición para repetir tirada
            context.RequestStateTransition?.Invoke(GameStateType.DiceRoll);
            
            // Aplicar Buff UI
            GameEvents.OnBuffApplied?.Invoke(owner, "Eco del Tiempo");
        }
    }
}
