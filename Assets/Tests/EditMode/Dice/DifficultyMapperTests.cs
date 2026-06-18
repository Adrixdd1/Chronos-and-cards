using System;
using UnityEngine;
using NUnit.Framework;
using ChronosAndCards.Data;
using ChronosAndCards.Gameplay.Dice;

namespace ChronosAndCards.Tests.Dice
{
    [TestFixture]
    public class DifficultyMapperTests
    {
        [Test]
        public void GetDifficulty_DefaultMapping_ReturnsIdentity()
        {
            var mapper = new DifficultyMapper(null);
            for (int i = 1; i <= 6; i++)
            {
                Assert.AreEqual(i, mapper.GetDifficulty(i), $"Default mapping should return identity for {i}.");
            }
        }

        [Test]
        public void GetDifficulty_CustomMapping_ReturnsConfiguredValue()
        {
            var config = ScriptableObject.CreateInstance<DifficultyMapConfig>();
            config.CustomMapping = new int[] { 1, 1, 2, 3, 4, 5 }; // Custom easy map

            var mapper = new DifficultyMapper(config);

            Assert.AreEqual(1, mapper.GetDifficulty(1));
            Assert.AreEqual(1, mapper.GetDifficulty(2));
            Assert.AreEqual(2, mapper.GetDifficulty(3));
            Assert.AreEqual(3, mapper.GetDifficulty(4));
            Assert.AreEqual(4, mapper.GetDifficulty(5));
            Assert.AreEqual(5, mapper.GetDifficulty(6));
        }

        [Test]
        public void GetDifficulty_InvalidValue_ThrowsException()
        {
            var mapper = new DifficultyMapper(null);
            
            Assert.Throws<ArgumentOutOfRangeException>(() => mapper.GetDifficulty(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => mapper.GetDifficulty(7));
            Assert.Throws<ArgumentOutOfRangeException>(() => mapper.GetDifficulty(-1));
        }
    }
}
