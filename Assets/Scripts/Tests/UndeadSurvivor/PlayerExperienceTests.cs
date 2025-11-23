using NUnit.Framework;
using UnityEngine;
using UndeadSurvivor;

namespace Tests.UndeadSurvivor
{
    /// <summary>
    /// PlayerExperience 컴포넌트 단위 테스트
    /// </summary>
    [TestFixture]
    public class PlayerExperienceTests
    {
        private GameObject _testObject;
        private PlayerExperience _playerExperience;

        [SetUp]
        public void SetUp()
        {
            _testObject = new GameObject("TestPlayer");
            _playerExperience = _testObject.AddComponent<PlayerExperience>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_testObject);
        }

        #region 초기화 테스트

        [Test]
        public void PlayerExperience_InitialValues_AreCorrect()
        {
            // Assert
            Assert.AreEqual(1, _playerExperience.CurrentLevel, "Initial level should be 1");
            Assert.AreEqual(0, _playerExperience.CurrentExp, "Initial exp should be 0");
            Assert.AreEqual(100, _playerExperience.ExpForNextLevel, "Exp for level 2 should be 100");
            Assert.AreEqual(1f, _playerExperience.ExpMultiplier, "Initial exp multiplier should be 1");
        }

        #endregion

        #region GainExp 테스트

        [Test]
        public void GainExp_IncreasesCurrentExp()
        {
            // Act
            _playerExperience.GainExp(50);

            // Assert
            Assert.AreEqual(50, _playerExperience.CurrentExp, "CurrentExp should be 50");
            Assert.AreEqual(1, _playerExperience.CurrentLevel, "Should still be level 1");
        }

        [Test]
        public void GainExp_WithMultiplier_IncreasesExpCorrectly()
        {
            // Arrange
            _playerExperience.SetExpMultiplier(1.5f);

            // Act
            _playerExperience.GainExp(50); // 50 * 1.5 = 75

            // Assert
            Assert.AreEqual(75, _playerExperience.CurrentExp, "CurrentExp should be 75 (50 * 1.5)");
        }

        [Test]
        public void GainExp_ReachingThreshold_TriggersLevelUp()
        {
            // Arrange
            bool levelUpTriggered = false;
            int newLevel = 0;

            _playerExperience.OnLevelUp += (level) =>
            {
                levelUpTriggered = true;
                newLevel = level;
            };

            // Act
            _playerExperience.GainExp(100); // Exactly 100 for level 2

            // Assert
            Assert.IsTrue(levelUpTriggered, "OnLevelUp should be triggered");
            Assert.AreEqual(2, newLevel, "New level should be 2");
            Assert.AreEqual(2, _playerExperience.CurrentLevel, "CurrentLevel should be 2");
            Assert.AreEqual(0, _playerExperience.CurrentExp, "CurrentExp should reset to 0");
        }

        [Test]
        public void GainExp_ExceedingThreshold_CarriesOverExp()
        {
            // Act
            _playerExperience.GainExp(150); // 100 for level 2, 50 carries over

            // Assert
            Assert.AreEqual(2, _playerExperience.CurrentLevel, "CurrentLevel should be 2");
            Assert.AreEqual(50, _playerExperience.CurrentExp, "50 exp should carry over to level 2");
            Assert.AreEqual(120, _playerExperience.ExpForNextLevel, "Exp for level 3 should be 120 (100 * 1.2)");
        }

        [Test]
        public void GainExp_MultiLevelUp_WorksCorrectly()
        {
            // Arrange
            int levelUpCount = 0;
            _playerExperience.OnLevelUp += (level) => levelUpCount++;

            // Act
            _playerExperience.GainExp(1000); // Should level up multiple times

            // Assert
            Assert.Greater(_playerExperience.CurrentLevel, 1, "Should have leveled up multiple times");
            Assert.Greater(levelUpCount, 1, "OnLevelUp should be triggered multiple times");
        }

        #endregion

        #region ExpMultiplier 테스트

        [Test]
        public void SetExpMultiplier_ChangesMultiplier()
        {
            // Act
            _playerExperience.SetExpMultiplier(2f);

            // Assert
            Assert.AreEqual(2f, _playerExperience.ExpMultiplier, "ExpMultiplier should be 2");
        }

        [Test]
        public void AddExpMultiplier_AddsToMultiplier()
        {
            // Arrange
            _playerExperience.SetExpMultiplier(1.5f);

            // Act
            _playerExperience.AddExpMultiplier(0.5f);

            // Assert
            Assert.AreEqual(2f, _playerExperience.ExpMultiplier, "ExpMultiplier should be 2 (1.5 + 0.5)");
        }

        [Test]
        public void SetExpMultiplier_NegativeValue_ClampsToZero()
        {
            // Act
            _playerExperience.SetExpMultiplier(-1f);

            // Assert
            Assert.AreEqual(0f, _playerExperience.ExpMultiplier, "ExpMultiplier should be clamped to 0");
        }

