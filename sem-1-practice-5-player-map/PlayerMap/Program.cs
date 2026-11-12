namespace PlayerMap;

public record struct Position(int X, int Y);
public record struct Player(Position Position)
{
    public Position GetPosition()
    {
        return Position;
    }

    public int GetX()
    {
        return Position.X;
    }

    public int GetY()
    {
        return Position.Y;
    }
}

public record struct Monster(Position Position)
{
    public Position GetPosition()
    {
        return Position;
    }

    public int GetX()
    {
        return Position.X;
    }

    public int GetY()
    {
        return Position.Y;
    }
}

public class PlayerMap
{
    private readonly int _width;
    private readonly int _height;
    private char[,] _map;
    public Player Player;

    private PlayerMap(int width, int height, char[,] map, Player player)
    {
        _width = width;
        _height = height;
        _map = map;
        Player = player;
    }

    public static PlayerMap Load(string mapPath)
    {
        string[] lines = File.ReadAllLines(mapPath);
        char[,] map = new char[lines.Length, lines[0].Length];
        Player player = new();
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            for (int j = 0; j < line.Length; j++)
            {
                char cell = line[j];
                if (cell == '@')
                {
                    player.Position = new Position(j, i);
                    cell = ' ';
                }
                map[i, j] = cell;
            }
        }
        return new(lines[0].Length, lines.Length, map, player);
    }

    public void ShowMap()
    {
        Console.Clear();
        for (int i = 0; i < _height; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                char cell = (Player.GetX() == j && Player.GetY() == i) ? '@' : _map[i, j];
                Console.Write($"{cell} ");
            }
            Console.WriteLine();
        }
    }

    public void Move(Position vector, out bool defeated)
    {
        int newX = Player.GetX() + vector.X;
        int newY = Player.GetY() + vector.Y;
        defeated = _map[newY, newX] == '&';
        if (newX < 0 || newY < 0 || newX == _width || newY == _height || _map[newY, newX] == '#' || defeated)
        {
            return;
        }
        Player.Position = new Position(newX, newY);
    }

    public static void Main()
    {
        var playerMap = PlayerMap.Load("map.txt");
        bool defeated = false;
        while (true)
        {
            playerMap.ShowMap();
            string input = InputTools.GetString("Управление: ");
            if (input == "")
            {
                Console.WriteLine("Пока!");
                break;
            }
            if (string.Equals(input, "W", StringComparison.OrdinalIgnoreCase))
            {
                playerMap.Move(new Position(0, -1), out defeated);
            }
            if (string.Equals(input, "S", StringComparison.OrdinalIgnoreCase))
            {
                playerMap.Move(new Position(0, 1), out defeated);
            }
            if (string.Equals(input, "A", StringComparison.OrdinalIgnoreCase))
            {
                playerMap.Move(new Position(-1, 0), out defeated);
            }
            if (string.Equals(input, "D", StringComparison.OrdinalIgnoreCase))
            {
                playerMap.Move(new Position(1, 0), out defeated);
            }

            if (defeated)
            {
                OutputTools.PrintError("Вы проиграли!");
                break;
            }
        }
    }
}
