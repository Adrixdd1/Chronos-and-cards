using System.Collections.Generic;
using ChronosAndCards.Data;

namespace ChronosAndCards.Interfaces
{
    /// <summary>
    /// Contrato que define el generador procedural o secuencial de casillas del tablero
    /// a partir de un ScriptableObject de configuración.
    /// </summary>
    public interface IBoardGenerator
    {
        /// <summary>
        /// Crea una secuencia estructurada de casillas basada en la configuración provista.
        /// </summary>
        /// <param name="config">Configuración y pesos de casillas del tablero.</param>
        /// <returns>Lista ordenada de casillas generadas.</returns>
        List<ITile> GenerateBoard(BoardConfig config);
    }
}
