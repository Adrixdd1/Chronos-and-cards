using NUnit.Framework;
using System.Collections.Generic;
using ChronosAndCards.Data;
using ChronosAndCards.Gameplay;

namespace ChronosAndCards.Tests.Content
{
    [TestFixture]
    public class AnswerEvaluatorTests
    {
        private AnswerEvaluator _evaluator;
        private TurnContext _turnContext;

        [SetUp]
        public void SetUp()
        {
            _evaluator = new AnswerEvaluator();
            _turnContext = new TurnContext();
            _turnContext.Reset();
        }

        [Test]
        public void Evaluate_CorrectAnswer_PerfectMultiplier()
        {
            var card = new CardData(1, "Pregunta?", null, null, "Respuesta");

            EvaluationResult result = _evaluator.Evaluate("Respuesta", card, _turnContext);

            Assert.IsTrue(result.IsCorrect);
            Assert.AreEqual(PerformanceMultiplier.Perfect, result.Multiplier);
            Assert.IsFalse(result.RequiresGmJudgment);
        }

        [Test]
        public void Evaluate_CorrectWithHint_WithHelpMultiplier()
        {
            var card = new CardData(1, "Pregunta?", "Hint", null, "Respuesta");
            _turnContext.UsedHint = true;

            EvaluationResult result = _evaluator.Evaluate("Respuesta", card, _turnContext);

            Assert.IsTrue(result.IsCorrect);
            Assert.AreEqual(PerformanceMultiplier.WithHelp, result.Multiplier);
        }

        [Test]
        public void Evaluate_CorrectWithRevealedOptions_WithHelpMultiplier()
        {
            var card = new CardData(1, "Pregunta?", null, new List<string> { "Respuesta", "Opcion B" }, "Respuesta");
            _turnContext.RevealedOptions = true;

            EvaluationResult result = _evaluator.Evaluate("Respuesta", card, _turnContext);

            Assert.IsTrue(result.IsCorrect);
            Assert.AreEqual(PerformanceMultiplier.WithHelp, result.Multiplier);
        }

        [Test]
        public void Evaluate_CorrectWithOverdrive_PerfectMultiplier()
        {
            var card = new CardData(1, "Pregunta?", "Hint", null, "Respuesta");
            _turnContext.UsedHint = true;
            _turnContext.IsOverdriveActive = true; // Overdrive cancels help penalty

            EvaluationResult result = _evaluator.Evaluate("Respuesta", card, _turnContext);

            Assert.IsTrue(result.IsCorrect);
            Assert.AreEqual(PerformanceMultiplier.Perfect, result.Multiplier);
        }

        [Test]
        public void Evaluate_WrongAnswer_FailMultiplier()
        {
            var card = new CardData(1, "Pregunta?", null, null, "Correcta");

            EvaluationResult result = _evaluator.Evaluate("Incorrecta", card, _turnContext);

            Assert.IsFalse(result.IsCorrect);
            Assert.AreEqual(PerformanceMultiplier.Fail, result.Multiplier);
        }

        [Test]
        public void Evaluate_CaseInsensitive_StillCorrect()
        {
            var card = new CardData(1, "Pregunta?", null, null, "CShArP");

            EvaluationResult result = _evaluator.Evaluate("csharp", card, _turnContext);

            Assert.IsTrue(result.IsCorrect);
            Assert.AreEqual(PerformanceMultiplier.Perfect, result.Multiplier);
        }

        [Test]
        public void Evaluate_WhitespaceAroundAnswer_StillCorrect()
        {
            var card = new CardData(1, "Pregunta?", null, null, "  Correcta  ");

            EvaluationResult result = _evaluator.Evaluate("Correcta", card, _turnContext);

            Assert.IsTrue(result.IsCorrect);
            Assert.AreEqual(PerformanceMultiplier.Perfect, result.Multiplier);
        }

        [Test]
        public void Evaluate_OpenQuestion_RequiresGmJudgment()
        {
            // Sin opciones múltiples y respuesta con criterio de GM
            var card = new CardData(6, "Explica algo", null, null, "[Criterio del GM: Explicación de SRP]");

            EvaluationResult result = _evaluator.Evaluate("Respuesta del jugador", card, _turnContext);

            Assert.IsFalse(result.IsCorrect, "isCorrect should be false initially, waiting for GM.");
            Assert.IsTrue(result.RequiresGmJudgment);
            Assert.AreEqual(PerformanceMultiplier.Fail, result.Multiplier, "Should be Fail by default until judged.");
        }
    }
}
