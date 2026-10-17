using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

public static class SafeFile
{
    public static void CreateDirectoryNameSafe(string dirPath)
    {
        string? dirName = Path.GetDirectoryName(dirPath);
        if (!string.IsNullOrEmpty(dirName))
            Directory.CreateDirectory(dirName);
    }

    public static void WriteAllTextSafe(string path, string content, Encoding? encoding = null)
    {
        CreateDirectoryNameSafe(path);

        if (encoding == null)
            File.WriteAllText(path, content);
        else
            File.WriteAllText(path, content, encoding);
    }

    public static void CopySafe(string pathFrom, string pathTo)
    {
        CreateDirectoryNameSafe(pathTo);

        File.Copy(pathFrom, pathTo);
    }
}

public class Logger
{
    public string Pattern;
    private StreamWriter? _writer;

    public void OpenFile(string filePath, bool append = true)
    {
        _writer = new StreamWriter(filePath, append, Encoding.UTF8);
    }

    public void InitLog(string logPath = "last.log", string pattern = "{0} [{1}][{2}] {3}: {4}")
    {
        Pattern = pattern;
        File.WriteAllText(logPath, string.Empty);
        OpenFile(logPath);
    }

    public void Write(string text)
    {
        if (_writer == null)
            throw new InvalidOperationException("Файл не был открыт. Сначала вызовите OpenFile()");
        _writer.WriteLine(text);
        _writer.Flush();
    }

    public void WriteOut(string text)
    {
        Write(text);
        Console.WriteLine(text);
    }

    public void Log(string level, string operation, string saveName, string message)
    {
        string dateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        string text = string.Format(Pattern, dateTime, level, operation, saveName, message);
        Write(text);
    }

    public void LogOut(string level, string operation, string saveName, string message)
    {
        Log(level, operation, saveName, message);
        Console.WriteLine(message);
    }

    public void CloseFile()
    {
        _writer?.Close();
    }
}

public class Program
{
    private const string IndexPath = "../../../index.json";
    private const string SavesDir = "../../../saves/";
    private const string BackupsDir = "../../../backups/";

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

