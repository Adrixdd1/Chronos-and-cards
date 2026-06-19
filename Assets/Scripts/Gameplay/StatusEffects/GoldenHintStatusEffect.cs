using UnityEngine;
using ChronosAndCards.Interfaces;

namespace ChronosAndCards.Gameplay.StatusEffects
{
    /// <summary>
    /// Efecto de estado: "Pista Dorada".
    /// Se activa cuando el jugador enfrenta una pregunta de Nivel 6:
    /// las pistas y revelación de opciones NO reducen el multiplicador.
    /// Se consume tras un uso (1 shot).
    /// </summary>
    public class GoldenHintStatusEffect : IStatusEffect
    {
        public string Name => "Pista Dorada";
        public string Description => "Tu próxima pregunta N6 no penaliza el multiplicador.";
        public StatusEffectDuration Duration => StatusEffectDuration.SingleUse;
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// Verifica si este efecto aplica al turno actual.
        /// Solo se activa si la carta del turno es de Nivel 6.
        /// </summary>
        public bool ShouldActivate(TurnContext turnContext)
        {
            return IsActive
                && turnContext.CurrentCard != null
                && turnContext.CurrentCard.Value.DifficultyLevel == 6;
        }

        /// <summary>
        /// Aplica el efecto al turno: activa IsOverdriveActive
        /// para que pistas/opciones no reduzcan el multiplicador.
        /// </summary>
        public void ApplyToTurn(TurnContext turnContext)
        {
            if (!ShouldActivate(turnContext)) return;

            turnContext.IsOverdriveActive = true;
            IsActive = false; // Consumido

            Debug.Log("GoldenHintStatusEffect: Pista Dorada activada. El multiplicador no se reducirá por ayuda en esta pregunta N6.");
        }

        /// <summary>Indica si el efecto ha expirado y debe ser removido.</summary>
        public bool IsExpired => !IsActive;
    }
}
