using MapEditor;

namespace MapEditor_Tests;

[TestFixture]
public class MapTests
{
    [Test]
    public void CreateSnapshot_And_Restore_RevertsGridChanges()
    {
        // Arrange
        var map = Map.CreateEmpty(3, 3);
        var snapshot = map.CreateSnapshot();

        // Act (меняем клетку на стену)
        map.Grid[1, 1] = new Cell('#');

        // Assert (до восстановления стена есть)
        Assert.That(map.Grid[1, 1].Type, Is.EqualTo('#'));

        // Act (восстанавливаем)
        map.Restore(snapshot);

        // Assert (стена исчезла, вернулась земля)
        Assert.That(map.Grid[1, 1].Type, Is.EqualTo('.'));
    }

    [Test]
    public void MovePlayer_ChangesPosition()
    {
        var map = Map.CreateEmpty(3, 3);
        var newPos = new Position(1, 1);

        map.MovePlayerTo(newPos);

        // Игрок - структура, поэтому проверяем значение
        Assert.That(map.Player.Position.X, Is.EqualTo(1));
        Assert.That(map.Player.Position.Y, Is.EqualTo(1));
    }

    [Test]
    public void MovePlayer_IntoWall_ThrowsBlockedException()
    {
        var map = Map.CreateEmpty(3, 3);
        map.Grid[1, 1] = new Cell('#'); // Ставим стену

        // Проверяем, что бросается правильное исключение
        Assert.Throws<Exceptions.BlockedException>(() => map.MovePlayerTo(new Position(1, 1)));
    }

    [Test]
    public void MovePlayer_OutOfBounds_ThrowsOutOfBoundsException()
    {
        var map = Map.CreateEmpty(3, 3);

        Assert.Throws<Exceptions.OutOfBoundsException>(() => map.MovePlayerTo(new Position(-1, 0)));
        Assert.Throws<Exceptions.OutOfBoundsException>(() => map.MovePlayerTo(new Position(0, 3)));
    }

    [Test]
    public void Snapshot_IsIndependentFromCurrentMap()
    {
        var map = Map.CreateEmpty(3, 3);
        var snapshot = map.CreateSnapshot();

        // Меняем оригинальную карту
        map.Grid[0, 0] = new Cell('#');

        // Снимок не должен измениться (проверка, что Cell - структура, а Clone работает)
        Assert.That(snapshot.Grid[0, 0].Type, Is.EqualTo('.'));
    }
}