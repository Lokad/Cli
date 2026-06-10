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
    [InlineData(new[] { "hello" }, "hello")]
    [InlineData(new[] { "hello", "world" }, "hello", "world")]
    [InlineData(new[] { "/alpha.txt", "/beta.txt" }, "/alpha.txt", "/beta.txt")]
    public void ParsesPositionalStringArray(string[] expected, params string[] arguments)
    {
        var parsed = Parser.Parse<PositionalArrayArgs>(arguments);

        Assert.Equal(expected, parsed.Input);
    }

    [Fact]
    public void ParsesFixedPositionalThenPositionalStringArray()
    {
        var parsed = Parser.Parse<PatternAndPathsArgs>(new[] { "world", "/file.txt", "/other.txt" });

        Assert.Equal("world", parsed.Pattern);
        Assert.Equal(new[] { "/file.txt", "/other.txt" }, parsed.Paths);
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

    public static TheoryData<string, string, string> DocumentationCases => new()
    {
        { nameof(BoolFlags), Parser.Document<BoolFlags>("cmd"), "cmd: Boolean flags command.\n  -a: Enables flag A.\n  -b: Enables flag B." },
        { nameof(StringArrayOptions), Parser.Document<StringArrayOptions>("cmd"), "cmd: String array options command.\n  --xval, -x: Adds an X value." },
        { nameof(PositionalArrayArgs), Parser.Document<PositionalArrayArgs>("cmd"), "cmd <input...>: Positional array command.\n  <input...>: Input values." },
        { nameof(MyArgs), Parser.Document<MyArgs>("cmd"), "cmd: Nullable depth command.\n  --max-depth, -d: Maximum depth for recursive traversal." },
        { nameof(ShortNumericArgs), Parser.Document<ShortNumericArgs>("cmd"), "cmd: Numeric short command.\n  -<number>: Numeric value." },
        { nameof(LetterAndDigitBoolFlags), Parser.Document<LetterAndDigitBoolFlags>("cmd"), "cmd: Letter and digit flags command.\n  -x: Enables X.\n  -4: Enables four." },
        { nameof(INestedChoiceArgs), Parser.Document<INestedChoiceArgs>("cmd"), "cmd sub: Nested sub-command group.\ncmd sub B: First nested sub-choice.\ncmd sub C: Second nested sub-choice.\ncmd A: Direct nested choice." },
        { nameof(INestedChoiceSubArgs), Parser.Document<INestedChoiceSubArgs>("cmd"), "cmd sub: Nested sub-command group.\ncmd sub B: First nested sub-choice.\ncmd sub C: Second nested sub-choice." },
        { nameof(NestedChoiceArgs), Parser.Document<NestedChoiceArgs>("cmd"), "cmd A: Direct nested choice." },
        { nameof(NestedChoiceSubArgs1), Parser.Document<NestedChoiceSubArgs1>("cmd"), "cmd B: First nested sub-choice." },
        { nameof(NestedChoiceSubArgs2), Parser.Document<NestedChoiceSubArgs2>("cmd"), "cmd C: Second nested sub-choice." },
        { nameof(INestedChoiceWithMembersArgs), Parser.Document<INestedChoiceWithMembersArgs>("cmd"), "cmd sub: Nested member sub-command group.\ncmd sub B <first-pos?>: First sub-choice with members.\n  <first-pos?>: First positional value.\n  --first: First named value.\ncmd sub C <second-pos?>: Second sub-choice with members.\n  <second-pos?>: Second positional value.\n  --second: Second named value.\ncmd A <direct-pos?>: Direct choice with members.\n  <direct-pos?>: Direct positional value.\n  --direct: Direct named value." },
        { nameof(INestedChoiceWithMembersSubArgs), Parser.Document<INestedChoiceWithMembersSubArgs>("cmd"), "cmd sub: Nested member sub-command group.\ncmd sub B <first-pos?>: First sub-choice with members.\n  <first-pos?>: First positional value.\n  --first: First named value.\ncmd sub C <second-pos?>: Second sub-choice with members.\n  <second-pos?>: Second positional value.\n  --second: Second named value." },
        { nameof(NestedChoiceWithMembersArgs), Parser.Document<NestedChoiceWithMembersArgs>("cmd"), "cmd A <direct-pos?>: Direct choice with members.\n  <direct-pos?>: Direct positional value.\n  --direct: Direct named value." },
        { nameof(NestedChoiceWithMembersSubArgs1), Parser.Document<NestedChoiceWithMembersSubArgs1>("cmd"), "cmd B <first-pos?>: First sub-choice with members.\n  <first-pos?>: First positional value.\n  --first: First named value." },
        { nameof(NestedChoiceWithMembersSubArgs2), Parser.Document<NestedChoiceWithMembersSubArgs2>("cmd"), "cmd C <second-pos?>: Second sub-choice with members.\n  <second-pos?>: Second positional value.\n  --second: Second named value." },
        { nameof(ILongOptionChoiceArgs), Parser.Document<ILongOptionChoiceArgs>("cmd"), "cmd --alpha <alpha-pos?>: Alpha command.\n  <alpha-pos?>: Alpha positional value.\n  --alpha-name: Alpha name.\ncmd --beta <beta-pos?>: Beta command.\n  <beta-pos?>: Beta positional value.\n  --beta-name: Beta name." },
        { nameof(LongOptionChoiceAlphaArgs), Parser.Document<LongOptionChoiceAlphaArgs>("cmd"), "cmd --alpha <alpha-pos?>: Alpha command.\n  <alpha-pos?>: Alpha positional value.\n  --alpha-name: Alpha name." },
        { nameof(LongOptionChoiceBetaArgs), Parser.Document<LongOptionChoiceBetaArgs>("cmd"), "cmd --beta <beta-pos?>: Beta command.\n  <beta-pos?>: Beta positional value.\n  --beta-name: Beta name." },
        { nameof(IShortOptionChoiceArgs), Parser.Document<IShortOptionChoiceArgs>("cmd"), "cmd -a <short-alpha-pos?>: Short alpha command.\n  <short-alpha-pos?>: Short alpha positional value.\n  -x: Short alpha name.\ncmd -b <short-beta-pos?>: Short beta command.\n  <short-beta-pos?>: Short beta positional value.\n  -y: Short beta name." },
        { nameof(ShortOptionChoiceAlphaArgs), Parser.Document<ShortOptionChoiceAlphaArgs>("cmd"), "cmd -a <short-alpha-pos?>: Short alpha command.\n  <short-alpha-pos?>: Short alpha positional value.\n  -x: Short alpha name." },
        { nameof(ShortOptionChoiceBetaArgs), Parser.Document<ShortOptionChoiceBetaArgs>("cmd"), "cmd -b <short-beta-pos?>: Short beta command.\n  <short-beta-pos?>: Short beta positional value.\n  -y: Short beta name." },
        { nameof(IPrefixFieldArgs), Parser.Document<IPrefixFieldArgs>("cmd"), "cmd xxx: Prefix field command.\n  --field: Field value." },
        { nameof(PrefixFieldArgs), Parser.Document<PrefixFieldArgs>("cmd"), "cmd xxx: Prefix field command.\n  --field: Field value." },
    };

    [Theory]
    [MemberData(nameof(DocumentationCases))]
    public void DocumentsExistingEndToEndTypes(string typeName, string actual, string expected)
    {
        Assert.NotEmpty(typeName);
        Assert.Equal(expected.Replace("\n", System.Environment.NewLine), actual);
    }
}

