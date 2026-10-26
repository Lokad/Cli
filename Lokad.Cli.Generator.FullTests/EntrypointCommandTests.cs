using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class EntrypointCommandTests
{
    [Fact]
    public void KeepsSiblingHelpOutOfDirectLeafInspection()
    {
        var arguments = new[] { "--help" };
        Assert.False(Parser.IsHelpRequested<EntrypointRunArgs>(arguments));
        Assert.True(Parser.Parse<EntrypointRunArgs>(arguments).DisplayHelp);
        Assert.True(Parser.IsHelpRequested<IEntrypointCommand>(arguments));
    }

    [Fact]
    public void LimitsTokenizationToTheEntrypointsReachableCommands()
    {
        Assert.Throws<ParseException>(() => Parser.Tokenize<EntrypointRunArgs>(new[] { "--other-only" }));
        Assert.Throws<ParseException>(() => Parser.Tokenize<EntrypointRunArgs>(new[] { "-h" }));
        Assert.True(Assert.IsType<EntrypointOtherArgs>(Parser.Parse<IEntrypointCommand>(
            new[] { "other", "--other-only" })).Other);
    }

    [Fact]
    public void PreservesInheritedLexicalPolicies()
    {
        var direct = Parser.Parse<EntrypointRunArgs>(new[] { "-vnreport.txt" });
        Assert.True(direct.Verbose);
        Assert.Equal("report.txt", direct.Name);
        var selected = Assert.IsType<EntrypointRunArgs>(Parser.Parse<IEntrypointCommand>(
            new[] { "run", "-vnreport.txt" }));
        Assert.Equal(direct, selected);
        Assert.Throws<ParseException>(() => Parser.Tokenize<EntrypointRunArgs>(new[] { "-help" }));
    }
}

[Arguments("Commands.", ShortOptionSyntax = ShortOptionSyntax.AttachedValues, AllowSingleDashLongOptions = false)]
[Choice(typeof(EntrypointRunArgs))]
[Choice(typeof(EntrypointHelpArgs))]
[Choice(typeof(EntrypointOtherArgs))]
public partial interface IEntrypointCommand { }

[Arguments("Run.", Prefix = "run")]
public partial record EntrypointRunArgs : IEntrypointCommand
{
    [Argument("Display help text as data.", Long = "help")] public bool DisplayHelp { get; init; }
    [Argument("Verbose.", Short = 'v')] public bool Verbose { get; init; }
    [Argument("Name.", Short = 'n', Long = "name")] public string? Name { get; init; }
}

[Arguments("Help.", Long = "help", Short = 'h', IsHelp = true)]
public partial record EntrypointHelpArgs : IEntrypointCommand;

[Arguments("Other.", Prefix = "other")]
public partial record EntrypointOtherArgs : IEntrypointCommand
{
    [Argument("Other option.", Long = "other-only")] public bool Other { get; init; }
}
