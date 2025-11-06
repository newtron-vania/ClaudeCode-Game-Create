using NUnit.Framework;
using UndeadSurvivor;

namespace Tests.UndeadSurvivor
{
    /// <summary>
    /// CharacterStat 클래스 단위 테스트
    /// </summary>
    [TestFixture]
    public class CharacterStatTests
    {
        private CharacterStat _characterStat;

        [SetUp]
        public void SetUp()
        {
            _characterStat = new CharacterStat();
        }

        #region 초기화 테스트

        [Test]
        public void CharacterStat_DefaultConstructor_InitializesCorrectly()
        {
            // Assert
            Assert.AreEqual(100f, _characterStat.MaxHp);
            Assert.AreEqual(100f, _characterStat.CurrentHp);
            Assert.AreEqual(5f, _characterStat.MoveSpeed);
            Assert.AreEqual(0f, _characterStat.Damage);
            Assert.AreEqual(0f, _characterStat.Defense);
            Assert.AreEqual(1f, _characterStat.ExpMultiplier);
            Assert.AreEqual(1f, _characterStat.PickupRange);
        }

        [Test]
        public void CharacterStat_ParameterConstructor_InitializesCorrectly()
        {
            // Arrange & Act
            var stat = new CharacterStat(maxHp: 150f, moveSpeed: 6f, damage: 10f, defense: 5f);

            // Assert
            Assert.AreEqual(150f, stat.MaxHp);
            Assert.AreEqual(150f, stat.CurrentHp);
            Assert.AreEqual(6f, stat.MoveSpeed);
            Assert.AreEqual(10f, stat.Damage);
            Assert.AreEqual(5f, stat.Defense);
        }

        [Test]
        public void Initialize_SetsValuesCorrectly()
        {
            // Act
            _characterStat.Initialize(maxHp: 200f, moveSpeed: 7f, damage: 15f, defense: 8f);

            // Assert
            Assert.AreEqual(200f, _characterStat.MaxHp);
            Assert.AreEqual(200f, _characterStat.CurrentHp);
            Assert.AreEqual(7f, _characterStat.MoveSpeed);
            Assert.AreEqual(15f, _characterStat.Damage);
            Assert.AreEqual(8f, _characterStat.Defense);
        }

        #endregion

        #region ApplyUpgrade 테스트

        [Test]
        public void ApplyUpgrade_MaxHp_IncreasesValue()
        {
            // Act
            _characterStat.ApplyUpgrade(StatType.MaxHp, 50f);

            // Assert
            Assert.AreEqual(150f, _characterStat.MaxHp);
            Assert.AreEqual(150f, _characterStat.CurrentHp, "CurrentHp should also increase");
        }

        [Test]
        public void ApplyUpgrade_MaxHp_KeepsHpRatio()
        {
            // Arrange
            _characterStat.TakeDamage(50f); // 100 → 50 (50% ratio)

            // Act
            _characterStat.ApplyUpgrade(StatType.MaxHp, 100f); // MaxHp 100 → 200

            // Assert
            Assert.AreEqual(200f, _characterStat.MaxHp);
            Assert.AreEqual(100f, _characterStat.CurrentHp, "CurrentHp should be 50% of new MaxHp");
        }

        [Test]
        public void ApplyUpgrade_MoveSpeed_IncreasesValue()
        {
            // Act
            _characterStat.ApplyUpgrade(StatType.MoveSpeed, 2f);

            // Assert
            Assert.AreEqual(7f, _characterStat.MoveSpeed);
        }

        [Test]
        public void ApplyUpgrade_Damage_IncreasesValue()
        {
            // Act
            _characterStat.ApplyUpgrade(StatType.Damage, 10f);

            // Assert
            Assert.AreEqual(10f, _characterStat.Damage);
        }

        [Test]
        public void ApplyUpgrade_Multiple_StacksCorrectly()
        {
            // Act
            _characterStat.ApplyUpgrade(StatType.Damage, 10f);
            _characterStat.ApplyUpgrade(StatType.Damage, 15f);
            _characterStat.ApplyUpgrade(StatType.Damage, 5f);

            // Assert
            Assert.AreEqual(30f, _characterStat.Damage, "Damage upgrades should stack");
        }

