using System;
using System.Linq;
using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class ArgumentGroupCommandTests
{
    [Fact]
    public void SharesNestedOptionsAcrossCommandsAndStandaloneParsing()
    {
        var options = new[] { "--timeout=30", "-vv", "--tag=one", "--tag=two", "-Fjson" };
        var copy = Assert.IsType<GroupCopyArgs>(Parser.Parse<IGroupedCommand>(new[] { "copy", "in", "out" }.Concat(options).ToArray()));
        var inspect = Assert.IsType<GroupInspectArgs>(Parser.Parse<IGroupedCommand>(new[] { "inspect" }.Concat(options).ToArray()));
        var standalone = Parser.Parse<SharedGroupOptions>(options);
        Assert.Equal("in", copy.Input);
        Assert.Equal("out", copy.Output);
        foreach (var common in new[] { copy.Common, inspect.Common, standalone })
        {
            Assert.Equal(30, common.Timeout);
            Assert.True(common.Logging.Verbose);
            Assert.Null(common.Logging.Help);
            Assert.Equal(new[] { "one", "two" }, common.Tags);
            Assert.Equal("json", common.Format.Value);
        }
    }

    [Fact]
    public void PreservesRequiredChecksConversionAndOptionalValues()
    {
        Assert.Equal("missing option `--timeout`", Assert.Throws<ParseException>(() => Parser.Parse<GroupInspectArgs>(Array.Empty<string>())).Message);
        Assert.Equal("invalid value `bad` for `--timeout`", Assert.Throws<ParseException>(() => Parser.Parse<GroupInspectArgs>(new[] { "--timeout=bad" })).Message);
        var parsed = Parser.Parse<GroupInspectArgs>(new[] { "--timeout=1", "--format" });
        Assert.True(parsed.Common.Format.IsPresent);
        Assert.False(parsed.Common.Format.HasValue);
        Assert.Empty(parsed.Common.Tags);
    }

    [Fact]
    public void FlattensRepeatedGroupTypesAtTheirOwnPositionalLocations()
    {
        var parsed = Parser.Parse<GroupOperandsArgs>(new[] { "before", "1", "2", "after" });
        Assert.Equal("before", parsed.Before);
        Assert.Equal(1, parsed.First.Value);
        Assert.Equal(2, parsed.Second.Value);
        Assert.Equal("after", parsed.After);
        Assert.Equal(3, Parser.Parse<GroupOperand>(new[] { "3" }).Value);
    }

    [Fact]
    public void UsesNestedValueModesAndHelpWithoutConsumingTheCursor()
    {
        var tokens = Parser.Tokenize<IGroupedCommand>(new[] { "copy", "--timeout=bad", "--help" });
        var first = tokens.Peek();
        Assert.True(Parser.IsHelpRequested<IGroupedCommand>(tokens));
        Assert.Same(first, tokens.Peek());
        Assert.False(Parser.IsHelpRequested<GroupInspectArgs>(new[] { "--pattern", "--help" }));
        Assert.False(Parser.IsHelpRequested<GroupInspectArgs>(new[] { "--format=--help" }));

        tokens = Parser.Tokenize<GroupInspectArgs>(new[] { "--timeout", "30", "--help" });
        Assert.True(Parser.IsHelpRequested<GroupInspectArgs>(tokens));
        var parsed = Parser.ParseTokenized<GroupInspectArgs>(tokens);
        Assert.Equal(30, parsed.Common.Timeout);
        Assert.True(parsed.Common.Logging.Help);
        Assert.Null(tokens.Peek());
    }

    [Fact]
    public void DocumentsEachLeafInsteadOfTheGroupOperand()
    {
        var documentation = Parser.Document<GroupCopyArgs>("app");
        Assert.StartsWith("app copy <Input> <Output>: Copy.", documentation);
        Assert.Equal(1, documentation.Split(Environment.NewLine).Count(line => line.Contains("--timeout")));
        Assert.Equal(1, documentation.Split(Environment.NewLine).Count(line => line.Contains("--verbose")));
        Assert.DoesNotContain("<Common>", documentation);
        Assert.DoesNotContain("<Logging>", documentation);
        Assert.Contains("--timeout: Timeout.", documentation);
    }

    [Fact]
    public void AppliesRemainderBoundariesAfterFlatteningOuterAndGroupOperands()
    {
        var arguments = new[] { "label", "child", "--unknown", "--help" };
        Assert.False(Parser.IsHelpRequested<GroupForwardArgs>(arguments));
        var parsed = Parser.Parse<GroupForwardArgs>(arguments);
        Assert.Equal("label", parsed.Label);
        Assert.Equal("child", parsed.Forward.Program);
        Assert.Equal(new[] { "--unknown", "--help" }, parsed.Forward.Tail);
        Assert.True(Parser.IsHelpRequested<GroupForwardArgs>(new[] { "--help", "label", "child", "--unknown" }));
    }

    [Theory]
    [InlineData("--verbose")]
    [InlineData("--help")]
    [InlineData("--unknown")]
    public void InheritsLiteralBoundariesThroughGroupInterfaces(string tail)
    {
        var arguments = new[] { "file", tail };
        var tokens = Parser.Tokenize<LiteralGroupArgs>(arguments);
        Assert.False(Parser.IsHelpRequested<LiteralGroupArgs>(tokens));
        var embedded = Parser.ParseTokenized<LiteralGroupArgs>(tokens).Common;
        var standalone = Parser.Parse<LiteralGroupOptions>(arguments);
        Assert.Equal(arguments, embedded.Values);
        Assert.Equal(standalone.Values, embedded.Values);
        Assert.False(embedded.Verbose || embedded.Help);
        Assert.Null(tokens.Peek());
    }

    [Fact]
    public void InheritsInterfacePoliciesWithoutImportingTheirChoiceOptions()
    {
        Assert.Throws<ParseException>(() => Parser.Tokenize<SharedGroupOptions>(new[] { "--other-only=value" }));
        Assert.Equal("value", Assert.IsType<OtherGroupPolicyArgs>(Parser.Parse<ISharedGroupPolicy>(
            new[] { "other", "--other-only=value" })).Value);
        Assert.Throws<ParseException>(() => Parser.Tokenize<GroupInspectArgs>(new[] { "--other-only=value" }));
        Assert.Throws<ParseException>(() => Parser.Tokenize<SharedGroupOptions>(new[] { "-timeout=30" }));
        Assert.Throws<ParseException>(() => Parser.Tokenize<GroupInspectArgs>(new[] { "-timeout=30" }));
    }
}

