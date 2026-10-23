using System;
using System.Collections.Generic;
using Lokad.Cli;

namespace Test;

[Arguments("Tokenization mode.", Long = "mode", Short = 'm')]
public partial record TokenizationOptions(
    [property: Argument("Constructor property value.", Long = "from-ctor", ShortNames = new[] { 'c' })]
    SnapshotReferenceValue<string?> FromConstructor,
    [Argument("Raw constructor parameter.", LongNames = new[] { "raw-param" }, Short = 'p')]
    ReadOnlyMemory<int> RawParameter)
{
    [Argument("Enable verbose output.", AllowRepeated = true,
        LongNames = new[] { "verbose", "chatty", "verbose", "chatty" }, ShortNames = new[] { 'v', 'V', 'v', 'V' })]
    public bool Verbose { get; init; }

    [Argument("Numeric depth.", ShortNumeric = true)]
    public int? Depth { get; init; }

    [Argument("Field value.", LongNames = new[] { "field" }, ShortNames = new[] { 'f' })]
    public IReadOnlyList<string> Field = Array.Empty<string>();

    [Argument("Optional reference value.", Long = "optional")]
    public SnapshotReferenceValue<string?>? Optional { get; init; }

    [Argument("Numbers.", Name = "number", ConsumeUntilBoundary = true)]
    public int[] Numbers { get; init; } = [];

    [Argument("Label.", Name = "label")]
    public string Label { get; init; } = "";
}

public sealed record SnapshotReferenceValue<T>(string Value) : IParsable<SnapshotReferenceValue<T>>
{
    public static SnapshotReferenceValue<T> Parse(string value, IFormatProvider? provider) =>
        throw new InvalidOperationException("Parse must not be called.");

    public static bool TryParse(string? value, IFormatProvider? provider, out SnapshotReferenceValue<T> result)
    {
        result = new SnapshotReferenceValue<T>(value ?? string.Empty);
        return !string.IsNullOrEmpty(value);
    }
}
