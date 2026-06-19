namespace ChronosAndCards.Gameplay.GmChallenge
{
    /// <summary>
    /// Tipos de recompensa disponibles en el Desafío del GM.
    /// Cada tipo tiene una implementación IGmReward correspondiente.
    /// </summary>
    public enum GmRewardType
    {
        /// <summary>+1 Pista y +1 Objeto del mazo.</summary>
        SupplyCrate,

        /// <summary>Token que permite responder un N6 sin penalización de multiplicador.</summary>
        GoldenHint,

        /// <summary>El ganador elige dos casillas adyacentes y las intercambia.</summary>
        BoardManipulation,

        /// <summary>Invulnerabilidad a efectos ofensivos durante 1 ronda.</summary>
        ImmunityShield
    }
}
