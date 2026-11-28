using Fastenshtein;

namespace GameServer;

public class Room
{
    public string Name { get; }
    private readonly List<Player> _players = [];
    public IReadOnlyList<Player> Players => _players;

    public Room(string name)
    {
        Name = name;
    }

    internal void AddPlayer(Player player)
    {
        _players.Add(player);
    }

    internal void RemovePlayer(Player player)
    {
        _players.Remove(player);
    }
}

public class Player
{
    private readonly string _login;
    private int _level;
    private int _health;
    private bool _isOnline;

    public string Login => _login;
    public int Level => _level;
    public int Health => _health;
    public bool IsOnline => _isOnline;

    public Player(string login, int level, int health, bool isOnline)
    {
        _login = login;
        _level = level;
        _health = health;
        _isOnline = isOnline;
    }

    public void SetLevel(int level)
    {
        if (level < 0)
            throw new ArgumentOutOfRangeException("Уровень не может быть отрицательным");
        _level = level;
    }

    public void SetHealth(int health)
    {
        if (health < 0)
            throw new ArgumentOutOfRangeException("Здоровье не может быть отрицательным");
        _health = health;
    }

    public void SetOnlineStatus(bool status)
    {
        _isOnline = status;
    }

    public void IncreaseLevel(int points)
    {
        if (points < 0)
            throw new ArgumentOutOfRangeException("Очки уровня не могут быть отрицательными");
        _level += points;
    }

    public void DecreaseLevel(int points)
    {
        if (points < 0)
            throw new ArgumentOutOfRangeException("Очки уровня не могут быть отрицательными");
        if (points > _level)
            throw new InvalidOperationException("Уровень не может быть отрицательным");
        _level -= points;
    }

    public void TakeDamage(int damage)
    {
        if (damage < 0)
            throw new ArgumentOutOfRangeException("Количество урона не может быть отрицательным");
        if (damage > _health)
            _health = 0;
        else
            _health -= damage;
    }

    public void Heal(int health)
    {
        if (health < 0)
            throw new ArgumentOutOfRangeException("Количество здоровья не может быть отрицательным");
        _health += health;
    }

    public void Connect()
    {
        if (_isOnline)
            throw new InvalidOperationException("Игрок уже подключён");
        _isOnline = true;
    }

    public void Disconnect()
    {
        if (!_isOnline)
            throw new InvalidOperationException("Игрок уже отключён");
        _isOnline = false;
    }

    public void SwitchOnlineStatus()
    {
        if (_isOnline)
            Disconnect();
        else
            Connect();
    }
}

public class GameServer
{
    private readonly List<Player> _playersList;
    private readonly Dictionary<string, Player> _playersDict;
    private readonly List<Room> _rooms;

    public IReadOnlyList<Player> PlayersList => _playersList;
    public IReadOnlyDictionary<string, Player> PlayersDict => _playersDict;
    public IReadOnlyList<Room> Rooms => _rooms;

    public GameServer(List<Player>? playersList = null, Dictionary<string, Player>? playersDict = null, List<Room>? rooms = null)
    {
        _playersList = playersList ?? [];
        _playersDict = playersDict ?? [];
        _rooms = rooms ?? [];
    }

    public static GameServer Load(string filename)
    {
        string[] lines = File.ReadAllLines(filename);
        List<Player> playersList = [];
        Dictionary<string, Player> playersDict = [];
        for (int i = 0; i < lines.Length; i++)
        {
            string[] elements = lines[i].Split('|');
            if (elements.Length != 4 || playersDict.ContainsKey(elements[0]) ||
                !int.TryParse(elements[1], out int level) || !int.TryParse(elements[2], out int health) || !bool.TryParse(elements[3], out bool isOnline))
                throw new InvalidDataException("Входной файл не соответствует шаблону");
            Player player = new(elements[0], level, health, isOnline);
            playersList.Add(player);
            playersDict[elements[0]] = player;
        }

        return new(playersList, playersDict, []);
    }

    private static string GetOnlineStatus(Player player)
    {
        return player.IsOnline ? "Online" : "Offline";
    }

    public void AddPlayer(Player player)
    {
        if (IsPlayerExists(player.Login))
        {
            throw new InvalidOperationException("Игрок с таким логином уже существует");
        }
        else
        {
            _playersList.Add(player);
            _playersDict[player.Login] = player;
        }
    }

    public void RemovePlayer(string login)
    {
        if (_playersDict.Remove(login, out Player? player))
        {
            _playersList.Remove(player);
            foreach (Room room in _rooms)
                room.RemovePlayer(player);
        }
        else
        {
            throw new InvalidOperationException("Игрока с таким логином не существует");
        }
    }

    public void ConnectPlayer(string login)
    {
        Player? player = GetPlayerByLogin(login) ?? throw new InvalidOperationException("Игрока с таким логином не существует");
        player.Connect();
    }

