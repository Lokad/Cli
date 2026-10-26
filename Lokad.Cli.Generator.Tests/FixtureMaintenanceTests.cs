#pragma warning disable RS1035 // These tests exercise fixture maintenance in temporary directories.

namespace Lokad.Cli.Generator.Tests;

public sealed class FixtureMaintenanceTests
{
    [Fact]
    public void RejectsUnknownFixtureNames()
    {
        Assert.Equal(1, Program.Main(new[] { "--refresh", "__MissingFixtureForRefreshTest" }));
    }

    [Fact]
    public void RefreshesOnlySnapshotsWithinFixtureDirectories()
    {
        var sandbox = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "lokad-cli-fixtures-" + Guid.NewGuid().ToString("N")));
        var testsRoot = Path.Combine(sandbox, "Tests");
        var fixture = Path.Combine(testsRoot, "Example");
        var outside = Path.Combine(sandbox, "Outside");
        Directory.CreateDirectory(fixture);
        Directory.CreateDirectory(outside);
        try
        {
            const string input = "using Lokad.Cli; [Arguments(\"Example.\")] public partial record Example;";
            File.WriteAllText(Path.Combine(fixture, "Input.cs"), input);
            File.WriteAllText(Path.Combine(fixture, "Old.lkc.g.cs"), "Stale snapshot.");
            File.WriteAllText(Path.Combine(fixture, "Diagnostics.txt"), "Stale diagnostics.");
            File.WriteAllText(Path.Combine(fixture, "Notes.md"), "Keep fixture notes.");
            File.WriteAllText(Path.Combine(fixture, "Support.cs"), "Keep unrelated source.");
            File.WriteAllText(Path.Combine(outside, "Input.cs"), input);
            File.WriteAllText(Path.Combine(outside, "Keep.txt"), "Keep outside files.");

            foreach (var name in new[] { "", ".", "..", ".. ", "../Outside", "..\\Outside", outside })
                Assert.False(GeneratorTestFixture.TryRefreshFixture(testsRoot, name));
            Assert.Equal(input, File.ReadAllText(Path.Combine(outside, "Input.cs")));
            Assert.Equal("Keep outside files.", File.ReadAllText(Path.Combine(outside, "Keep.txt")));

            Assert.True(GeneratorTestFixture.TryRefreshFixture(testsRoot, "Example"));
            Assert.False(File.Exists(Path.Combine(fixture, "Old.lkc.g.cs")));
            Assert.False(File.Exists(Path.Combine(fixture, "Diagnostics.txt")));
            Assert.Equal(input, File.ReadAllText(Path.Combine(fixture, "Input.cs")));
            Assert.Equal("Keep fixture notes.", File.ReadAllText(Path.Combine(fixture, "Notes.md")));
            Assert.Equal("Keep unrelated source.", File.ReadAllText(Path.Combine(fixture, "Support.cs")));
            var generated = File.ReadAllText(Path.Combine(fixture, "Example.lkc.g.cs"));
            Assert.True(GeneratorTestFixture.TryRefreshFixture(testsRoot, "Example"));
            Assert.Equal(generated, File.ReadAllText(Path.Combine(fixture, "Example.lkc.g.cs")));
        }
        finally
        {
            Directory.Delete(sandbox, recursive: true);
        }
    }
}
