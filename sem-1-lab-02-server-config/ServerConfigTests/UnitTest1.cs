using ServerConfig;

namespace ServerConfigTests;

public class ProgramTests
{
    private static (int state, List<string> FatalExceptions, List<string> Warnings) Call(
        bool connected = true,
        bool initialized = true,
        bool running = false,
        bool updating = false,
        bool restoring = false,
        bool backupCreated = true,
        bool changesTested = true,
        uint ping = 10,
        uint lastCleanupDays = 1)
    {
        return Program.HandleServerInfo(
            connected,
            initialized,
            running,
            updating,
            restoring,
            backupCreated,
            changesTested,
            ping,
            lastCleanupDays);
    }

    [Test]
    public void Test_ServerOff_Fatal()
    {
        var (state, FatalExceptions, Warnings) = Call(connected: false);

        Assert.Multiple(() =>
        {
            Assert.That(state, Is.EqualTo(-1));
            Assert.That(FatalExceptions, Is.EqualTo(new[]
            {
                "Unable to establish a connection to the server"
            }));
            Assert.That(Warnings, Is.Empty);
        });
    }

    [Test]
    public void Test_ServerNotInit_Fatal()
    {
        var (state, FatalExceptions, Warnings) = Call(initialized: false);

        Assert.Multiple(() =>
        {
            Assert.That(state, Is.EqualTo(-1));
            Assert.That(FatalExceptions, Is.EqualTo(new[]
            {
                "The server cannot be initialized"
            }));
            Assert.That(Warnings, Is.Empty);
        });
    }

    [Test]
    public void Test_ServerCanStart_NoWarnings()
    {
        var result = Call();

        Assert.Multiple(() =>
        {
            Assert.That(result.state, Is.EqualTo(0));
            Assert.That(result.FatalExceptions, Is.Empty);
            Assert.That(result.Warnings, Is.Empty);
        });
    }

    [Test]
    public void Test_Running_Warning()
    {
        var result = Call(running: true);

        Assert.Multiple(() =>
        {
            Assert.That(result.state, Is.EqualTo(1));
            Assert.That(result.FatalExceptions, Is.Empty);
            Assert.That(result.Warnings, Is.EqualTo(new[]
            {
                "The server was already running. Are you trying to restart it?"
            }));
        });
    }

    [Test]
    public void Test_Updating_Warning()
    {
        var result = Call(updating: true);

        Assert.Multiple(() =>
        {
            Assert.That(result.state, Is.EqualTo(1));
            Assert.That(result.FatalExceptions, Is.Empty);
            Assert.That(result.Warnings, Is.EqualTo(new[]
            {
                "The server is currently being updated. You can start it, but only with the administrator's permission"
            }));
        });
    }

    [Test]
    public void Test_Restoring_Warning()
    {
        var result = Call(restoring: true);

        Assert.Multiple(() =>
        {
            Assert.That(result.state, Is.EqualTo(1));
            Assert.That(result.FatalExceptions, Is.Empty);
            Assert.That(result.Warnings, Is.EqualTo(new[]
            {
                "The server is currently being restored. You can start it, but only with the administrator's permission"
            }));
        });
    }

    [Test]
    public void Test_BackupNotCreated_Warning()
    {
        var result = Call(backupCreated: false);

        Assert.Multiple(() =>
        {
            Assert.That(result.state, Is.EqualTo(1));
            Assert.That(result.FatalExceptions, Is.Empty);
            Assert.That(result.Warnings, Is.EqualTo(new[]
            {
                "Server backup was not performed – starting it is unsafe"
            }));
        });
    }

    [Test]
    public void Test_ChangesNotTested_Warning()
    {
        var result = Call(changesTested: false);

        Assert.Multiple(() =>
        {
            Assert.That(result.state, Is.EqualTo(1));
            Assert.That(result.FatalExceptions, Is.Empty);
            Assert.That(result.Warnings, Is.EqualTo(new[]
            {
                "The changes to the server have not been verified by the development team. Launching it could lead to unforeseen consequences"
            }));
        });
    }

    [Test]
    public void Test_Ping299_NoLatencyWarning()
    {
        var result = Call(ping: 299);

        Assert.Multiple(() =>
        {
            Assert.That(result.state, Is.EqualTo(0));
            Assert.That(result.FatalExceptions, Is.Empty);
            Assert.That(result.Warnings, Is.Empty);
        });
    }

    [Test]
    public void Test_Ping300_HighLatencyWarning()
    {
        var result = Call(ping: 300);

        Assert.Multiple(() =>
        {
            Assert.That(result.state, Is.EqualTo(1));
            Assert.That(result.FatalExceptions, Is.Empty);
            Assert.That(result.Warnings, Is.EqualTo(new[]
            {
                "The server has high latency. This is abnormal behavior"
            }));
        });
    }