[Choice(typeof(GroupCopyArgs))]
[Choice(typeof(GroupInspectArgs))]
public partial interface IGroupedCommand { }

[Arguments("Copy.", Prefix = "copy")]
public partial record GroupCopyArgs(
    [property: Argument("Input.")] string Input,
    [property: Argument("Common options.")] SharedGroupOptions Common,
    [property: Argument("Output.")] string Output) : IGroupedCommand;

[Arguments("Inspect.", Prefix = "inspect")]
public partial record GroupInspectArgs([property: Argument("Common options.")] SharedGroupOptions Common) : IGroupedCommand;

[Arguments("Common options.")]
public partial record SharedGroupOptions([property: Argument("Timeout.", Long = "timeout")] int Timeout) : ISharedGroupPolicy
{
    [Argument("Logging options.")] public GroupLoggingOptions Logging { get; init; }
    [Argument("Tags.", Long = "tag")] public string[] Tags { get; init; } = Array.Empty<string>();
    [Argument("Format.", Long = "format", Short = 'F')] public OptionalValue<string> Format { get; init; }
    [Argument("Pattern.", Long = "pattern", ConsumeNextValueVerbatim = true)] public string? Pattern { get; init; }
}

[Arguments("Shared policy.", ShortOptionSyntax = ShortOptionSyntax.AttachedValues, AllowSingleDashLongOptions = false)]
[Choice(typeof(SharedGroupOptions))]
[Choice(typeof(OtherGroupPolicyArgs))]
public partial interface ISharedGroupPolicy { }

[Arguments("Other policy choice.", Prefix = "other")]
public partial record OtherGroupPolicyArgs([property: Argument("Value.", Long = "other-only")] string Value) : ISharedGroupPolicy;

[Arguments("Logging options.")]
public partial struct GroupLoggingOptions
{
    [Argument("Verbose.", Long = "verbose", Short = 'v', AllowRepeated = true)] public bool Verbose;
    [Argument("Help.", Long = "help", Short = 'h', IsHelp = true)] public bool? Help { get; init; }
}

[Arguments("Operand.")]
public partial record struct GroupOperand([property: Argument("Value.")] int Value);

[Arguments("Operands.")]
public partial record GroupOperandsArgs(
    [property: Argument("Before.")] string Before,
    [property: Argument("First.")] GroupOperand First,
    [property: Argument("Second.")] GroupOperand Second,
    [property: Argument("After.")] string After);

[Arguments("Forward.")]
public partial record GroupForwardArgs(
    [property: Argument("Label.")] string Label,
    [property: Argument("Forward options.")] ForwardGroupOptions Forward);

[Arguments("Forward options.")]
public partial record ForwardGroupOptions(
    [property: Argument("Program.")] string Program,
    [property: Argument("Tail.", Remainder = true)] string[] Tail)
{
    [Argument("Help.", Long = "help", IsHelp = true)] public bool Help { get; init; }
}

[Arguments("Literal options policy.", StopAtFirstOperand = true)]
[Choice(typeof(LiteralGroupOptions))]
public partial interface ILiteralGroupPolicy { }

public interface ILiteralGroup : ILiteralGroupPolicy { }

[Arguments("Literal options.")]
public partial record LiteralGroupOptions([property: Argument("Values.")] string[] Values) : ILiteralGroup
{
    [Argument("Verbose.", Long = "verbose")] public bool Verbose { get; init; }
    [Argument("Help.", Long = "help", IsHelp = true)] public bool Help { get; init; }
}

[Arguments("Literal group.")]
public partial record LiteralGroupArgs([property: Argument("Common options.")] LiteralGroupOptions Common);
