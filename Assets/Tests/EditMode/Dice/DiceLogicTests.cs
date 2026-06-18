using System;
using System.Collections.Generic;
using NUnit.Framework;
using ChronosAndCards.Gameplay.Dice;

namespace ChronosAndCards.Tests.Dice
{
    [TestFixture]
    public class DiceLogicTests
    {
        [Test]
        public void Roll_ReturnsValueBetween1And6()
        {
            var dice = new DiceLogic();
            for (int i = 0; i < 1000; i++)
            {
                int rolledValue = 0;
                Action<int> handler = (val) => rolledValue = val;
                dice.OnDiceResult += handler;
                
                dice.Roll();
                
                dice.OnDiceResult -= handler;

                Assert.IsTrue(rolledValue >= 1 && rolledValue <= 6, $"Rolled value {rolledValue} is out of bounds [1, 6] on iteration {i}.");
            }
        }

        [Test]
        public void Roll_WithSeed_ProducesReproducibleResults()
        {
            int seed = 42;
            var dice1 = new DiceLogic(seed);
            var dice2 = new DiceLogic(seed);

            var results1 = new List<int>();
            var results2 = new List<int>();

            Action<int> handler1 = (val) => results1.Add(val);
            Action<int> handler2 = (val) => results2.Add(val);

            dice1.OnDiceResult += handler1;
            dice2.OnDiceResult += handler2;

            for (int i = 0; i < 50; i++)
            {
                dice1.Roll();
                dice2.Roll();
            }

            dice1.OnDiceResult -= handler1;
            dice2.OnDiceResult -= handler2;

            CollectionAssert.AreEqual(results1, results2, "Seeded dice rolls did not produce reproducible results.");
        }

        [Test]
        public void Roll_FiresOnDiceResultEvent()
        {
            var dice = new DiceLogic();
            bool eventFired = false;
            int rolledValue = 0;

            Action<int> handler = (val) =>
            {
                eventFired = true;
                rolledValue = val;
            };

            dice.OnDiceResult += handler;
            dice.Roll();
            dice.OnDiceResult -= handler;

            Assert.IsTrue(eventFired, "OnDiceResult event did not fire.");
            Assert.IsTrue(rolledValue >= 1 && rolledValue <= 6);
        }

        [Test]
        public void IsValidResult_ReturnsTrue_For1To6()
        {
            var dice = new DiceLogic();
            for (int i = 1; i <= 6; i++)
            {
                Assert.IsTrue(dice.IsValidResult(i), $"IsValidResult should return true for {i}.");
            }
        }

        [Test]
        public void IsValidResult_ReturnsFalse_For0And7AndNegative()
        {
            var dice = new DiceLogic();
            Assert.IsFalse(dice.IsValidResult(0), "IsValidResult should return false for 0.");
            Assert.IsFalse(dice.IsValidResult(7), "IsValidResult should return false for 7.");
            Assert.IsFalse(dice.IsValidResult(-1), "IsValidResult should return false for -1.");
        }

        [Test]
        public void Roll_UniformDistribution()
        {
            // 10,000 rolls to test uniformity
            var dice = new DiceLogic(12345);
            var counts = new Dictionary<int, int>
            {
                {1, 0}, {2, 0}, {3, 0}, {4, 0}, {5, 0}, {6, 0}
            };

            Action<int> handler = (val) => counts[val]++;
            dice.OnDiceResult += handler;

            int iterations = 10000;
            for (int i = 0; i < iterations; i++)
            {
                dice.Roll();
            }

            dice.OnDiceResult -= handler;

            double expected = iterations / 6.0; // ~1666.67
            double tolerance = iterations * 0.03; // ±3% tolerance (300)

            for (int i = 1; i <= 6; i++)
            {
                int count = counts[i];
                Assert.AreEqual(expected, (double)count, tolerance, $"Face {i} count {count} deviates too much from expected {expected:F2} (tolerance {tolerance}).");
            }
        }
    }
}
