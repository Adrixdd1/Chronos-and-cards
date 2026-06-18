using System.Collections.Generic;
using ChronosAndCards.Data;

namespace ChronosAndCards.Interfaces
{
    /// <summary>
    /// Contrato responsable de interpretar el contenido en formato Markdown
    /// provisto por el GM para construir el pool de cartas de preguntas estructuradas.
    /// </summary>
    public interface IContentParser
    {
        /// <summary>
        /// Procesa un bloque de texto Markdown y lo convierte en una lista de estructuras CardData.
        /// </summary>
        /// <param name="markdownContent">Texto plano en formato Markdown.</param>
        /// <returns>Lista de preguntas parseadas.</returns>
        List<CardData> Parse(string markdownContent);
    }
}
