using System;
using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class PrefixCommandTests
{
    [Theory]
    [InlineData(1, "a.txt", "copy", "b.txt")]
    [InlineData(2, "copy", "b.txt")]
    public void KeepsLaterDataAfterConsumingThePrefix(int consumed, params string[] expected)
    {
        var tokens = Parser.Tokenize<PrefixCopyArgs>(new[] { "copy", "a.txt", "copy", "b.txt" });
        for (var i = 0; i < consumed; i++)
            tokens.Shift();
        Assert.True(tokens.HasPositionalPrefix("copy"));
        Assert.Equal(expected, Parser.ParseTokenized<PrefixCopyArgs>(tokens).Files);
        Assert.True(tokens.HasPositionalPrefix("copy"));
    }

    [Fact]
    public void KeepsChildSelectorsOutOfDataAfterConsumingAnAncestorPrefix()
    {
        var tokens = Parser.Tokenize<INestedChoiceWithMembersSubArgs>(new[] { "sub", "B", "first-pos", "--first=first-name" });
        Assert.Equal(new ValueArg("sub"), tokens.Shift());
        var parsed = Assert.IsType<NestedChoiceWithMembersSubArgs1>(Parser.ParseTokenized<INestedChoiceWithMembersSubArgs>(tokens));
        Assert.Equal("first-pos", parsed.Positional);
        Assert.Equal("first-name", parsed.Named);
    }

    [Fact]
    public void KeepsRepeatedPrefixDataInCollectionBoundaryBinding()
    {
        var tokens = Parser.Tokenize<IBoundaryCommand>(new[] { "numbers", "10", "numbers", "12" });
        Assert.Equal(new ValueArg("numbers"), tokens.Shift());
        var parsed = Assert.IsType<ArrayBoundaryArgs>(Parser.ParseTokenized<IBoundaryCommand>(tokens));
        Assert.Equal(new[] { 10 }, parsed.Numbers);
        Assert.Equal("numbers", parsed.Label);
        Assert.Equal(12, parsed.Count);
    }

    [Theory]
    [InlineData("a.txt", "copy", "b.txt")]
    [InlineData("copy", "a.txt", "copy", "b.txt")]
    [InlineData("--source", "input.txt", "copy", "a.txt", "copy", "b.txt")]
    [InlineData("--source", "copy", "a.txt", "copy", "b.txt")]
    [InlineData("--", "a.txt", "copy", "b.txt")]
    public void KeepsLaterPrefixLookingOperands(params string[] arguments)
    {
        Assert.Equal(new[] { "a.txt", "copy", "b.txt" }, Parser.Parse<PrefixCopyArgs>(arguments).Files);
    }

    [Fact]
    public void SeparatesGroupedOptionValuesFromDirectAndSelectedPrefixes()
    {
        var arguments = new[] { "--source", "copy", "-pp", "copy", "other", "copy", "a.txt", "copy", "b.txt" };
        var direct = Parser.Parse<PrefixCopyArgs>(arguments);
        var selected = Assert.IsType<PrefixCopyArgs>(Parser.Parse<IPrefixCopyCommand>(arguments));
        Assert.Equal(new[] { "copy", "other" }, direct.Patterns);
        Assert.Equal("copy", direct.Source);
        Assert.Equal(new[] { "a.txt", "copy", "b.txt" }, direct.Files);
        Assert.Equal(direct.Patterns, selected.Patterns);
        Assert.Equal(direct.Source, selected.Source);
        Assert.Equal(direct.Files, selected.Files);
    }

    [Fact]
    public void KeepsPrefixLookingDataAfterTheFirstOperandBoundary()
    {
        var arguments = new[] { "a.txt", "copy", "b.txt" };
        Assert.Equal(arguments, Parser.Parse<StoppedPrefixCopyArgs>(arguments).Files);
    }
}

[Choice(typeof(PrefixCopyArgs))]
public partial interface IPrefixCopyCommand { }

[Arguments("Copy.", Prefix = "copy")]
public partial record PrefixCopyArgs([property: Argument("Files.")] string[] Files) : IPrefixCopyCommand
{
    [Argument("Source.", Long = "source")] public string? Source { get; init; }
    [Argument("Patterns.", Short = 'p')] public string[] Patterns { get; init; } = Array.Empty<string>();
}

[Arguments("Copy.", Prefix = "copy", StopAtFirstOperand = true)]
public partial record StoppedPrefixCopyArgs([property: Argument("Files.")] string[] Files);
