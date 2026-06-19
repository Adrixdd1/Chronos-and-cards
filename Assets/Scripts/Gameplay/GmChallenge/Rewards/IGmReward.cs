using ChronosAndCards.Core;
using ChronosAndCards.Data;
using ChronosAndCards.Gameplay.GmChallenge;

namespace ChronosAndCards.Gameplay.GmChallenge.Rewards
{
    /// <summary>
    /// Interfaz para las recompensas del Desafío del GM.
    /// Cada recompensa implementa su efecto de forma independiente
    /// siguiendo el patrón Strategy.
    /// </summary>
    public interface IGmReward
    {
        /// <summary>Nombre visible de la recompensa.</summary>
        string Name { get; }

        /// <summary>Descripción del efecto para la UI.</summary>
        string Description { get; }

        /// <summary>Tipo de recompensa para identificación.</summary>
        GmRewardType RewardType { get; }

        /// <summary>
        /// Aplica el efecto de la recompensa al jugador ganador.
        /// </summary>
        /// <param name="winner">El jugador que ganó el desafío.</param>
        /// <param name="context">Contexto del juego para acceso a sistemas.</param>
        void Apply(IPlayer winner, GameContext context);

        /// <summary>
        /// Verifica si la recompensa requiere interacción adicional del jugador.
        /// (ej. BoardManipulation necesita selección de casillas).
        /// </summary>
        bool RequiresInteraction { get; }
    }
}
