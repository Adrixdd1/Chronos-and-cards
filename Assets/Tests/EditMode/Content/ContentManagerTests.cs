using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using ChronosAndCards.Core;
using ChronosAndCards.Data;

namespace ChronosAndCards.Tests.Content
{
    [TestFixture]
    public class ContentManagerTests
    {
        private GameObject _gameObject;
        private ContentManager _contentManager;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("ContentManagerTestObject");
            _contentManager = _gameObject.AddComponent<ContentManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void DrawCard_ReturnsCardOfRequestedLevel()
        {
            var cards = new List<CardData>
            {
                new CardData(2, "Pregunta L2", null, null, "Correcta", false)
            };
            _contentManager.LoadCards(cards, false);

            CardData drawn = _contentManager.DrawCard(2);
            Assert.AreEqual(2, drawn.DifficultyLevel);
            Assert.AreEqual("Pregunta L2", drawn.QuestionText);
        }

        [Test]
        public void DrawCard_ExhaustedLevel_FallsBackToClosestLevel()
        {
            // Cargar cartas solo de nivel 1 y nivel 3
            var cards = new List<CardData>
            {
                new CardData(1, "Pregunta L1", null, null, "Correcta", false),
                new CardData(3, "Pregunta L3", null, null, "Correcta", false)
            };
            _contentManager.LoadCards(cards, false);

            // Solicitar nivel 2 (está vacío)
            // Fallback debe ir primero al más fácil (L1 es nivel 2 - 1, L3 es nivel 2 + 1)
            // En nuestra búsqueda por offsets, offset 1 comprueba lower (L1) y luego upper (L3)
            CardData drawn = _contentManager.DrawCard(2);
            
            Assert.AreEqual(1, drawn.DifficultyLevel, "Debería haber hecho fallback a L1.");
        }

        [Test]
        public void DrawCard_AllLevelsExhausted_ReturnsEmptyCard()
        {
            _contentManager.LoadCards(new List<CardData>(), false);

            CardData drawn = _contentManager.DrawCard(1);
            Assert.IsTrue(drawn.QuestionText.Contains("No hay más preguntas disponibles"));
            Assert.AreEqual("N/A", drawn.CorrectAnswer);
        }

        [Test]
        public void DrawCard_EmitsOnDeckLevelEmpty_WhenLevelExhausted()
        {
            var cards = new List<CardData>
            {
                new CardData(1, "Pregunta L1", null, null, "Correcta", false)
            };
            _contentManager.LoadCards(cards, false);

            bool eventFired = false;
            int emptyLevel = -1;
            _contentManager.OnDeckLevelEmpty += (lvl) =>
            {
                eventFired = true;
                emptyLevel = lvl;
            };

            _contentManager.DrawCard(1);

            Assert.IsTrue(eventFired);
            Assert.AreEqual(1, emptyLevel);
        }

        [Test]
        public void DrawGmChallenge_ReturnsGmCard()
        {
            var cards = new List<CardData>
            {
                new CardData(5, "Reto GM", null, null, "Correcta", true)
            };
            _contentManager.LoadCards(cards, false);

            CardData drawn = _contentManager.DrawGmChallenge();
            Assert.IsTrue(drawn.IsGmChallenge);
            Assert.AreEqual("Reto GM", drawn.QuestionText);
        }

        [Test]
        public void DrawGmChallenge_NoGmCards_FallsBackToHighLevel()
        {
            // Sin cartas GM, pero con cartas de nivel 6
            var cards = new List<CardData>
            {
                new CardData(6, "Pregunta L6 normal", null, null, "Correcta", false)
            };
            _contentManager.LoadCards(cards, false);

            CardData drawn = _contentManager.DrawGmChallenge();
            Assert.IsFalse(drawn.IsGmChallenge);
            Assert.AreEqual(6, drawn.DifficultyLevel);
            Assert.AreEqual("Pregunta L6 normal", drawn.QuestionText);
        }

        [Test]
        public void LoadCards_WithShuffle_DifferentOrderThanInput()
        {
            // Generar 100 cartas del mismo nivel para que el shuffle sea estadísticamente visible
            var inputList = new List<CardData>();
            for (int i = 0; i < 100; i++)
            {
                inputList.Add(new CardData(3, $"P{i}", null, null, "Resp", false));
            }

            _contentManager.LoadCards(inputList, true);

            var drawnList = new List<CardData>();
            while (_contentManager.GetRemainingCards(3) > 0)
            {
                drawnList.Add(_contentManager.DrawCard(3));
            }

            // Comprobar si al menos una posición difiere del orden inicial
            bool orderChanged = false;
            for (int i = 0; i < 100; i++)
            {
                if (drawnList[i].QuestionText != $"P{i}")
                {
                    orderChanged = true;
                    break;
                }
            }

            Assert.IsTrue(orderChanged, "Shuffling should change the ordering of cards.");
        }

        [Test]
        public void GetRemainingCards_ReturnsCorrectCount()
        {
            var cards = new List<CardData>
            {
                new CardData(1, "P1", null, null, "R", false),
                new CardData(1, "P2", null, null, "R", false),
                new CardData(3, "P3", null, null, "R", false)
            };
            _contentManager.LoadCards(cards, false);

            Assert.AreEqual(2, _contentManager.GetRemainingCards(1));
            Assert.AreEqual(0, _contentManager.GetRemainingCards(2));
            Assert.AreEqual(1, _contentManager.GetRemainingCards(3));
            Assert.AreEqual(3, _contentManager.TotalRemainingCards);
        }

        [Test]
        public void HasCardsAvailable_TrueWhenNotEmpty_FalseWhenEmpty()
        {
            var cards = new List<CardData>
            {
                new CardData(1, "P1", null, null, "R", false)
            };
            _contentManager.LoadCards(cards, false);

            Assert.IsTrue(_contentManager.HasCardsAvailable(1));
            Assert.IsFalse(_contentManager.HasCardsAvailable(2));

            _contentManager.DrawCard(1);
            Assert.IsFalse(_contentManager.HasCardsAvailable(1));
        }
    }
}
