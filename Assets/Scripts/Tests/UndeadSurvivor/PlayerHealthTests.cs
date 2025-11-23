using NUnit.Framework;
using UnityEngine;
using UndeadSurvivor;

namespace Tests.UndeadSurvivor
{
    /// <summary>
    /// PlayerHealth 컴포넌트 단위 테스트
    /// </summary>
    [TestFixture]
    public class PlayerHealthTests
    {
        private GameObject _testObject;
        private PlayerHealth _playerHealth;

        [SetUp]
        public void SetUp()
        {
            _testObject = new GameObject("TestPlayer");
            _playerHealth = _testObject.AddComponent<PlayerHealth>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_testObject);
        }

        #region 초기화 테스트

        [Test]
        public void PlayerHealth_InitialValues_AreCorrect()
        {
            // Arrange & Act (Awake에서 초기화됨)

            // Assert
            Assert.AreEqual(100f, _playerHealth.MaxHp, "Initial MaxHp should be 100");
            Assert.AreEqual(100f, _playerHealth.CurrentHp, "Initial CurrentHp should be 100");
            Assert.IsTrue(_playerHealth.IsAlive, "Player should be alive initially");
        }

        #endregion

        #region SetMaxHp 테스트

        [Test]
        public void SetMaxHp_IncreasesMaxHp_KeepsPercentage()
        {
            // Arrange
            _playerHealth.TakeDamage(50f); // 100 → 50 (50%)

            // Act
            _playerHealth.SetMaxHp(200f, keepPercentage: true);

            // Assert
            Assert.AreEqual(200f, _playerHealth.MaxHp, "MaxHp should be 200");
            Assert.AreEqual(100f, _playerHealth.CurrentHp, "CurrentHp should be 100 (50% of 200)");
        }

        [Test]
        public void SetMaxHp_IncreasesMaxHp_DoesNotKeepPercentage()
        {
            // Arrange
            _playerHealth.TakeDamage(50f); // 100 → 50

            // Act
            _playerHealth.SetMaxHp(200f, keepPercentage: false);

            // Assert
            Assert.AreEqual(200f, _playerHealth.MaxHp, "MaxHp should be 200");
            Assert.AreEqual(200f, _playerHealth.CurrentHp, "CurrentHp should be 200 (full heal)");
        }

        #endregion

        #region TakeDamage 테스트

        [Test]
        public void TakeDamage_ReducesCurrentHp()
        {
            // Arrange
            float initialHp = _playerHealth.CurrentHp;

            // Act
            _playerHealth.TakeDamage(30f);

            // Assert
            Assert.AreEqual(initialHp - 30f, _playerHealth.CurrentHp, "CurrentHp should be reduced by 30");
        }

        [Test]
        public void TakeDamage_WithDefense_ReducesDamage()
        {
            // Arrange
            float initialHp = _playerHealth.CurrentHp;
            float defense = 10f;

            // Act
            _playerHealth.TakeDamage(30f, defense);

            // Assert
            Assert.AreEqual(initialHp - 20f, _playerHealth.CurrentHp, "CurrentHp should be reduced by 20 (30 - 10)");
        }

        [Test]
        public void TakeDamage_WithHighDefense_DealsMinimum1Damage()
        {
            // Arrange
            float initialHp = _playerHealth.CurrentHp;
            float defense = 100f; // 방어력이 피해보다 높음

            // Act
            _playerHealth.TakeDamage(30f, defense);

            // Assert
            Assert.AreEqual(initialHp - 1f, _playerHealth.CurrentHp, "Minimum damage should be 1");
        }

        [Test]
        public void TakeDamage_ToZero_TriggersOnDeath()
        {
            // Arrange
            bool deathTriggered = false;
            _playerHealth.OnDeath += () => deathTriggered = true;

            // Act
            _playerHealth.TakeDamage(100f);

            // Assert
            Assert.AreEqual(0f, _playerHealth.CurrentHp, "CurrentHp should be 0");
            Assert.IsFalse(_playerHealth.IsAlive, "Player should be dead");
            Assert.IsTrue(deathTriggered, "OnDeath event should be triggered");
        }

