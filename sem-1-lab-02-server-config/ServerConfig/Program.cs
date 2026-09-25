namespace ServerConfig;

public partial class Program
{
    public static
    (
        bool connected, bool initialized, bool running,
        bool updating, bool restoring, bool backupCreated,
        bool changesTested, uint ping, uint lastCleanupDays
    ) GetRandomInfo()
    {
        bool connected = Random.Shared.Next(101) <= 90;
        bool initialized = connected && Random.Shared.Next(101) <= 90;
        bool running = connected && initialized && Random.Shared.Next(101) <= 10;
        bool updating = connected && initialized && !running && Random.Shared.Next(101) <= 10;
        bool restoring = connected && initialized && !running && Random.Shared.Next(101) <= 10;
        bool backupCreated = connected && initialized && Random.Shared.Next(101) <= 90;
        bool changesTested = connected && initialized && Random.Shared.Next(101) <= 90;
        uint ping = connected && initialized ? (uint)Random.Shared.Next(1, 1000) : 0;
        uint lastCleanupDays = connected && initialized ? (uint)Random.Shared.Next(14) : 0;

        return (
            connected, initialized, running, updating, restoring,
            backupCreated, changesTested, ping, lastCleanupDays
        );
    }

    public static void PrintServerInfo
    (
        bool connected, bool initialized, bool running,
        bool updating, bool restoring, bool backupCreated,
        bool changesTested, uint ping, uint lastCleanupDays
    )
    {
        var fields = new (string Label, string Value)[]
        {
            ("Connected to the Server", connected.ToString()),
            ("Server initialized successfully", initialized.ToString()),
            ("Server is currently running", running.ToString()),
            ("Server is currently updating", updating.ToString()),
            ("Server is currently restoring", restoring.ToString()),
            ("Backup created on server side", backupCreated.ToString()),
            ("Changes tested by developer team", changesTested.ToString()),
            ("Server's average response time", $"{ping} ms"),
            ("Last cleanup was performed", $"{lastCleanupDays} days ago")
        };

        int contentWidth = 0;
        int labelWidth = 0;
        foreach (var (label, value) in fields)
        {
            if (value.Length > contentWidth) contentWidth = value.Length;
            if (label.Length > labelWidth) labelWidth = label.Length;
        }
        labelWidth += 2;
        int totalWidth = labelWidth + contentWidth + 3;

        string horizontalLine = new('─', totalWidth);

        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Server info");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"╭{horizontalLine}╮");

        // Пробегаемся по полям и выводим таблицу
        foreach (var (label, value) in fields)
        {
            Console.WriteLine($"│ {label.PadRight(labelWidth)} {value.PadRight(contentWidth)} │");
        }

        Console.WriteLine($"╰{horizontalLine}╯");
        Console.ResetColor();
    }

    public static (int state, List<string> FatalExceptions, List<string> Warnings) HandleServerInfo
    (
        bool connected, bool initialized, bool running,
        bool updating, bool restoring, bool backupCreated,
        bool changesTested, uint ping, uint lastCleanupDays
    )
    {
        int state;
        List<string> FatalExceptions = [];
        List<string> Warnings = [];

        if (!connected) FatalExceptions.Add("Unable to establish a connection to the server");
        else if (!initialized) FatalExceptions.Add("The server cannot be initialized");
        else
        {
            if (running) Warnings.Add("The server was already running. Are you trying to restart it?");
            if (updating) Warnings.Add("The server is currently being updated. You can start it, but only with the administrator's permission");
            if (restoring) Warnings.Add("The server is currently being restored. You can start it, but only with the administrator's permission");
            if (!backupCreated) Warnings.Add("Server backup was not performed – starting it is unsafe");
            if (!changesTested) Warnings.Add("The changes to the server have not been verified by the development team. Launching it could lead to unforeseen consequences");
            if (ping >= 300) Warnings.Add("The server has high latency. This is abnormal behavior");
            if (ping >= 800) FatalExceptions.Add("The server has very high latency. Launching is not permitted.");
            if (lastCleanupDays > 7) Warnings.Add("The server hasn't been cleaned up in a long time. This could cause unstable behavior");
        }
        if (FatalExceptions.Count > 0) state = -1;
        else if (Warnings.Count > 0) state = 1;
        else state = 0;

        if (state == -1)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"The server cannot be started for the following reasons ({FatalExceptions.Count}):");
            for (int i = 0; i < FatalExceptions.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {FatalExceptions[i]}");
            }
        }
        else if (state == 1)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"The server can be started, but only with administrator permission, for the following reasons ({Warnings.Count}):");
            for (int i = 0; i < Warnings.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {Warnings[i]}");
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"The server can be started");
        }
        Console.ResetColor();

        return (state, FatalExceptions, Warnings);
    }

    public static void Main()
    {
        (
            bool connected, bool initialized, bool running,
            bool updating, bool restoring, bool backupCreated,
            bool changesTested, uint ping, uint lastCleanupDays
        ) = GetRandomInfo();
        PrintServerInfo(
            connected, initialized, running, updating, restoring,
            backupCreated, changesTested, ping, lastCleanupDays
        );
        HandleServerInfo(
            connected, initialized, running, updating, restoring,
            backupCreated, changesTested, ping, lastCleanupDays
        );
    }
}
