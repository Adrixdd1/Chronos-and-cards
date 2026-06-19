namespace ChronosAndCards.Interfaces
{
    /// <summary>
    /// Fases del turno en las que un ítem puede ser activado.
    /// Cada ítem define su fase requerida; el ItemEffectExecutor
    /// solo permite la activación si la fase actual coincide.
    /// </summary>
    public enum ItemActivationPhase
    {
        /// <summary>Fase inactiva o de configuración donde no se pueden usar ítems.</summary>
        None,

        /// <summary>
        /// Antes de responder la pregunta (ej. Overdrive).
        /// El jugador ya tiene la carta pero no ha respondido.
        /// </summary>
        BeforeAnswer,

        /// <summary>
        /// Después de fallar una respuesta (ej. Eco del Tiempo).
        /// El jugador acaba de recibir el resultado negativo.
        /// </summary>
        AfterFail,

        /// <summary>
        /// Durante el turno de un rival (ej. Sabotaje, Robo de Pregunta).
        /// El ítem se usa para interferir con otro jugador.
        /// </summary>
        RivalTurn,

        /// <summary>
        /// Sustituye el turno normal del jugador (ej. Duelo de Posiciones).
        /// Se activa en lugar de tirar el dado.
        /// </summary>
        ReplaceTurn,

        /// <summary>
        /// Ventana de reacción ante un efecto ofensivo (ej. Parry).
        /// Se activa en respuesta a un ataque de otro jugador.
        /// </summary>
        Reaction
    }
}
