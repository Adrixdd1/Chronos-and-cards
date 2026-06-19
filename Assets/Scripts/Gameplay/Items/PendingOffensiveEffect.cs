using ChronosAndCards.Interfaces;

namespace ChronosAndCards.Gameplay.Items
{
    /// <summary>
    /// Representa un efecto ofensivo pendiente que puede ser contrarrestado.
    /// Se crea cuando un efecto ofensivo se activa y se mantiene durante la ventana de reacción.
    /// </summary>
    public class PendingOffensiveEffect
    {
        /// <summary>Jugador que activó el efecto ofensivo.</summary>
        public IPlayer Attacker { get; set; }

        /// <summary>Jugador objetivo del efecto ofensivo.</summary>
        public IPlayer Target { get; set; }

        /// <summary>El ítem ofensivo que fue usado.</summary>
        public IItem UsedItem { get; set; }

        /// <summary>Si true, el efecto fue anulado por un counter (hold-then-apply).</summary>
        public bool IsCountered { get; set; }

        /// <summary>Tiempo restante de la ventana de reacción (en segundos).</summary>
        public float ReactionTimeRemaining { get; set; }
    }
}
