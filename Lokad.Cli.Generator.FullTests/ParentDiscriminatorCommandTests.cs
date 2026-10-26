using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class ParentDiscriminatorCommandTests
{
    [Theory]
    [InlineData("--tools", "--admin", "run")]
    [InlineData("-t", "-a", "run")]
    [InlineData("-tav", "run")]
    [InlineData("run", "--tools", "--admin")]
    public void AcceptsSelectedAncestorOptions(params string[] arguments)
    {
        Assert.IsType<ParentRunArgs>(Parser.Parse<IParentRoot>(arguments));
    }

    [Fact]
    public void BindsActualMembersAlongsideGroupedSelectors()
    {
        var parsed = Assert.IsType<ParentRunArgs>(Parser.Parse<IParentRoot>(new[] { "-tav", "run", "--name=--other" }));
        Assert.True(parsed.Verbose);
        Assert.Equal("--other", parsed.Name);
        Assert.IsType<ParentRunArgs>(Parser.Parse<IParentTools>(new[] { "--tools", "--admin", "run" }));
    }

    [Theory]
    [InlineData("--tools=bad", "--admin", "run")]
    [InlineData("-t=bad", "--admin", "run")]
    [InlineData("--tools", "--admin=bad", "run")]
    [InlineData("--tools", "-a=bad", "run")]
    [InlineData("--tools", "--other", "--admin", "run")]
    public void RejectsValuedFlagsAndUnselectedBranches(params string[] arguments)
    {
        Assert.Throws<ParseException>(() => Parser.Parse<IParentRoot>(arguments));
    }

    [Fact]
    public void DoesNotAcceptAncestorsWhenParsingTheLeafDirectly()
    {
        Assert.Throws<ParseException>(() => Parser.Parse<ParentRunArgs>(new[] { "--tools", "run" }));
    }
}

[Choice(typeof(IParentTools))]
[Choice(typeof(ParentOtherArgs))]
public partial interface IParentRoot { }

[Arguments("Tools.", Long = "tools", Short = 't')]
[Choice(typeof(IParentAdmin))]
public partial interface IParentTools : IParentRoot { }

[Arguments("Admin.", Long = "admin", Short = 'a')]
[Choice(typeof(ParentRunArgs))]
public partial interface IParentAdmin : IParentTools { }

[Arguments("Run.", Prefix = "run")]
public partial record ParentRunArgs : IParentAdmin
{
    [Argument("Verbose.", Short = 'v')] public bool Verbose { get; init; }
    [Argument("Name.", Long = "name")] public string? Name { get; init; }
}

[Arguments("Other.", Long = "other")]
public partial record ParentOtherArgs : IParentRoot;
