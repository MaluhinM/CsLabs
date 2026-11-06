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

public class Map
{
    public readonly HashSet<char> StopObjects = ['#', 'W'];
    public readonly HashSet<char> WalkObjects = ['.'];
    public Player Player;
    private uint _width;
    private uint _height;
    private Cell[,] _grid;

    private Map(uint width, uint height, Cell[,] grid)
    {
        _width = width;
        _height = height;
        _grid = grid;
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
                    player.Position = new Position(i, j);
                    cell = new Cell('.');
                }
                grid[i, j] = cell;
            }
        }

        return new(width, height, grid);
    }

    public Snapshot CreateSnapshot()
    {
        return new Snapshot(Player, _grid);
    }

    public void Restore(Snapshot snapshot)
    {
        Player.Position = snapshot.PlayerPosition;
        _grid = snapshot.Grid;
    }

    public void MovePlayer(Position position)
    {
        Player.Position = position;
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
                        map._grid = new Cell[height, width];
                        map.Player = new Player(new Position(X: 0, Y: 0));
                        for (int i = 0; i < height; i++)
                        {
                            for (int j = 0; j < width; j++)
                            {
                                map._grid[i, j] = new('.');
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
                        map._grid[xCell, yCell] = new(typeCell);
                        break;
                    }
                case 3:
                    {
                        // TODO: Continue working
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
