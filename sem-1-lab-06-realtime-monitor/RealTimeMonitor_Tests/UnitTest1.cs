using RealTimeMonitor;

namespace RealTimeMonitor_Tests;

[TestFixture]
public class CreateGraphTests
{
    private static List<double[]> MakeStack(params (double cpu, double ram, double temp)[] values)
    {
        var result = new List<double[]>();
        foreach (var v in values)
            result.Add([v.cpu, v.ram, v.temp]);
        return result;
    }

    [Test]
    public void CreateGraph_ReturnsThirteenLines_WhenDataIsSufficient()
    {
        var stack = MakeStack((50, 30, 60), (40, 45, 65));

        string[] result = Program.CreateGraph(stack, scaleStep: 1);

        Assert.That(result.Length, Is.EqualTo(13));
    }

    [Test]
    public void CreateGraph_ReturnsStub_WhenStackSmallerThanScaleStep()
    {
        var stack = MakeStack((50, 30, 60));

        string[] result = Program.CreateGraph(stack, scaleStep: 5);

        Assert.Multiple(() =>
        {
            Assert.That(result.Length, Is.EqualTo(13));
            Assert.That(result[0], Does.Contain("Сбор данных"));
            Assert.That(result.Skip(1), Has.All.Empty);
        });
    }

    [Test]
    public void CreateGraph_ContainsFilledBlocks_WhenValuesAreHigh()
    {
        var stack = MakeStack((100, 100, 100), (100, 100, 100));

        string[] result = Program.CreateGraph(stack, scaleStep: 1);

        // Верхняя строка (метка 100) должна содержать блоки
        Assert.That(result[1], Does.Contain("█"));
    }

    [Test]
    public void CreateGraph_TopLineIsAlmostEmpty_WhenValuesAreLow()
    {
        var stack = MakeStack((1, 1, 2), (1, 1, 1));

        string[] result = Program.CreateGraph(stack, scaleStep: 1);

        // Верхняя строка (метка 100) должна содержать только 1 блок от графика температуры
        int blocksInTopLine = CountOccurrences(result[1], "███");
        Assert.That(blocksInTopLine, Is.EqualTo(1));
    }

    [Test]
    public void CreateGraph_AggregatesValues_WhenScaleStepGreaterThanOne()
    {
        // Два измерения по 100 → среднее 100, при scaleStep=2 одна колонка
        var stack = MakeStack((100, 100, 100), (100, 100, 100));

        string[] result = Program.CreateGraph(stack, scaleStep: 2);

        // Колонка одна, значит в строке ровно три блока " ███" (по одному на график)
        int blocksInTopLine = CountOccurrences(result[1], "███");
        Assert.That(blocksInTopLine, Is.EqualTo(3));
    }

    [Test]
    public void CreateGraph_ZeroTemperature_DoesNotCrash()
    {
        var stack = MakeStack((0, 0, 0), (0, 0, 0));

        string[] result = Program.CreateGraph(stack, scaleStep: 1);

        Assert.That(result.Length, Is.EqualTo(13));
    }

    private static int CountOccurrences(string text, string substring)
    {
        int count = 0, index = 0;
        while ((index = text.IndexOf(substring, index)) != -1)
        {
            count++;
            index += substring.Length;
        }
        return count;
    }
}