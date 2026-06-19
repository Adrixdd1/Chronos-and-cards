using ChronosAndCards.Gameplay;

namespace ChronosAndCards.Interfaces
{
    /// <summary>
    /// Interfaz para efectos de estado temporales aplicados a un jugador.
    /// Los efectos pueden tener diferentes duraciones (por rondas, por turnos,
    /// de un solo uso) y se aplican/expiran automáticamente.
    /// </summary>
    public interface IStatusEffect
    {
        /// <summary>Nombre del efecto para la UI.</summary>
        string Name { get; }

        /// <summary>Descripción del efecto para la UI.</summary>
        string Description { get; }

        /// <summary>Tipo de duración del efecto.</summary>
        StatusEffectDuration Duration { get; }

        /// <summary>Indica si el efecto está activo.</summary>
        bool IsActive { get; }

        /// <summary>Indica si el efecto ha expirado y debe ser removido.</summary>
        bool IsExpired { get; }

        /// <summary>
        /// Verifica si este efecto debe activarse en el turno actual.
        /// </summary>
        bool ShouldActivate(TurnContext turnContext);

        /// <summary>
        /// Aplica el efecto al turno actual (ej. activa Overdrive, bloquea pistas, etc.).
        /// Solo se llama si ShouldActivate() retornó true.
        /// </summary>
        void ApplyToTurn(TurnContext turnContext);
    }
}