[Arguments("Boolean flags command.")]
public partial record BoolFlags
{
    [Argument("Enables flag A.", Short = 'a')]
    public bool A { get; init; }

    [Argument("Enables flag B.", Short = 'b')]
    public bool B { get; init; }
}

[Arguments("String array options command.")]
public partial record StringArrayOptions
{
    [Argument("Adds an X value.", Short = 'x', Long = "xval")]
    public string[] X { get; init; } = System.Array.Empty<string>();
}

[Arguments("Positional array command.")]
public partial record PositionalArrayArgs
{
    [Argument("Input values.", Name = "input")]
    public string[] Input { get; init; } = System.Array.Empty<string>();
}

[Arguments("Pattern and paths command.")]
public partial record PatternAndPathsArgs
{
    [Argument("Pattern value.", Name = "pattern")]
    public string Pattern { get; init; } = "";

    [Argument("Path values.", Name = "path")]
    public string[] Paths { get; init; } = System.Array.Empty<string>();
}

[Arguments("Nullable depth command.")]
public partial record MyArgs
{
    [Argument(
        "Maximum depth for recursive traversal.",
        Short = 'd',
        Long = "max-depth")]
    public int? MaxDepth { get; init; }
}

[Arguments("Numeric short command.")]
public partial record ShortNumericArgs
{
    [Argument("Numeric value.", ShortNumeric = true)]
    public int Value { get; init; }
}

[Arguments("Letter and digit flags command.")]
public partial record LetterAndDigitBoolFlags
{
    [Argument("Enables X.", Short = 'x')]
    public bool X { get; init; }

    [Argument("Enables four.", Short = '4')]
    public bool Four { get; init; }
}

