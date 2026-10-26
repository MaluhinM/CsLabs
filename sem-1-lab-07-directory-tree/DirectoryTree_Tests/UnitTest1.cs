using DirectoryTree;

namespace DirectoryTree_Tests;

[TestFixture]
public class BuildTreeTests
{
    // 1. Проверка базового примера из задания (покрывает баг с пустыми строками, 
    // определение последнего элемента и подсчет детей).
    [Test]
    public void TreeBuilder_ExampleFromTask_ReturnsCorrectTreeWithChildCounts()
    {
        string[] input = {
            "A", "",
            "BEGIN B", "BEGIN C", "END", "",
            "BEGIN D", "END", "END", "",
            "BEGIN E", "END"
        };

        string[] expected = {
            "A (2)",
            "├─B (2)",
            "│ ├─C (0)",
            "│ └─D (0)",
            "└─E (0)"
        };

        var result = Program.TreeBuilder(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    // 2. Проверка глубокой вложенности (покрывает баг с рекурсией и правильностью отступов GetIndent).
    [Test]
    public void TreeBuilder_DeepNesting_CalculatesIndentsCorrectly()
    {
        string[] input = {
            "Root",
            "BEGIN A",
            "BEGIN B",
            "BEGIN C",
            "END",
            "END",
            "END"
        };

        string[] expected = {
            "Root (1)",
            "└─A (1)",
            "│ └─B (1)",
            "│ │ └─C (0)"
        };

        var result = Program.TreeBuilder(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    // 3. Проверка нескольких элементов на одном уровне (покрывает баг с _lineIndex += 2, 
    // когда программа перепрыгивала через элементы).
    [Test]
    public void TreeBuilder_MultipleSiblings_HandlesIndexCorrectly()
    {
        string[] input = {
            "Root", "",
            "BEGIN A", "END", "",
            "BEGIN B", "END", "",
            "BEGIN C", "END"
        };

        string[] expected = {
            "Root (3)",
            "├─A (0)",
            "├─B (0)",
            "└─C (0)"
        };

        var result = Program.TreeBuilder(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    // 4. Проверка программы только с корневым элементом (без детей).
    [Test]
    public void TreeBuilder_OnlyRoot_ReturnsRootWithoutChildren()
    {
        string[] input = { "Root" };

        string[] expected = { "Root (0)" };

        var result = Program.TreeBuilder(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    // 5. Проверка вложенных элементов и братьев на одном уровне (проверка IsLastChild).
    [Test]
    public void TreeBuilder_MixedNesting_HandlesBranchesAndLeaves()
    {
        string[] input = {
            "Root",
            "BEGIN A",
            "BEGIN B",
            "END",
            "END",
            "BEGIN C",
            "END"
        };

        string[] expected = {
            "Root (2)",
            "├─A (1)",
            "│ └─B (0)",
            "└─C (0)"
        };

        var result = Program.TreeBuilder(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    // 6. Проверка на наличие множества пустых строк подряд.
    [Test]
    public void TreeBuilder_MultipleEmptyLines_DoesNotBreakParsing()
    {
        string[] input = {
            "Root", "", "", "",
            "BEGIN A", "", "",
            "END", "", ""
        };

        string[] expected = {
            "Root (1)",
            "└─A (0)"
        };

        var result = Program.TreeBuilder(input);

        Assert.That(result, Is.EqualTo(expected));
    }
}