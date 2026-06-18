using System.Collections.Generic;
using System.Text;

namespace ChronosAndCards.Data
{
    /// <summary>
    /// Representa un error fatal encontrado durante el parsing de una pregunta.
    /// </summary>
    public struct ParseError
    {
        /// <summary>Línea exacta del archivo Markdown donde ocurrió el error.</summary>
        public int LineNumber;
        /// <summary>Mensaje descriptivo del error.</summary>
        public string Message;
        /// <summary>Contenido original de la línea.</summary>
        public string SourceLine;

        public ParseError(int lineNumber, string message, string sourceLine)
        {
            LineNumber = lineNumber;
            Message = message;
            SourceLine = sourceLine;
        }
    }

    /// <summary>
    /// Representa una advertencia de formato menor encontrada durante el parsing.
    /// </summary>
    public struct ParseWarning
    {
        /// <summary>Línea exacta del archivo Markdown donde ocurrió la advertencia.</summary>
        public int LineNumber;
        /// <summary>Mensaje descriptivo de la advertencia.</summary>
        public string Message;
        /// <summary>Contenido original de la línea.</summary>
        public string SourceLine;

        public ParseWarning(int lineNumber, string message, string sourceLine)
        {
            LineNumber = lineNumber;
            Message = message;
            SourceLine = sourceLine;
        }
    }

    /// <summary>
    /// Contiene los resultados y diagnósticos del procesamiento de un archivo de preguntas.
    /// </summary>
    public class ParseResult
    {
        /// <summary>Lista de cartas parseadas exitosamente.</summary>
        public List<CardData> Cards { get; } = new List<CardData>();

        /// <summary>Lista de errores fatales que hicieron descartar preguntas.</summary>
        public List<ParseError> Errors { get; } = new List<ParseError>();

        /// <summary>Lista de advertencias menores de formato.</summary>
        public List<ParseWarning> Warnings { get; } = new List<ParseWarning>();

        /// <summary>Indica si hay al menos una carta utilizable.</summary>
        public bool IsUsable => Cards.Count > 0;

        /// <summary>Indica si el archivo se procesó completamente limpio sin errores ni warnings.</summary>
        public bool IsClean => Errors.Count == 0 && Warnings.Count == 0;

        /// <summary>
        /// Genera un resumen legible del resultado de la validación y distribución de cartas.
        /// </summary>
        public string GetSummary()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Parsing completado: {Cards.Count} cartas válidas, {Errors.Count} errores, {Warnings.Count} warnings.");

            // Contar por nivel
            int n1 = 0, n2 = 0, n3 = 0, n4 = 0, n5 = 0, n6 = 0, gm = 0;
            foreach (var card in Cards)
            {
                if (card.IsGmChallenge)
                {
                    gm++;
                }
                else
                {
                    switch (card.DifficultyLevel)
                    {
                        case 1: n1++; break;
                        case 2: n2++; break;
                        case 3: n3++; break;
                        case 4: n4++; break;
                        case 5: n5++; break;
                        case 6: n6++; break;
                    }
                }
            }

            sb.AppendLine($"Distribución: N1={n1}, N2={n2}, N3={n3}, N4={n4}, N5={n5}, N6={n6}, GM={gm}");

            if (Errors.Count > 0)
            {
                sb.AppendLine("Errores:");
                foreach (var err in Errors)
                {
                    sb.AppendLine($"  - Línea {err.LineNumber}: {err.Message} (Línea: \"{err.SourceLine}\")");
                }
            }

            if (Warnings.Count > 0)
            {
                sb.AppendLine("Warnings:");
                foreach (var warn in Warnings)
                {
                    sb.AppendLine($"  - Línea {warn.LineNumber}: {warn.Message} (Línea: \"{warn.SourceLine}\")");
                }
            }

            return sb.ToString().TrimEnd();
        }
    }
}
