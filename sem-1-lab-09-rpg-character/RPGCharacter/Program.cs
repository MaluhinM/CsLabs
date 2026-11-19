namespace RPGCharacter;

public class Character
{
    public readonly string Name;
    private string _className;
    private int _level;
    private int _experience;
    public int Health;
    private readonly int _maxHealth;
    private int _gold;
    private int _strength;
    private int _defense;
    private readonly List<string> _inventory;
    private readonly int _inventoryCapacity;

    private Character(string name, int maxHealth)
    {
        Name = name;
        _className = "Крестьянин";
        _level = 0;
        _experience = 0;
        Health = maxHealth;
        _maxHealth = maxHealth;
        _gold = 0;
        _strength = 1;
        _defense = 1;
        _inventory = [];
        _inventoryCapacity = 3;
    }

    public void SetClassName(string className)
    {
        if (string.IsNullOrEmpty(className))
            throw new ArgumentNullException(nameof(className), "Название класса не может быть пустым");
        _className = className;
    }

    public void TakeDamage(int damage)
    {
        if (damage < 0)
            throw new ArgumentOutOfRangeException(nameof(damage), damage, "Количество получаемого урона не может быть отрицательным");
        if (damage > Health)
        {
            Health = 0;
        }
        else
        {
            Health -= damage;
        }
    }

    public void RestoreHealth(int heal)
    {
        if (heal < 0)
            throw new ArgumentOutOfRangeException(nameof(heal), heal, "Количество восстанавливаемого здоровья не может быть отрицательным");
        if (Health + heal > _maxHealth)
        {
            Health = _maxHealth;
        }
        else
        {
            Health += heal;
        }
    }

    private int CalculateLevelUpExperience()
    {
        return (int)Math.Pow(2, _level);
    }

    public void IncreaseExperience(int experience)
    {
        if (experience < 0)
            throw new ArgumentOutOfRangeException(nameof(experience), experience, "Количество добавляемого опыта не может быть отрицательным");
        int levelUpExperience = CalculateLevelUpExperience();
        int newExperience = _experience + experience;
        if (newExperience > levelUpExperience)
        {
            _level += newExperience / levelUpExperience;
            _experience = newExperience % levelUpExperience;
        }
        else
        {
            _experience = newExperience;
        }
    }

    public void PickUpGold(int goldCount)
    {
        if (goldCount < 0)
            throw new ArgumentOutOfRangeException(nameof(goldCount), goldCount, "Количество получаемого золота не может быть отрицательным");
        _gold += goldCount;
    }

    public void SpendGold(int goldCount)
    {
        if (goldCount < 0)
            throw new ArgumentOutOfRangeException(nameof(goldCount), goldCount, "Количество затрачиваемого золота не может быть отрицательным");
        if (goldCount > _gold)
            throw new InvalidOperationException("Недостаточно имеющегося золота");
        _gold -= goldCount;
    }

    public void EditStrength(int value)
    {
        if (value < 0 && -value > _strength)
            throw new InvalidOperationException("Значение силы не может быть отрицательным");
        _strength += value;
    }

    public void EditDefense(int value)
    {
        if (value < 0 && -value > _defense)
            throw new InvalidOperationException("Значение защиты не может быть отрицательным");
        _defense += value;
    }

    public void AddItem(string item)
    {
        if (_inventory.Count == _inventoryCapacity)
            throw new InvalidOperationException($"Недостаточно вместительности инвентаря для добавления предмета {item}");
        _inventory.Add(item);
    }

    public void RemoveItem(string item)
    {
        if (!_inventory.Remove(item))
            throw new InvalidOperationException($"В инвентаре нет предмета '{item}'");
    }

    public string GetInfo()
    {
        string[] keys =
        [
            "Name:", "Class:", "Level:", "Experience:", "Health:", "Max Health:", "Gold:", "Strength:", "Defense:"
        ];
        string[] values =
        [
            Name, _class, _level.ToString(), _experience.ToString(), Health.ToString(), _maxHealth.ToString(), _gold.ToString(), _strength.ToString(), _defense.ToString()
        ];
        int maxKeyLength = keys.Max(s => s.Length);
        int maxValueLength = values.Max(s => s.Length);
        int sumWidth = maxKeyLength + maxValueLength + 1;
        int totalWidth = sumWidth + 2;
        string horizontalLine = new('─', totalWidth);
        string masterFormat = "│ {0} │";
        string format1 = string.Format(masterFormat, "{0,-" + maxKeyLength + "} {1,-" + maxValueLength + "}");
        string format2 = string.Format(masterFormat, "{0,-" + sumWidth + "}");
        string inventoryTitle = "Inventory";
        string inventoryLeftPadding = new(' ', (totalWidth - inventoryTitle.Length - 1) / 2);
        string inventoryRightPadding = (totalWidth - inventoryTitle.Length - 1) % 2 == 0 ? new(' ', (totalWidth - inventoryTitle.Length - 1) / 2 - 1) : inventoryLeftPadding;

        string result = $"╭{horizontalLine}╮";

        for (int i = 0; i < keys.Length; i++)
        {
            result += (format1, keys[i], values[i]);
        }

        result += $"├{horizontalLine}┤";
        result += (masterFormat, $"{inventoryLeftPadding}{inventoryTitle}{inventoryRightPadding}");

        for (int i = 0; i < _inventory.Count; i++)
        {
            result += (format2, $"{i + 1}. {_inventory[i]}");
        }

        result += $"╰{horizontalLine}╯";

        return result;
    }
}

public class Program
{
    public static void Main()
    {

    }
}