    public void DisconnectPlayer(string login)
    {
        Player? player = GetPlayerByLogin(login) ?? throw new InvalidOperationException("Игрока с таким логином не существует");
        player.Disconnect();
    }

    public string GetPlayersList()
    {
        if (_playersList.Count == 0)
        {
            return "Нет игроков на сервере";
        }
        string result = "Игроки на сервере:";

        for (int i = 0; i < _playersList.Count; i++)
        {
            result += $"\n{i + 1}. {_playersList[i].Login} • {_playersList[i].Level} lvl • {_playersList[i].Health} HP [{GetOnlineStatus(_playersList[i])}]";
        }

        return result;
    }

    public bool IsPlayerExists(string login)
    {
        return !string.IsNullOrWhiteSpace(login) && _playersDict.ContainsKey(login);
    }

    public Player? GetPlayerByLogin(string login)
    {
        if (string.IsNullOrWhiteSpace(login) || !_playersDict.TryGetValue(login, out Player? player))
        {
            return null;
        }
        return player;
    }

    public string? CorrectLogin(string login, int maxDistance = 3)
    {
        int distance = maxDistance + 1;
        string? result = null;
        foreach (string playerLogin in _playersDict.Keys)
        {
            int currentDistance = Levenshtein.Distance(login, playerLogin);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                result = playerLogin;
            }
        }
        return result;
    }

    public Player? FindByLogin(string login)
    {
        string? correctedLogin = CorrectLogin(login);
        if (correctedLogin is null)
            return null;
        return GetPlayerByLogin(correctedLogin);
    }

    public void EditPlayer(string login, int? level, int? health, bool? isOnline)
    {
        Player? player = GetPlayerByLogin(login) ?? throw new InvalidOperationException("Игрока с таким логином не существует");
        player.SetLevel(level ?? player.Level);
        player.SetHealth(health ?? player.Health);
        player.SetOnlineStatus(isOnline ?? player.IsOnline);
    }

    public string GetPlayerInfoString(Player player)
    {
        string[] keys =
        [
            "Login:", "Level:", "Health:", "Status:"
        ];
        string[] values =
        [
            player.Login, player.Level.ToString(), player.Health.ToString(), GetOnlineStatus(player)
        ];
        int maxKeyLength = keys.Max(s => s.Length);
        int maxValueLength = values.Max(s => s.Length);

        List<string> enteredRoomNames = [];
        foreach (Room room in _rooms)
        {
            if (room.Players.Contains(player))
                enteredRoomNames.Add(room.Name);
        }
        int maxListWidth = enteredRoomNames.Count == 0 ? 0 : enteredRoomNames.Max(s => s.Length) + enteredRoomNames.Count.ToString().Length + 2;

        int sumWidth = maxKeyLength + maxValueLength + 1;
        int minWidth = Math.Max(maxListWidth, 16);
        int totalWidth = Math.Max(sumWidth, minWidth) + 2;
        if (sumWidth < minWidth)
        {
            maxValueLength = minWidth - maxKeyLength - 1;
            sumWidth = minWidth;
        }
        string horizontalLine = new('─', totalWidth);
        string masterFormat = "│ {0} │\n";
        string format1 = string.Format(masterFormat, "{0,-" + maxKeyLength + "} {1,-" + maxValueLength + "}");
        string format2 = string.Format(masterFormat, "{0,-" + sumWidth + "}");
        string subsectionTitle = enteredRoomNames.Count == 0 ? "No entered rooms" : "Entered Rooms";
        string subsectionLeftPadding = new(' ', (totalWidth - subsectionTitle.Length - 1) / 2);
        string subsectionRightPadding = (totalWidth - subsectionTitle.Length - 1) % 2 == 0 ? new(' ', (totalWidth - subsectionTitle.Length - 1) / 2 - 1) : subsectionLeftPadding;

        string result = $"╭{horizontalLine}╮\n";

        for (int i = 0; i < keys.Length; i++)
        {
            result += string.Format(format1, keys[i], values[i]);
        }

        result += $"├{horizontalLine}┤\n";
        result += string.Format(masterFormat, $"{subsectionLeftPadding}{subsectionTitle}{subsectionRightPadding}");

        int counter = 1;
        foreach (string room in enteredRoomNames)
        {
            result += string.Format(format2, $"{counter}. {room}");
            counter++;
        }

        result += $"╰{horizontalLine}╯";

        return result;
    }

    public bool IsRoomExists(string name)
    {
        foreach (Room room in _rooms)
            if (room.Name == name)
                return true;
        return false;
    }

    public Room? GetRoomByName(string name)
    {
        foreach (Room room in _rooms)
            if (room.Name == name)
                return room;
        return null;
    }

    public bool CreateRoom(string name)
    {
        if (IsRoomExists(name))
            return false;
        _rooms.Add(new Room(name));
        return true;
    }

    public bool RemoveRoom(string name)
    {
        Room? room = GetRoomByName(name);
        if (room is null)
            return false;
        _rooms.Remove(room);
        return true;
    }

    public void AddPlayerToRoom(string login, string roomName)
    {
        Player player = GetPlayerByLogin(login)
        ?? throw new InvalidOperationException("Игрока с таким логином не существует");
        Room room = GetRoomByName(roomName)
        ?? throw new InvalidOperationException("Такой комнаты не существует");
        if (room.Players.Contains(player))
            throw new InvalidOperationException("Игрок уже в комнате");
        room.AddPlayer(player);
    }

    public void RemovePlayerFromRoom(string login, string roomName)
    {
        Player player = GetPlayerByLogin(login)
        ?? throw new InvalidOperationException("Игрока с таким логином не существует");
        Room room = GetRoomByName(roomName)
        ?? throw new InvalidOperationException("Такой комнаты не существует");
        if (!room.Players.Contains(player))
            throw new InvalidOperationException("Игрок не состоит в этой комнате");
        room.RemovePlayer(player);
    }
}

