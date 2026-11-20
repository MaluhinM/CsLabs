namespace RPGCharacter;

public class Character
{
    private string _className;
    private int _health;
    private int _level;
    private int _experience;
    private int _gold;
    private int _strength;
    private int _defense;
    private int _inventoryCapacity;
    private readonly List<string> _inventory = [];

    public string Name { get; }
    public string ClassName => _className;
    public int Health => _health;
    public int MaxHealth { get; }
    public int Level => _level;
    public int Experience => _experience;
    public int Gold => _gold;
    public int Strength => _strength;
    public int Defense => _defense;
    public int InventoryCapacity => _inventoryCapacity;
    public IReadOnlyList<string> Inventory => _inventory;

    public Character(string name, string className = "None", int maxHealth = 100, int level = 1, int experience = 0, int gold = 0, int strength = 1, int defense = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя не может быть пустым", nameof(name));
        if (string.IsNullOrWhiteSpace(className))
            throw new ArgumentException("Название класса не может быть пустым", nameof(className));
        if (maxHealth <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxHealth), maxHealth, "Максимальное здоровье должно быть положительным");
        if (level < 1)
            throw new ArgumentOutOfRangeException(nameof(level), level, "Уровень не может быть меньше 1");
        if (experience < 0 || gold < 0 || strength < 0 || defense < 0)
            throw new ArgumentOutOfRangeException("Опыт, золото, сила и защита не могут быть отрицательными");

        Name = name;
        _className = className;
        _health = maxHealth;
        MaxHealth = maxHealth;
        _level = level;
        _experience = experience;
        _gold = gold;
        _strength = strength;
        _defense = defense;
        _inventory = [];
        _inventoryCapacity = 3;
    }

    public void SetClassName(string className)
    {
        if (string.IsNullOrEmpty(className))
            throw new ArgumentException("Название класса не может быть пустым", nameof(className));
        _className = className;
    }

    public void TakeDamage(int damage)
    {
        if (damage < 0)
            throw new ArgumentOutOfRangeException(nameof(damage), damage, "Количество получаемого урона не может быть отрицательным");
        int takenDamage = damage * 100 / (100 + Defense);
        if (takenDamage > Health)
        {
            _health = 0;
        }
        else
        {
            _health -= takenDamage;
        }
    }

    public void RestoreHealth(int heal)
    {
        if (heal < 0)
            throw new ArgumentOutOfRangeException(nameof(heal), heal, "Количество восстанавливаемого здоровья не может быть отрицательным");
        if (Health + heal > MaxHealth)
        {
            _health = MaxHealth;
        }
        else
        {
            _health += heal;
        }
    }

    public int GetLevelUpExperience()
    {
        return (int)Math.Pow(2, Level);
    }

    public void IncreaseExperience(int experience)
    {
        if (experience < 0)
            throw new ArgumentOutOfRangeException(nameof(experience), experience, "Количество добавляемого опыта не может быть отрицательным");

        _experience += experience;

        while (_experience >= GetLevelUpExperience())
        {
            _experience -= GetLevelUpExperience();
            _level++;
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
        if (goldCount > Gold)
            throw new InvalidOperationException("Недостаточно имеющегося золота");
        _gold -= goldCount;
    }

    public void EditStrength(int value)
    {
        int newStrength = Strength + value;
        if (newStrength < 0)
            throw new InvalidOperationException("Значение силы не может быть отрицательным");
        _strength = newStrength;
    }

    public void EditDefense(int value)
    {
        int newDefense = Defense + value;
        if (newDefense < 0)
            throw new InvalidOperationException("Значение защиты не может быть отрицательным");
        _defense = newDefense;
    }

    public void AddItem(string item)
    {
        if (string.IsNullOrWhiteSpace(item))
            throw new InvalidOperationException("Название предмета не может быть пустым");
        if (Inventory.Count == InventoryCapacity)
            throw new InvalidOperationException($"Недостаточно вместимости инвентаря для добавления предмета '{item}'");
        _inventory.Add(item);
    }

    public void RemoveItem(string item)
    {
        if (!_inventory.Remove(item))
            throw new InvalidOperationException($"В инвентаре нет предмета '{item}'");
    }

    public void EditInventoryCapacity(int value)
    {
        int newCapacity = _inventoryCapacity + value;
        if (newCapacity < 0)
            throw new InvalidOperationException("Вместимость инвентаря не может быть отрицательной");
        if (newCapacity < _inventory.Count)
            throw new InvalidOperationException(
                $"Невозможно уменьшить вместимость до {newCapacity}, " +
                $"поскольку уже занято {_inventory.Count} ячеек");
        _inventoryCapacity = newCapacity;
    }

    public string GetInfo()
    {
        string[] keys =
        [
            "Name:", "Class:", "Health:", "Max Health:", "Level:", "Experience:", "Level Up:", "Gold:", "Strength:", "Defense:"
        ];
        string[] values =
        [
            Name, ClassName, Health.ToString(), MaxHealth.ToString(), Level.ToString(), Experience.ToString(), GetLevelUpExperience().ToString(), Gold.ToString(), Strength.ToString(), Defense.ToString()
        ];
        int maxKeyLength = keys.Max(s => s.Length);
        int maxValueLength = values.Max(s => s.Length);
        int sumWidth = maxKeyLength + maxValueLength + 1;
        int totalWidth = sumWidth + 2;
        string horizontalLine = new('─', totalWidth);
        string masterFormat = "│ {0} │\n";
        string format1 = string.Format(masterFormat, "{0,-" + maxKeyLength + "} {1,-" + maxValueLength + "}");
        string format2 = string.Format(masterFormat, "{0,-" + sumWidth + "}");
        string inventoryTitle = "Inventory";
        string inventoryLeftPadding = new(' ', (totalWidth - inventoryTitle.Length - 1) / 2);
        string inventoryRightPadding = (totalWidth - inventoryTitle.Length - 1) % 2 == 0 ? new(' ', (totalWidth - inventoryTitle.Length - 1) / 2 - 1) : inventoryLeftPadding;

        string result = $"╭{horizontalLine}╮\n";

        for (int i = 0; i < keys.Length; i++)
        {
            result += string.Format(format1, keys[i], values[i]);
        }

        result += $"├{horizontalLine}┤\n";
        result += string.Format(masterFormat, $"{inventoryLeftPadding}{inventoryTitle}{inventoryRightPadding}");

        for (int i = 0; i < Inventory.Count; i++)
        {
            result += string.Format(format2, $"{i + 1}. {Inventory[i]}");
        }

        result += $"╰{horizontalLine}╯";

        return result;
    }
}

