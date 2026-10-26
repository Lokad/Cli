using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests.GeneratedNames;

public sealed class CollidingMemberCommandTests
{
    [Fact]
    public void ParserFacadeSupportsMembersNamedAfterParserMethods()
    {
        var tokens = Parser.Tokenize<MethodNameArgs>(new[] { "--parse=1", "--tokenize=2", "--tokens=3", "--document=4" });
        var parsed = Parser.ParseTokenized<MethodNameArgs>(tokens);
        Assert.Equal(1, parsed.Parse);
        Assert.Equal(2, parsed.Tokenize);
        Assert.Equal(3, parsed.ParseTokenized);
        Assert.Equal(4, parsed.Document);
        Assert.Contains("--document", Parser.Document<MethodNameArgs>("app"));
        Assert.Equal(5, Parser.Parse<Document>(new[] { "--value=5" }).Value);
        Assert.Equal(6, Document.Parse(new[] { "--value=6" }).Value);
    }

    [Fact]
    public void GeneratedHelpersAvoidTypeMemberAndGeneratedLookingNames()
    {
        var parsed = Parser.Parse<HelperCollisionArgs>(new[] { "--number=1", "--document=2", "--tokenize=text" });
        Assert.Equal(1, parsed.Number);
        Assert.Equal(2, parsed.DocumentCore);
        Assert.Equal("text", parsed.TokenizeCore);
        Assert.Equal("user", HelperCollisionArgs.ParseInt32__Cli);
        Assert.Equal("holder", HelperCollisionArgs.OptionNames__Cli_);
        Assert.Contains("--document", Parser.Document<HelperCollisionArgs>("app"));
        Assert.Equal(7, Parser.Parse<ParseInt32>(new[] { "--value=7" }).Value);
        Assert.Equal(8, Parser.Parse<TryParseInt32__Cli>(new[] { "--value=8" }).Value);
        Assert.IsType<HelperChoiceArgs>(Parser.Parse<IHelperChoice>(new[] { "run" }));
        Assert.Contains("run", Parser.Document<IHelperChoice>("app"));
    }
}

[Arguments("Members named after parser methods.")]
public partial record MethodNameArgs
{
    [Argument("Parse.", Long = "parse")] public int Parse { get; init; }
    [Argument("Tokenize.", Long = "tokenize")] public int Tokenize { get; init; }
    [Argument("Tokens.", Long = "tokens")] public int ParseTokenized;
    [Argument("Document.", Long = "document")] public int Document { get; init; }
}

[Arguments("Type named after a parser method.")]
public partial record Document([property: Argument("Value.", Long = "value")] int Value);

[Arguments("Helper collisions.")]
public partial class HelperCollisionArgs
{
    private HelperCollisionArgs([Argument("Number.", Long = "number")] int value) => Number = value;
    public int Number { get; }
    [Argument("Document.", Long = "document")] public int DocumentCore { get; init; }
    [Argument("Tokenize.", Long = "tokenize")] public string? TokenizeCore { get; init; }
    public static string ParseInt32__Cli => "user";
    public static string OptionNames__Cli_ => "holder";
}

[Arguments("Type named after a conversion helper.")]
public partial record ParseInt32([property: Argument("Value.", Long = "value")] int Value);

[Arguments("Type named after a generated-looking helper.")]
public partial record TryParseInt32__Cli([property: Argument("Value.", Long = "value")] int Value);

public interface IUserHelperNames
{
    static string ParseChoice__Cli => "user";
}

[Choice(typeof(HelperChoiceArgs))]
public partial interface IHelperChoice : IUserHelperNames { }

[Arguments("Run.", Prefix = "run")]
public partial record HelperChoiceArgs : IHelperChoice;