        #endregion

        #region 레벨 계산 테스트

        [Test]
        public void ExpForNextLevel_IncreasesBy20Percent()
        {
            // Arrange & Act
            _playerExperience.GainExp(100); // Level 1 → 2

            // Assert
            Assert.AreEqual(120, _playerExperience.ExpForNextLevel, "Level 2 → 3 should require 120 exp (100 * 1.2)");

            // Act
            _playerExperience.GainExp(120); // Level 2 → 3

            // Assert
            Assert.AreEqual(144, _playerExperience.ExpForNextLevel, "Level 3 → 4 should require 144 exp (120 * 1.2)");
        }

        [Test]
        public void ExpForNextLevel_Level1To5_CorrectValues()
        {
            // Level 1 → 2
            Assert.AreEqual(100, _playerExperience.ExpForNextLevel);

            _playerExperience.GainExp(100);
            // Level 2 → 3
            Assert.AreEqual(120, _playerExperience.ExpForNextLevel);

            _playerExperience.GainExp(120);
            // Level 3 → 4
            Assert.AreEqual(144, _playerExperience.ExpForNextLevel);

            _playerExperience.GainExp(144);
            // Level 4 → 5
            Assert.AreEqual(172, _playerExperience.ExpForNextLevel);
        }

        #endregion

        #region 이벤트 테스트

        [Test]
        public void OnExpChanged_TriggersOnGainExp()
        {
            // Arrange
            bool eventTriggered = false;
            int recordedCurrentExp = 0;
            int recordedExpForNextLevel = 0;
            int recordedCurrentLevel = 0;

            _playerExperience.OnExpChanged += (currentExp, expForNextLevel, currentLevel) =>
            {
                eventTriggered = true;
                recordedCurrentExp = currentExp;
                recordedExpForNextLevel = expForNextLevel;
                recordedCurrentLevel = currentLevel;
            };

            // Act
            _playerExperience.GainExp(50);

            // Assert
            Assert.IsTrue(eventTriggered, "OnExpChanged should be triggered");
            Assert.AreEqual(50, recordedCurrentExp, "Event should report correct CurrentExp");
            Assert.AreEqual(100, recordedExpForNextLevel, "Event should report correct ExpForNextLevel");
            Assert.AreEqual(1, recordedCurrentLevel, "Event should report correct CurrentLevel");
        }

        [Test]
        public void OnExpGained_TriggersWithCorrectAmount()
        {
            // Arrange
            bool eventTriggered = false;
            int recordedExpAmount = 0;

            _playerExperience.OnExpGained += (expAmount) =>
            {
                eventTriggered = true;
                recordedExpAmount = expAmount;
            };

            // Act
            _playerExperience.GainExp(75);

            // Assert
            Assert.IsTrue(eventTriggered, "OnExpGained should be triggered");
            Assert.AreEqual(75, recordedExpAmount, "Event should report correct exp amount");
        }

        [Test]
        public void OnLevelUp_TriggersWithCorrectLevel()
        {
            // Arrange
            bool eventTriggered = false;
            int recordedLevel = 0;

            _playerExperience.OnLevelUp += (level) =>
            {
                eventTriggered = true;
                recordedLevel = level;
            };

            // Act
            _playerExperience.GainExp(100);

            // Assert
            Assert.IsTrue(eventTriggered, "OnLevelUp should be triggered");
            Assert.AreEqual(2, recordedLevel, "Event should report correct new level");
        }

        [Test]
        public void OnLevelUp_MultiLevel_TriggersMultipleTimes()
        {
            // Arrange
            int levelUpCount = 0;
            _playerExperience.OnLevelUp += (level) => levelUpCount++;

            // Act
            _playerExperience.GainExp(500); // Multiple level ups

            // Assert
            Assert.Greater(levelUpCount, 1, "OnLevelUp should be triggered multiple times");
        }

        #endregion

        #region 엣지 케이스 테스트

        [Test]
        public void GainExp_ZeroAmount_DoesNotTriggerEvents()
        {
            // Arrange
            bool eventTriggered = false;
            _playerExperience.OnExpGained += (exp) => eventTriggered = true;

            // Act
            _playerExperience.GainExp(0);

            // Assert
            Assert.IsFalse(eventTriggered, "OnExpGained should not trigger for 0 exp");
        }

        [Test]
        public void GainExp_NegativeAmount_DoesNothing()
        {
            // Arrange
            int initialExp = _playerExperience.CurrentExp;

            // Act
            _playerExperience.GainExp(-50);

            // Assert
            Assert.AreEqual(initialExp, _playerExperience.CurrentExp, "Negative exp should not affect CurrentExp");
        }

        #endregion
    }
}
