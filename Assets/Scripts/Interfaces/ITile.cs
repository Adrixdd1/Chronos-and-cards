using ChronosAndCards.Core;
using ChronosAndCards.Data;

namespace ChronosAndCards.Interfaces
{
    /// <summary>
    /// Contrato para las casillas del tablero, controlando su tipo y la lógica
    /// gatillada cuando un jugador aterriza en ellas.
    /// </summary>
    public interface ITile
    {
        /// <summary>Tipo de casilla de acuerdo al enum de dominio TileType.</summary>
        TileType Type { get; }

        /// <summary>
        /// Se ejecuta cuando un jugador aterriza en esta casilla.
        /// </summary>
        /// <param name="player">Jugador que ha aterrizado.</param>
        void OnPlayerLanded(IPlayer player);
    }
}
