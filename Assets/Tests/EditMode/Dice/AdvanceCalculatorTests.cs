using NUnit.Framework;
using ChronosAndCards.Data;
using ChronosAndCards.Gameplay;

namespace ChronosAndCards.Tests.Dice
{
    [TestFixture]
    public class AdvanceCalculatorTests
    {
        [TestCase(1, PerformanceMultiplier.Perfect, 1)]
        [TestCase(6, PerformanceMultiplier.Perfect, 6)]
        [TestCase(1, PerformanceMultiplier.WithHelp, 1)] // 1 * 0.5 = 0.5 -> rounds to 1
        [TestCase(2, PerformanceMultiplier.WithHelp, 1)] // 2 * 0.5 = 1.0 -> rounds to 1
        [TestCase(3, PerformanceMultiplier.WithHelp, 2)] // 3 * 0.5 = 1.5 -> rounds to 2
        [TestCase(5, PerformanceMultiplier.WithHelp, 3)] // 5 * 0.5 = 2.5 -> rounds to 3
        [TestCase(6, PerformanceMultiplier.WithHelp, 3)] // 6 * 0.5 = 3.0 -> rounds to 3
        [TestCase(1, PerformanceMultiplier.Fail, 0)]
        [TestCase(6, PerformanceMultiplier.Fail, 0)]
        public void Calculate_WithDefaultRounding_ReturnsExpectedValues(int diceValue, PerformanceMultiplier multiplier, int expected)
        {
            var calculator = new AdvanceCalculator(AdvanceCalculator.RoundingMode.Round);
            int result = calculator.Calculate(diceValue, multiplier);
            Assert.AreEqual(expected, result, $"Failed for dice: {diceValue}, multiplier: {multiplier}");
        }

        [Test]
        public void Calculate_WithFailPenalty_ReturnsNegative()
        {
            var calculator = new AdvanceCalculator();
            int penaltyValue = 2;
            int result = calculator.Calculate(4, PerformanceMultiplier.Fail, penaltyValue);
            Assert.AreEqual(-2, result, "Calculator failed to return negative penalty value for Fail.");
            
            // Perfect should not apply penalty
            int resultPerfect = calculator.Calculate(4, PerformanceMultiplier.Perfect, penaltyValue);
            Assert.AreEqual(4, resultPerfect);
        }

        [Test]
        public void Calculate_RoundingModeFloor_RoundsDown()
        {
            var calculator = new AdvanceCalculator(AdvanceCalculator.RoundingMode.Floor);
            
            // 3 * 0.5 = 1.5 -> floor is 1
            Assert.AreEqual(1, calculator.Calculate(3, PerformanceMultiplier.WithHelp));
            
            // 5 * 0.5 = 2.5 -> floor is 2
            Assert.AreEqual(2, calculator.Calculate(5, PerformanceMultiplier.WithHelp));
        }

        [Test]
        public void Calculate_RoundingModeCeil_RoundsUp()
        {
            var calculator = new AdvanceCalculator(AdvanceCalculator.RoundingMode.Ceil);
            
            // 1 * 0.5 = 0.5 -> ceil is 1
            Assert.AreEqual(1, calculator.Calculate(1, PerformanceMultiplier.WithHelp));
            
            // 3 * 0.5 = 1.5 -> ceil is 2
            Assert.AreEqual(2, calculator.Calculate(3, PerformanceMultiplier.WithHelp));
        }
    }
}