        [Test]
        public void TakeDamage_WhenDead_DoesNothing()
        {
            // Arrange
            _playerHealth.TakeDamage(100f); // 사망
            Assert.IsFalse(_playerHealth.IsAlive, "Player should be dead");

            // Act
            _playerHealth.TakeDamage(50f); // 이미 죽었는데 추가 피해

            // Assert
            Assert.AreEqual(0f, _playerHealth.CurrentHp, "CurrentHp should remain 0");
        }

        #endregion

        #region Heal 테스트

        [Test]
        public void Heal_IncreasesCurrentHp()
        {
            // Arrange
            _playerHealth.TakeDamage(50f); // 100 → 50

            // Act
            _playerHealth.Heal(20f);

            // Assert
            Assert.AreEqual(70f, _playerHealth.CurrentHp, "CurrentHp should be 70");
        }

        [Test]
        public void Heal_CannotExceedMaxHp()
        {
            // Arrange
            _playerHealth.TakeDamage(20f); // 100 → 80

            // Act
            _playerHealth.Heal(50f); // 80 + 50 = 130이지만 100으로 제한

            // Assert
            Assert.AreEqual(100f, _playerHealth.CurrentHp, "CurrentHp should not exceed MaxHp");
        }

        [Test]
        public void HealPercentage_HealsCorrectAmount()
        {
            // Arrange
            _playerHealth.TakeDamage(50f); // 100 → 50

            // Act
            _playerHealth.HealPercentage(0.5f); // 50% of MaxHp = 50

            // Assert
            Assert.AreEqual(100f, _playerHealth.CurrentHp, "CurrentHp should be 100 (50 + 50)");
        }

        [Test]
        public void Heal_WhenDead_DoesNothing()
        {
            // Arrange
            _playerHealth.TakeDamage(100f); // 사망

            // Act
            _playerHealth.Heal(50f);

            // Assert
            Assert.AreEqual(0f, _playerHealth.CurrentHp, "Dead player cannot be healed");
        }

        #endregion

        #region 이벤트 테스트

        [Test]
        public void OnHealthChanged_TriggersOnDamage()
        {
            // Arrange
            bool eventTriggered = false;
            float recordedCurrentHp = 0f;
            float recordedMaxHp = 0f;

            _playerHealth.OnHealthChanged += (currentHp, maxHp) =>
            {
                eventTriggered = true;
                recordedCurrentHp = currentHp;
                recordedMaxHp = maxHp;
            };

            // Act
            _playerHealth.TakeDamage(30f);

            // Assert
            Assert.IsTrue(eventTriggered, "OnHealthChanged should be triggered");
            Assert.AreEqual(70f, recordedCurrentHp, "Event should report correct CurrentHp");
            Assert.AreEqual(100f, recordedMaxHp, "Event should report correct MaxHp");
        }

        [Test]
        public void OnDamaged_TriggersWithCorrectAmount()
        {
            // Arrange
            bool eventTriggered = false;
            float recordedDamage = 0f;

            _playerHealth.OnDamaged += (damage) =>
            {
                eventTriggered = true;
                recordedDamage = damage;
            };

            // Act
            _playerHealth.TakeDamage(30f);

            // Assert
            Assert.IsTrue(eventTriggered, "OnDamaged should be triggered");
            Assert.AreEqual(30f, recordedDamage, "Event should report correct damage amount");
        }

        [Test]
        public void OnHealed_TriggersWithCorrectAmount()
        {
            // Arrange
            _playerHealth.TakeDamage(50f); // 100 → 50

            bool eventTriggered = false;
            float recordedHealAmount = 0f;

            _playerHealth.OnHealed += (healAmount) =>
            {
                eventTriggered = true;
                recordedHealAmount = healAmount;
            };

            // Act
            _playerHealth.Heal(30f);

            // Assert
            Assert.IsTrue(eventTriggered, "OnHealed should be triggered");
            Assert.AreEqual(30f, recordedHealAmount, "Event should report correct heal amount");
        }

        #endregion
    }
}
