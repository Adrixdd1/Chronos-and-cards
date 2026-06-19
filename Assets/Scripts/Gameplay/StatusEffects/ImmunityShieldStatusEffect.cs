using UnityEngine;
using ChronosAndCards.Interfaces;
using ChronosAndCards.Gameplay;

namespace ChronosAndCards.Gameplay.StatusEffects
{
    /// <summary>
    /// Efecto de estado: "Escudo de Inmunidad".
    /// Protege al jugador de todos los efectos ofensivos (Sabotaje, Robo, Duelo)
    /// durante 1 ronda completa. Se remueve automáticamente al inicio
    /// de la ronda posterior a la de protección.
    /// </summary>
    public class ImmunityShieldStatusEffect : IStatusEffect
    {
        private readonly int _expirationRound;

        public string Name => "Escudo de Inmunidad";
        public string Description => "Invulnerable a efectos ofensivos.";
        public StatusEffectDuration Duration => StatusEffectDuration.Rounds;
        public bool IsActive { get; private set; } = true;

        /// <summary>Ronda en la que expira el escudo.</summary>
        public int ExpirationRound => _expirationRound;

        /// <param name="expirationRound">Ronda en la que expira (exclusive).</param>
        public ImmunityShieldStatusEffect(int expirationRound)
        {
            _expirationRound = expirationRound;
        }

        /// <summary>
        /// Verifica si el escudo protege contra un efecto ofensivo en el turno actual.
        /// </summary>
        public bool ProtectsAgainstOffensive()
        {
            return IsActive;
        }

        /// <summary>
        /// Actualiza el estado del escudo basándose en la ronda actual.
        /// </summary>
        public void OnRoundChanged(int currentRound)
        {
            if (currentRound >= _expirationRound)
            {
                IsActive = false;
                Debug.Log("ImmunityShieldStatusEffect: Escudo de Inmunidad expirado.");
            }
        }

        /// <summary>Indica si el efecto ha expirado y debe ser removido.</summary>
        public bool IsExpired => !IsActive;

        /// <summary>
        /// No aplica nada al TurnContext directamente.
        /// La protección se verifica en ItemEffectExecutor.
        /// </summary>
        public bool ShouldActivate(TurnContext turnContext) => false;
        public void ApplyToTurn(TurnContext turnContext) { }
    }
}