    [Test]
    public void Test_Ping799_HighLatencyWarningOnly()
    {
        var result = Call(ping: 799);

        Assert.Multiple(() =>
        {
            Assert.That(result.state, Is.EqualTo(1));
            Assert.That(result.FatalExceptions, Is.Empty);
            Assert.That(result.Warnings, Is.EqualTo(new[]
            {
                "The server has high latency. This is abnormal behavior"
            }));
        });
    }

    [Test]
    public void Test_Ping800_VeryHighLatencyFatal()
    {
        var result = Call(ping: 800);

        Assert.Multiple(() =>
        {
            Assert.That(result.state, Is.EqualTo(-1));
            Assert.That(result.FatalExceptions, Is.EqualTo(new[]
            {
                "The server has very high latency. Launching is not permitted."
            }));
            Assert.That(result.Warnings, Is.EqualTo(new[]
            {
                "The server has high latency. This is abnormal behavior"
            }));
        });
    }

    [Test]
    public void Test_LastCleanupDays7_NoWarning()
    {
        var result = Call(lastCleanupDays: 7);

        Assert.Multiple(() =>
        {
            Assert.That(result.state, Is.EqualTo(0));
            Assert.That(result.FatalExceptions, Is.Empty);
            Assert.That(result.Warnings, Is.Empty);
        });
    }

    [Test]
    public void Test_LastCleanupDays8_Warning()
    {
        var result = Call(lastCleanupDays: 8);

        Assert.Multiple(() =>
        {
            Assert.That(result.state, Is.EqualTo(1));
            Assert.That(result.FatalExceptions, Is.Empty);
            Assert.That(result.Warnings, Is.EqualTo(new[]
            {
                "The server hasn't been cleaned up in a long time. This could cause unstable behavior"
            }));
        });
    }

    [Test]
    public void Test_MultipleWarnings_State1()
    {
        var result = Call(
            running: true,
            updating: true,
            restoring: true,
            backupCreated: false,
            changesTested: false,
            ping: 300,
            lastCleanupDays: 8);

        Assert.Multiple(() =>
        {
            Assert.That(result.state, Is.EqualTo(1));
            Assert.That(result.FatalExceptions, Is.Empty);
            Assert.That(result.Warnings, Is.EquivalentTo(new[]
            {
                "The server was already running. Are you trying to restart it?",
                "The server is currently being updated. You can start it, but only with the administrator's permission",
                "The server is currently being restored. You can start it, but only with the administrator's permission",
                "Server backup was not performed – starting it is unsafe",
                "The changes to the server have not been verified by the development team. Launching it could lead to unforeseen consequences",
                "The server has high latency. This is abnormal behavior",
                "The server hasn't been cleaned up in a long time. This could cause unstable behavior"
            }));
        });
    }

    [Test]
    public void Test_FatalTakesPrecedenceOverWarnings()
    {
        var result = Call(
            backupCreated: false,
            ping: 800);

        Assert.Multiple(() =>
        {
            Assert.That(result.state, Is.EqualTo(-1));
            Assert.That(result.FatalExceptions, Is.EqualTo(new[]
            {
                "The server has very high latency. Launching is not permitted."
            }));
            Assert.That(result.Warnings, Is.EquivalentTo(new[]
            {
                "Server backup was not performed – starting it is unsafe",
                "The server has high latency. This is abnormal behavior"
            }));
        });
    }

    [Test]
    public void Test_NotConnected_OnlyConnectionFatal()
    {
        var result = Call(
            connected: false,
            initialized: true,
            running: true,
            backupCreated: false,
            ping: 900,
            lastCleanupDays: 10);

        Assert.Multiple(() =>
        {
            Assert.That(result.state, Is.EqualTo(-1));
            Assert.That(result.FatalExceptions, Is.EqualTo(new[]
            {
                "Unable to establish a connection to the server"
            }));
            Assert.That(result.Warnings, Is.Empty);
        });
    }

    [Test]
    public void Test_NotInitialized_OnlyInitFatal()
    {
        var result = Call(
            connected: true,
            initialized: false,
            running: true,
            backupCreated: false,
            ping: 900,
            lastCleanupDays: 10);

        Assert.Multiple(() =>
        {
            Assert.That(result.state, Is.EqualTo(-1));
            Assert.That(result.FatalExceptions, Is.EqualTo(new[]
            {
                "The server cannot be initialized"
            }));
            Assert.That(result.Warnings, Is.Empty);
        });
    }
}
