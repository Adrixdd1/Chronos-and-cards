namespace ChronosAndCards.Interfaces
{
    /// <summary>
    /// Contrato para el mazo de consumibles en el juego, que permite robar ítems
    /// y controlar la cantidad restante.
    /// </summary>
    public interface IItemDeck
    {
        /// <summary>
        /// Roba un objeto del mazo.
        /// </summary>
        /// <returns>La instancia del objeto IItem robado.</returns>
        IItem DrawItem();

        /// <summary>Cantidad de objetos restantes en el mazo.</summary>
        int RemainingItems { get; }

        /// <summary>Indica si el mazo se encuentra vacío.</summary>
        bool IsEmpty { get; }
    }
}
