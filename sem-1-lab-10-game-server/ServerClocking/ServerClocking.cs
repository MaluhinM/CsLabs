using GameServer;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ServerClocking;

class BenchmarkServer : GameServer.GameServer
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public Player? SearchPlayerInList(string login)
    {
        foreach (Player player in PlayersList)
        {
            if (player.Login == login)
                return player;
        }
        return null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Player? SearchPlayerInDict(string login)
    {
        return PlayersDict.GetValueOrDefault(login);
    }
}

class ClockingTest
{
    public static void Main()
    {
        BenchmarkServer benchmarkServer = new();

        string[] lines = File.ReadAllLines("players.csv");
        for (int i = 0; i < lines.Length; i++)
        {
            string[] elements = lines[i].Split('|');
            if (elements.Length != 4 || benchmarkServer.IsPlayerExists(elements[0]))
                throw new InvalidDataException("Входной файл не соответствует шаблону");
            benchmarkServer.AddPlayer(new Player(elements[0], int.Parse(elements[1]), int.Parse(elements[2]), bool.Parse(elements[3])));
        }
        string searchLogin = lines[0].Split('|')[0];
        //Player? searchPlayer = serverClocking.GetPlayerByLogin(searchLogin) ?? throw new InvalidDataException();

        for (var i = 0; i < 1000; i++)
        {
            benchmarkServer.SearchPlayerInList(searchLogin);
        }
        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < 100000; i++)
        {
            benchmarkServer.SearchPlayerInList(searchLogin);
            //if (serverClocking.SearchPlayerInList(searchLogin) != searchPlayer)
            //    throw new ArithmeticException();
        }
        stopwatch.Stop();
        Console.WriteLine($"Время поиска игрока {searchLogin} в списке: {stopwatch.Elapsed.TotalMilliseconds} мс");

        for (var i = 0; i < 1000; i++)
        {
            benchmarkServer.SearchPlayerInDict(searchLogin);
        }
        stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < 100000; i++)
        {
            benchmarkServer.SearchPlayerInDict(searchLogin);
            //if (serverClocking.SearchPlayerInDict(searchLogin) != searchPlayer)
            //    throw new ArithmeticException();
        }
        stopwatch.Stop();
        Console.WriteLine($"Время поиска игрока {searchLogin} в словаре: {stopwatch.Elapsed.TotalMilliseconds} мс");
    }
}