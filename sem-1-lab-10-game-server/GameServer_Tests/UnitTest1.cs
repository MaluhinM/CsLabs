using GameServer;

namespace GameServer_Tests;

public class GameServerTests
{
    [Test]
    public void FindByLogin_ReturnsExpectedPlayer()
    {
        var server = new GameServer.GameServer();
        server.AddPlayer(new Player("PixelMage", 12, 100, true));

        var found = server.FindByLogin("PixelMage");

        Assert.That(found, Is.Not.Null);
        Assert.That(found!.Level, Is.EqualTo(12));
    }

    [Test]
    public void ConnectPlayer_DuplicateLogin_Throws()
    {
        var server = new GameServer.GameServer();
        server.AddPlayer(new Player("A", 1, 100, true));
        Assert.Throws<InvalidOperationException>(() =>
            server.AddPlayer(new Player("A", 2, 100, true)));
    }

    [Test]
    public void TakeDamage_LethalDamage_HealthBecomesZero_NotNegative()
    {
        var p = new Player("A", 1, 50, true);
        p.TakeDamage(100);
        Assert.That(p.Health, Is.EqualTo(0));
    }

    [Test]
    public void TakeDamage_NormalDamage_ReducesHealth()
    {
        var p = new Player("A", 1, 50, true);
        p.TakeDamage(20);
        Assert.That(p.Health, Is.EqualTo(30));
    }

    [Test]
    public void RemovePlayer_AlsoRemovesFromRooms()
    {
        var server = new GameServer.GameServer();
        var p = new Player("A", 1, 100, true);
        server.AddPlayer(p);
        server.CreateRoom("room");
        server.AddPlayerToRoom("A", "room");

        server.RemovePlayer("A");

        Assert.That(server.GetRoomByName("room")!.Players, Is.Empty);
    }

    [Test]
    public void CorrectLogin_ReturnsNullForVeryDifferent()
    {
        var server = new GameServer.GameServer();
        server.AddPlayer(new Player("PixelMage", 12, 100, true));
        Assert.That(server.CorrectLogin("zzzzzzz"), Is.Null);
    }

    [Test]
    public void CorrectLogin_CorrectsTypo()
    {
        var server = new GameServer.GameServer();
        server.AddPlayer(new Player("PixelMage", 12, 100, true));
        Assert.That(server.CorrectLogin("PixelMge"), Is.EqualTo("PixelMage"));
    }
}