using System;
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
