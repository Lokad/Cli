using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public class GeneratorFullTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false, "-a")]
    [InlineData(false, true, "-b")]
    [InlineData(true, true, "-a", "-b")]
    [InlineData(true, true, "-ab")]
    [InlineData(true, true, "-ba")]
    public void ParsesOptionalBoolShortFlags(bool expectedA, bool expectedB, params string[] arguments)
    {
        var parsed = Parser.Parse<BoolFlags>(arguments);

        Assert.Equal(expectedA, parsed.A);
        Assert.Equal(expectedB, parsed.B);
    }

    [Theory]
    [InlineData("-c", "unknown short option `-c`")]
    [InlineData("--a", "unknown option `--a`")]
    public void RejectsUnknownBoolFlagForms(string argument, string expectedMessage)
    {
        var ex = Assert.Throws<ParseException>(() => Parser.Parse<BoolFlags>(new[] { argument }));

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Theory]
    [InlineData("-a", "-a")]
    [InlineData("-aa")]
    public void RejectsDuplicateBoolFlags(params string[] arguments)
    {
        var ex = Assert.Throws<ParseException>(() => Parser.Parse<BoolFlags>(arguments));

        Assert.Equal("duplicate option `-a`", ex.Message);
    }

    public static TheoryData<string[], string[]> StringArrayOptionCases => new()
    {
        { System.Array.Empty<string>(), System.Array.Empty<string>() },
        { new[] { "hello" }, new[] { "-x", "hello" } },
        { new[] { "hello", "world" }, new[] { "-xx", "hello", "world" } },
        { new[] { "hello" }, new[] { "--xval=hello" } },
        { new[] { "hello", "world" }, new[] { "--xval", "hello", "--xval", "world" } },
        { new[] { "hello", "world" }, new[] { "-x", "hello", "--xval=world" } },
        { new[] { "hello" }, new[] { "-x=hello" } },
    };

    [Theory]
    [MemberData(nameof(StringArrayOptionCases))]
    public void ParsesStringArrayOptions(string[] expected, string[] arguments)
    {
        var parsed = Parser.Parse<StringArrayOptions>(arguments);

        Assert.Equal(expected, parsed.X);
    }

    [Theory]
    [InlineData("option `--xval` expects a value", "--xval", "--xval", "hello", "world")]
    [InlineData("option `-x` expects a value", "-xx", "hello")]
    [InlineData("unexpected argument `world`", "-x", "hello", "world")]
    [InlineData("option `-x` expects a value", "-x", "--xval=hello", "world")]
    [InlineData("unexpected argument `hello`", "hello", "-x")]
    [InlineData("unexpected = in short option list `-xx=hello`", "-xx=hello")]
    public void RejectsInvalidStringArrayOptionForms(string expectedMessage, params string[] arguments)
    {
        var ex = Assert.Throws<ParseException>(() => Parser.Parse<StringArrayOptions>(arguments));

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(10, "-d", "10")]
    [InlineData(10, "-d10")]
    [InlineData(10, "--max-depth", "10")]
    [InlineData(10, "--max-depth=10")]
    public void ParsesNullableIntShortAndLongOptions(int? expectedMaxDepth, params string[] arguments)
    {
        var parsed = Parser.Parse<MyArgs>(arguments);

        Assert.Equal(expectedMaxDepth, parsed.MaxDepth);
    }

    [Theory]
    [InlineData("invalid value `abc` for `-d`", "-d", "abc")]
    [InlineData("unknown short option `-1`", "-10")]
    public void RejectsInvalidNullableIntShortOptionValue(string expectedMessage, params string[] arguments)
    {
        var ex = Assert.Throws<ParseException>(() => Parser.Parse<MyArgs>(arguments));

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void ParsesShortNumericIntArgument()
    {
        var parsed = Parser.Parse<ShortNumericArgs>(new[] { "-10" });

        Assert.Equal(10, parsed.Value);
    }

    [Theory]
    [InlineData(true, true, "-x4")]
    [InlineData(true, true, "-4x")]
    [InlineData(false, true, "-4")]
    [InlineData(true, true, "-x", "-4")]
    [InlineData(true, true, "-4", "-x")]
    public void ParsesBoolFlagsWithLetterAndDigitShortNames(bool expectedX, bool expectedFour, params string[] arguments)
    {
        var parsed = Parser.Parse<LetterAndDigitBoolFlags>(arguments);

        Assert.Equal(expectedX, parsed.X);
        Assert.Equal(expectedFour, parsed.Four);
    }

    [Fact]
    public void RejectsUnknownDigitShortBoolFlag()
    {
        var ex = Assert.Throws<ParseException>(() => Parser.Parse<LetterAndDigitBoolFlags>(new[] { "-5" }));

        Assert.Equal("unknown short option `-5`", ex.Message);
    }

    [Fact]
    public void ParsesDirectChoiceWithPrefix()
    {
        var parsed = Parser.Parse<INestedChoiceArgs>(new[] { "A" });

        Assert.IsType<NestedChoiceArgs>(parsed);
    }

    [Fact]
    public void ParsesFirstSubChoiceWithOuterAndInnerPrefixes()
    {
        var parsed = Parser.Parse<INestedChoiceArgs>(new[] { "sub", "B" });

        Assert.IsType<NestedChoiceSubArgs1>(parsed);
    }

    [Fact]
    public void ParsesSecondSubChoiceWithOuterAndInnerPrefixes()
    {
        var parsed = Parser.Parse<INestedChoiceArgs>(new[] { "sub", "C" });

        Assert.IsType<NestedChoiceSubArgs2>(parsed);
    }

    [Fact]
    public void ParsesDirectChoiceMembers()
    {
        var parsed = Assert.IsType<NestedChoiceWithMembersArgs>(
            Parser.Parse<INestedChoiceWithMembersArgs>(new[] { "A", "direct-pos", "--direct", "direct-name" }));

        Assert.Equal("direct-pos", parsed.Positional);
        Assert.Equal("direct-name", parsed.Named);
    }

    [Fact]
    public void ParsesFirstSubChoiceMembers()
    {
        var parsed = Assert.IsType<NestedChoiceWithMembersSubArgs1>(
            Parser.Parse<INestedChoiceWithMembersArgs>(new[] { "sub", "B", "first-pos", "--first", "first-name" }));

        Assert.Equal("first-pos", parsed.Positional);
        Assert.Equal("first-name", parsed.Named);
    }

    [Fact]
    public void ParsesSecondSubChoiceMembers()
    {
        var parsed = Assert.IsType<NestedChoiceWithMembersSubArgs2>(
            Parser.Parse<INestedChoiceWithMembersArgs>(new[] { "sub", "C", "second-pos", "--second", "second-name" }));

        Assert.Equal("second-pos", parsed.Positional);
        Assert.Equal("second-name", parsed.Named);
    }

    [Theory]
    [InlineData("unexpected argument `wrong-pos`", "A", "direct-pos", "wrong-pos")]
    [InlineData("unexpected option `--first`", "A", "--first", "first-name")]
    [InlineData("unexpected option `--direct`", "sub", "B", "--direct", "direct-name")]
    [InlineData("unexpected option `--second`", "sub", "B", "--second", "second-name")]
    [InlineData("unexpected option `--first`", "sub", "C", "--first", "first-name")]
    public void RejectsWrongChoiceMembersAsUnexpected(string expectedMessage, params string[] arguments)
    {
        var ex = Assert.Throws<ParseException>(() => Parser.Parse<INestedChoiceWithMembersArgs>(arguments));

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Theory]
    [InlineData("unknown option `--missing`", "A", "--missing", "value")]
    [InlineData("unknown option `--missing`", "sub", "B", "--missing", "value")]
    [InlineData("unknown option `--missing`", "sub", "C", "--missing", "value")]
    public void RejectsUnknownNestedChoiceMembersAsUnknown(string expectedMessage, params string[] arguments)
    {
        var ex = Assert.Throws<ParseException>(() => Parser.Parse<INestedChoiceWithMembersArgs>(arguments));

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void ParsesChoiceWithLongOptionDiscriminator()
    {
        var parsed = Assert.IsType<LongOptionChoiceAlphaArgs>(
            Parser.Parse<ILongOptionChoiceArgs>(new[] { "--alpha", "alpha-pos", "--alpha-name", "alpha-name" }));

        Assert.Equal("alpha-pos", parsed.Positional);
        Assert.Equal("alpha-name", parsed.Name);
    }

    [Fact]
    public void ParsesOtherChoiceWithLongOptionDiscriminator()
    {
        var parsed = Assert.IsType<LongOptionChoiceBetaArgs>(
            Parser.Parse<ILongOptionChoiceArgs>(new[] { "--beta", "beta-pos", "--beta-name", "beta-name" }));

        Assert.Equal("beta-pos", parsed.Positional);
        Assert.Equal("beta-name", parsed.Name);
    }

    [Fact]
    public void RejectsIncompatibleLongOptionChoiceDiscriminators()
    {
        var ex = Assert.Throws<ParseException>(
            () => Parser.Parse<ILongOptionChoiceArgs>(new[] { "--alpha", "--beta" }));

        Assert.Equal("incompatible options `--alpha` and `--beta`", ex.Message);
    }

    [Fact]
    public void ParsesChoiceWithShortOptionDiscriminator()
    {
        var parsed = Assert.IsType<ShortOptionChoiceAlphaArgs>(
            Parser.Parse<IShortOptionChoiceArgs>(new[] { "-a", "alpha-pos", "-x", "alpha-name" }));

        Assert.Equal("alpha-pos", parsed.Positional);
        Assert.Equal("alpha-name", parsed.Name);
    }

    [Fact]
    public void ParsesOtherChoiceWithShortOptionDiscriminator()
    {
        var parsed = Assert.IsType<ShortOptionChoiceBetaArgs>(
            Parser.Parse<IShortOptionChoiceArgs>(new[] { "-b", "beta-pos", "-y", "beta-name" }));

        Assert.Equal("beta-pos", parsed.Positional);
        Assert.Equal("beta-name", parsed.Name);
    }

    [Fact]
    public void RejectsIncompatibleShortOptionChoiceDiscriminators()
    {
        var ex = Assert.Throws<ParseException>(
            () => Parser.Parse<IShortOptionChoiceArgs>(new[] { "-ab" }));

        Assert.Equal("incompatible options `-a` and `-b`", ex.Message);
    }

    [Fact]
    public void SkipsOnlyPositionalDiscriminatorWhenOptionValueMatchesPrefix()
    {
        var parsed = Assert.IsType<PrefixFieldArgs>(
            Parser.Parse<IPrefixFieldArgs>(new[] { "--field", "xxx", "--field", "yyy", "xxx" }));

        Assert.Equal(new[] { "xxx", "yyy" }, parsed.Field);

        var equivalent = Assert.IsType<PrefixFieldArgs>(
            Parser.Parse<IPrefixFieldArgs>(new[] { "xxx", "--field=xxx", "--field=yyy" }));

        Assert.Equal(parsed.Field, equivalent.Field);
    }
}

[Arguments]
public partial record BoolFlags
{
    [Argument(Short = 'a')]
    public bool A { get; init; }

    [Argument(Short = 'b')]
    public bool B { get; init; }
}

[Arguments]
public partial record StringArrayOptions
{
    [Argument(Short = 'x', Long = "xval")]
    public string[] X { get; init; } = System.Array.Empty<string>();
}

[Arguments]
public partial record MyArgs
{
    [Argument(
        "Maximum depth for recursive traversal.",
        Short = 'd',
        Long = "max-depth")]
    public int? MaxDepth { get; init; }
}

[Arguments]
public partial record ShortNumericArgs
{
    [Argument(ShortNumeric = true)]
    public int Value { get; init; }
}

[Arguments]
public partial record LetterAndDigitBoolFlags
{
    [Argument(Short = 'x')]
    public bool X { get; init; }

    [Argument(Short = '4')]
    public bool Four { get; init; }
}

[Choice(typeof(INestedChoiceSubArgs))]
[Choice(typeof(NestedChoiceArgs))]
public partial interface INestedChoiceArgs
{
}

[Choice(typeof(NestedChoiceSubArgs1))]
[Choice(typeof(NestedChoiceSubArgs2))]
[Arguments(Prefix = "sub")]
public partial interface INestedChoiceSubArgs : INestedChoiceArgs
{
}

[Arguments(Prefix = "A")]
public partial record NestedChoiceArgs : INestedChoiceArgs;

[Arguments(Prefix = "B")]
public partial record NestedChoiceSubArgs1 : INestedChoiceSubArgs;

[Arguments(Prefix = "C")]
public partial record NestedChoiceSubArgs2 : INestedChoiceSubArgs;

[Choice(typeof(INestedChoiceWithMembersSubArgs))]
[Choice(typeof(NestedChoiceWithMembersArgs))]
public partial interface INestedChoiceWithMembersArgs
{
}

[Choice(typeof(NestedChoiceWithMembersSubArgs1))]
[Choice(typeof(NestedChoiceWithMembersSubArgs2))]
[Arguments(Prefix = "sub")]
public partial interface INestedChoiceWithMembersSubArgs : INestedChoiceWithMembersArgs
{
}

[Arguments(Prefix = "A")]
public partial record NestedChoiceWithMembersArgs : INestedChoiceWithMembersArgs
{
    [Argument]
    public string? Positional { get; init; }

    [Argument(Long = "direct")]
    public string? Named { get; init; }
}

[Arguments(Prefix = "B")]
public partial record NestedChoiceWithMembersSubArgs1 : INestedChoiceWithMembersSubArgs
{
    [Argument]
    public string? Positional { get; init; }

    [Argument(Long = "first")]
    public string? Named { get; init; }
}

[Arguments(Prefix = "C")]
public partial record NestedChoiceWithMembersSubArgs2 : INestedChoiceWithMembersSubArgs
{
    [Argument]
    public string? Positional { get; init; }

    [Argument(Long = "second")]
    public string? Named { get; init; }
}

[Choice(typeof(LongOptionChoiceAlphaArgs))]
[Choice(typeof(LongOptionChoiceBetaArgs))]
public partial interface ILongOptionChoiceArgs
{
}

[Arguments(Long = "alpha")]
public partial record LongOptionChoiceAlphaArgs : ILongOptionChoiceArgs
{
    [Argument]
    public string? Positional { get; init; }

    [Argument(Long = "alpha-name")]
    public string? Name { get; init; }
}

[Arguments(Long = "beta")]
public partial record LongOptionChoiceBetaArgs : ILongOptionChoiceArgs
{
    [Argument]
    public string? Positional { get; init; }

    [Argument(Long = "beta-name")]
    public string? Name { get; init; }
}

[Choice(typeof(ShortOptionChoiceAlphaArgs))]
[Choice(typeof(ShortOptionChoiceBetaArgs))]
public partial interface IShortOptionChoiceArgs
{
}

[Arguments(Short = 'a')]
public partial record ShortOptionChoiceAlphaArgs : IShortOptionChoiceArgs
{
    [Argument]
    public string? Positional { get; init; }

    [Argument(Short = 'x')]
    public string? Name { get; init; }
}

[Arguments(Short = 'b')]
public partial record ShortOptionChoiceBetaArgs : IShortOptionChoiceArgs
{
    [Argument]
    public string? Positional { get; init; }

    [Argument(Short = 'y')]
    public string? Name { get; init; }
}

[Choice(typeof(PrefixFieldArgs))]
public partial interface IPrefixFieldArgs
{
}

[Arguments(Prefix = "xxx")]
public partial record PrefixFieldArgs : IPrefixFieldArgs
{
    [Argument(Long = "field")]
    public string[] Field { get; init; } = System.Array.Empty<string>();
}