        List<SaveEntry>? jsonData;
        try
        {
            jsonData = JsonConvert.DeserializeObject<List<SaveEntry>>(jsonString);
        }
        catch (JsonReaderException ex)
        {
            PrintError($"Ошибка парсинга JSON: {ex.Message}");
            indexData = [];
            return false;
        }
        catch (JsonSerializationException ex)
        {
            PrintError($"Ошибка сериализации/приведения типа: {ex.Message}");
            indexData = [];
            return false;
        }
        catch (JsonException ex)
        {
            PrintError($"Ошибка Json.NET: {ex.Message}");
            indexData = [];
            return false;
        }
        catch (Exception ex)
        {
            PrintError($"Непредвиденная ошибка: {ex.Message}");
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
        string filePath = Path.Combine(savesDirectory, entry.File);
        if (!Path.Exists(filePath))
        {
            PrintError($"ОШИБКА: Файла {filePath} не существует");
            saveData = new();
            return false;
        }
        string jsonString = File.ReadAllText(filePath);
        SaveData? jsonData;
        try
        {
            jsonData = JsonConvert.DeserializeObject<SaveData>(jsonString);
        }
        catch (JsonReaderException ex)
        {
            PrintError($"Ошибка парсинга JSON: {ex.Message}");
            saveData = new();
            return false;
        }
        catch (JsonSerializationException ex)
        {
            PrintError($"Ошибка сериализации/приведения типа: {ex.Message}");
            saveData = new();
            return false;
        }
        catch (JsonException ex)
        {
            PrintError($"Ошибка Json.NET: {ex.Message}");
            saveData = new();
            return false;
        }
        catch (Exception ex)
        {
            PrintError($"Непредвиденная ошибка: {ex.Message}");
            saveData = new();
            return false;
        }

        if (jsonData is null)
        {
            PrintError($"ОШИБКА: Не удалось обработать файл {filePath}");
            saveData = new();
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

    public static bool FindByName(List<SaveEntry> entries, string name, out SaveEntry result)
    {
        foreach (SaveEntry entry in entries)
        {
            if (entry.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                result = entry;
                return true;
            }
        }
        PrintError($"ОШИБКА: Сохранения с именем \"{name}\" не существует");
        result = null;
        return false;
    }

    public static void SaveGame(string savesDirectory, SaveEntry entry, SaveData save)
    {
        string jsonString = JsonConvert.SerializeObject(save, Formatting.Indented);
        string filePath = Path.Combine(savesDirectory, entry.File);
        SafeFile.WriteAllTextSafe(filePath, jsonString);
    }

    public static void SaveIndex(string indexPath, List<SaveEntry> entries)
    {
        JsonSerializerSettings settings = new()
        {
            Formatting = Formatting.Indented,
            DateFormatString = "yyyy-MM-ddTHH:mm:ss"
        };
        string jsonString = JsonConvert.SerializeObject(entries, settings);
        SafeFile.WriteAllTextSafe(indexPath, jsonString);
    }

    public static bool CreateBackup(string savesDirectory, string backupsDirectory, SaveEntry entry)
    {
        string filePath = Path.Combine(savesDirectory, entry.File);
        string backupPath = Path.Combine(backupsDirectory, entry.File);
        if (!File.Exists(filePath))
        {
            PrintError($"ОШИБКА: Файла {filePath} не существует");
            return false;
        }
        if (File.Exists(backupPath))
            File.Delete(backupPath);
        SafeFile.CopySafe(filePath, backupPath);
        return true;
    }

    public static bool RestoreBackup(string savesDirectory, string backupsDirectory, SaveEntry entry)
    {
        string filePath = Path.Combine(savesDirectory, entry.File);
        string backupPath = Path.Combine(backupsDirectory, entry.File);
        if (!File.Exists(backupPath))
        {
            PrintError($"ОШИБКА: Файла {backupPath} не существует");
            return false;
        }
        SafeFile.CreateDirectoryNameSafe(filePath);
        File.Copy(backupPath, filePath, overwrite: true);
        return true;
    }

    public static bool DeleteSave(string savesDirectory, string indexPath, List<SaveEntry> entries, SaveEntry entry)
    {
        if (!entries.Contains(entry))
        {
            PrintError("ОШИБКА: Переданного сохранения нет в списке индекса");
            return false;
        }

        string savePath = Path.Combine(savesDirectory, entry.File);
        if (File.Exists(savePath))
            File.Delete(savePath);
        else
            Console.WriteLine($"ПРЕДУПРЕЖДЕНИЕ: Файла {savePath} не существует");

        entries.Remove(entry);
        SaveIndex(indexPath, entries);
        return true;
    }

    private static void Test()
    {
        if (!LoadIndex(IndexPath, out List<SaveEntry> indexData))
        {
            Console.WriteLine("Не удалось корректно обработать файл index.json");
            return;
        }
        if (!FindByName(indexData, "slot-01", out SaveEntry saveSlot))
        {
            Console.WriteLine("Не удалось найти сохранение");
            return;
        }
        if (!LoadSave(SavesDir, saveSlot, out SaveData saveData))
        {
            Console.WriteLine($"Не удалось корректно обработать сохранение {saveSlot.Name} в файле {saveSlot.File}");
            return;
        }
        PrintSave(saveSlot, saveData);
        //SaveGame("../../../saves1/", saveSlot, saveData);
        //CreateBackup(SavesDir, BackupsDir, saveSlot);
        //File.Delete(Path.Combine(SavesDir, saveSlot.File));
        //RestoreBackup(SavesDir, BackupsDir, saveSlot);
        //DeleteSave(SavesDir, IndexPath, indexData, saveSlot);
    }

    private static string GetString(string prompt)
    {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.Write(prompt);
        Console.ResetColor();
        string? input = Console.ReadLine();
        if (input is null) return string.Empty;
        return input;
    }

    private static int GetNumber(string prompt)
    {
        while (true)
        {
            string input = GetString(prompt);
            bool success = int.TryParse(input, out int result);
            if (success) return result;
            PrintError("Введённое значение не является числом. Пожалуйста, попробуйте ещё раз");
        }
    }

    public static void ShowSavesList(List<SaveEntry> entries)
    {
        Console.WriteLine("Список всех сохранений:");
        for (int i = 0; i < entries.Count; i++)
            Console.WriteLine($"{i + 1}. {entries[i].Name}: {entries[i].File} - {entries[i].UpdatedAt}");
    }

    public static void Main()
    {
        if (!LoadIndex(IndexPath, out List<SaveEntry> indexData))
        {
            Console.WriteLine("Не удалось корректно обработать файл index.json");
            return;
        }

        Logger logger = new();
        logger.InitLog();
        bool working = true;
        while (working)
        {
            Console.WriteLine(
                """

                Выберите номер операции:
                    1. Показать список сохранений.
                    2. Посмотреть данные сохранения.
                    3. Изменить данные персонажа и записать их обратно.
                    4. Создать резервную копию.
                    5. Восстановить из резервной копии.
                    6. Удалить сохранение.
                    7. Выйти.
                """
            );
            int option = GetNumber("Номер: ");
            switch (option)
            {
                case 1:
                    {
                        ShowSavesList(indexData);
                        break;
                    }
                case 2:
                    {
                        string saveName = GetString("Имя сохранения: ");
                        if (!FindByName(indexData, saveName, out SaveEntry entry))
                        {
                            logger.Log("Warning", "ShowSaveData", saveName, "Не удалось найти сохранение в индексе с таким именем");
                            break;
                        }
                        if (!LoadSave(SavesDir, entry, out SaveData saveData))
                        {
                            logger.Log("Error", "ShowSaveData", saveName, "Не удалось загрузить сохранение с таким именем");
                            break;
                        }
                        PrintSave(entry, saveData);
                        logger.Log("Info", "ShowSaveData", saveName, "Выведены данные сохранения");
                        break;
                    }
                case 3:
                    {
                        string saveName = GetString("Имя сохранения: ");
                        if (!FindByName(indexData, saveName, out SaveEntry entry))
                        {
                            logger.Log("Warning", "EditSaveData", saveName, "Не удалось найти сохранение в индексе с таким именем");
                            break;
                        }
                        if (!LoadSave(SavesDir, entry, out SaveData saveData))
                        {
                            logger.Log("Error", "EditSaveData", saveName, "Не удалось загрузить сохранение с таким именем");
                            break;
                        }

                        string mapEnter = GetString($"Карта [{saveData.Map}]: ");
                        if (!string.IsNullOrEmpty(mapEnter))
                            saveData.Map = mapEnter;
                        string xEnter = GetString($"Координата x [{saveData.Position.Xpos}]: ");
                        if (!string.IsNullOrEmpty(xEnter) && int.TryParse(xEnter, out int xNum))
                            saveData.Position.Xpos = xNum;
                        string yEnter = GetString($"Координата y [{saveData.Position.Ypos}]: ");
                        if (!string.IsNullOrEmpty(yEnter) && int.TryParse(yEnter, out int yNum))
                            saveData.Position.Ypos = yNum;
                        string healthEnter = GetString($"Здоровье [{saveData.Health}]: ");
                        if (!string.IsNullOrEmpty(healthEnter) && int.TryParse(healthEnter, out int healthNum))
                            saveData.Health = healthNum;

                        int count = 0;
                        List<string> newInventory = [];
                        while (true)
                        {
                            string currentValue = count < saveData.Inventory.Count ? $" [{saveData.Inventory[count]}]" : "";
                            string userInput = GetString($"Предмет № {count + 1}{currentValue}: ");
                            if (string.IsNullOrEmpty(userInput))
                            {
                                for (int i = count; i < saveData.Inventory.Count; i++)
                                    newInventory.Add(saveData.Inventory[i]);
                                break;
                            }
                            newInventory.Add(userInput);
                            count++;
                        }
                        saveData.Inventory = newInventory;
                        SaveGame(SavesDir, entry, saveData);
                        entry.UpdatedAt = DateTime.Now;
                        SaveIndex(IndexPath, indexData);
                        logger.LogOut("Info", "EditSaveData", saveName, "Сохранение успешно изменено");
                        break;
                    }
                case 4:
                    {
                        string saveName = GetString("Имя сохранения: ");
                        if (!FindByName(indexData, saveName, out SaveEntry entry))
                        {
                            logger.Log("Warning", "BackupSaveData", saveName, "Не удалось найти сохранение в индексе с таким именем");
                            break;
                        }
                        if (CreateBackup(SavesDir, BackupsDir, entry))
                            logger.LogOut("Info", "BackupSaveData", saveName, "Резервная копия сохранения успешно создана");
                        else
                            logger.Log("Error", "BackupSaveData", saveName, "Не удалось создать резервную копию сохранения");
                        break;
                    }
                case 5:
                    {
                        string saveName = GetString("Имя сохранения: ");
                        if (!FindByName(indexData, saveName, out SaveEntry entry))
                        {
                            logger.Log("Warning", "RestoreSaveData", saveName, "Не удалось найти сохранение в индексе с таким именем");
                            break;
                        }
                        if (RestoreBackup(SavesDir, BackupsDir, entry))
                            logger.LogOut("Info", "RestoreSaveData", saveName, "Сохранение успешно восстановлено");
                        else
                            logger.Log("Error", "RestoreSaveData", saveName, "Не удалось восстановить сохранение");
                        break;
                    }
                case 6:
                    {
                        string saveName = GetString("Имя сохранения: ");
                        if (!FindByName(indexData, saveName, out SaveEntry entry))
                        {
                            logger.Log("Warning", "DeleteSaveData", saveName, "Не удалось найти сохранение в индексе с таким именем");
                            break;
                        }
                        if (DeleteSave(SavesDir, IndexPath, indexData, entry))
                            logger.LogOut("Info", "DeleteSaveData", saveName, "Сохранение успешно удалено");
                        else
                            logger.Log("Error", "DeleteSaveData", saveName, "Не удалось удалить сохранение");
                        break;
                    }
                case 7:
                    {
                        logger.CloseFile();
                        working = false;
                        Console.WriteLine("Пока!");
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Такой операции нет");
                        break;
                    }
            }
        }
    }
}
