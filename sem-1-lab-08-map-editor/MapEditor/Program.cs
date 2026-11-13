using System.Text.RegularExpressions;

namespace MapEditor;

public readonly record struct Position(int X, int Y);

public readonly record struct Cell(char Type);

public record struct Player(Position Position);

public record class Snapshot(DateTime DateTime, Player Player, Cell[,] Grid, int Width, int Height);

public class Map
{
    public readonly HashSet<char> StopObjects = ['#', '~', '^'];
    public readonly HashSet<char> WalkObjects = ['.'];
    public Player Player;
    public Cell[,] Grid;
    public int Width;
    public int Height;

    private Map(Player player, Cell[,] grid, int width, int height)
    {
        Player = player;
        Grid = grid;
        Width = width;
        Height = height;
    }

    public static Map CreateEmpty(int width, int height)
    {
        Cell[,] grid = new Cell[height, width];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                grid[i, j] = new Cell('.');
            }
        }
        Player player = new();
        return new(player, grid, width, height);
    }

    public static Map Load(string mapPath)
    {
        string[] lines = File.ReadAllLines(mapPath);
        string pattern = @"^(?<width>\d+)\|(?<height>\d+)$";
        Match match = Regex.Match(lines[0], pattern);
        if (!match.Success)
            throw new InvalidDataException("Первая строка файла должна содержать структуру вида 'width|height'");
        int width = int.Parse(match.Groups["width"].Value);
        int height = int.Parse(match.Groups["height"].Value);
        Player player = new();
        Cell[,] grid = new Cell[height, width];
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
                else if (cell.Type == 'W')
                {
                    cell = new Cell('~');
                }
                else if (cell.Type == 'L')
                {
                    cell = new Cell('^');
                }
                grid[i, j] = cell;
            }
        }

        return new(player, grid, width, height);
    }

    public Snapshot CreateSnapshot()
    {
        return new Snapshot(DateTime.Now, Player, (Cell[,])Grid.Clone(), Width, Height);
    }

    public void Restore(Snapshot snapshot)
    {
        Player.Position = snapshot.Player.Position;
        Grid = (Cell[,])snapshot.Grid.Clone();
        Width = snapshot.Width;
        Height = snapshot.Height;
    }

    public void MovePlayerTo(Position position)
    {
        if (position.X < 0 || position.X >= Width || position.Y < 0 || position.Y >= Height)
            throw new Exceptions.OutOfBoundsException();
        if (StopObjects.Contains(Grid[position.Y, position.X].Type))
            throw new Exceptions.BlockedException();
        Player.Position = position;
    }

    public void MovePlayer(Position vector)
    {
        MovePlayerTo(new Position(vector.X + Player.Position.X, vector.Y + Player.Position.Y));
    }

    public Cell[,] GetPlayerMap()
    {
        Cell[,] map = (Cell[,])Grid.Clone();
        map[Player.Position.Y, Player.Position.X] = new Cell('P');
        return map;
    }

    public void PrintMap()
    {
        Cell[,] grid = GetPlayerMap();
        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                Console.Write(grid[i, j].Type);
            }
            Console.WriteLine();
        }
    }
}

