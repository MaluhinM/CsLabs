using ServerConfig;

namespace ServerConfigTests;

public class ProgramTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test_ServerOff()
    {
        (int state, List<string> FatalExceptions, List<string> Warnings) result = Program.HandleServerInfo(
            false, false, false,
            false, false, false,
            false, 0, 0
        );

        var expectedResult = (
            -1,
            FatalExceptions: new List<string> { "Unable to establish a connection to the server" },
            Warnings: new List<string> { }
        );

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}
