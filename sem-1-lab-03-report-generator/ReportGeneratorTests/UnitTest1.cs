using ReportGenerator;

namespace ReportGeneratorTests;

public class ProgramTests
{
    [Test]
    public void EventServer_Test()
    {
        string[] logs = File.ReadAllLines("event_server.log");
        string expecting = """
            # Итоги события: Восстание Ледяного Пламени

            Дата и время начала: 01.09.2026 12:18:25
            Продолжительность: 00:07:05.5140000
            Победитель: Ночные совы
            Очки победителя: 2500
            Ивентовый предмет: Сердце Эмберфанга
            Утешительная награда Железные волки: 800 очков
            Предупреждений во время события: 6
            Ошибок во время события: 1

            Игрок ВоронФокс стал лидером по количеству побед: 14 шт. врагов
            """;

        string result = Program.BuildReport(logs);

        Assert.That(result, Is.EqualTo(expecting));
    }
}
