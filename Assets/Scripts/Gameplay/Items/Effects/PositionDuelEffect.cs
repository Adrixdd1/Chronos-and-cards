using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Data;
using ChronosAndCards.Interfaces;

namespace ChronosAndCards.Gameplay.Items.Effects
{
    /// <summary>
    /// Efecto Ofensivo: Inicia un Duelo de Posiciones.
    /// Sustituye el turno normal del jugador.
    /// </summary>
    public class PositionDuelEffect : IItemEffect
    {
        public ItemActivationPhase ActivationPhase => ItemActivationPhase.ReplaceTurn;

        public bool IsOffensive => true;

        public bool CanBeActivated(IPlayer owner, GameContext context)
        {
            // Solo en su propio turno
            return context.ActivePlayer == owner;
        }

        public void Execute(IPlayer owner, GameContext context)
        {
            // Opcional: El TargetPlayer podría ser seteado por la UI antes de ejecutar, 
            // pero si no lo está, DuelState pedirá seleccionarlo.
            
            // Empuja el contexto del duelo
            context.PushSubTurn(this);

            // Transiciona al estado de duelo
            context.RequestStateTransition?.Invoke(GameStateType.Duel);
        }
    }
}
