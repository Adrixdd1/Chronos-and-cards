namespace ChronosAndCards.Data
{
    /// <summary>
    /// Identificadores de los estados de la máquina de estados principal.
    /// Utilizado para solicitar transiciones desde efectos desacoplados (ítems).
    /// </summary>
    public enum GameStateType
    {
        DiceRoll,
        Movement,
        CardDraw,
        Resolution,
        TileEffect,
        NextPlayer,
        Duel,
        GameOver,
        GmChallenge,
        Setup
    }
}