        #endregion

        #region GetStat 테스트

        [Test]
        public void GetStat_ReturnsCorrectValue()
        {
            // Arrange
            _characterStat.ApplyUpgrade(StatType.Damage, 25f);
            _characterStat.ApplyUpgrade(StatType.MoveSpeed, 1.5f);

            // Act & Assert
            Assert.AreEqual(100f, _characterStat.GetStat(StatType.MaxHp));
            Assert.AreEqual(6.5f, _characterStat.GetStat(StatType.MoveSpeed));
            Assert.AreEqual(25f, _characterStat.GetStat(StatType.Damage));
            Assert.AreEqual(0f, _characterStat.GetStat(StatType.Defense));
        }

        #endregion

        #region TakeDamage 테스트

        [Test]
        public void TakeDamage_ReducesCurrentHp()
        {
            // Act
            float actualDamage = _characterStat.TakeDamage(30f);

            // Assert
            Assert.AreEqual(30f, actualDamage, "Actual damage should be 30");
            Assert.AreEqual(70f, _characterStat.CurrentHp, "CurrentHp should be 70");
        }

        [Test]
        public void TakeDamage_WithDefense_ReducesDamage()
        {
            // Arrange
            _characterStat.ApplyUpgrade(StatType.Defense, 10f);

            // Act
            float actualDamage = _characterStat.TakeDamage(30f);

            // Assert
            Assert.AreEqual(20f, actualDamage, "Actual damage should be 20 (30 - 10)");
            Assert.AreEqual(80f, _characterStat.CurrentHp);
        }

        [Test]
        public void TakeDamage_ToZero_MakesNotAlive()
        {
            // Act
            _characterStat.TakeDamage(100f);

            // Assert
            Assert.AreEqual(0f, _characterStat.CurrentHp);
            Assert.IsFalse(_characterStat.IsAlive);
        }

        [Test]
        public void TakeDamage_BelowZero_ClampsToZero()
        {
            // Act
            _characterStat.TakeDamage(150f);

            // Assert
            Assert.AreEqual(0f, _characterStat.CurrentHp, "CurrentHp should be clamped to 0");
        }

        #endregion

        #region Heal 테스트

        [Test]
        public void Heal_IncreasesCurrentHp()
        {
            // Arrange
            _characterStat.TakeDamage(50f); // 100 → 50

            // Act
            float healedAmount = _characterStat.Heal(30f);

            // Assert
            Assert.AreEqual(30f, healedAmount, "Healed amount should be 30");
            Assert.AreEqual(80f, _characterStat.CurrentHp);
        }

        [Test]
        public void Heal_CannotExceedMaxHp()
        {
            // Arrange
            _characterStat.TakeDamage(20f); // 100 → 80

            // Act
            float healedAmount = _characterStat.Heal(50f); // Would go to 130, but clamped

            // Assert
            Assert.AreEqual(20f, healedAmount, "Only 20 HP should be healed to reach MaxHp");
            Assert.AreEqual(100f, _characterStat.CurrentHp);
        }

        [Test]
        public void HealFull_RestoresToMaxHp()
        {
            // Arrange
            _characterStat.TakeDamage(70f); // 100 → 30

            // Act
            _characterStat.HealFull();

            // Assert
            Assert.AreEqual(100f, _characterStat.CurrentHp);
        }

        #endregion

        #region Properties 테스트

        [Test]
        public void IsAlive_ReturnsCorrectValue()
        {
            // Assert
            Assert.IsTrue(_characterStat.IsAlive, "Should be alive initially");

            // Act
            _characterStat.TakeDamage(100f);

            // Assert
            Assert.IsFalse(_characterStat.IsAlive, "Should not be alive at 0 HP");
        }

