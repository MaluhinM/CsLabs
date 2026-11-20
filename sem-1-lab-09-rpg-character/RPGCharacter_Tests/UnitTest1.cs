using RPGCharacter;

namespace RPGCharacter_Tests;

public class CharacterTests
{
    // ─── Конструктор: все проверки входных данных ───

    [Test]
    public void Constructor_EmptyClassName_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Character("Hero", className: ""));
    }

    [Test]
    public void Constructor_NonPositiveMaxHealth_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Character("Hero", maxHealth: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Character("Hero", maxHealth: -10));
    }

    [Test]
    public void Constructor_LevelBelowOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Character("Hero", level: 0));
    }

    [Test]
    public void Constructor_NegativeExperience_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Character("Hero", experience: -1));
    }

    [Test]
    public void Constructor_NegativeGold_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Character("Hero", gold: -1));
    }

    [Test]
    public void Constructor_NegativeStrength_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Character("Hero", strength: -1));
    }

    [Test]
    public void Constructor_NegativeDefense_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Character("Hero", defense: -1));
    }

    // ─── Здоровье ───

    [Test]
    public void TakeDamage_ReducesHealth()
    {
        var c = new Character("Hero", maxHealth: 100);
        c.TakeDamage(30);
        Assert.That(c.Health, Is.EqualTo(70));
    }

    [Test]
    public void RestoreHealth_Negative_Throws()
    {
        var c = new Character("Hero", maxHealth: 100);
        Assert.Throws<ArgumentOutOfRangeException>(() => c.RestoreHealth(-5));
    }

    // ─── Золото ───

    [Test]
    public void PickUpGold_Negative_Throws()
    {
        var c = new Character("Hero");
        Assert.Throws<ArgumentOutOfRangeException>(() => c.PickUpGold(-10));
    }

    [Test]
    public void SpendGold_Negative_Throws()
    {
        var c = new Character("Hero", gold: 100);
        Assert.Throws<ArgumentOutOfRangeException>(() => c.SpendGold(-10));
    }

    [Test]
    public void SpendGold_ReducesGold()
    {
        var c = new Character("Hero", gold: 100);
        c.SpendGold(30);
        Assert.That(c.Gold, Is.EqualTo(70));
    }

    // ─── Сила и защита ───

    [Test]
    public void EditStrength_BelowZero_Throws()
    {
        var c = new Character("Hero", strength: 5);
        Assert.Throws<InvalidOperationException>(() => c.EditStrength(-10));
    }

    [Test]
    public void EditStrength_IncreasesValue()
    {
        var c = new Character("Hero", strength: 5);
        c.EditStrength(3);
        Assert.That(c.Strength, Is.EqualTo(8));
    }

    [Test]
    public void EditDefense_BelowZero_Throws()
    {
        var c = new Character("Hero", defense: 5);
        Assert.Throws<InvalidOperationException>(() => c.EditDefense(-10));
    }

    // ─── Инвентарь ───

    [Test]
    public void AddItem_EmptyName_Throws()
    {
        var c = new Character("Hero");
        Assert.Throws<InvalidOperationException>(() => c.AddItem(""));
        Assert.Throws<InvalidOperationException>(() => c.AddItem("   "));
    }

    [Test]
    public void EditInventoryCapacity_BelowZero_Throws()
    {
        var c = new Character("Hero");
        Assert.Throws<InvalidOperationException>(() => c.EditInventoryCapacity(-10));
    }

    [Test]
    public void EditInventoryCapacity_BelowOccupied_Throws()
    {
        var c = new Character("Hero");
        c.AddItem("a");
        c.AddItem("b");
        // Занято 2 ячейки, пытаемся уменьшить до 1 (3 - 2 = 1)
        Assert.Throws<InvalidOperationException>(() => c.EditInventoryCapacity(-2));
    }

    [Test]
    public void Inventory_IsReadOnlyFromOutside()
    {
        var c = new Character("Hero");
        c.AddItem("sword");
        // Проверяем, что наружу отдаётся IReadOnlyList, а не List
        Assert.That(c.Inventory, Is.AssignableTo<IReadOnlyList<string>>());
        // Следующая строка не скомпилируется, если раскомментировать:
        // c.Inventory.Add("hack");  // List<string> не имеет Add в IReadOnlyList
    }

    // ─── Опыт ───

    [Test]
    public void IncreaseExperience_WithoutLevelUp_Accumulates()
    {
        var c = new Character("Hero", level: 1, experience: 0);
        c.IncreaseExperience(1); // порог 2, добавляем 1
        Assert.That(c.Level, Is.EqualTo(1));
        Assert.That(c.Experience, Is.EqualTo(1));
    }
}