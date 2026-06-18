using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ChronosAndCards.Interfaces;

namespace ChronosAndCards.Data
{
    /// <summary>
    /// Motor de parsing encargado de interpretar contenido Markdown y construir cartas de preguntas.
    /// </summary>
    public class MarkdownContentParser : IContentParser
    {
        private static readonly Regex NormalHeaderRegex = new Regex(@"^#\s*\[(\d+)\]\s*(.+)$", RegexOptions.Compiled);
        private static readonly Regex GmHeaderRegex = new Regex(@"^##\s*\[GM\]\s*(.+)$", RegexOptions.Compiled);

        /// <summary>
        /// Parsea el contenido Markdown y retorna la lista de cartas válidas ordenadas por dificultad.
        /// </summary>
        public List<CardData> Parse(string markdownContent)
        {
            ParseResult result = ParseWithDetails(markdownContent);
            return result.Cards;
        }

        /// <summary>
        /// Parsea el contenido Markdown y retorna el ParseResult detallado con diagnósticos.
        /// </summary>
        public ParseResult ParseWithDetails(string markdownContent)
        {
            ParseResult result = new ParseResult();

            if (string.IsNullOrEmpty(markdownContent))
            {
                return result;
            }

            string[] lines = markdownContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // Estado del acumulador de la pregunta actual
            int? currentDifficulty = null;
            string currentQuestionText = null;
            List<string> currentOptions = new List<string>();
            List<string> currentHintLines = new List<string>();
            string currentAnswer = null;
            bool currentIsGm = false;
            int questionStartLine = -1;
            bool isCurrentQuestionValid = false;

            // Función local para procesar y guardar la pregunta acumulada
            void SaveCurrentQuestion()
            {
                if (questionStartLine == -1) return;

                if (!isCurrentQuestionValid)
                {
                    // Ya se reportó un error fatal para esta pregunta
                    return;
                }

                if (string.IsNullOrEmpty(currentAnswer))
                {
                    result.Errors.Add(new ParseError(questionStartLine, "Pregunta descartada: Falta la respuesta correcta ('=' obligatorio).", currentQuestionText));
                    return;
                }

                if (currentDifficulty == null || currentDifficulty < 1 || currentDifficulty > 6)
                {
                    result.Errors.Add(new ParseError(questionStartLine, "Pregunta descartada: Dificultad fuera del rango permitido (1–6).", currentQuestionText));
                    return;
                }

                // Warnings no fatales
                if (currentOptions.Count == 0)
                {
                    result.Warnings.Add(new ParseWarning(questionStartLine, "Pregunta sin opciones múltiples (se tratará como pregunta abierta).", currentQuestionText));
                }
                if (currentHintLines.Count == 0)
                {
                    result.Warnings.Add(new ParseWarning(questionStartLine, "Pregunta sin pista definida.", currentQuestionText));
                }

                string hintText = currentHintLines.Count > 0 ? string.Join("\n", currentHintLines) : null;
                List<string> optionsList = currentOptions.Count > 0 ? new List<string>(currentOptions) : null;

                try
                {
                    CardData card = new CardData(
                        currentDifficulty.Value,
                        currentQuestionText,
                        hintText,
                        optionsList,
                        currentAnswer,
                        currentIsGm
                    );
                    result.Cards.Add(card);
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new ParseError(questionStartLine, $"Error al inicializar CardData: {ex.Message}", currentQuestionText));
                }
            }

