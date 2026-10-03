namespace LogParser;

internal class Program
{
    public static (DateTime, string, string, string) GetLogParts(string line)
    {
        int index1 = line.IndexOf(' ') + 1;
        int index2 = line.IndexOf(' ', index1);

        string datetimeStr = line.Substring(0, index2);
        DateTime dateTime = DateTime.Parse(datetimeStr);

        index1 = line.IndexOf('[', index2 + 1) + 1;
        index2 = line.IndexOf(']', index1);

        string level = line.Substring(index1, index2 - index1);

        index1 = line.IndexOf('[', index2 + 1) + 1;
        index2 = line.IndexOf(']', index1);

        string type = line.Substring(index1, index2 - index1);

        index1 = line.IndexOf(' ', index2 + 1) + 1;

        string text = line.Substring(index1);

        return (dateTime, level, type, text);
    }

    public static void Main()
    {
        string[] lines = File.ReadAllLines("../../../event_server.log");
        string viewLevel = "Warning";  // Info, Warning or Error

        foreach (string line in lines)
        {
            (DateTime dateTime, string level, string type, string text) = GetLogParts(line);
            if (level != viewLevel) continue;
            Console.WriteLine($"{dateTime} [{level}][{type}] {text}");
        }
    }
}