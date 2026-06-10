namespace Lokad.Cli.Tests;

public class RuntimeApiTests
{
    [Fact]
    public void ParserDelegatesToStaticInterfaceMembers()
    {
        Assert.Throws<System.NotImplementedException>(() => Parser.Parse<Parsed>(System.Array.Empty<string>()));
        Assert.Throws<System.NotImplementedException>(() => Parser.Document<Parsed>("cmd"));
    }

    [Fact]
    public void ParserAcceptsEnumerableArguments()
    {
        System.Collections.Generic.IEnumerable<string> arguments = Enumerate("one", "two");

        var parsed = Parser.Parse<CapturedParsed>(arguments);

        Assert.Equal(new[] { "one", "two" }, parsed.Arguments);
    }

    [Fact]
    public void ParserAcceptsReadOnlyListArguments()
    {
        System.Collections.Generic.IReadOnlyList<string> arguments = new System.Collections.Generic.List<string>
        {
            "one",
            "two"
        };

        var parsed = Parser.Parse<CapturedParsed>(arguments);

        Assert.Equal(new[] { "one", "two" }, parsed.Arguments);
    }

    [Fact]
    public void ParserAcceptsReadOnlySpanArguments()
    {
        var arguments = new[] { "one", "two" };

        var parsed = Parser.Parse<CapturedParsed>(new System.ReadOnlySpan<string>(arguments));

        Assert.Equal(new[] { "one", "two" }, parsed.Arguments);
    }

    [Fact]
    public void RangesParsesCommaSeparatedValues()
    {
        var ranges = Ranges.Parse("1,4,5-6,-10,20-", null);

        Assert.Equal(5, ranges.Count);
        Assert.Equal((1, 1), ranges[0]);
        Assert.Equal((4, 4), ranges[1]);
        Assert.Equal((5, 6), ranges[2]);
        Assert.Equal((int.MinValue, 10), ranges[3]);
        Assert.Equal((20, int.MaxValue), ranges[4]);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(",")]
    [InlineData("1,,2")]
    [InlineData("-")]
    [InlineData("1-2-3")]
    [InlineData("abc")]
    public void RangesRejectsInvalidValues(string value)
    {
        Assert.False(Ranges.TryParse(value, null, out _));
        Assert.Throws<System.FormatException>(() => Ranges.Parse(value, null));
    }

    private sealed class Parsed : ICliParsed<Parsed>
    {
        public static Parsed Parse(System.Collections.Generic.IReadOnlyList<string> arguments)
        {
            throw new System.NotImplementedException();
        }

        public static Parsed ParseTokenized(TokenizedArguments arguments)
        {
            throw new System.NotImplementedException();
        }

        public static string Document(string commandName)
        {
            throw new System.NotImplementedException();
        }
    }

    private sealed class CapturedParsed : ICliParsed<CapturedParsed>
    {
        public CapturedParsed(System.Collections.Generic.IReadOnlyList<string> arguments)
        {
            Arguments = arguments;
        }

        public System.Collections.Generic.IReadOnlyList<string> Arguments { get; }

        public static CapturedParsed Parse(System.Collections.Generic.IReadOnlyList<string> arguments)
        {
            return new CapturedParsed(arguments);
        }

        public static CapturedParsed ParseTokenized(TokenizedArguments arguments)
        {
            throw new System.NotImplementedException();
        }

        public static string Document(string commandName)
        {
            throw new System.NotImplementedException();
        }
    }

    private static System.Collections.Generic.IEnumerable<string> Enumerate(params string[] arguments)
    {
        foreach (var argument in arguments)
            yield return argument;
    }
}