class Program
{
    public static void Main()
    {
        GameServer server = GameServer.Load("players.csv");

        Console.OutputEncoding = System.Text.Encoding.UTF8;
        bool working = true;
        while (working)
        {
            Console.WriteLine("""

                1  - Подключить/добавить игрока
                2  - Отключить игрока
                3  - Удалить игрока
                4  - Показать всех игроков
                5  - Найти игрока
                6  - Изменить характеристики игрока
                7  - Показать информацию об игроке
                8  - Добавить игрока в комнату
                9  - Удалить игрока из комнаты
                10 - Создать комнату
                11 - Удалить комнату
                12 - Показать все комнаты
                13 - Показать участников комнаты
                0  - Выход
                """);

            int? option = InputTools.GetNumber("Выбор: ");
            switch (option)
            {
                case 1:
                    {
                        string login = InputTools.GetString("Логин: ");
                        if (string.IsNullOrWhiteSpace(login))
                            break;
                        try
                        {
                            if (server.IsPlayerExists(login))
                            {
                                server.ConnectPlayer(login);
                                OutputTools.PrintSuccess($"Игрок {login} успешно подключён");
                            }
                            else
                            {
                                int? level = InputTools.GetNumber("Уровень: ");
                                if (level is null)
                                    break;
                                int? health = InputTools.GetNumber("Здоровье: ");
                                if (health is null)
                                    break;
                                server.AddPlayer(new Player(login, level.Value, health.Value, true));
                                OutputTools.PrintSuccess($"Игрок {login} успешно добавлен");
                            }
                        }
                        catch (InvalidOperationException e)
                        {
                            OutputTools.PrintError(e.Message);
                        }
                        break;
                    }
                case 2:
                    {
                        string login = InputTools.GetString("Логин: ");
                        if (string.IsNullOrWhiteSpace(login))
                            break;
                        try
                        {
                            string? correctedLogin = server.CorrectLogin(login);
                            if (correctedLogin is null)
                            {
                                OutputTools.PrintError("Не удалось найти игрока с таким логином");
                                break;
                            }
                            server.DisconnectPlayer(correctedLogin);
                            OutputTools.PrintSuccess($"Игрок {correctedLogin} успешно отключён");
                        }
                        catch (InvalidOperationException e)
                        {
                            OutputTools.PrintError(e.Message);
                        }
                        break;
                    }
                case 3:
                    {
                        string login = InputTools.GetString("Логин: ");
                        if (string.IsNullOrWhiteSpace(login))
                            break;
                        try
                        {
                            server.RemovePlayer(login);
                            OutputTools.PrintSuccess($"Игрок {login} успешно удалён");
                        }
                        catch (InvalidOperationException e)
                        {
                            OutputTools.PrintError(e.Message);
                        }
                        break;
                    }
                case 4:
                    {
                        Console.WriteLine(server.GetPlayersList());
                        break;
                    }
                case 5:
                    {
                        string login = InputTools.GetString("Логин: ");
                        if (string.IsNullOrWhiteSpace(login))
                            break;
                        string? correctedLogin = server.CorrectLogin(login);
                        if (correctedLogin is null)
                        {
                            OutputTools.PrintError("Не удалось найти игрока с таким логином");
                            break;
                        }
                        OutputTools.PrintSuccess($"Игрок найден: {correctedLogin}");
                        break;
                    }
                case 6:
                    {
                        string login = InputTools.GetString("Логин: ");
                        if (string.IsNullOrWhiteSpace(login))
                            break;
                        try
                        {
                            string? correctedLogin = server.CorrectLogin(login);
                            if (correctedLogin is null)
                            {
                                OutputTools.PrintError("Не удалось найти игрока с таким логином");
                                break;
                            }
                            int? level = InputTools.GetNumber("Уровень (Enter - не менять): ");
                            int? health = InputTools.GetNumber("Здоровье (Enter - не менять): ");
                            string statusInput = InputTools.GetString("Статус (Enter - не менять): ");
                            bool? isOnline;
                            if (bool.TryParse(statusInput, out bool isOnlineValue))
                                isOnline = isOnlineValue;
                            else
                                isOnline = null;
                            server.EditPlayer(correctedLogin, level, health, isOnline);
                            OutputTools.PrintSuccess($"Характеристики игрока {correctedLogin} успешно изменены");
                        }
                        catch (InvalidOperationException e)
                        {
                            OutputTools.PrintError(e.Message);
                        }
                        break;
                    }
                case 7:
                    {
                        string login = InputTools.GetString("Логин: ");
                        if (string.IsNullOrWhiteSpace(login))
                            break;
                        Player? player = server.FindByLogin(login);
                        if (player is null)
                        {
                            OutputTools.PrintError("Не удалось найти игрока с таким логином");
                            break;
                        }
                        OutputTools.PrintSuccess($"Характеристики игрока {player.Login}:");
                        OutputTools.PrintSuccess(server.GetPlayerInfoString(player));
                        break;
                    }
                case 8:
                    {
                        string login = InputTools.GetString("Логин: ");
                        if (string.IsNullOrWhiteSpace(login))
                            break;
                        string roomName = InputTools.GetString("Название комнаты для добавления: ").Trim();
                        if (string.IsNullOrWhiteSpace(roomName))
                            break;
                        try
                        {
                            server.AddPlayerToRoom(login, roomName);
                            OutputTools.PrintSuccess($"Игрок {login} добавлен в комнату {roomName}");
                        }
                        catch (InvalidOperationException e)
                        {
                            OutputTools.PrintError(e.Message);
                        }
                        break;
                    }
                case 9:
                    {
                        string login = InputTools.GetString("Логин: ");
                        if (string.IsNullOrWhiteSpace(login))
                            break;
                        string roomName = InputTools.GetString("Название комнаты для удаления: ").Trim();
                        if (string.IsNullOrWhiteSpace(roomName))
                            break;
                        try
                        {
                            server.RemovePlayerFromRoom(login, roomName);
                            OutputTools.PrintSuccess($"Игрок {login} удалён из комнаты {roomName}");
                        }
                        catch (InvalidOperationException e)
                        {
                            OutputTools.PrintError(e.Message);
                        }
                        break;
                    }
                case 10:
                    {
                        string roomName = InputTools.GetString("Название комнаты: ").Trim();
                        if (string.IsNullOrWhiteSpace(roomName))
                            break;
                        if (server.CreateRoom(roomName))
                        {
                            OutputTools.PrintSuccess($"Комната {roomName} успешно создана");
                        }
                        else
                        {
                            OutputTools.PrintError("Такая комната уже существует");
                        }
                        break;
                    }
                case 11:
                    {
                        string roomName = InputTools.GetString("Название комнаты: ").Trim();
                        if (string.IsNullOrWhiteSpace(roomName))
                            break;
                        if (server.RemoveRoom(roomName))
                        {
                            OutputTools.PrintSuccess($"Комната {roomName} успешно удалена");
                        }
                        else
                        {
                            OutputTools.PrintError("Такой комнаты не существует");
                        }
                        break;
                    }
                case 12:
                    {
                        if (server.Rooms.Count == 0)
                        {
                            OutputTools.PrintError("Никакие комнаты ещё не были созданы");
                            break;
                        }
                        Console.WriteLine("Все существующие комнаты:");
                        for (int i = 0; i < server.Rooms.Count; i++)
                        {
                            Console.WriteLine($"{i + 1}. {server.Rooms[i].Name} ({server.Rooms[i].Players.Count})");
                        }
                        break;
                    }
                case 13:
                    {
                        string roomName = InputTools.GetString("Название комнаты: ").Trim();
                        if (string.IsNullOrWhiteSpace(roomName))
                            break;
                        Room? room = server.GetRoomByName(roomName);
                        if (room is null)
                        {
                            OutputTools.PrintError("Такой комнаты не существует");
                            break;
                        }
                        if (room.Players.Count == 0)
                        {
                            OutputTools.PrintError("В комнате пока нет участников");
                            break;
                        }
                        Console.WriteLine("Участники комнаты:");
                        for (int i = 0; i < room.Players.Count; i++)
                        {
                            Console.WriteLine($"{i + 1}. {room.Players[i].Login}");
                        }
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
