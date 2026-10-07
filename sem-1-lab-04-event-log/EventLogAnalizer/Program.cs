using System.Text.RegularExpressions;

namespace EventLogAnalizer;

public struct LogEntry
{
    public DateTime Timestamp;
    public string Level;
    public string Category;
    public string Message;
}

public class Program
{
    public static LogEntry ParseLine(string line)
    {
        LogEntry entry;

        int index1 = line.IndexOf(' ') + 1;
        int index2 = line.IndexOf(' ', index1);

        string datetimeStr = line.Substring(0, index2);
        entry.Timestamp = DateTime.Parse(datetimeStr);

        index1 = line.IndexOf('[', index2 + 1) + 1;
        index2 = line.IndexOf(']', index1);
        entry.Level = line.Substring(index1, index2 - index1);

        index1 = line.IndexOf('[', index2 + 1) + 1;
        index2 = line.IndexOf(']', index1);
        entry.Category = line.Substring(index1, index2 - index1);

        index1 = line.IndexOf(' ', index2 + 1) + 1;
        entry.Message = line.Substring(index1);

        return entry;
    }

    public static LogEntry[] ParseLog(string[] lines)
    {
        LogEntry[] result = new LogEntry[lines.Length];
        for (int i = 0; i < lines.Length; i++)
            result[i] = ParseLine(lines[i]);
        return result;
    }

    public static List<LogEntry> FilterByDate(LogEntry[] entries, DateTime date)
    {
        List<LogEntry> result = [];
        foreach (LogEntry entry in entries)
        {
            if (entry.Timestamp.Date == date)
                result.Add(entry);
        }
        return result;
    }

    public static List<LogEntry> FilterByLevel(LogEntry[] entries, string level)
    {
        List<LogEntry> result = [];
        foreach (LogEntry entry in entries)
        {
            if (entry.Level == level)
                result.Add(entry);
        }
        return result;
    }

    public static List<LogEntry> FilterByCategory(LogEntry[] entries, string category)
    {
        List<LogEntry> result = [];
        foreach (LogEntry entry in entries)
        {
            if (entry.Category == category)
                result.Add(entry);
        }
        return result;
    }

    public static List<LogEntry> Search(LogEntry[] entries, string text)
    {
        List<LogEntry> result = [];
        foreach (LogEntry entry in entries)
        {
            if (entry.Message.Contains(text, StringComparison.OrdinalIgnoreCase))
                result.Add(entry);
        }
        return result;
    }

    public static uint CountByLevel(LogEntry[] entries, string level)
    {
        uint count = 0;
        foreach (LogEntry entry in entries)
        {
            if (entry.Level == level)
                count++;
        }
        return count;
    }

    public static string GetServerStatus(LogEntry[] entries)
    {
        if (CountByLevel(FilterByCategory(entries, "Server").ToArray(), "Fatal") > 0)
            return "КРИТИЧЕСКАЯ ОШИБКА: сервер остановлен";
        bool haveError = CountByLevel(entries, "Error") > 0;
        bool haveFatal = CountByLevel(entries, "Fatal") > 0;
        if (!haveError && !haveFatal)
            return "Сервер работает штатно";
        if (haveError)
            return "Есть ошибки: требуется проверка";
        return $"Есть ошибки: {haveError}\nЕсть критические ошибки: {haveFatal}";
    }

    public static void ExportFiltered(List<LogEntry> entries, string exportPath = "export_filtered.txt")
    {
        using (StreamWriter writer = new(exportPath))
        {
            foreach (LogEntry entry in entries)
                writer.WriteLine($"{entry.Timestamp} [{entry.Level}][{entry.Category}] {entry.Message}");
        }
    }

    public static void Main()
    {
        string[] lines = File.ReadAllLines("event_server.log");
        LogEntry[] entries = ParseLog(lines);
        ExportFiltered(FilterByLevel(entries, "Error"));
        Console.Write(GetServerStatus(entries));
    }
}
