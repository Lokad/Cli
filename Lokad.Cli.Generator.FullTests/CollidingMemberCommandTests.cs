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
