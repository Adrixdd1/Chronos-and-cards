using System.Collections.Generic;
using ChronosAndCards.Interfaces;

namespace ChronosAndCards.Core
{
    /// <summary>
    /// Contrato que representa a un jugador en el juego, definiendo su estado,
    /// posición en el tablero, pistas disponibles, e inventario de objetos consumibles.
    /// </summary>
    public interface IPlayer
    {
        /// <summary>Nombre del jugador.</summary>
        string PlayerName { get; }

        /// <summary>Índice del jugador (0-based, determina el orden de turnos).</summary>
        int PlayerIndex { get; }

        /// <summary>Posición actual en el tablero (casilla actual).</summary>
        int Position { get; }

        /// <summary>Cantidad de pistas disponibles del jugador.</summary>
        int HintCount { get; }

        /// <summary>Inventario de objetos en posesión del jugador (lista de solo lectura).</summary>
        IReadOnlyList<IItem> Inventory { get; }

        /// <summary>Indica si el jugador debe perder su siguiente turno por penalización.</summary>
        bool IsSkipNextTurn { get; }

        /// <summary>
        /// Hace avanzar al jugador una cantidad relativa de casillas en el tablero.
        /// </summary>
        /// <param name="tiles">Cantidad de casillas a avanzar.</param>
        void MoveForward(int tiles);

        /// <summary>
        /// Mueve al jugador a una posición absoluta en el tablero (usado para efectos o duelos).
        /// </summary>
        /// <param name="pos">Posición absoluta de destino.</param>
        void MoveToPosition(int pos);

        /// <summary>
        /// Añade o sustrae una cantidad de pistas al saldo del jugador.
        /// </summary>
        /// <param name="amount">Cantidad a modificar (puede ser positivo o negativo).</param>
        void AddHint(int amount);

        /// <summary>
        /// Añade un objeto consumible al inventario del jugador.
        /// </summary>
        /// <param name="item">Objeto a añadir.</param>
        void AddItem(IItem item);

        /// <summary>
        /// Remueve un objeto consumible del inventario del jugador.
        /// </summary>
        /// <param name="item">Objeto a remover.</param>
        void RemoveItem(IItem item);

        /// <summary>
        /// Define si el jugador tiene la penalización de saltar su siguiente turno.
        /// </summary>
        /// <param name="skip">True para penalizar, false para limpiar.</param>
        void SetSkipNextTurn(bool skip);
    }
}
