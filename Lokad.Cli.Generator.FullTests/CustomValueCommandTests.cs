using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class CustomValueCommandTests
{
    [Fact]
    public void ParsesCustomValuesThroughTryParse()
    {
        var parsed = Parser.Parse<CustomValueArgs>(new[]
        {
            "--id", "42", "--many", "1", "--many=2", "9"
        });

        Assert.Equal(42u, parsed.Id?.Value);
        Assert.Equal(new[] { new UnsignedId(1), new UnsignedId(2) }, parsed.Many);
        Assert.Equal(9u, parsed.Positional?.Value);
    }

    [Theory]
    [InlineData("--id=42")]
    [InlineData("-i42")]
    [InlineData("-i=42")]
    public void ParsesRequiredInlineCustomValue(string argument)
    {
        var parsed = Parser.Parse<RequiredCustomValueArgs>(new[] { argument });

        Assert.Equal(42u, parsed.Id.Value);
    }

    public static TheoryData<string[], string> InvalidValueCases => new()
    {
        { new[] { "--id", "item-42" }, "invalid value `item-42` for `--id`" },
        { new[] { "--id=4294967296" }, "invalid value `4294967296` for `--id`" },
        { new[] { "-i", "bad" }, "invalid value `bad` for `-i`" },
        { new[] { "bad" }, "invalid value `bad` for `argument`" },
        { new[] { "--many", "1", "--many=bad" }, "invalid value `bad` for `--many`" },
        { new[] { "--id=" }, "invalid value `` for `--id`" },
        { new[] { "--id=-1" }, "invalid value `-1` for `--id`" }
    };

    [Theory]
    [MemberData(nameof(InvalidValueCases))]
    public void ReportsInvalidCustomValuesAsParseExceptions(string[] arguments, string expectedMessage)
    {
        var error = Assert.Throws<ParseException>(() => Parser.Parse<CustomValueArgs>(arguments));

        Assert.Equal(expectedMessage, error.Message);
    }

    [Fact]
    public void LeavesAbsentCustomValuesUnbound()
    {
        var parsed = Parser.Parse<CustomValueArgs>(Array.Empty<string>());

        Assert.Null(parsed.Id);
        Assert.Null(parsed.Positional);
        Assert.Empty(parsed.Many);
    }

    [Theory]
    [InlineData("--id")]
    [InlineData("-i")]
    [InlineData("--many")]
    public void ReportsMissingCustomOptionValues(string option)
    {
        var error = Assert.Throws<ParseException>(() => Parser.Parse<CustomValueArgs>(new[] { option }));

        Assert.Equal($"option `{option}` expects a value", error.Message);
    }

    [Fact]
    public void RequiresNonNullableCustomValue()
    {
        var error = Assert.Throws<ParseException>(() =>
            Parser.Parse<RequiredCustomValueArgs>(Array.Empty<string>()));

        Assert.Equal("missing option `--id`", error.Message);
    }

    [Fact]
    public void PreservesExceptionsThrownByCustomTryParse()
    {
        var error = Assert.Throws<FormatException>(() =>
            Parser.Parse<CustomValueArgs>(new[] { "--id=throw" }));

        Assert.Equal("Custom TryParse failure.", error.Message);
    }

    [Theory]
    [InlineData("--value=alpha")]
    [InlineData("--value", "alpha")]
    [InlineData("--label=alpha")]
    [InlineData("-valpha")]
    [InlineData("-v=alpha")]
    [InlineData("-V", "alpha")]
    public void BindsRequiredAndOptionalReferenceConstructors(params string[] arguments)
    {
        var optional = Parser.Parse<CustomReferenceValueArgs>(arguments);
        var requiredArguments = new List<string>(arguments) { "beta", "--field=gamma" };
        var required = Parser.Parse<RequiredReferenceValueArgs>(requiredArguments);

        Assert.Equal("alpha", optional.Value?.Value);
        Assert.Equal("alpha", required.Value.Value);
        Assert.Equal("beta", required.Input.Value);
        Assert.Equal("gamma", required.Field.Value);
    }

    [Fact]
    public void BindsReferenceFieldsPropertiesAndCollections()
    {
        var parsed = Parser.Parse<CustomReferenceValueArgs>(new[]
        {
            "--value=alpha", "--other=gamma", "--many=one", "--many=two", "beta"
        });

        Assert.Equal("alpha", parsed.Value?.Value);
        Assert.Equal("beta", parsed.Input?.Value);
        Assert.Equal("gamma", parsed.Other?.Value);
        Assert.Equal(new[] { new LabelValue("one"), new LabelValue("two") }, parsed.Many);
    }

    [Fact]
    public void LeavesAbsentReferenceValuesNull()
    {
        var parsed = Parser.Parse<CustomReferenceValueArgs>(Array.Empty<string>());

        Assert.Null(parsed.Value);
        Assert.Null(parsed.Input);
        Assert.Null(parsed.Other);
        Assert.Empty(parsed.Many);
    }

    [Theory]
    [InlineData("missing option `--value`")]
    [InlineData("missing argument", "--value=alpha")]
    [InlineData("missing option `--field`", "--value=alpha", "beta")]
    [InlineData("option `--value` expects a value", "--value")]
    [InlineData("option `--value` expects a value", "--label", "--field=gamma")]
    public void ReportsMissingRequiredReferences(string expectedMessage, params string[] arguments)
    {
        var error = Assert.Throws<ParseException>(() => Parser.Parse<RequiredReferenceValueArgs>(arguments));

        Assert.Equal(expectedMessage, error.Message);
    }

    [Theory]
    [InlineData("invalid value `bad` for `--value`", "--label=bad")]
    [InlineData("invalid value `bad` for `--value`", "-V", "bad")]
    [InlineData("invalid value `` for `--value`", "--value=")]
    [InlineData("invalid value `bad` for `argument`", "bad")]
    [InlineData("invalid value `bad` for `--other`", "--other=bad")]
    [InlineData("invalid value `bad` for `--many`", "--many=bad")]
    public void ReportsInvalidReferenceValues(string expectedMessage, params string[] arguments)
    {
        var error = Assert.Throws<ParseException>(() => Parser.Parse<CustomReferenceValueArgs>(arguments));

        Assert.Equal(expectedMessage, error.Message);
    }

    [Fact]
    public void PreservesReferenceScalarOverwriteAndTokenizedBinding()
    {
        var tokens = Parser.Tokenize<CustomReferenceValueArgs>(new[] { "--value=alpha", "--label=beta" });

        Assert.Equal("beta", Parser.ParseTokenized<CustomReferenceValueArgs>(tokens).Value?.Value);
    }

    [Fact]
    public void PreservesExceptionsThrownByReferenceTryParse()
    {
        var error = Assert.Throws<FormatException>(() =>
            Parser.Parse<CustomReferenceValueArgs>(new[] { "--value=throw" }));

        Assert.Equal("Custom reference TryParse failure.", error.Message);
    }

    [Fact]
    public void DocumentsReferencePresenceLikeValueTypePresence()
    {
        Assert.Contains("<input?>", Parser.Document<CustomReferenceValueArgs>("cmd"));
        Assert.Contains("<input>", Parser.Document<RequiredReferenceValueArgs>("cmd"));
    }

    [Fact]
    public void PreservesRequiredEmptyStringValues()
    {
        var parsed = Parser.Parse<PatternAndPathsArgs>(new[] { string.Empty });

        Assert.Equal(string.Empty, parsed.Pattern);
        Assert.Empty(parsed.Paths);
    }
}