class Program
{
    public static void Main()
    {
        var map = Map.Load("map.txt");
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
            int? option = InputTools.GetNumber("Номер: ");
            switch (option)
            {
                case 1:
                    {
                        int? width = InputTools.GetNumber("Ширина: ");
                        if (width is null)
                            break;
                        if (width <= 0)
                        {
                            OutputTools.PrintError("Ширина должна быть больше нуля");
                            break;
                        }
                        int? height = InputTools.GetNumber("Высота: ");
                        if (height is null)
                            break;
                        if (height <= 0)
                        {
                            OutputTools.PrintError("Высота должна быть больше нуля");
                            break;
                        }
                        map = Map.CreateEmpty(width.Value, height.Value);
                        break;
                    }
                case 2:
                    {
                        int? xCell = InputTools.GetNumber("Координата X клетки: ");
                        if (xCell is null)
                            break;
                        if (xCell < 0)
                        {
                            OutputTools.PrintError("Координата не может быть меньше нуля");
                            break;
                        }
                        if (xCell >= map.Width)
                        {
                            OutputTools.PrintError($"Введённое значение выходит за ширину карты ({map.Width})");
                            break;
                        }
                        int? yCell = InputTools.GetNumber("Координата Y клетки: ");
                        if (yCell is null)
                            break;
                        if (yCell < 0)
                        {
                            OutputTools.PrintError("Координата не может быть меньше нуля");
                            break;
                        }
                        if (yCell >= map.Height)
                        {
                            OutputTools.PrintError($"Введённое значение выходит за высоту карты ({map.Height})");
                            break;
                        }
                        if (map.Player.Position.X == xCell && map.Player.Position.Y == yCell)
                        {
                            OutputTools.PrintError($"Нельзя изменить клетку, на которой стоит игрок");
                            break;
                        }
                        string typeInput = InputTools.GetString("Тип клетки: ");
                        if (string.IsNullOrEmpty(typeInput))
                        {
                            break;
                        }
                        char typeCell = typeInput[0];
                        if (!(map.StopObjects.Contains(typeCell) || map.WalkObjects.Contains(typeCell)))
                        {
                            OutputTools.PrintError("Введённый тип клетки не поддерживается");
                            break;
                        }
                        map.Grid[yCell.Value, xCell.Value] = new(typeCell);
                        break;
                    }
                case 3:
                    {
                        Console.WriteLine("Текущая карта:");
                        map.PrintMap();
                        break;
                    }
                case 4:
                    {
                        int? xCell = InputTools.GetNumber("Координата X клетки: ");
                        if (xCell is null)
                            break;
                        int? yCell = InputTools.GetNumber("Координата Y клетки: ");
                        if (yCell is null)
                            break;

                        try
                        {
                            map.MovePlayerTo(new Position(xCell.Value, yCell.Value));
                        }
                        catch (Exceptions.PlayerMovementException e)
                        {
                            OutputTools.PrintError(e.Message);
                        }
                        break;
                    }
                case 5:
                    {
                        while (true)
                        {
                            map.PrintMap();
                            string control = InputTools.GetString("Управление WASD: ");
                            Position vector = control.ToUpper() switch
                            {
                                "W" => new Position(0, -1),
                                "A" => new Position(-1, 0),
                                "S" => new Position(0, 1),
                                "D" => new Position(1, 0),
                                _ => new Position(0, 0)
                            };

                            if (vector.X == 0 && vector.Y == 0)
                                break;

                            int xCell = map.Player.Position.X + vector.X;
                            int yCell = map.Player.Position.Y + vector.Y;

                            try
                            {
                                map.MovePlayerTo(new Position(xCell, yCell));
                            }
                            catch (Exceptions.PlayerMovementException e)
                            {
                                OutputTools.PrintError(e.Message);
                            }
                        }
                        break;
                    }
                case 6:
                    {
                        Snapshot snapshot = map.CreateSnapshot();
                        snapshots.Add(snapshot);
                        Console.WriteLine($"Точка восстановления '{snapshot.DateTime}' была создана!");
                        break;
                    }
                case 7:
                    {
                        if (snapshots.Count == 0)
                        {
                            Console.WriteLine("Вы ещё не создавали точки сохранения");
                            break;
                        }
                        Console.WriteLine("\nТочки сохранения:");
                        for (int i = 0; i < snapshots.Count; i++)
                        {
                            Console.WriteLine($"{i + 1}. {snapshots[i].DateTime}");
                        }
                        int? choice = InputTools.GetNumber("Выбор: ");
                        if (choice is null)
                            break;
                        if (choice < 1 || choice > snapshots.Count)
                        {
                            OutputTools.PrintError($"Доступен выбор точек восстановления от 1 до {snapshots.Count} включительно");
                            break;
                        }
                        Snapshot toRestore = snapshots[choice.Value - 1];
                        map.Restore(toRestore);
                        Console.WriteLine($"Точка восстановления '{toRestore.DateTime}' успешно применена:");
                        map.PrintMap();
                        break;
                    }
                case 0:
                    Console.WriteLine("Пока!");
                    working = false;
                    break;
                case null:
                    break;
                default:
                    OutputTools.PrintError("Такой операции нет! Попробуйте ещё раз");
                    break;
            }
        }
    }
}
