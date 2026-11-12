/*
===========================
 Программа ещё не доделана
     Работа в процессе
===========================
*/

using System.Text.RegularExpressions;

namespace MapEditor;

public readonly record struct Position(int X, int Y);

public readonly record struct Cell(char Type);

public record struct Player(Position Position);

public record class Snapshot(Player Player, Cell[,] Grid)
{
    public Position PlayerPosition { get; set; } = Player.Position;
}

class OutOfBoundsException : Exception
{
    public OutOfBoundsException() : base("Игрок не может оказаться в стене или воде") { }

    public OutOfBoundsException(string message) : base(message) { }

    public OutOfBoundsException(string message, Exception innerException) : base(message, innerException) { }
}

public class Map
{
    public readonly HashSet<char> StopObjects = ['#', 'W'];
    public readonly HashSet<char> WalkObjects = ['.'];
    public Player Player;
    public Cell[,] Grid;
    private uint _width;
    private uint _height;

    private Map(Player player, Cell[,] grid, uint width, uint height)
    {
        Player = player;
        Grid = grid;
        _width = width;
        _height = height;
    }

    public static Map Load(string mapPath)
    {
        string[] lines = File.ReadAllLines(mapPath);
        string pattern = @"^(?<width>\d+)\|(?<height>\d+)$";
        Match match = Regex.Match(lines[0], pattern);
        if (!match.Success)
            throw new InvalidDataException("Первая строка файла должна содержать структуру вида 'width|height'");
        uint width = uint.Parse(match.Groups["width"].Value);
        uint height = uint.Parse(match.Groups["height"].Value);
        Cell[,] grid = new Cell[height, width];
        Player player = new();
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Cell cell = new(lines[i + 1][j]);
                if (cell.Type == 'P')
                {
                    player.Position = new Position(j, i);
                    cell = new Cell('.');
                }
                grid[i, j] = cell;
            }
        }

        return new(player, grid, width, height);
    }

    public Snapshot CreateSnapshot()
    {
        return new Snapshot(Player, Grid);
    }

    public void Restore(Snapshot snapshot)
    {
        Player.Position = snapshot.PlayerPosition;
        Grid = snapshot.Grid;
    }

    public void MovePlayerTo(Position position)
    {
        if (StopObjects.Contains(Grid[position.Y, position.X].Type))
            throw new OutOfBoundsException();
        Player.Position = position;
    }

    public void MovePlayer(Position vector)
    {
        MovePlayerTo(new Position(vector.X + Player.Position.X, vector.Y + Player.Position.Y));
    }

    public Cell[,] GetMap()
    {
        Cell[,] map = (Cell[,])Grid.Clone();
        map[Player.Position.Y, Player.Position.X] = new Cell('P');
        return map;
    }

    public static void Main()
    {
        var map = Load("map.txt");
        List<Snapshot> snapshots = [];

        bool working = true;
        while (working)
        {
            Console.WriteLine(
                """

                Выберите номер операции:
                    1. Создать новую карту заданного размера.
                    2. Изменить тип отдельной клетки.
                    3. Показать карту.
                    4. Разместить игрока на карте.
                    5. Переместить игрока по карте.
                    6. Сохранить текущее состояние карты.
                    7. Восстановить ранее сохранённое состояние.
                    0. Выйти.
                """
            );
            int option = InputTools.GetNumber("Номер: ");
            switch (option)
            {
                case 1:
                    {
                        int width = InputTools.GetNumber("Ширина: ");
                        if (width <= 0)
                        {
                            OutputTools.PrintError("Ширина должна быть больше нуля");
                            break;
                        }
                        int height = InputTools.GetNumber("Высота: ");
                        if (height <= 0)
                        {
                            OutputTools.PrintError("Высота должна быть больше нуля");
                            break;
                        }
                        map._width = (uint)width;
                        map._height = (uint)height;
                        map.Grid = new Cell[height, width];
                        map.Player = new Player(new Position(X: 0, Y: 0));
                        for (int i = 0; i < height; i++)
                        {
                            for (int j = 0; j < width; j++)
                            {
                                map.Grid[i, j] = new('.');
                            }
                        }
                        break;
                    }
                case 2:
                    {
                        int xCell = InputTools.GetNumber("Координата X клетки: ");
                        if (xCell < 0)
                        {
                            OutputTools.PrintError("Координата не может быть меньше нуля");
                            break;
                        }
                        if (xCell >= map._width)
                        {
                            OutputTools.PrintError($"Введённое значение выходит за ширину карты ({map._width})");
                            break;
                        }
                        int yCell = InputTools.GetNumber("Координата Y клетки: ");
                        if (yCell < 0)
                        {
                            OutputTools.PrintError("Координата не может быть меньше нуля");
                            break;
                        }
                        if (yCell >= map._height)
                        {
                            OutputTools.PrintError($"Введённое значение выходит за высоту карты ({map._height})");
                            break;
                        }
                        char typeCell = InputTools.GetString("Тип клетки: ").ToCharArray()[0];
                        if (!(map.StopObjects.Contains(typeCell) || map.WalkObjects.Contains(typeCell)))
                        {
                            OutputTools.PrintError("Введённый тип клетки не поддерживается");
                            break;
                        }
                        map.Grid[xCell, yCell] = new(typeCell);
                        break;
                    }
                case 3:
                    {
                        Console.WriteLine("Текущая карта:");
                        Cell[,] grid = map.GetMap();
                        for (int i = 0; i < map._height; i++)
                        {
                            for (int j = 0; j < map._width; j++)
                            {
                                Console.Write($"{grid[i, j].Type} ");
                            }
                            Console.WriteLine();
                        }
                        break;
                    }
                case 4:
                    {
                        int xCell = InputTools.GetNumber("Координата X клетки: ");
                        if (xCell < 0)
                        {
                            OutputTools.PrintError("Координата не может быть меньше нуля");
                            break;
                        }
                        if (xCell >= map._width)
                        {
                            OutputTools.PrintError($"Введённое значение выходит за ширину карты ({map._width})");
                            break;
                        }
                        int yCell = InputTools.GetNumber("Координата Y клетки: ");
                        if (yCell < 0)
                        {
                            OutputTools.PrintError("Координата не может быть меньше нуля");
                            break;
                        }
                        if (yCell >= map._height)
                        {
                            OutputTools.PrintError($"Введённое значение выходит за высоту карты ({map._height})");
                            break;
                        }

                        try
                        {
                            map.MovePlayerTo(new Position(xCell, yCell));
                        }
                        catch (OutOfBoundsException e)
                        {
                            OutputTools.PrintError(e.Message);
                        }
                        break;
                    }
                case 5:
                    {
                        string control = InputTools.GetString("Управление WASD: ");
                        Position vector = control switch
                        {
                            "W" => new Position(0, -1),
                            "A" => new Position(-1, 0),
                            "S" => new Position(0, 1),
                            "D" => new Position(1, 0),
                            _ => new Position(0, 0)
                        };

                        int xCell = map.Player.Position.X + vector.X;
                        if (xCell < 0)
                        {
                            OutputTools.PrintError("Координата не может быть меньше нуля");
                            break;
                        }
                        if (xCell >= map._width)
                        {
                            OutputTools.PrintError($"Введённое значение выходит за ширину карты ({map._width})");
                            break;
                        }
                        int yCell = map.Player.Position.Y + vector.Y;
                        if (yCell < 0)
                        {
                            OutputTools.PrintError("Координата не может быть меньше нуля");
                            break;
                        }
                        if (yCell >= map._height)
                        {
                            OutputTools.PrintError($"Введённое значение выходит за высоту карты ({map._height})");
                            break;
                        }

                        try
                        {
                            map.MovePlayerTo(new Position(xCell, yCell));
                        }
                        catch (OutOfBoundsException e)
                        {
                            OutputTools.PrintError(e.Message);
                        }
                        break;
                    }
                case 0:
                    Console.WriteLine("Пока!");
                    working = false;
                    break;
                default:
                    OutputTools.PrintError("Такой операции нет! Попробуйте ещё раз");
                    break;
            }
        }
    }
}
