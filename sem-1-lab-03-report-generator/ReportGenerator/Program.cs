using System.Text.RegularExpressions;

namespace ReportGenerator;

class Program
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

    public static string BuildReport(string[] logs)
    {
        string eventName = "Восстание Ледяного Пламени";
        string startPoint = $"Событие началось: {eventName}";
        string endPoint = $"Событие \"{eventName}\" закрыто";
        string winnerPattern = $"объявлены победителями события \"{eventName}\"";

        bool eventStarted = false;
        DateTime startDateTime = DateTime.MinValue;
        TimeSpan duration = TimeSpan.MinValue;
        string winner = "<undefined>";
        uint warningCount = 0;
        uint errorCount = 0;

        foreach (string line in logs)
        {
            (DateTime dateTime, string level, string type, string text) = GetLogParts(line);
            if (eventStarted)
            {
                if (text == endPoint)
                {
                    duration = dateTime - startDateTime;
                    break;
                }
                if (type == "Reward" && text.Contains(winnerPattern))
                    winner = text[..(text.IndexOf(winnerPattern) - 1)];
                if (level == "Warning") warningCount += 1;
                else if (level == "Error") errorCount += 1;
            }
            else
            {
                if (text == startPoint)
                {
                    eventStarted = true;
                    startDateTime = dateTime;
                }
            }
        }

        if (!eventStarted)
            throw new InvalidDataException("Не было встречено лога начала события");

        string scorePattern = @$"{winner} получили (\d+) очков события";
        uint winnerScore = 0;
        foreach (string line in logs)
        {
            (DateTime dateTime, string level, string type, string text) = GetLogParts(line);
            if (type == "Reward")
            {
                Match match = Regex.Match(text, scorePattern);
                if (match.Success)
                    winnerScore = uint.Parse(match.Groups[1].Value);
            }
        }

        string report = $"""
            # Итоги события: {eventName}

            Дата и время начала: {startDateTime}
            Продолжительность: {duration}
            Победитель: {winner}
            Очки победителя: {winnerScore}
            """;
        return report;
    }

    public static void Main()
    {
        string[] logs = File.ReadAllLines("event_server.log");
        Console.WriteLine(BuildReport(logs));
    }
}
