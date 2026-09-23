using System.Text.Json;

namespace GameServerMonitoring;

class Program
{
    // Для красивой записи JSON с отступами
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static void Main()
    {
        // Получаем случайные данные о сервере и красиво выводим их
        var (playersCount, workload, ping, PlayersInfo) = GetRandomInfo();
        PrintServerInfo(playersCount, workload, ping, PlayersInfo);
    }

    // Генератор случайных данных о сервере
    static (uint playersCount, string workload, uint ping, Dictionary<uint, object> PlayersInfo) GetRandomInfo()
    {
        Random rand = new();

        // Так как переменная будет использоваться в цикле for, то используем int, чтобы придерживаться установленным нормам
        int playersCount = rand.Next(0, 10_000);
        // Используем свитч для компактного выбора статуса нагрузки на сервер
        string workload = playersCount switch
        {
            0 => "Taking a break",
            < 1_000 => "Light load",
            < 3_000 => "Moderate load",
            < 5_000 => "Significant load",
            <= 10_000 => "Tremendous load",
            _ => "Undefined"
        };

        uint ping = (uint)rand.Next(1, 1_000);

        // Объявляем множество ID игроков, чтобы исключить повторения
        HashSet<uint> PlayersID = [];
        // Создаём словарь для записи информации об игроках на сервере по их ID
        Dictionary<uint, object> PlayersInfo = [];

        // Пробегаемся по всем игрокам на сервере и генерируем информацию о них
        for (int i = 0; i < playersCount; ++i)
        {
            uint id;

            /*  Получаем случайный ID.
                Если такой уже есть - получаем новый.
                Иначе добавляем его во множество и цикл завершается.  */
            do
            {
                id = (uint)rand.Next(1_000_000, 10_000_000);
            } while (!PlayersID.Add(id));

            string login = $"User{id}";
            bool isOnline = rand.Next(0, 2) == 1;
            /*  Определяем время игрока в оффлайне.
                Если он в сети - ставим ноль (беззнаковый).
                Иначе "придумываем" даунтайм.  */
            uint downTime = isOnline ? 0u : (uint)rand.Next(1, 31536000);
            string levelName = $"Level{rand.Next(0, 100)}";

            // Получаем случайные координаты x и y
            double x = rand.Next(-10000, 10001) * rand.NextDouble();
            double y = rand.Next(-1000, 1001) * rand.NextDouble();
            // Формируем объект с координатами
            var coordinates = new { X = x, Y = y };

            // Добавляем данные каждого пользователя в словарик
            PlayersInfo.Add(id, new
            {
                Login = login,
                OnlineStatus = isOnline,
                Downtime = downTime,
                LevelName = levelName,
                Coordinates = coordinates
            });
        }

        // Возвращаем все данные в кортеже (количество игроков беззнаковое)
        return ((uint)playersCount, workload, ping, PlayersInfo);
    }

    // Функция вывода данных о сервере
    static void PrintServerInfo(uint players, string workload, uint ping, Dictionary<uint, object> PlayersInfo)
    {
        // Создаём массив из полей
        var fields = new (string Label, string Value)[]
        {
            ("Players", players.ToString()),
            ("Workload", workload),
            ("Ping", ping.ToString()),
        };

        int contentWidth = 0;
        int labelWidth = 0;
        // Пробегаемся по полям для определения ширины будущей таблицы
        foreach (var (label, value) in fields)
        {
            if (value.Length > contentWidth) contentWidth = value.Length;
            if (label.Length > labelWidth) labelWidth = label.Length;
        }
        // Небольшой сдвиг из-за пробелов
        labelWidth += 2;
        int totalWidth = labelWidth + contentWidth + 3;

        string horizontalLine = new('─', totalWidth);

        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Server status");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"╭{horizontalLine}╮");

        // Пробегаемся по полям и выводим таблицу
        foreach (var (label, value) in fields)
        {
            Console.WriteLine($"│ {label.PadRight(labelWidth)} {value.PadRight(contentWidth)} │");
        }

        Console.WriteLine($"╰{horizontalLine}╯");

        // Получаем строку JSON с отступами
        string jsonString = JsonSerializer.Serialize(PlayersInfo, JsonOptions);
        // Записываем её в файл
        File.WriteAllText("serverinfo.json", jsonString);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("More info in 'serverinfo.json' file");
        Console.ResetColor();
    }
}