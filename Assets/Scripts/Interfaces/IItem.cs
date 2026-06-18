namespace ChronosAndCards.Interfaces
{
    /// <summary>
    /// Contrato base para los objetos consumibles en el juego.
    /// </summary>
    public interface IItem
    {
        /// <summary>Nombre del objeto.</summary>
        string Name { get; }

        /// <summary>Descripción detallada del efecto del objeto.</summary>
        string Description { get; }
    }
}