            for (int i = 0; i < lines.Length; i++)
            {
                int lineNumber = i + 1;
                string line = lines[i];
                string trimmed = line.Trim();

                if (string.IsNullOrEmpty(trimmed))
                {
                    continue; // Líneas vacías se omiten
                }

                // 1. Detección de Headers
                if (trimmed.StartsWith("#"))
                {
                    // Guardar la pregunta previa
                    SaveCurrentQuestion();

                    // Reiniciar acumulador
                    currentDifficulty = null;
                    currentQuestionText = null;
                    currentOptions.Clear();
                    currentHintLines.Clear();
                    currentAnswer = null;
                    currentIsGm = false;
                    questionStartLine = lineNumber;
                    isCurrentQuestionValid = true;

                    Match gmMatch = GmHeaderRegex.Match(trimmed);
                    if (gmMatch.Success)
                    {
                        currentDifficulty = 5; // Dificultad por defecto para GM
                        currentQuestionText = gmMatch.Groups[1].Value.Trim();
                        currentIsGm = true;
                    }
                    else
                    {
                        Match normalMatch = NormalHeaderRegex.Match(trimmed);
                        if (normalMatch.Success)
                        {
                            if (int.TryParse(normalMatch.Groups[1].Value, out int diff))
                            {
                                currentDifficulty = diff;
                                currentQuestionText = normalMatch.Groups[2].Value.Trim();
                                if (diff < 1 || diff > 6)
                                {
                                    result.Errors.Add(new ParseError(lineNumber, $"Nivel de dificultad '{diff}' fuera del rango permitido (1–6).", trimmed));
                                    isCurrentQuestionValid = false;
                                }
                            }
                            else
                            {
                                result.Errors.Add(new ParseError(lineNumber, "Nivel de dificultad no numérico.", trimmed));
                                isCurrentQuestionValid = false;
                            }
                        }
                        else
                        {
                            result.Errors.Add(new ParseError(lineNumber, "Header malformado. Debe coincidir con '# [Dificultad] Pregunta' o '## [GM] Pregunta'.", trimmed));
                            isCurrentQuestionValid = false;
                        }
                    }
                }
                // 2. Acumuladores de campos
                else if (trimmed.StartsWith(">"))
                {
                    if (questionStartLine == -1)
                    {
                        result.Warnings.Add(new ParseWarning(lineNumber, "Pista declarada fuera de una pregunta activa. Omitida.", trimmed));
                        continue;
                    }

                    string hintPart = trimmed.Substring(1).Trim();
                    currentHintLines.Add(hintPart);
                }
                else if (trimmed.StartsWith("-") || trimmed.StartsWith("*"))
                {
                    if (questionStartLine == -1)
                    {
                        result.Warnings.Add(new ParseWarning(lineNumber, "Opción declarada fuera de una pregunta activa. Omitida.", trimmed));
                        continue;
                    }

                    string optionPart = trimmed.Substring(1).Trim();
                    if (currentOptions.Contains(optionPart))
                    {
                        result.Warnings.Add(new ParseWarning(lineNumber, $"Opción duplicada: '{optionPart}'.", trimmed));
                    }
                    currentOptions.Add(optionPart);
                }
                else if (trimmed.StartsWith("="))
                {
                    if (questionStartLine == -1)
                    {
                        result.Warnings.Add(new ParseWarning(lineNumber, "Respuesta declarada fuera de una pregunta activa. Omitida.", trimmed));
                        continue;
                    }

                    if (!string.IsNullOrEmpty(currentAnswer))
                    {
                        result.Warnings.Add(new ParseWarning(lineNumber, "Respuesta duplicada para la misma pregunta. Sobrescribiendo.", trimmed));
                    }
                    currentAnswer = trimmed.Substring(1).Trim();
                }
                else
                {
                    // Línea no reconocida
                    result.Warnings.Add(new ParseWarning(lineNumber, $"Línea no reconocida: \"{trimmed}\". Omitida.", line));
                }
            }

            // Guardar la última pregunta al terminar el archivo
            SaveCurrentQuestion();

            // Ordenar por dificultad
            result.Cards.Sort((c1, c2) => c1.DifficultyLevel.CompareTo(c2.DifficultyLevel));

            // Validación de balance
            ValidateContentBalance(result);

            return result;
        }

        private void ValidateContentBalance(ParseResult result)
        {
            if (result.Cards.Count == 0) return;

            // Verificar si hay alguna dificultad sin preguntas
            HashSet<int> presentLevels = new HashSet<int>();
            foreach (var card in result.Cards)
            {
                if (!card.IsGmChallenge)
                {
                    presentLevels.Add(card.DifficultyLevel);
                }
            }

            for (int i = 1; i <= 6; i++)
            {
                if (!presentLevels.Contains(i))
                {
                    result.Warnings.Add(new ParseWarning(0, $"Mazo desbalanceado: No se encontraron preguntas para la dificultad {i}.", "N/A"));
                }
            }
        }
    }
}
