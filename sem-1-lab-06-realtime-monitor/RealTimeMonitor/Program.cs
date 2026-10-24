using DataGenerator;

namespace RealTimeMonitor;

public static class Program
{
    // volatile гарантирует, что каждое чтение/запись идёт напрямую в память, без кеширования в регистре
    private static volatile int _scaleStep = 1;
    private const int GraphLinesCount = 13;
    private const int ValuesCount = 300;     // 300 измерений для того, чтобы выводить график в масштабе 5 минут
    private const int MaxColumnsCount = 10;  // 20 колонок для каждого графика - слишком много для корректного отображения
    private const int OutputLinesCount = 2;
    private static readonly List<double[]> _stack = [];

    public static void Main()
    {
        Console.Clear();
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // Создаём объект, который будет получать данные о состоянии сервера.
        ServerMonitor monitor = new();

        // Сообщаем библиотеке, какой метод нужно вызывать
        // каждый раз, когда появляются новые данные.
        monitor.DataReceived += OnDataReceived;

        // Запускаем получение данных.
        // После этого библиотека сама будет передавать новые значения примерно раз в секунду.
        monitor.Start();

        Console.WriteLine("Мониторинг запущен.");
        Console.WriteLine("Для завершения программы нажмите Enter.");
        Console.Write(new string('\n', OutputLinesCount));

        while (true)
        {
            // Не даём программе завершиться сразу после запуска.
            string? input = Console.ReadLine();
            input = input is null ? "" : input;

            // При желании можно остановить получение данных.
            if (input == "")
            {
                monitor.Stop();
                break;
            }
            // Если пользователь ввёл число, тогда пытаемся установить его как масштаб графика
            // Пользователь может установить любой целый масштаб от 1 до 300 включительно (по умолчанию 1)
            else if (int.TryParse(input, out int scale))
            {
                if (scale >= 1 && scale <= ValuesCount)
                    _scaleStep = scale;
                else
                    Console.WriteLine($"Масштаб может быть от 1 до {ValuesCount} включительно");
            }
        }
        Console.WriteLine("\nМониторинг остановлен.");
    }

    public static string[] CreateGraph(List<double[]> stack, int scaleStep = 1)
    {
        // Чтобы не упасть, если в стеке меньше данных, чем нужно для одной точки масштаба
        if (stack.Count < scaleStep)
        {
            List<string> stub = new List<string>();
            stub.Add($"Сбор данных для графика с масштабом {scaleStep}...");

            // Заполняем остальные строки будущей таблицы пустыми пробелами, затирая старую
            for (int i = 1; i < GraphLinesCount; i++)
            {
                stub.Add("");
            }
            return stub.ToArray();
        }

        List<string> graph = [];
        int labelsCount = GraphLinesCount - 3;
        int[] labelPercents = new int[labelsCount];
        int columnsCount = Math.Min(stack.Count / scaleStep, Math.Min(MaxColumnsCount, ValuesCount / scaleStep));

        for (int k = 0; k < labelsCount; k++)
        {
            int percent = (labelsCount - k) * 100 / labelsCount;
            labelPercents[k] = percent;
            int digitsCount = percent.ToString().Length;
            graph.Add($"  {new string(' ', 3 - digitsCount)}{percent} │");
        }

        double[] temperatureMeans = new double[columnsCount];
        for (int i = 0; i < columnsCount; i++)
        {
            double temperatureSum = 0;
            for (int s = 0; s < scaleStep; s++)
                temperatureSum += stack[(i * scaleStep) + s][2];
            temperatureMeans[i] = temperatureSum / scaleStep;
        }

        double maxTemperature = temperatureMeans.Max();
        if (maxTemperature == 0) maxTemperature = 1;

        double[,] valuesClusters = new double[columnsCount, 3];
        for (int i = 0; i < columnsCount; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                double valueSum = 0;
                for (int s = 0; s < scaleStep; s++)
                    valueSum += stack[(i * scaleStep) + s][j];
                valuesClusters[i, j] = valueSum / scaleStep;
            }
            valuesClusters[i, 2] = temperatureMeans[i] / maxTemperature * 100;
        }

        for (int j = 0; j < 3; j++)
        {
            for (int k = 0; k < labelsCount; k++)
            {
                for (int i = 0; i < columnsCount; i++)
                {
                    if (valuesClusters[i, j] >= labelPercents[k])
                        graph[k] += " ███";
                    else
                        graph[k] += "    ";
                }
                graph[k] += " │";
            }
        }

        string solidLine = new('─', 4 * columnsCount + 1);

        string lastLine = " ";
        for (int i = 1; i <= columnsCount; i++)
        {
            int digitsCount = i.ToString().Length;
            lastLine += $" {i}{new string(' ', 3 - digitsCount)}";
        }

        graph.Insert(0, $"      ╭{solidLine}┬{solidLine}┬{solidLine}╮");
        graph.Add($"      ╰{solidLine}┴{solidLine}┴{solidLine}╯");
        graph.Add($"      {lastLine} {lastLine} {lastLine}");

        return graph.ToArray();
    }

    public static void OnDataReceived(double cpu, double ram, double temperature)
    {
        // Этот метод автоматически вызывается библиотекой
        // каждый раз, когда появляются новые данные.

        // cpu — текущая загрузка процессора (в процентах).
        // ram — использование оперативной памяти (в процентах).
        // temperature — температура процессора (в градусах Цельсия).

        double cpuRound = Math.Round(cpu);
        double ramRound = Math.Round(ram);
        double tempRound = Math.Round(temperature);

        _stack.Insert(0, [cpuRound, ramRound, tempRound]);
        if (_stack.Count > ValuesCount)
            _stack.RemoveAt(_stack.Count - 1);

        Console.SetCursorPosition(0, 2);

        string[] graphLines = CreateGraph(_stack, _scaleStep);
        int windowWidth = Math.Max(Console.WindowWidth - 1, 1);
        foreach (string line in graphLines)
            Console.WriteLine(line.PadRight(windowWidth));

        Console.WriteLine($"CPU: {cpuRound}%".PadRight(windowWidth));
        Console.WriteLine($"RAM: {ramRound}%".PadRight(windowWidth));
        Console.WriteLine($"Temperature: {tempRound} °C".PadRight(windowWidth));
    }
}
