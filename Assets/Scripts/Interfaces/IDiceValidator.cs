namespace ChronosAndCards.Interfaces
{
    /// <summary>
    /// Contrato encargado de validar la exactitud o la legalidad de un resultado
    /// arrojado por el dado.
    /// </summary>
    public interface IDiceValidator
    {
        /// <summary>
        /// Comprueba si el resultado del dado está en un rango permitido u obedece las reglas.
        /// </summary>
        /// <param name="result">El número a validar.</param>
        /// <returns>True si es un valor legal.</returns>
        bool IsValidResult(int result);
    }
}
