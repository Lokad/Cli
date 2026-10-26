using System;
using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class QuotedSelectorCommandTests
{
    [Theory]
    [InlineData(@"x\y", typeof(PathSelector))]
    [InlineData("quote\"{x}\nline", typeof(QuotedSelector))]
    public void PreservesSelectorsInParsingDocumentationAndErrors(string selector, Type expectedType)
    {
        Assert.IsType(expectedType, Parser.Parse<IQuotedSelector>(new[] { selector }));
        Assert.Contains(selector, Parser.Document<IQuotedSelector>("app"));
        var error = Assert.Throws<ParseException>(() => Parser.Parse<IQuotedSelector>(new[] { "--other", selector }));
        Assert.Equal($"incompatible options `--other` and `{selector}`", error.Message);
    }
}

[Choice(typeof(OtherSelector))]
[Choice(typeof(PathSelector))]
[Choice(typeof(QuotedSelector))]
public partial interface IQuotedSelector { }

[Arguments("Other.", Long = "other")]
public partial record OtherSelector : IQuotedSelector;

[Arguments("Path.", Prefix = @"x\y")]
public partial record PathSelector : IQuotedSelector;

[Arguments("Quotes and line breaks.", Prefix = "quote\"{x}\nline")]
public partial record QuotedSelector : IQuotedSelector;
