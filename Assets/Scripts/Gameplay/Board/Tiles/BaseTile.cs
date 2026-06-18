using ChronosAndCards.Interfaces;
using ChronosAndCards.Core;
using ChronosAndCards.Data;

namespace ChronosAndCards.Gameplay.Board
{
    /// <summary>
    /// Clase abstracta base que implementa el ciclo y comportamiento común
    /// de todas las casillas (Tiles) lógicas del tablero.
    /// </summary>
    public abstract class BaseTile : ITile
    {
        /// <summary>Tipo de casilla de dominio.</summary>
        public abstract TileType Type { get; }

        /// <summary>Índice secuencial de la casilla (0-based).</summary>
        public int Index { get; }

        /// <summary>Indica si la casilla está descubierta y visible para los jugadores (modo Exploración).</summary>
        public bool IsDiscovered { get; set; }

        /// <summary>
        /// Constructor para establecer el índice único de la casilla.
        /// </summary>
        public BaseTile(int index)
        {
            Index = index;
            IsDiscovered = false;
        }

        /// <summary>
        /// Método abstracto para ejecutar la lógica particular de cada casilla al caer en ella.
        /// </summary>
        public abstract void OnPlayerLanded(IPlayer player);

        /// <summary>
        /// Emite el evento global de aplicación de efecto de casilla.
        /// </summary>
        protected void EmitTileEffect(IPlayer player)
        {
            GameEvents.OnTileEffectApplied?.Invoke(player, Type);
        }
    }
}
