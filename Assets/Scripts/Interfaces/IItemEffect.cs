using ChronosAndCards.Core;
using ChronosAndCards.Data;

namespace ChronosAndCards.Interfaces
{
    /// <summary>
    /// Contrato que define el comportamiento y las condiciones de activación
    /// de los objetos consumibles o cartas de ítems en el juego.
    /// </summary>
    public interface IItemEffect
    {
        /// <summary>Fase del ciclo del turno en la que este objeto puede ser activado.</summary>
        ItemActivationPhase ActivationPhase { get; }

        /// <summary>Indica si el efecto es ofensivo (puede ser contrarrestado y requiere Target).</summary>
        bool IsOffensive { get; }

        /// <summary>
        /// Comprueba si el objeto puede ser activado por el jugador dado el contexto actual del juego.
        /// </summary>
        /// <param name="owner">El jugador dueño o instigador del objeto.</param>
        /// <param name="context">El contexto de datos del turno actual.</param>
        /// <returns>True si se cumplen todas las condiciones para su activación.</returns>
        bool CanBeActivated(IPlayer owner, GameContext context);

        /// <summary>
        /// Ejecuta el efecto del consumible alterando la economía, la posición o las reglas.
        /// </summary>
        /// <param name="owner">El jugador que activa el objeto.</param>
        /// <param name="context">El contexto de datos del turno actual.</param>
        void Execute(IPlayer owner, GameContext context);
    }
}
