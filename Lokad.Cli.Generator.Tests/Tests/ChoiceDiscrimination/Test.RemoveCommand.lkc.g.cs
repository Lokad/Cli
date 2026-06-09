#nullable enable
#pragma warning disable 0162, 0219
namespace Test;
partial record RemoveCommand : global::Lokad.Cli.ICliParsed<global::Test.RemoveCommand>
{
    public static global::Test.RemoveCommand Parse(string[] arguments)
    {
        // Tokenization
        var tokenized = global::Lokad.Cli.ArgumentTokenizer.Tokenize(
            arguments,
            new char[] { 'r', 'i' },
            new string[] { @"add", @"name" },
            false);

        return ParseTokenizedCore(tokenized);
    }

    public static global::Test.RemoveCommand ParseTokenized(global::Lokad.Cli.TokenizedArguments arguments)
    {
        if (arguments == null) throw new global::System.ArgumentNullException(nameof(arguments));

        return ParseTokenizedCore(arguments);
    }

    private static global::Test.RemoveCommand ParseTokenizedCore(global::Lokad.Cli.TokenizedArguments tokenized)
    {
        // Discrimination
        _ = tokenized;

        // Binding
        var arg0Seen = false;
        int arg0Value = default;
        var positionalIndex = 0;
        global::Lokad.Cli.ITokenizedArguments? current;
        while ((current = tokenized.Shift()) != null)
        {
            switch (current)
            {
                case global::Lokad.Cli.LongArgWithValue longArgWithValue:
                    throw new global::Lokad.Cli.ParseException($"unexpected option `--{longArgWithValue.Name}`");
                    break;
                case global::Lokad.Cli.LongArg longArg:
                    throw new global::Lokad.Cli.ParseException($"unexpected option `--{longArg.Name}`");
                    break;
                case global::Lokad.Cli.ShortArgWithValue shortArgWithValue:
                    if (shortArgWithValue.Name == 'i')
                    {
                        arg0Value = ParseInt32(shortArgWithValue.Value, @"-i");
                        arg0Seen = true;
                        break;
                    }
                    if (shortArgWithValue.Name == 'r')
                        break;
                    throw new global::Lokad.Cli.ParseException($"unexpected option `-{shortArgWithValue.Name}`");
                    break;
                case global::Lokad.Cli.ShortArgSet shortArgSet:
                    foreach (var shortOption in shortArgSet.Options)
                    {
                        if (shortOption == 'i')
                        {
                            var valueToken = tokenized.Shift();
                            if (!(valueToken is global::Lokad.Cli.ValueArg optionValue))
                                throw new global::Lokad.Cli.ParseException(@"option `-i` expects a value");
                            arg0Value = ParseInt32(optionValue.Value, @"-i");
                            arg0Seen = true;
                            continue;
                        }
                        if (shortOption == 'r')
                            continue;
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

        return new global::Test.RemoveCommand()
        {
            Id = arg0Seen ? arg0Value : default(global::System.Nullable<int>),
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
