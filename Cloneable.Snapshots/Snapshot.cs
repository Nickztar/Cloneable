namespace Cloneable.Snapshots;

public class SnapshotTests
{
    [Theory]
    [ClassData(typeof(PlaygroundData))]
    public Task TestPlayground(string file)
    {
        return SnapshotHelpers.Verify(File.ReadAllText(file)).UseDirectory($"Snapshots/{file}").UseParameters(file);
    }
}

public class PlaygroundData : TheoryData<string>
{
    public PlaygroundData()
    {
        foreach (var fileName in Directory.EnumerateFiles("Playground", "*.cs"))
        {
            Add(fileName);
        }        
    }
}
