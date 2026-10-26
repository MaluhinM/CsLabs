namespace DirectoryTree;

public class Program
{
    // Current line in file
    private static int _lineIndex;
    // Был убран глобальный параметр _depth для глубины: он будет меняться локально внутри функции BuildTree через параметр

    // File content
    private static string[] _lines = null!;

    private static void Main()
    {
        _lines = File.ReadAllLines("tree.txt");

        List<string> tree = TreeBuilder(_lines);

        foreach (string line in tree)
            Console.WriteLine(line);

        // Исправлено завершение работы программы: теперь выводится, когда можно нажать Enter для выхода
        Console.Write("Нажмите ВВОД для продолжения: ");
        Console.ReadLine();
    }

    public static List<string> TreeBuilder(string[] lines)
    {
        if (lines.Length == 0)
            return [];

        _lines = lines;
        _lineIndex = 1;

        (bool isLast, int childCount) rootInfo = IsLastChild(0, 0);
        string suffix = $" ({rootInfo.childCount})";
        List<string> tree = [lines[0] + suffix];
        tree.AddRange(BuildTree(0));

        return tree;
    }

    // Функция PrintTree была переделана в BuildTree, которая использует рекурсию
    private static List<string> BuildTree(int currentDepth)
    {
        const string BeginPrefix = "BEGIN ";

        List<string> result = [];

        while (_lineIndex < _lines.Length)
        {
            string line = _lines[_lineIndex].Trim();
            // Переделана логика счёта индекса: индекс увеличивается на 1 сразу после получения строки
            _lineIndex++;

            if (line == "")
            {
                // Здесь раньше было увеличение индекса на 2
                continue;
            }

            else if (line == "END")
            {
                // Здесь раньше было увеличение индекса на 1
                return result;
            }

            else if (line.StartsWith(BeginPrefix))
            {
                // Мы получаем подстроку теперь не с 5 индекса, а с 6, чтобы исключить пробел
                var name = line.Substring(BeginPrefix.Length);

                // Определяем, является ли дочерний элемент последним, и на основе результата выбираем префикс для вывода
                (bool isLast, int childrenCount) = IsLastChild(_lineIndex, currentDepth);
                string prefix = isLast ? "└─" : "├─";
                string suffix = $" ({childrenCount})";

                result.Add(GetIndent(currentDepth) + prefix + name + suffix);
                result.AddRange(BuildTree(currentDepth + 1));
            }
        }
        return result;
    }

    // Добавлен блок кода для того, чтобы понять, является ли дочерний элемент последним
    private static (bool isLast, int childCount) IsLastChild(int lineIndex, int currentDepth)
    {
        int subsequentDepth = currentDepth;  // Определяем уровень вложенности, который будем рассчитывать
        bool isLast = true;
        int childrenCount = 0;
        // Пробегаемся по индексам строк и ищем строку, где определяется ещё один дочерний элемент на том же самом уровне
        for (int i = lineIndex; i < _lines.Length; i++)
        {
            // Получаем строку по индексу
            string subsequentLine = _lines[i].Trim();
            // Если содержимое элемента закончилось, то уменьшаем глубину вложенности
            if (subsequentLine == "END")
            {
                subsequentDepth--;
                continue;
            }
            // Если начинается новый элемент, то смотрим, стоит ли он на том же самом уровне
            if (subsequentLine.StartsWith("BEGIN "))
            {
                // Если уровни совпадают, значит это нужный дочерний элемент
                if (subsequentDepth == currentDepth)
                    childrenCount++;
                // Увеличиваем глубину вложенности
                subsequentDepth++;
                // Если уровни совпадают, значит наш элемент не является последним
                if (subsequentDepth == currentDepth)
                    isLast = false;
            }
        }
        return (isLast, childrenCount);
    }

    private static string GetIndent(int currentDepth)
    {
        // Тяжёлый цикл был заменён на конкатенацию лёгкого Enumerable объекта
        return string.Concat(Enumerable.Repeat("│ ", currentDepth));
    }
}