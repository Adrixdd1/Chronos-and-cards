namespace ChronosAndCards.Interfaces
{
    /// <summary>
    /// Interfaz que representa un efecto de estado temporal aplicado a un jugador.
    /// </summary>
    public interface IStatusEffect
    {
        /// <summary>Nombre identificador del efecto.</summary>
        string Name { get; }

        /// <summary>Cantidad de turnos restantes del efecto.</summary>
        int RemainingTurns { get; set; }

        /// <summary>Indica si el efecto ha expirado.</summary>
        bool IsExpired => RemainingTurns <= 0;
    }
}
