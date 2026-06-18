using System;
using System.Collections.Generic;
using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Data;

namespace ChronosAndCards.Gameplay
{
    /// <summary>
    /// Estructura de resultado tras la evaluación de una respuesta.
    /// </summary>
    public struct EvaluationResult
    {
        /// <summary>Indica si la respuesta fue correcta (en caso de evaluación directa).</summary>
        public bool IsCorrect;
        /// <summary>El multiplicador resultante del desempeño.</summary>
        public PerformanceMultiplier Multiplier;
        /// <summary>Indica si la pregunta es abierta y requiere el juicio del GM.</summary>
        public bool RequiresGmJudgment;
    }

    /// <summary>
    /// Evaluador de respuestas encargado de comparar el input del jugador con la respuesta correcta y calcular penalizaciones.
    /// </summary>
    public class AnswerEvaluator
    {
        /// <summary>
        /// Evalúa la respuesta provista por el jugador para la carta actual, calculando el multiplicador de avance.
        /// </summary>
        public EvaluationResult Evaluate(string playerAnswer, CardData card, TurnContext turnContext)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext), "El TurnContext no puede ser nulo.");
            }

            bool requiresGm = IsOpenEndedQuestion(card);

            bool isCorrect = false;
            if (!requiresGm)
            {
                isCorrect = IsAnswerCorrect(playerAnswer, card, turnContext);
            }

            PerformanceMultiplier multiplier;

            if (requiresGm)
            {
                // Por defecto, las preguntas abiertas inician en Fail hasta que el GM juzgue.
                multiplier = PerformanceMultiplier.Fail;
            }
            else if (!isCorrect)
            {
                multiplier = PerformanceMultiplier.Fail;
            }
            else if (turnContext.UsedHint || turnContext.RevealedOptions)
            {
                // Penalizado con WithHelp a menos que Overdrive esté activo
                multiplier = turnContext.IsOverdriveActive 
                    ? PerformanceMultiplier.Perfect 
                    : PerformanceMultiplier.WithHelp;
            }
            else
            {
                multiplier = PerformanceMultiplier.Perfect;
            }

            return new EvaluationResult
            {
                IsCorrect = isCorrect,
                Multiplier = multiplier,
                RequiresGmJudgment = requiresGm
            };
        }

        /// <summary>
        /// Marca las opciones de una carta como reveladas en el contexto del turno y emite el evento correspondiente.
        /// </summary>
        public void RevealOptions(CardData card, TurnContext turnContext)
        {
            if (turnContext == null) return;

            turnContext.RevealedOptions = true;
            GameEvents.OnOptionsRevealed?.Invoke(card.Options ?? new List<string>());
            Debug.Log("AnswerEvaluator: Opciones múltiples reveladas al jugador. Penalización aplicada.");
        }

        private bool IsAnswerCorrect(string playerAnswer, CardData card, TurnContext context)
        {
            if (string.IsNullOrEmpty(card.CorrectAnswer) || playerAnswer == null)
            {
                return false;
            }

            return string.Equals(playerAnswer.Trim(), card.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private bool IsOpenEndedQuestion(CardData card)
        {
            bool hasNoOptions = card.Options == null || card.Options.Count == 0;
            bool startsWithGmCriteria = card.CorrectAnswer != null && card.CorrectAnswer.StartsWith("[Criterio del GM:", StringComparison.OrdinalIgnoreCase);

            return hasNoOptions && startsWithGmCriteria;
        }
    }
}