public class Program
{
    public static void Main()
    {
        string line = File.ReadAllLines("character.txt")[0];
        string[] parts = line.Split('|');
        if (parts.Length != 8)
        {
            OutputTools.PrintError("Не удалось разбить строку входного файла");
            return;
        }
        Character character = new(
            name: parts[0],
            className: parts[1],
            maxHealth: int.Parse(parts[2]),
            level: int.Parse(parts[3]),
            experience: int.Parse(parts[4]),
            gold: int.Parse(parts[5]),
            strength: int.Parse(parts[6]),
            defense: int.Parse(parts[7])
            );

        Console.OutputEncoding = System.Text.Encoding.UTF8;
        bool working = true;
        while (working)
        {
            Console.WriteLine("""

                1 - Показать информацию о персонаже
                2 - Получить урон
                3 - Восстановить здоровье
                4 - Получить опыт
                5 - Получить золото
                6 - Потратить золото
                7 - Изменить параметр силы
                8 - Изменить параметр защиты
                9 - Изменить параметр вместимости инвентаря
                10 - Добавить предмет в инвентарь
                11 - Убрать предмет из инвентаря
                0 - Выход
                """);

            int? option = InputTools.GetNumber("Выбор: ");
            switch (option)
            {
                case 1:
                    {
                        Console.WriteLine(character.GetInfo());
                        break;
                    }
                case 2:
                    {
                        int? damage = InputTools.GetNumber("Полученный урон: ");
                        if (damage is null)
                            break;
                        try
                        {
                            int oldHealth = character.Health;
                            character.TakeDamage(damage.Value);
                            Console.WriteLine($"Получено {oldHealth - character.Health} урона! Осталось {character.Health}/{character.MaxHealth} - {100 * character.Health / character.MaxHealth}% здоровья");
                        }
                        catch (ArgumentOutOfRangeException e)
                        {
                            OutputTools.PrintError(e.Message);
                        }
                        break;
                    }
                case 3:
                    {
                        int? heal = InputTools.GetNumber("Восстановленное здоровье: ");
                        if (heal is null)
                            break;
                        try
                        {
                            int oldHealth = character.Health;
                            character.RestoreHealth(heal.Value);
                            Console.WriteLine($"Восстановлено {character.Health - oldHealth} здоровья! Здоровье: {character.Health}/{character.MaxHealth} - {100 * character.Health / character.MaxHealth}%");
                        }
                        catch (ArgumentOutOfRangeException e)
                        {
                            OutputTools.PrintError(e.Message);
                        }
                        break;
                    }
                case 4:
                    {
                        int? experience = InputTools.GetNumber("Количество полученного опыта: ");
                        if (experience is null)
                            break;
                        try
                        {
                            int oldLevel = character.Level;
                            character.IncreaseExperience(experience.Value);
                            Console.Write($"Получено {experience.Value} опыта! ");
                            if (oldLevel == character.Level)
                            {
                                Console.Write($"Уровень: {oldLevel}. ");
                            }
                            else
                            {
                                Console.Write($"Новый уровень: {oldLevel} -> {character.Level}. ");
                            }
                            Console.WriteLine($"Опыт: {character.Experience}/{character.GetLevelUpExperience()} - {100 * character.Experience / character.GetLevelUpExperience()}%");
                        }
                        catch (ArgumentOutOfRangeException e)
                        {
                            OutputTools.PrintError(e.Message);
                        }
                        break;
                    }
                case 5:
                    {
                        int? goldCount = InputTools.GetNumber("Количество полученного золота: ");
                        if (goldCount is null)
                            break;
                        try
                        {
                            character.PickUpGold(goldCount.Value);
                            Console.WriteLine($"Получено {goldCount.Value} золота! Количество имеющегося золота: {character.Gold}");
                        }
                        catch (ArgumentOutOfRangeException e)
                        {
                            OutputTools.PrintError(e.Message);
                        }
                        break;
                    }
                case 6:
                    {
                        int? goldCount = InputTools.GetNumber("Количество потраченного золота: ");
                        if (goldCount is null)
                            break;
                        try
                        {
                            character.SpendGold(goldCount.Value);
                            Console.WriteLine($"Потрачено {goldCount.Value} золота! Количество имеющегося золота: {character.Gold}");
                        }
                        catch (ArgumentOutOfRangeException e)
                        {
                            OutputTools.PrintError(e.Message);
                        }
                        catch (InvalidOperationException e)
                        {
                            OutputTools.PrintError(e.Message);
                        }
                        break;
                    }
                case 7:
                    {
                        int? strengthVector = InputTools.GetNumber("Вектор изменения параметра силы: ");
                        if (strengthVector is null)
                            break;
                        try
                        {
                            character.EditStrength(strengthVector.Value);
                            if (strengthVector >= 0)
                                Console.Write($"Сила увеличена на {strengthVector}! ");
                            else
                                Console.Write($"Сила уменьшена на {-strengthVector}! ");
                            Console.WriteLine($"Текущее значение силы: {character.Strength}");
                        }
                        catch (InvalidOperationException e)
                        {
                            OutputTools.PrintError(e.Message);
                        }
                        break;
                    }
                case 8:
                    {
                        int? defenseVector = InputTools.GetNumber("Вектор изменения параметра защиты: ");
                        if (defenseVector is null)
                            break;
                        try
                        {
                            character.EditDefense(defenseVector.Value);
                            if (defenseVector >= 0)
                                Console.Write($"Защита увеличена на {defenseVector}! ");
                            else
                                Console.Write($"Защита уменьшена на {-defenseVector}! ");
                            Console.WriteLine($"Текущее значение защиты: {character.Defense}");
                        }
                        catch (InvalidOperationException e)
                        {
                            OutputTools.PrintError(e.Message);
                        }
                        break;
                    }
                case 9:
                    {
                        int? capacityVector = InputTools.GetNumber("Вектор изменения параметра вместимости инвентаря: ");
                        if (capacityVector is null)
                            break;
                        try
                        {
                            character.EditInventoryCapacity(capacityVector.Value);
                            if (capacityVector >= 0)
                                Console.Write($"Вместимость увеличена на {capacityVector}! ");
                            else
                                Console.Write($"Вместимость уменьшена на {-capacityVector}! ");
                            Console.WriteLine($"Текущее значение вместимости инвентаря: {character.InventoryCapacity}");
                        }
                        catch (InvalidOperationException e)
                        {
                            OutputTools.PrintError(e.Message);
                        }
                        break;
                    }
                case 10:
                    {
                        string itemName = InputTools.GetString("Название получаемого предмета: ").Trim();
                        if (string.IsNullOrEmpty(itemName))
                            break;
                        try
                        {
                            character.AddItem(itemName);
                            Console.WriteLine($"Предмет {itemName} добавлен в инвентарь! Количество имеющихся вещей: {character.Inventory.Count}/{character.InventoryCapacity} - {100 * character.Inventory.Count / character.InventoryCapacity}%");
                        }
                        catch (InvalidOperationException e)
                        {
                            OutputTools.PrintError(e.Message);
                        }
                        break;
                    }
                case 11:
                    {
                        string itemName = InputTools.GetString("Название убираемого предмета: ").Trim();
                        if (string.IsNullOrEmpty(itemName))
                            break;
                        try
                        {
                            character.RemoveItem(itemName);
                            Console.WriteLine($"Предмет {itemName} убран из инвентаря! Количество имеющихся вещей: {character.Inventory.Count}/{character.InventoryCapacity} - {100 * character.Inventory.Count / character.InventoryCapacity}%");
                        }
                        catch (InvalidOperationException e)
                        {
                            OutputTools.PrintError(e.Message);
                        }
                        break;
                    }
                case 0:
                    Console.WriteLine("Пока!");
                    working = false;
                    break;
                case null:
                    break;
                default:
                    {
                        OutputTools.PrintError("Такой операции нет! Попробуйте ещё раз");
                        break;
                    }
            }
        }
    }
}
