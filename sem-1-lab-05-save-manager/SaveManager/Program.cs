using Newtonsoft.Json;

namespace SaveManager;

public class SaveEntry
{
    public string Name { get; set; }
    public string File { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Position
{
    [JsonProperty("x")]
    public int Xpos { get; set; }
    [JsonProperty("y")]
    public int Ypos { get; set; }
}

public class SaveData
{
    public string Character { get; set; }
    public string Map { get; set; }
    public Position Position { get; set; }
    public int Health { get; set; }
    public List<string> Inventory { get; set; }
}

public class Program
{
    public static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static bool LoadIndex(string indexPath, out List<SaveEntry> indexData)
    {
        if (!File.Exists(indexPath))
        {
            PrintError($"ОШИБКА: Файла {indexPath} не существует");
            indexData = [];
            return false;
        }
        string jsonString = File.ReadAllText(indexPath);

        List<SaveEntry>? jsonData = [];
        try
        {
            jsonData = JsonConvert.DeserializeObject<List<SaveEntry>>(jsonString);
        }
        catch (JsonReaderException ex)
        {
            Console.WriteLine($"Ошибка парсинга JSON: {ex.Message}");
            indexData = [];
            return false;
        }
        catch (JsonSerializationException ex)
        {
            Console.WriteLine($"Ошибка сериализации/приведения типа: {ex.Message}");
            indexData = [];
            return false;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Ошибка Json.NET: {ex.Message}");
            indexData = [];
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Непредвиденная ошибка: {ex.Message}");
            indexData = [];
            return false;
        }

        if (jsonData is null)
        {
            PrintError($"ОШИБКА: Не удалось обработать файл {indexPath}");
            indexData = [];
            return false;
        }
        indexData = jsonData;
        return true;
    }

    public static bool LoadSave(string savesDirectory, SaveEntry entry, out SaveData saveData)
    {
        string filePath = $"{savesDirectory}/{entry.File}";
        if (!Path.Exists(filePath))
        {
            PrintError($"ОШИБКА: Файла {filePath} не существует");
            saveData = new SaveData();
            return false;
        }
        string jsonString = File.ReadAllText(filePath);
        SaveData? jsonData = new();
        try
        {
            jsonData = JsonConvert.DeserializeObject<SaveData>(jsonString);
        }
        catch (JsonReaderException ex)
        {
            Console.WriteLine($"Ошибка парсинга JSON: {ex.Message}");
            saveData = new SaveData();
            return false;
        }
        catch (JsonSerializationException ex)
        {
            Console.WriteLine($"Ошибка сериализации/приведения типа: {ex.Message}");
            saveData = new SaveData();
            return false;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Ошибка Json.NET: {ex.Message}");
            saveData = new SaveData();
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Непредвиденная ошибка: {ex.Message}");
            saveData = new SaveData();
            return false;
        }

        if (jsonData is null)
        {
            PrintError($"ОШИБКА: Не удалось обработать файл {filePath}");
            saveData = new SaveData();
            return false;
        }
        saveData = jsonData;
        return true;
    }

    public static void PrintSave(SaveEntry entry, SaveData save)
    {
        // [MEME] Stay in Character: https://www.reddit.com/r/backroomsfilm/comments/1v4f05q/stay_in_character/
        string[] keys =
        [
            "Name:", "(Stay in) Character:", "Map:", "Position:", "Health:"
        ];
        string[] values =
        [
            entry.Name, save.Character, save.Map, $"({save.Position.Xpos}, {save.Position.Ypos})", save.Health.ToString()
        ];
        int maxKeyLength = keys.Max(s => s.Length);
        int maxValueLength = values.Max(s => s.Length);
        int sumWidth = maxKeyLength + maxValueLength + 1;
        int totalWidth = sumWidth + 2;
        string horizontalLine = new('─', totalWidth);
        string masterFormat = "│ {0} │";
        string format1 = string.Format(masterFormat, "{0,-" + maxKeyLength + "} {1,-" + maxValueLength + "}");
        string format2 = string.Format(masterFormat, "{0,-" + sumWidth + "}");
        string itemsTitle = "Items";
        string itemsLeftPadding = new(' ', (totalWidth - itemsTitle.Length - 1) / 2);
        string itemsRightPadding = (totalWidth - itemsTitle.Length - 1) % 2 == 0 ? new(' ', (totalWidth - itemsTitle.Length - 1) / 2 - 1) : itemsLeftPadding;

        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("Save info:");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"╭{horizontalLine}╮");

        for (int i = 0; i < keys.Length; i++)
        {
            Console.WriteLine(format1, keys[i], values[i]);
        }

        Console.WriteLine($"├{horizontalLine}┤");
        Console.WriteLine(masterFormat, $"{itemsLeftPadding}{itemsTitle}{itemsRightPadding}");

        for (int i = 0; i < save.Inventory.Count; i++)
        {
            string line = $"{i + 1}. {save.Inventory[i]}";
            Console.WriteLine(format2, line);
        }

        Console.WriteLine($"╰{horizontalLine}╯");

        Console.ResetColor();
    }

    public static void Main()
    {
        if (!LoadIndex("../../../index.json", out List<SaveEntry> indexData))
        {
            Console.WriteLine("Не удалось корректно обработать файл index.json");
            return;
        }
        SaveEntry saveSlot = indexData[0];
        if (!LoadSave("../../../saves/", saveSlot, out SaveData saveData))
        {
            Console.WriteLine($"Не удалось корректно обработать сохранение {saveSlot.Name} в файле {saveSlot.File}");
            return;
        }
        PrintSave(saveSlot, saveData);
    }
}
