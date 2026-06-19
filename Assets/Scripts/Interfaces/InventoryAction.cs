namespace ChronosAndCards.Interfaces
{
    /// <summary>Tipos de acción sobre el inventario de un jugador.</summary>
    public enum InventoryAction
    {
        /// <summary>Se añadió un ítem al inventario.</summary>
        Added,

        /// <summary>Se removió un ítem del inventario (consumido o robado).</summary>
        Removed
    }
}
