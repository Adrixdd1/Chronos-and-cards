namespace ChronosAndCards.Interfaces
{
    /// <summary>
    /// Define la duración de un efecto de estado temporal.
    /// </summary>
    public enum StatusEffectDuration
    {
        /// <summary>Se consume tras un uso (ej. Pista Dorada).</summary>
        SingleUse,

        /// <summary>Dura N rondas completas (ej. Escudo de Inmunidad).</summary>
        Rounds,

        /// <summary>Dura N turnos del jugador afectado.</summary>
        Turns,

        /// <summary>Permanente hasta remoción explícita.</summary>
        Permanent
    }
}
