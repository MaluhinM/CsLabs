namespace ServerConfig;

class Program
{
    static (bool running, bool restoring) GetRandomInfo()
    {
        bool running = Random.Shared.Next(2) == 0;
        bool restoring = Random.Shared.Next(2) == 0;

        return (running, restoring);
    }

    public static void Main()
    {
        (bool running, bool restoring) = GetRandomInfo();
        Console.WriteLine(running);
        Console.WriteLine(restoring);
    }
}