[Choice(typeof(INestedChoiceSubArgs))]
[Choice(typeof(NestedChoiceArgs))]
public partial interface INestedChoiceArgs
{
}

[Choice(typeof(NestedChoiceSubArgs1))]
[Choice(typeof(NestedChoiceSubArgs2))]
[Arguments("Nested sub-command group.", Prefix = "sub")]
public partial interface INestedChoiceSubArgs : INestedChoiceArgs
{
}

[Arguments("Direct nested choice.", Prefix = "A")]
public partial record NestedChoiceArgs : INestedChoiceArgs;

[Arguments("First nested sub-choice.", Prefix = "B")]
public partial record NestedChoiceSubArgs1 : INestedChoiceSubArgs;

[Arguments("Second nested sub-choice.", Prefix = "C")]
public partial record NestedChoiceSubArgs2 : INestedChoiceSubArgs;

[Choice(typeof(INestedChoiceWithMembersSubArgs))]
[Choice(typeof(NestedChoiceWithMembersArgs))]
public partial interface INestedChoiceWithMembersArgs
{
}

[Choice(typeof(NestedChoiceWithMembersSubArgs1))]
[Choice(typeof(NestedChoiceWithMembersSubArgs2))]
[Arguments("Nested member sub-command group.", Prefix = "sub")]
public partial interface INestedChoiceWithMembersSubArgs : INestedChoiceWithMembersArgs
{
}

[Arguments("Direct choice with members.", Prefix = "A")]
public partial record NestedChoiceWithMembersArgs : INestedChoiceWithMembersArgs
{
    [Argument("Direct positional value.", Name = "direct-pos")]
    public string? Positional { get; init; }

    [Argument("Direct named value.", Long = "direct")]
    public string? Named { get; init; }
}

[Arguments("First sub-choice with members.", Prefix = "B")]
public partial record NestedChoiceWithMembersSubArgs1 : INestedChoiceWithMembersSubArgs
{
    [Argument("First positional value.", Name = "first-pos")]
    public string? Positional { get; init; }

    [Argument("First named value.", Long = "first")]
    public string? Named { get; init; }
}

[Arguments("Second sub-choice with members.", Prefix = "C")]
public partial record NestedChoiceWithMembersSubArgs2 : INestedChoiceWithMembersSubArgs
{
    [Argument("Second positional value.", Name = "second-pos")]
    public string? Positional { get; init; }

    [Argument("Second named value.", Long = "second")]
    public string? Named { get; init; }
}

[Choice(typeof(LongOptionChoiceAlphaArgs))]
[Choice(typeof(LongOptionChoiceBetaArgs))]
public partial interface ILongOptionChoiceArgs
{
}

[Arguments("Alpha command.", Long = "alpha")]
public partial record LongOptionChoiceAlphaArgs : ILongOptionChoiceArgs
{
    [Argument("Alpha positional value.", Name = "alpha-pos")]
    public string? Positional { get; init; }

    [Argument("Alpha name.", Long = "alpha-name")]
    public string? Name { get; init; }
}

[Arguments("Beta command.", Long = "beta")]
public partial record LongOptionChoiceBetaArgs : ILongOptionChoiceArgs
{
    [Argument("Beta positional value.", Name = "beta-pos")]
    public string? Positional { get; init; }

    [Argument("Beta name.", Long = "beta-name")]
    public string? Name { get; init; }
}

[Choice(typeof(ShortOptionChoiceAlphaArgs))]
[Choice(typeof(ShortOptionChoiceBetaArgs))]
public partial interface IShortOptionChoiceArgs
{
}

[Arguments("Short alpha command.", Short = 'a')]
public partial record ShortOptionChoiceAlphaArgs : IShortOptionChoiceArgs
{
    [Argument("Short alpha positional value.", Name = "short-alpha-pos")]
    public string? Positional { get; init; }

    [Argument("Short alpha name.", Short = 'x')]
    public string? Name { get; init; }
}

[Arguments("Short beta command.", Short = 'b')]
public partial record ShortOptionChoiceBetaArgs : IShortOptionChoiceArgs
{
    [Argument("Short beta positional value.", Name = "short-beta-pos")]
    public string? Positional { get; init; }

    [Argument("Short beta name.", Short = 'y')]
    public string? Name { get; init; }
}

[Choice(typeof(PrefixFieldArgs))]
public partial interface IPrefixFieldArgs
{
}

[Arguments("Prefix field command.", Prefix = "xxx")]
public partial record PrefixFieldArgs : IPrefixFieldArgs
{
    [Argument("Field value.", Long = "field")]
    public string[] Field { get; init; } = System.Array.Empty<string>();
}
