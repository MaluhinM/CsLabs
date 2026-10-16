using SaveManager;

[TestFixture]
public class SaveManagerTests
{
    private string _testDirectory;
    private string _indexPath;
    private string _savesDirectory;
    private string _backupsDirectory;

    [SetUp]
    public void SetUp()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "SaveManagerTests-" + Guid.NewGuid());
        _indexPath = Path.Combine(_testDirectory, "index.json");
        _savesDirectory = Path.Combine(_testDirectory, "saves");
        _backupsDirectory = Path.Combine(_testDirectory, "backups");

        Directory.CreateDirectory(_savesDirectory);
        Directory.CreateDirectory(_backupsDirectory);

        File.WriteAllText(_indexPath,
            "[{\"name\":\"slot-01\",\"file\":\"save-01.json\",\"updatedAt\":\"2026-04-18T10:15:00\"}]");
        File.WriteAllText(Path.Combine(_savesDirectory, "save-01.json"),
            "{\"character\":\"Mira\",\"map\":\"Ashen Coast\",\"position\":{\"x\":128,\"y\":47},\"health\":76,\"inventory\":[\"iron sword\"]}");
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, true);
    }

    // ─────────────────────────────────────────────────────────────
    // LoadIndex
    // ─────────────────────────────────────────────────────────────

    [Test]
    public void LoadIndex_ReadsSaveEntry()
    {
        bool result = Program.LoadIndex(_indexPath, out List<SaveEntry> entries);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(entries, Has.Count.EqualTo(1));
            Assert.That(entries[0].Name, Is.EqualTo("slot-01"));
            Assert.That(entries[0].File, Is.EqualTo("save-01.json"));
            Assert.That(entries[0].UpdatedAt, Is.EqualTo(new DateTime(2026, 4, 18, 10, 15, 0)));
        });
    }

    [Test]
    public void LoadIndex_MissingFile_ReturnsFalseAndEmptyList()
    {
        string missingPath = Path.Combine(_testDirectory, "missing.json");

        bool result = Program.LoadIndex(missingPath, out List<SaveEntry> entries);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(entries, Is.Empty);
        });
    }

    [Test]
    public void LoadIndex_InvalidJson_ReturnsFalseAndEmptyList()
    {
        File.WriteAllText(_indexPath, "{ this is not valid json ");

        bool result = Program.LoadIndex(_indexPath, out List<SaveEntry> entries);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(entries, Is.Empty);
        });
    }

    [Test]
    public void LoadIndex_NullJson_ReturnsFalse()
    {
        File.WriteAllText(_indexPath, "null");

        bool result = Program.LoadIndex(_indexPath, out List<SaveEntry> entries);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(entries, Is.Empty);
        });
    }

    // ─────────────────────────────────────────────────────────────
    // LoadSave
    // ─────────────────────────────────────────────────────────────

    [Test]
    public void LoadSave_ReadsCharacterAndWorldData()
    {
        Program.LoadIndex(_indexPath, out List<SaveEntry> entries);
        SaveEntry entry = entries[0];

        bool result = Program.LoadSave(_savesDirectory, entry, out SaveData save);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(save.Character, Is.EqualTo("Mira"));
            Assert.That(save.Map, Is.EqualTo("Ashen Coast"));
            Assert.That(save.Position.Xpos, Is.EqualTo(128));
            Assert.That(save.Position.Ypos, Is.EqualTo(47));
            Assert.That(save.Health, Is.EqualTo(76));
            Assert.That(save.Inventory, Does.Contain("iron sword"));
        });
    }

    [Test]
    public void LoadSave_MissingFile_ReturnsFalse()
    {
        var entry = new SaveEntry { Name = "missing", File = "missing.json" };

        bool result = Program.LoadSave(_savesDirectory, entry, out _);

        Assert.That(result, Is.False);
    }

    [Test]
    public void LoadSave_InvalidJson_ReturnsFalse()
    {
        File.WriteAllText(Path.Combine(_savesDirectory, "broken.json"), "not a json");
        var entry = new SaveEntry { Name = "broken", File = "broken.json" };

        bool result = Program.LoadSave(_savesDirectory, entry, out _);

        Assert.That(result, Is.False);
    }

    // ─────────────────────────────────────────────────────────────
    // FindByName
    // ─────────────────────────────────────────────────────────────

    [Test]
    public void FindByName_IsCaseInsensitive()
    {
        Program.LoadIndex(_indexPath, out List<SaveEntry> entries);

        bool result = Program.FindByName(entries, "SLOT-01", out SaveEntry entry);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(entry.Name, Is.EqualTo("slot-01"));
        });
    }

    [Test]
    public void FindByName_ReturnsFalseForMissingEntry()
    {
        Program.LoadIndex(_indexPath, out List<SaveEntry> entries);

        bool result = Program.FindByName(entries, "missing", out _);

        Assert.That(result, Is.False);
    }

    [Test]
    public void FindByName_EmptyName_ReturnsFalse()
    {
        Program.LoadIndex(_indexPath, out List<SaveEntry> entries);

        bool result = Program.FindByName(entries, "", out _);

        Assert.That(result, Is.False);
    }

    // ─────────────────────────────────────────────────────────────
    // SaveGame
    // ─────────────────────────────────────────────────────────────

    [Test]
    public void SaveGame_WritesEditedDataThatCanBeLoadedAgain()
    {
        Program.LoadIndex(_indexPath, out List<SaveEntry> entries);
        SaveEntry entry = entries[0];
        Program.LoadSave(_savesDirectory, entry, out SaveData save);
        save.Map = "Crystal Cavern";
        save.Position.Xpos = 10;
        save.Position.Ypos = 25;
        save.Health = 50;
        save.Inventory.Add("crystal key");

        Program.SaveGame(_savesDirectory, entry, save);
        Program.LoadSave(_savesDirectory, entry, out SaveData loaded);

        Assert.Multiple(() =>
        {
            Assert.That(loaded.Map, Is.EqualTo("Crystal Cavern"));
            Assert.That(loaded.Position.Xpos, Is.EqualTo(10));
            Assert.That(loaded.Position.Ypos, Is.EqualTo(25));
            Assert.That(loaded.Health, Is.EqualTo(50));
            Assert.That(loaded.Inventory, Does.Contain("crystal key"));
        });
    }

    [Test]
    public void SaveGame_RoundTripPreservesAllFields()
    {
        var entry = new SaveEntry { Name = "test", File = "roundtrip.json" };
        var original = new SaveData
        {
            Character = "Hero",
            Map = "Crystal Cavern",
            Position = new Position { Xpos = -42, Ypos = 100 },
            Health = 42,
            Inventory = ["sword", "shield", "potion"]
        };

        Program.SaveGame(_savesDirectory, entry, original);
        Program.LoadSave(_savesDirectory, entry, out SaveData loaded);

        Assert.Multiple(() =>
        {
            Assert.That(loaded.Character, Is.EqualTo("Hero"));
            Assert.That(loaded.Map, Is.EqualTo("Crystal Cavern"));
            Assert.That(loaded.Position.Xpos, Is.EqualTo(-42));
            Assert.That(loaded.Position.Ypos, Is.EqualTo(100));
            Assert.That(loaded.Health, Is.EqualTo(42));
            Assert.That(loaded.Inventory, Is.EqualTo(new[] { "sword", "shield", "potion" }));
        });
    }

    [Test]
    public void SaveGame_CreatesDirectoryIfMissing()
    {
        string newDir = Path.Combine(_testDirectory, "new_saves");
        var entry = new SaveEntry { Name = "fresh", File = "fresh.json" };
        var save = new SaveData
        {
            Character = "Tester",
            Map = "Test Map",
            Position = new Position { Xpos = 1, Ypos = 2 },
            Health = 100,
            Inventory = ["item"]
        };

        Program.SaveGame(newDir, entry, save);

        Assert.That(File.Exists(Path.Combine(newDir, "fresh.json")), Is.True);
    }

    // ─────────────────────────────────────────────────────────────
    // SaveIndex
    // ─────────────────────────────────────────────────────────────

    [Test]
    public void SaveIndex_WritesEntriesThatCanBeLoadedAgain()
    {
        Program.LoadIndex(_indexPath, out List<SaveEntry> entries);
        entries.Add(new SaveEntry
        {
            Name = "manual",
            File = "manual.json",
            UpdatedAt = new DateTime(2026, 5, 1, 12, 30, 0)
        });

        Program.SaveIndex(_indexPath, entries);
        Program.LoadIndex(_indexPath, out List<SaveEntry> loaded);

        Assert.Multiple(() =>
        {
            Assert.That(loaded, Has.Count.EqualTo(2));
            Assert.That(loaded[1].Name, Is.EqualTo("manual"));
            Assert.That(loaded[1].UpdatedAt, Is.EqualTo(new DateTime(2026, 5, 1, 12, 30, 0)));
        });
    }

    [Test]
    public void SaveIndex_UsesFormatWithoutFractionalSeconds()
    {
        Program.LoadIndex(_indexPath, out List<SaveEntry> entries);
        entries[0].UpdatedAt = new DateTime(2026, 4, 18, 10, 15, 0);

        Program.SaveIndex(_indexPath, entries);

        string json = File.ReadAllText(_indexPath);
        Assert.Multiple(() =>
        {
            Assert.That(json, Does.Contain("2026-04-18T10:15:00"));
            Assert.That(json, Does.Not.Contain("2026-04-18T10:15:00."));   // без миллисекунд
        });
    }

    // ─────────────────────────────────────────────────────────────
    // CreateBackup
    // ─────────────────────────────────────────────────────────────

    [Test]
    public void CreateBackup_CopiesSaveFile()
    {
        Program.LoadIndex(_indexPath, out List<SaveEntry> entries);
        SaveEntry entry = entries[0];

        bool result = Program.CreateBackup(_savesDirectory, _backupsDirectory, entry);

        string backupPath = Path.Combine(_backupsDirectory, entry.File);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(File.Exists(backupPath), Is.True);
            Assert.That(File.ReadAllText(backupPath),
                Is.EqualTo(File.ReadAllText(Path.Combine(_savesDirectory, entry.File))));
        });
    }

    [Test]
    public void CreateBackup_OverwritesExistingBackup()
    {
        Program.LoadIndex(_indexPath, out List<SaveEntry> entries);
        SaveEntry entry = entries[0];

        Program.CreateBackup(_savesDirectory, _backupsDirectory, entry);
        File.WriteAllText(Path.Combine(_savesDirectory, entry.File),
            "{\"character\":\"Updated\",\"map\":\"New\",\"position\":{\"x\":1,\"y\":2},\"health\":1,\"inventory\":[]}");
        Program.CreateBackup(_savesDirectory, _backupsDirectory, entry);

        string backupPath = Path.Combine(_backupsDirectory, entry.File);
        Assert.That(File.ReadAllText(backupPath), Does.Contain("Updated"));
    }

    [Test]
    public void CreateBackup_MissingSource_ReturnsFalse()
    {
        var entry = new SaveEntry { Name = "missing", File = "missing.json" };

        bool result = Program.CreateBackup(_savesDirectory, _backupsDirectory, entry);

        Assert.That(result, Is.False);
    }

    // ─────────────────────────────────────────────────────────────
    // RestoreBackup
    // ─────────────────────────────────────────────────────────────

    [Test]
    public void RestoreBackup_ReplacesCurrentSaveWithBackup()
    {
        Program.LoadIndex(_indexPath, out List<SaveEntry> entries);
        SaveEntry entry = entries[0];
        Program.CreateBackup(_savesDirectory, _backupsDirectory, entry);
        File.WriteAllText(Path.Combine(_savesDirectory, entry.File), "changed");

        bool result = Program.RestoreBackup(_savesDirectory, _backupsDirectory, entry);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(File.ReadAllText(Path.Combine(_savesDirectory, entry.File)),
                Does.Contain("Ashen Coast"));
        });
    }

    [Test]
    public void RestoreBackup_MissingBackup_ReturnsFalseAndDoesNotChangeSave()
    {
        Program.LoadIndex(_indexPath, out List<SaveEntry> entries);
        SaveEntry entry = entries[0];
        string savePath = Path.Combine(_savesDirectory, entry.File);
        string originalContent = File.ReadAllText(savePath);

        bool result = Program.RestoreBackup(_savesDirectory, _backupsDirectory, entry);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(File.ReadAllText(savePath), Is.EqualTo(originalContent));
        });
    }

    // ─────────────────────────────────────────────────────────────
    // DeleteSave
    // ─────────────────────────────────────────────────────────────

    [Test]
    public void DeleteSave_RemovesFileAndIndexEntry()
    {
        Program.LoadIndex(_indexPath, out List<SaveEntry> entries);
        SaveEntry entry = entries[0];

        bool result = Program.DeleteSave(_savesDirectory, _indexPath, entries, entry);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(File.Exists(Path.Combine(_savesDirectory, entry.File)), Is.False);
        });

        Program.LoadIndex(_indexPath, out entries);
        Assert.That(entries, Is.Empty);
    }

    [Test]
    public void DeleteSave_MissingFile_StillRemovesIndexEntry()
    {
        Program.LoadIndex(_indexPath, out List<SaveEntry> entries);
        SaveEntry entry = entries[0];
        File.Delete(Path.Combine(_savesDirectory, entry.File));

        bool result = Program.DeleteSave(_savesDirectory, _indexPath, entries, entry);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(entries, Is.Empty);
        });

        Program.LoadIndex(_indexPath, out List<SaveEntry> reloaded);
        Assert.That(reloaded, Is.Empty);
    }

    [Test]
    public void DeleteSave_EntryNotInList_ReturnsFalse()
    {
        Program.LoadIndex(_indexPath, out List<SaveEntry> entries);
        var stranger = new SaveEntry { Name = "stranger", File = "stranger.json" };

        bool result = Program.DeleteSave(_savesDirectory, _indexPath, entries, stranger);

        Assert.That(result, Is.False);
    }

    [Test]
    public void DeleteSave_KeepsOtherEntriesIntact()
    {
        Program.LoadIndex(_indexPath, out List<SaveEntry> entries);
        SaveEntry first = entries[0];
        entries.Add(new SaveEntry
        {
            Name = "slot-02",
            File = "save-02.json",
            UpdatedAt = new DateTime(2026, 4, 18, 11, 0, 0)
        });
        Program.SaveIndex(_indexPath, entries);
        Program.LoadIndex(_indexPath, out entries);

        Program.DeleteSave(_savesDirectory, _indexPath, entries,
            entries.First(e => e.Name == "slot-01"));

        Program.LoadIndex(_indexPath, out List<SaveEntry> reloaded);
        Assert.Multiple(() =>
        {
            Assert.That(reloaded, Has.Count.EqualTo(1));
            Assert.That(reloaded[0].Name, Is.EqualTo("slot-02"));
        });
    }
}