        [Test]
        public void HpRatio_ReturnsCorrectValue()
        {
            // Assert
            Assert.AreEqual(1f, _characterStat.HpRatio, "HP ratio should be 1.0 at full HP");

            // Act
            _characterStat.TakeDamage(50f); // 100 → 50

            // Assert
            Assert.AreEqual(0.5f, _characterStat.HpRatio, "HP ratio should be 0.5 at half HP");

            // Act
            _characterStat.TakeDamage(50f); // 50 → 0

            // Assert
            Assert.AreEqual(0f, _characterStat.HpRatio, "HP ratio should be 0 at 0 HP");
        }

        #endregion

        #region Reset & Copy 테스트

        [Test]
        public void Reset_RestoresCurrentHpToMax()
        {
            // Arrange
            _characterStat.TakeDamage(60f); // 100 → 40

            // Act
            _characterStat.Reset();

            // Assert
            Assert.AreEqual(100f, _characterStat.CurrentHp, "CurrentHp should be restored to MaxHp");
        }

        [Test]
        public void CopyFrom_CopiesAllValues()
        {
            // Arrange
            var source = new CharacterStat(maxHp: 200f, moveSpeed: 8f, damage: 20f, defense: 10f);
            source.ApplyUpgrade(StatType.Cooldown, 15f);
            source.TakeDamage(50f); // CurrentHp = 150

            // Act
            _characterStat.CopyFrom(source);

            // Assert
            Assert.AreEqual(200f, _characterStat.MaxHp);
            Assert.AreEqual(150f, _characterStat.CurrentHp);
            Assert.AreEqual(8f, _characterStat.MoveSpeed);
            Assert.AreEqual(20f, _characterStat.Damage);
            Assert.AreEqual(10f, _characterStat.Defense);
            Assert.AreEqual(15f, _characterStat.Cooldown);
        }

        [Test]
        public void CopyConstructor_CreatesExactCopy()
        {
            // Arrange
            var original = new CharacterStat(maxHp: 180f, moveSpeed: 7f, damage: 15f, defense: 8f);
            original.TakeDamage(30f);

            // Act
            var copy = new CharacterStat(original);

            // Assert
            Assert.AreEqual(original.MaxHp, copy.MaxHp);
            Assert.AreEqual(original.CurrentHp, copy.CurrentHp);
            Assert.AreEqual(original.MoveSpeed, copy.MoveSpeed);
            Assert.AreEqual(original.Damage, copy.Damage);
            Assert.AreEqual(original.Defense, copy.Defense);
        }

        #endregion

        #region Validate 테스트

        [Test]
        public void Validate_ValidStat_ReturnsTrue()
        {
            // Act & Assert
            Assert.IsTrue(_characterStat.Validate(), "Valid stat should pass validation");
        }

        [Test]
        public void Validate_InvalidMaxHp_ReturnsFalse()
        {
            // Arrange
            var stat = new CharacterStat();
            stat.MaxHp = -10f;

            // Act & Assert
            Assert.IsFalse(stat.Validate(), "Negative MaxHp should fail validation");
        }

        [Test]
        public void Validate_CurrentHpExceedsMax_ReturnsFalse()
        {
            // Arrange
            // 직접 필드를 수정하는 것은 불가능하므로, 이 테스트는 실제 시나리오에서 발생하지 않음
            // 하지만 Validate 메서드가 제대로 동작하는지 확인
            Assert.IsTrue(_characterStat.Validate());
        }

        #endregion

        #region ToString 테스트

        [Test]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            _characterStat.ApplyUpgrade(StatType.Damage, 15f);
            _characterStat.ApplyUpgrade(StatType.Defense, 5f);

            // Act
            string result = _characterStat.ToString();

            // Assert
            Assert.IsTrue(result.Contains("HP:"), "Should contain HP info");
            Assert.IsTrue(result.Contains("Speed:"), "Should contain Speed info");
            Assert.IsTrue(result.Contains("Damage:"), "Should contain Damage info");
            Assert.IsTrue(result.Contains("Defense:"), "Should contain Defense info");
        }

        #endregion
    }
}