[Arguments("Custom value command.")]
public partial record CustomValueArgs
{
    [Argument("Identifier.", Long = "id", Short = 'i')]
    public UnsignedId? Id { get; init; }

    [Argument("Repeated identifiers.", Long = "many")]
    public UnsignedId[] Many { get; init; } = [];

    [Argument("Positional identifier.")]
    public UnsignedId? Positional { get; init; }
}

[Arguments("Required custom value command.")]
public partial record RequiredCustomValueArgs
{
    [Argument("Identifier.", Long = "id", Short = 'i')]
    public UnsignedId Id { get; init; }
}

public readonly record struct UnsignedId(uint Value) : IParsable<UnsignedId>
{
    public static UnsignedId Parse(string value, IFormatProvider? provider) =>
        throw new InvalidOperationException("Parse must not be called.");

    public static bool TryParse(string? value, IFormatProvider? provider, out UnsignedId result)
    {
        Assert.Null(provider);
        if (value == "throw")
            throw new FormatException("Custom TryParse failure.");

        var success = uint.TryParse(value, NumberStyles.None, provider, out var parsed);
        result = new UnsignedId(parsed);
        return success;
    }
}

[Arguments("Custom reference value command.")]
public partial record CustomReferenceValueArgs(
    [property: Argument("Value.", LongNames = new[] { "value", "label" }, ShortNames = new[] { 'v', 'V' })] LabelValue? Value)
{
    [Argument("Positional value.", Name = "input")]
    public LabelValue? Input;

    [Argument("Other value.", Long = "other")]
    public GenericLabelValue<string?>? Other { get; init; }

    [Argument("Repeated values.", Long = "many")]
    public IReadOnlyList<LabelValue> Many { get; init; } = Array.Empty<LabelValue>();
}

[Arguments("Required reference value command.")]
public partial record RequiredReferenceValueArgs(
    [Argument("Value.", LongNames = new[] { "value", "label" }, ShortNames = new[] { 'v', 'V' })] LabelValue Value)
{
    [Argument("Positional value.", Name = "input")]
    public LabelValue Input { get; init; } = new("initial");

    [Argument("Field value.", Long = "field")]
    public LabelValue Field = new("initial");
}

public sealed record LabelValue(string Value) : IParsable<LabelValue>, IReadOnlyList<char>
{
    public int Count => Value.Length;

    public char this[int index] => Value[index];

    public IEnumerator<char> GetEnumerator() => ((IEnumerable<char>)Value).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static LabelValue Parse(string value, IFormatProvider? provider) =>
        throw new InvalidOperationException("Parse must not be called.");

    public static bool TryParse(string? value, IFormatProvider? provider, out LabelValue result)
    {
        Assert.Null(provider);
        if (value == "throw")
            throw new FormatException("Custom reference TryParse failure.");

        result = new LabelValue(value ?? string.Empty);
        return !string.IsNullOrEmpty(value) && value != "bad";
    }
}

public sealed record GenericLabelValue<T>(string Value) : IParsable<GenericLabelValue<T>>
{
    public static GenericLabelValue<T> Parse(string value, IFormatProvider? provider) =>
        throw new InvalidOperationException("Parse must not be called.");

    public static bool TryParse(string? value, IFormatProvider? provider, out GenericLabelValue<T> result)
    {
        var success = LabelValue.TryParse(value, provider, out var parsed);
        result = new GenericLabelValue<T>(parsed.Value);
        return success;
    }
}
