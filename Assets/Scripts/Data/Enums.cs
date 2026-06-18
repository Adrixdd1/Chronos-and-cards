namespace ChronosAndCards.Data
{
    /// <summary>
    /// Tipos de casilla del tablero.
    /// </summary>
    public enum TileType
    {
        /// <summary>Flujo normal.</summary>
        Neutral,
        /// <summary>+1 Pista.</summary>
        HintBoost,
        /// <summary>-1 Pista.</summary>
        HintTrap,
        /// <summary>Altera reglas temporalmente.</summary>
        Event,
        /// <summary>El jugador roba un objeto.</summary>
        Item
    }

    /// <summary>
    /// Fase en la que un objeto consumible puede activarse.
    /// </summary>
    public enum ItemActivationPhase
    {
        /// <summary>Antes de responder (ej. Overdrive).</summary>
        BeforeAnswer,
        /// <summary>Tras un fallo (ej. Eco del Tiempo).</summary>
        AfterFail,
        /// <summary>Durante el turno del rival (ej. Sabotaje, Robo).</summary>
        RivalTurn,
        /// <summary>Como reacción a un efecto entrante (ej. Parry).</summary>
        Reaction,
        /// <summary>Sustituye el turno normal (ej. Duelo de Posiciones).</summary>
        ReplaceTurn
    }

    /// <summary>
    /// Multiplicador de desempeño para la fórmula de avance.
    /// </summary>
    public enum PerformanceMultiplier
    {
        /// <summary>x1.0 — Sin ayuda.</summary>
        Perfect = 100,
        /// <summary>x0.5 — Usó pista o reveló opciones.</summary>
        WithHelp = 50,
        /// <summary>x0.0 — Respuesta incorrecta.</summary>
        Fail = 0
    }

    /// <summary>
    /// Modos de juego / tipos de tablero.
    /// </summary>
    public enum BoardMode
    {
        /// <summary>Ruta directa inicio-fin (Tipo Oca).</summary>
        Linear,
        /// <summary>Red de nodos tipo laberinto (Multipath).</summary>
        Exploration
    }
    
    /// <summary>
    /// Acción sobre el inventario para eventos.
    /// </summary>
    public enum InventoryAction
    {
        /// <summary>Objeto añadido al inventario.</summary>
        Added,
        /// <summary>Objeto removido del inventario.</summary>
        Removed,
        /// <summary>Objeto consumido/usado.</summary>
        Used
    }

    /// <summary>
    /// Tipo de modificador temporal de reglas que aplica una casilla de Evento.
    /// </summary>
    public enum TileEventType
    {
        /// <summary>"Siguiente pista gratuita" — no reduce multiplicador.</summary>
        FreeHint,
        /// <summary>"Avanza N casillas extra".</summary>
        ExtraMovement,
        /// <summary>"Tu próximo movimiento se reduce a la mitad".</summary>
        ReducedMovement,
        /// <summary>"Intercambia posición con un jugador al azar".</summary>
        SwapPositionRandom,
        /// <summary>"Tu próxima respuesta correcta vale el doble".</summary>
        DoubleReward,
        /// <summary>"Avanza sin responder pregunta".</summary>
        SkipQuestion
    }
}
