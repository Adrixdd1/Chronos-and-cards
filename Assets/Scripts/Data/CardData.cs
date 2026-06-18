using System;
using System.Collections.Generic;

namespace ChronosAndCards.Data
{
    /// <summary>
    /// Modelo de datos inmutable para las cartas de preguntas del juego.
    /// </summary>
    [Serializable]
    public struct CardData
    {
        /// <summary>Nivel de dificultad de la pregunta (1–6).</summary>
        public int DifficultyLevel;

        /// <summary>Texto de la pregunta a presentar al jugador.</summary>
        public string QuestionText;

        /// <summary>Pista opcional asociada a la pregunta (puede ser null).</summary>
        public string Hint;

        /// <summary>Opciones múltiples para responder (null si es de respuesta abierta).</summary>
        public List<string> Options;

        /// <summary>La respuesta correcta o el criterio de evaluación de la misma.</summary>
        public string CorrectAnswer;

        /// <summary>Indica si la carta pertenece al pool de retos del Game Master (GM).</summary>
        public bool IsGmChallenge;

        /// <summary>
        /// Constructor para inicializar una carta de pregunta con validación.
        /// </summary>
        public CardData(int difficultyLevel, string questionText, string hint, List<string> options, string correctAnswer, bool isGmChallenge = false)
        {
            if (difficultyLevel < 1 || difficultyLevel > 6)
            {
                throw new ArgumentOutOfRangeException(nameof(difficultyLevel), "El nivel de dificultad debe estar entre 1 y 6.");
            }

            DifficultyLevel = difficultyLevel;
            QuestionText = questionText;
            Hint = hint;
            Options = options;
            CorrectAnswer = correctAnswer;
            IsGmChallenge = isGmChallenge;
        }

        /// <summary>
        /// Valida si la estructura contiene valores correctos y consistentes.
        /// </summary>
        /// <returns>True si la dificultad está en el rango permitido (1-6).</returns>
        public readonly bool IsValid()
        {
            return DifficultyLevel >= 1 && DifficultyLevel <= 6;
        }
    }
}
