#nullable enable
#pragma warning disable 0162, 0219
namespace Test;
partial record AddCommand : global::Lokad.Cli.ICliParsed<global::Test.AddCommand>
{
    public static global::Test.AddCommand Parse(string[] arguments)
    {
        // Tokenization
        var tokenized = global::Lokad.Cli.ArgumentTokenizer.Tokenize(
            arguments,
            new char[] { 'r', 'i' },
            new string[] { @"add", @"name" },
            false);

        return ParseTokenizedCore(tokenized);
    }

    public static global::Test.AddCommand ParseTokenized(global::Lokad.Cli.TokenizedArguments arguments)
    {
        if (arguments == null) throw new global::System.ArgumentNullException(nameof(arguments));

        return ParseTokenizedCore(arguments);
    }

    private static global::Test.AddCommand ParseTokenizedCore(global::Lokad.Cli.TokenizedArguments tokenized)
    {
        // Discrimination
        _ = tokenized;

        // Binding
        var arg0Seen = false;
        string? arg0Value = default;
        var positionalIndex = 0;
        global::Lokad.Cli.ITokenizedArguments? current;
        while ((current = tokenized.Shift()) != null)
        {
            switch (current)
            {
                case global::Lokad.Cli.LongArgWithValue longArgWithValue:
                    if (longArgWithValue.Name == @"name")
                    {
                        arg0Value = longArgWithValue.Value;
                        arg0Seen = true;
                        break;
                    }
                    if (longArgWithValue.Name == @"add")
                        break;
                    throw new global::Lokad.Cli.ParseException($"unexpected option `--{longArgWithValue.Name}`");
                    break;
                case global::Lokad.Cli.LongArg longArg:
                    if (longArg.Name == @"name")
                    {
                        var valueToken = tokenized.Shift();
                        if (!(valueToken is global::Lokad.Cli.ValueArg optionValue))
                            throw new global::Lokad.Cli.ParseException(@"option `--name` expects a value");
                        arg0Value = optionValue.Value;
                        arg0Seen = true;
                        break;
                    }
                    if (longArg.Name == @"add")
                        break;
                    throw new global::Lokad.Cli.ParseException($"unexpected option `--{longArg.Name}`");
                    break;
                case global::Lokad.Cli.ShortArgWithValue shortArgWithValue:
                    throw new global::Lokad.Cli.ParseException($"unexpected option `-{shortArgWithValue.Name}`");
                    break;
                case global::Lokad.Cli.ShortArgSet shortArgSet:
                    foreach (var shortOption in shortArgSet.Options)
                    {
                        throw new global::Lokad.Cli.ParseException($"unexpected option `-{shortOption}`");
                    }
                    break;
                case global::Lokad.Cli.NumericShortArg numericShortArg:
                    throw new global::Lokad.Cli.ParseException($"unexpected argument `-{numericShortArg.Value}`");
                    break;
                case global::Lokad.Cli.ValueArg valueArg:
                    if (tokenized.MaybeSkipPositional(valueArg.Value))
                        break;
                    throw new global::Lokad.Cli.ParseException($"unexpected argument `{valueArg.Value}`");
                    break;
            }
        }

        return new global::Test.AddCommand()
        {
            Name = arg0Seen ? arg0Value : default,
        };
    }

    private static int ParseInt32(string value, string label)
    {
        if (global::System.Int32.TryParse(value, global::System.Globalization.NumberStyles.Integer, global::System.Globalization.CultureInfo.InvariantCulture, out var parsed))
            return parsed;

        throw new global::Lokad.Cli.ParseException($"invalid value `{value}` for `{label}`");
    }

    private static uint ParseUInt32(string value, string label)
    {
        if (global::System.UInt32.TryParse(value, global::System.Globalization.NumberStyles.Integer, global::System.Globalization.CultureInfo.InvariantCulture, out var parsed))
            return parsed;

        throw new global::Lokad.Cli.ParseException($"invalid value `{value}` for `{label}`");
    }

    private static long ParseInt64(string value, string label)
    {
        if (global::System.Int64.TryParse(value, global::System.Globalization.NumberStyles.Integer, global::System.Globalization.CultureInfo.InvariantCulture, out var parsed))
            return parsed;

        throw new global::Lokad.Cli.ParseException($"invalid value `{value}` for `{label}`");
    }

    private static ulong ParseUInt64(string value, string label)
    {
        if (global::System.UInt64.TryParse(value, global::System.Globalization.NumberStyles.Integer, global::System.Globalization.CultureInfo.InvariantCulture, out var parsed))
            return parsed;

        throw new global::Lokad.Cli.ParseException($"invalid value `{value}` for `{label}`");
    }

    private static double ParseDouble(string value, string label)
    {
        if (global::System.Double.TryParse(value, global::System.Globalization.NumberStyles.Float | global::System.Globalization.NumberStyles.AllowThousands, global::System.Globalization.CultureInfo.InvariantCulture, out var parsed))
            return parsed;

        throw new global::Lokad.Cli.ParseException($"invalid value `{value}` for `{label}`");
    }

    private static global::System.DateTime ParseDateTime(string value, string label)
    {
        if (global::System.DateTime.TryParse(value, global::System.Globalization.CultureInfo.InvariantCulture, global::System.Globalization.DateTimeStyles.None, out var parsed))
            return parsed;

        throw new global::Lokad.Cli.ParseException($"invalid value `{value}` for `{label}`");
    }

    private static TEnum ParseEnum<TEnum>(string value, string label)
        where TEnum : struct, global::System.Enum
    {
        if (global::System.Enum.TryParse<TEnum>(value, ignoreCase: false, out var parsed))
            return parsed;

        throw new global::Lokad.Cli.ParseException($"invalid value `{value}` for `{label}`");
    }

}
