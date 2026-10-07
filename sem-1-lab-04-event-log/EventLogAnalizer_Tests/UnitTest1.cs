using EventLogAnalizer;

namespace EventLogAnalizer_Tests;

public class ProgramTests
{
    [Test]
    public void EventServer_Test()
    {
        string[] lines = File.ReadAllLines("event_server.log");
        LogEntry[] entries = Program.ParseLog(lines);
        string result = Program.GetServerStatus(entries);

        Assert.That(result, Is.EqualTo("КРИТИЧЕСКАЯ ОШИБКА: сервер остановлен"));
    }
}
