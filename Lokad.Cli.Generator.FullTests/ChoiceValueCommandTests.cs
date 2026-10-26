using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class ChoiceValueCommandTests
{
    [Theory]
    [InlineData("--add", "--name", "list")]
    [InlineData("--name", "list", "--add")]
    [InlineData("--add", "-n", "list")]
    [InlineData("--add", "--name=list")]
    public void ExcludesOptionValuesFromPrefixSelection(params string[] arguments)
    {
        Assert.Equal("list", Assert.IsType<ValueAddArgs>(Parser.Parse<IValueChoice>(arguments)).Name);
    }

    [Theory]
    [InlineData("--input", "copy")]
    [InlineData("--input=copy")]
    [InlineData("-i", "copy")]
    public void KeepsTheDefaultCommandWhenOnlyAnOptionValueMatchesAPrefix(params string[] arguments)
    {
        Assert.Equal("copy", Assert.IsType<ValueDefaultArgs>(Parser.Parse<IValueChoice>(arguments)).Input);
    }

    [Fact]
    public void PreservesGroupedOperandsAndActualCommandPrefixes()
    {
        var parsed = Assert.IsType<ValueAddArgs>(Parser.Parse<IValueChoice>(new[] { "--add", "-pp", "copy", "list" }));
        Assert.Equal(new[] { "copy", "list" }, parsed.Paths);
        Assert.IsType<ValueCopyArgs>(Parser.Parse<IValueChoice>(new[] { "copy" }));
        Assert.IsType<ValueListArgs>(Parser.Parse<IValueChoice>(new[] { "list" }));
    }
}

[Choice(typeof(ValueAddArgs))]
[Choice(typeof(ValueCopyArgs))]
[Choice(typeof(ValueListArgs))]
[Choice(typeof(ValueDefaultArgs))]
public partial interface IValueChoice { }

[Arguments("Add.", Long = "add")]
public partial record ValueAddArgs : IValueChoice
{
    [Argument("Name.", Long = "name", Short = 'n')] public string? Name { get; init; }
    [Argument("Paths.", Long = "path", Short = 'p')] public string[] Paths { get; init; } = System.Array.Empty<string>();
}

[Arguments("Copy.", Prefix = "copy")]
public partial record ValueCopyArgs : IValueChoice;

[Arguments("List.", Prefix = "list")]
public partial record ValueListArgs : IValueChoice;

[Arguments("Default.")]
public partial record ValueDefaultArgs : IValueChoice
{
    [Argument("Input.", Long = "input", Short = 'i')] public string? Input { get; init; }
}
