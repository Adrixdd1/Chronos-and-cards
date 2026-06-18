using NUnit.Framework;
using System.Collections.Generic;
using ChronosAndCards.Data;

namespace ChronosAndCards.Tests.Content
{
    [TestFixture]
    public class MarkdownContentParserTests
    {
        private MarkdownContentParser _parser;

        [SetUp]
        public void SetUp()
        {
            _parser = new MarkdownContentParser();
        }

        [Test]
        public void Parse_ValidFile_ExtractsAllQuestions()
        {
            string markdown = @"
# [1] Pregunta 1
- Opcion A
* Opcion B
= Opcion B

# [2] Pregunta 2
> Pista
- Opcion A
- Opcion B
= Opcion A
";
            List<CardData> cards = _parser.Parse(markdown);
            Assert.AreEqual(2, cards.Count);
        }

        [Test]
        public void Parse_QuestionWithAllFields_CorrectCardData()
        {
            string markdown = @"
# [3] Pregunta Completa
> Esta es una pista
- Opcion 1
* Opcion 2
= Opcion 2
";
            ParseResult result = _parser.ParseWithDetails(markdown);
            Assert.AreEqual(1, result.Cards.Count);
            CardData card = result.Cards[0];
            Assert.AreEqual(3, card.DifficultyLevel);
            Assert.AreEqual("Pregunta Completa", card.QuestionText);
            Assert.AreEqual("Esta es una pista", card.Hint);
            Assert.AreEqual(2, card.Options.Count);
            Assert.AreEqual("Opcion 1", card.Options[0]);
            Assert.AreEqual("Opcion 2", card.Options[1]);
            Assert.AreEqual("Opcion 2", card.CorrectAnswer);
            Assert.IsFalse(card.IsGmChallenge);
        }

        [Test]
        public void Parse_QuestionWithoutHint_HintIsNull()
        {
            string markdown = @"
# [1] Sin Pista
- Opcion A
- Opcion B
= Opcion A
";
            List<CardData> cards = _parser.Parse(markdown);
            Assert.AreEqual(1, cards.Count);
            Assert.IsNull(cards[0].Hint);
        }

        [Test]
        public void Parse_QuestionWithoutOptions_OptionsIsNull()
        {
            string markdown = @"
# [1] Sin Opciones
= Exacta
";
            List<CardData> cards = _parser.Parse(markdown);
            Assert.AreEqual(1, cards.Count);
            Assert.IsNull(cards[0].Options);
        }

        [Test]
        public void Parse_QuestionWithMultipleHintLines_ConcatenatesHints()
        {
            string markdown = @"
# [4] Multiples Pistas
> Linea 1
> Linea 2
= Resp
";
            List<CardData> cards = _parser.Parse(markdown);
            Assert.AreEqual(1, cards.Count);
            Assert.AreEqual("Linea 1\nLinea 2", cards[0].Hint);
        }

        [Test]
        public void Parse_AsteriskAndDashOptions_BothWork()
        {
            string markdown = @"
# [1] Mix Opciones
- Opcion A
* Opcion B
= Opcion B
";
            List<CardData> cards = _parser.Parse(markdown);
            Assert.AreEqual(1, cards.Count);
            Assert.AreEqual(2, cards[0].Options.Count);
        }

        [Test]
        public void Parse_GmQuestion_IsGmChallengeTrue()
        {
            string markdown = @"
## [GM] Reto del Game Master
- Opcion A
= Opcion A
";
            List<CardData> cards = _parser.Parse(markdown);
            Assert.AreEqual(1, cards.Count);
            Assert.IsTrue(cards[0].IsGmChallenge);
            Assert.AreEqual(5, cards[0].DifficultyLevel); // GM default level is 5
        }

        [Test]
        public void Parse_InvalidLevel_ErrorLogged_QuestionSkipped()
        {
            string markdown = @"
# [9] Dificultad Invalida
= Respuesta
";
            ParseResult result = _parser.ParseWithDetails(markdown);
            Assert.AreEqual(0, result.Cards.Count);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].Message.Contains("Rango"));
        }

        [Test]
        public void Parse_MissingAnswer_ErrorLogged_QuestionSkipped()
        {
            string markdown = @"
# [1] Sin Respuesta
- Opcion A
- Opcion B
";
            ParseResult result = _parser.ParseWithDetails(markdown);
            Assert.AreEqual(0, result.Cards.Count);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].Message.Contains("Falta la respuesta"));
        }

        [Test]
        public void Parse_MalformedHeader_ErrorLogged_LineSkipped()
        {
            string markdown = @"
# Pregunta sin dificultad entre corchetes
= Respuesta
";
            ParseResult result = _parser.ParseWithDetails(markdown);
            Assert.AreEqual(0, result.Cards.Count);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].Message.Contains("Header malformado"));
        }

        [Test]
        public void Parse_UnrecognizedLine_WarningLogged()
        {
            string markdown = @"
# [1] Pregunta
Esto es texto no reconocido
= Respuesta
";
            ParseResult result = _parser.ParseWithDetails(markdown);
            Assert.AreEqual(1, result.Cards.Count);
            Assert.AreEqual(1, result.Warnings.Count);
            Assert.IsTrue(result.Warnings[0].Message.Contains("Línea no reconocida"));
        }

        [Test]
        public void Parse_EmptyFile_ReturnsEmptyList()
        {
            List<CardData> cards = _parser.Parse("");
            Assert.AreEqual(0, cards.Count);
        }

        [Test]
        public void Parse_Utf8WithSpecialCharacters_PreservesText()
        {
            string markdown = @"
# [1] ¿Cuál es tu opinión sobre C#?
= Excelente, está genial.
";
            List<CardData> cards = _parser.Parse(markdown);
            Assert.AreEqual(1, cards.Count);
            Assert.AreEqual("¿Cuál es tu opinión sobre C#?", cards[0].QuestionText);
            Assert.AreEqual("Excelente, está genial.", cards[0].CorrectAnswer);
        }

        [Test]
        public void Parse_ReturnsCardsOrderedByLevel()
        {
            string markdown = @"
# [3] Pregunta Nivel 3
= R3

# [1] Pregunta Nivel 1
= R1

# [6] Pregunta Nivel 6
= R6
";
            List<CardData> cards = _parser.Parse(markdown);
            Assert.AreEqual(3, cards.Count);
            Assert.AreEqual(1, cards[0].DifficultyLevel);
            Assert.AreEqual(3, cards[1].DifficultyLevel);
            Assert.AreEqual(6, cards[2].DifficultyLevel);
        }
    }
}
