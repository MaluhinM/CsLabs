using System.Text.RegularExpressions;

namespace ReportGenerator;

public class Program
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

        string winnerPattern = @$"(?<winner>[\w\s]+) объявлены победителями события ""{eventName}""";
        string awardPattern = @$"(?<awardRecipient>[\w\s]+) получили утешительную награду: (?<awardScore>\d+) очков события";
        string victoryWinnerPattern = @$"Игрок (?<victoryWinner>[\w\s]+) победил (?<victoryNumber>\d+) враг[а,ов]";

        bool eventStarted = false;
        DateTime startDateTime = DateTime.MinValue;
        TimeSpan duration = TimeSpan.MinValue;
        string winner = "<undefined>";
        string awardRecipient = "<undefined>";
        uint awardScore = 0;
        uint warningCount = 0;
        uint errorCount = 0;
        string victoryWinner = "<undefined>";
        uint victoryNumber = 0;

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
                if (type == "Reward")
                {
                    Match match = Regex.Match(text, winnerPattern);
                    if (match.Success)
                        winner = match.Groups["winner"].Value;
                    else
                    {
                        match = Regex.Match(text, awardPattern);
                        if (match.Success)
                        {
                            awardRecipient = match.Groups["awardRecipient"].Value;
                            awardScore = uint.Parse(match.Groups["awardScore"].Value);
                        }
                    }
                }
                else if (type == "Statistics")
                {
                    Match match = Regex.Match(text, victoryWinnerPattern);
                    if (match.Success)
                    {
                        uint number = uint.Parse(match.Groups["victoryNumber"].Value);
                        if (number > victoryNumber)
                        {
                            victoryWinner = match.Groups["victoryWinner"].Value;
                            victoryNumber = number;
                        }
                    }
                }
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

        string winnerScorePattern = @$"{winner} получили (?<winnerScore>\d+) очков события";
        string eventItemPattern = @$"{winner} получили ивентовый предмет: (?<eventItem>[\w\s]+)";

        uint winnerScore = 0;
        string eventItem = "<undefined>";
        foreach (string line in logs)
        {
            (DateTime dateTime, string level, string type, string text) = GetLogParts(line);
            if (type == "Reward")
            {
                Match match = Regex.Match(text, winnerScorePattern);
                if (match.Success)
                    winnerScore = uint.Parse(match.Groups["winnerScore"].Value);
            }
            else if (type == "Loot")
            {
                Match match = Regex.Match(text, eventItemPattern);
                if (match.Success)
                    eventItem = match.Groups["eventItem"].Value;
            }
        }

        string report = $"""
            # Итоги события: {eventName}

            Дата и время начала: {startDateTime}
            Продолжительность: {duration}
            Победитель: {winner}
            Очки победителя: {winnerScore}
            Ивентовый предмет: {eventItem}
            Утешительная награда {awardRecipient}: {awardScore} очков
            Предупреждений во время события: {warningCount}
            Ошибок во время события: {errorCount}

            Игрок {victoryWinner} стал лидером по количеству побед: {victoryNumber} шт. врагов
            """;
        return report;
    }

    public static void Main()
    {
        string[] logs = File.ReadAllLines("event_server.log");
        Console.WriteLine(BuildReport(logs));
    }
}
