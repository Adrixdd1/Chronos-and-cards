using NUnit.Framework;
using System.Reflection;
using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Data;
using ChronosAndCards.Gameplay;
using ChronosAndCards.Gameplay.Dice;

namespace ChronosAndCards.Tests.Content
{
    [TestFixture]
    public class ContentPipelineIntegrationTests
    {
        private GameObject _gameObject;
        private ContentManager _contentManager;
        private ContentLoader _contentLoader;
        private HintSystem _hintSystem;
        private AnswerEvaluator _answerEvaluator;
        private TurnContext _turnContext;
        private Gameplay.Player _player;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("IntegrationTestObject");
            _contentManager = _gameObject.AddComponent<ContentManager>();
            _contentLoader = _gameObject.AddComponent<ContentLoader>();

            // Inyectar ContentManager en ContentLoader
            typeof(ContentLoader).GetField("_contentManager", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_contentLoader, _contentManager);

            _hintSystem = new HintSystem();
            _answerEvaluator = new AnswerEvaluator();

            _player = new Gameplay.Player("JohnDoe", 0, 3);
            _turnContext = new TurnContext();
            _turnContext.Reset();
            _turnContext.ActivePlayer = _player;

            GameEvents.ClearAll();
        }

        [TearDown]
        public void TearDown()
        {
            GameEvents.ClearAll();
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void FullPipeline_ParseLoadDrawEvaluate_WorksEndToEnd()
        {
            string markdown = @"
# [2] ¿Es C# orientado a objetos?
- No
* Sí
= Sí
";
            // 1. Cargar desde string (dispara parsing interno y carga en ContentManager)
            _contentLoader.LoadFromString(markdown);

            Assert.AreEqual(1, _contentManager.TotalRemainingCards);

            // 2. Extraer carta
            CardData card = _contentManager.DrawCard(2);
            Assert.AreEqual(2, card.DifficultyLevel);
            Assert.AreEqual("¿Es C# orientado a objetos?", card.QuestionText);

            // 3. Evaluar respuesta correcta
            _turnContext.CurrentCard = card;
            EvaluationResult eval = _answerEvaluator.Evaluate("Sí", card, _turnContext);

            Assert.IsTrue(eval.IsCorrect);
            Assert.AreEqual(PerformanceMultiplier.Perfect, eval.Multiplier);
        }

        [Test]
        public void FullPipeline_WithHintUsage_ReducesMultiplier()
        {
            string markdown = @"
# [3] ¿Qué palabra clave impide herencia?
> Es sealed.
- virtual
* sealed
= sealed
";
            _contentLoader.LoadFromString(markdown);

            CardData card = _contentManager.DrawCard(3);
            _turnContext.CurrentCard = card;

            // Usar pista
            bool hintUsed = _hintSystem.TryUseHint(_player, card, _turnContext);
            Assert.IsTrue(hintUsed);
            Assert.AreEqual(2, _player.HintCount); // 3 -> 2
            Assert.IsTrue(_turnContext.UsedHint);

            // Evaluar respuesta correcta
            EvaluationResult eval = _answerEvaluator.Evaluate("sealed", card, _turnContext);

            Assert.IsTrue(eval.IsCorrect);
            Assert.AreEqual(PerformanceMultiplier.WithHelp, eval.Multiplier, "Debería penalizar a WithHelp (x0.5).");
        }

        [Test]
        public void FullPipeline_WithFallback_DrawsFromClosestLevel()
        {
            // Cargar solo una carta de nivel 1
            string markdown = @"
# [1] Pregunta de Nivel 1
= Uno
";
            _contentLoader.LoadFromString(markdown);

            // Intentar extraer de nivel 2 (que está vacío)
            CardData card = _contentManager.DrawCard(2);

            Assert.AreEqual(1, card.DifficultyLevel, "Debería haber hecho fallback al nivel 1 por cercanía.");
            Assert.AreEqual("Pregunta de Nivel 1", card.QuestionText);
        }
    }
}
