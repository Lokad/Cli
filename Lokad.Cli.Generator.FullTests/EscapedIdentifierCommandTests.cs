using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests.@event;

public sealed class EscapedIdentifierCommandTests
{
    [Fact]
    public void ParsesKeywordTypesAndMembersInAnEscapedNamespace()
    {
        Assert.Equal(7, Parser.Parse<@class>(new[] { "--number=7" }).@event.@namespace);
        Assert.Equal(9, Parser.Parse<@struct>(new[] { "--number=9" }).@namespace);
        Assert.Contains("--number", Parser.Document<@class>("app"));
    }
}

[Arguments("Keyword command.")]
public partial record @class([property: Argument("Options.")] @struct @event);

[Arguments("Keyword group.")]
public partial struct @struct
{
    [Argument("Number.", Long = "number")] public int @namespace;
}
