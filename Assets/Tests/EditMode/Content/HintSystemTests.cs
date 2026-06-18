using NUnit.Framework;
using ChronosAndCards.Core;
using ChronosAndCards.Data;
using ChronosAndCards.Gameplay;
using ChronosAndCards.Interfaces;

namespace ChronosAndCards.Tests.Content
{
    [TestFixture]
    public class HintSystemTests
    {
        private HintSystem _hintSystem;
        private IPlayer _player;
        private TurnContext _turnContext;

        [SetUp]
        public void SetUp()
        {
            _hintSystem = new HintSystem();
            _player = new Gameplay.Player("TestPlayer", 0, 3); // starts with 3 hints
            _turnContext = new TurnContext();
            _turnContext.Reset();
            _turnContext.ActivePlayer = _player;
            GameEvents.ClearAll();
        }

        [TearDown]
        public void TearDown()
        {
            GameEvents.ClearAll();
        }

        [Test]
        public void TryUseHint_HasHints_DecreasesCount()
        {
            var card = new CardData(1, "Pregunta", "PistaTexto", null, "Resp");

            bool success = _hintSystem.TryUseHint(_player, card, _turnContext);

            Assert.IsTrue(success);
            Assert.AreEqual(2, _player.HintCount);
            Assert.IsTrue(_turnContext.UsedHint);
        }

        [Test]
        public void TryUseHint_NoHints_ReturnsFalse()
        {
            var playerNoHints = new Gameplay.Player("NoHints", 0, 0);
            var card = new CardData(1, "Pregunta", "PistaTexto", null, "Resp");

            bool success = _hintSystem.TryUseHint(playerNoHints, card, _turnContext);

            Assert.IsFalse(success);
            Assert.AreEqual(0, playerNoHints.HintCount);
            Assert.IsFalse(_turnContext.UsedHint);
        }

        [Test]
        public void TryUseHint_NoHintOnCard_ReturnsFalse()
        {
            var cardNoHint = new CardData(1, "Pregunta", null, null, "Resp"); // No hint text

            bool success = _hintSystem.TryUseHint(_player, cardNoHint, _turnContext);

            Assert.IsFalse(success);
            Assert.AreEqual(3, _player.HintCount);
            Assert.IsFalse(_turnContext.UsedHint);
        }

        [Test]
        public void TryUseHint_AlreadyUsed_ReturnsFalse()
        {
            var card = new CardData(1, "Pregunta", "PistaTexto", null, "Resp");
            _turnContext.UsedHint = true; // Already used this turn

            bool success = _hintSystem.TryUseHint(_player, card, _turnContext);

            Assert.IsFalse(success);
            Assert.AreEqual(3, _player.HintCount);
        }

        [Test]
        public void TryUseHint_EmitsOnHintRevealed()
        {
            var card = new CardData(1, "Pregunta", "PistaTexto", null, "Resp");
            
            bool eventFired = false;
            string revealedHint = null;
            GameEvents.OnHintRevealed += (hint) =>
            {
                eventFired = true;
                revealedHint = hint;
            };

            _hintSystem.TryUseHint(_player, card, _turnContext);

            Assert.IsTrue(eventFired);
            Assert.AreEqual("PistaTexto", revealedHint);
        }

        [Test]
        public void CanUseHint_ReflectsCorrectState()
        {
            var card = new CardData(1, "Pregunta", "PistaTexto", null, "Resp");
            var cardNoHint = new CardData(1, "Pregunta", null, null, "Resp");
            var playerNoHints = new Gameplay.Player("NoHints", 0, 0);

            Assert.IsTrue(_hintSystem.CanUseHint(_player, card, _turnContext));
            Assert.IsFalse(_hintSystem.CanUseHint(playerNoHints, card, _turnContext));
            Assert.IsFalse(_hintSystem.CanUseHint(_player, cardNoHint, _turnContext));

            _turnContext.UsedHint = true;
            Assert.IsFalse(_hintSystem.CanUseHint(_player, card, _turnContext));
        }
    }
}
