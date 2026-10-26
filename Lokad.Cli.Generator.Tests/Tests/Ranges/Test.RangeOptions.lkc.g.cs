#nullable enable
namespace Test;
partial record RangeOptions : global::Lokad.Cli.ICliParsed<global::Test.RangeOptions>
{
    // A separate holder allows parsing from the argument type's static initializers.
    private static class OptionNames__Cli
    {
        public static readonly global::System.Collections.Generic.IReadOnlyDictionary<char, global::Lokad.Cli.OptionValueMode> Short = new global::System.Collections.Generic.Dictionary<char, global::Lokad.Cli.OptionValueMode> { ['r'] = global::Lokad.Cli.OptionValueMode.None };
        public static readonly global::System.Collections.Generic.IReadOnlyDictionary<string, global::Lokad.Cli.OptionValueMode> Long = new global::System.Collections.Generic.Dictionary<string, global::Lokad.Cli.OptionValueMode> { [@"ranges"] = global::Lokad.Cli.OptionValueMode.None };
    }

    public static global::Test.RangeOptions Parse(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        return ParseTokenizedCore__Cli(TokenizeCore__Cli(arguments));
    }

    public static global::Lokad.Cli.TokenizedArguments Tokenize(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        return TokenizeCore__Cli(arguments);
    }

    static bool global::Lokad.Cli.ICliParsed<global::Test.RangeOptions>.IsHelpRequested(global::Lokad.Cli.TokenizedArguments arguments)
    {
        if (arguments == null) throw new global::System.ArgumentNullException(nameof(arguments));

        return false;
    }

    public static global::Test.RangeOptions ParseTokenized(global::Lokad.Cli.TokenizedArguments arguments)
    {
        if (arguments == null) throw new global::System.ArgumentNullException(nameof(arguments));

        return ParseTokenizedCore__Cli(arguments);
    }

    public static string Document(string commandName)
    {
        return DocumentCore__Cli(commandName);
    }

    private static global::Lokad.Cli.TokenizedArguments TokenizeCore__Cli(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        return global::Lokad.Cli.ArgumentTokenizer.Tokenize(
            arguments,
            OptionNames__Cli.Short,
            OptionNames__Cli.Long,
            false,
            global::Lokad.Cli.ShortOptionSyntax.GroupedValues,
            true);
    }

    private static global::Test.RangeOptions ParseTokenizedCore__Cli(global::Lokad.Cli.TokenizedArguments tokenized)
    {
        // Discrimination
        _ = tokenized;

        // Binding
        var arg0Seen = false;
        global::Lokad.Cli.Ranges arg0Value = default;
        global::Lokad.Cli.ITokenizedArguments? current;
        while ((current = tokenized.Shift()) != null)
        {
            switch (current)
            {
                case global::Lokad.Cli.LongArgWithValue longArgWithValue:
                    if (longArgWithValue.Name == @"ranges")
                    {
                        arg0Value = ParseCustom__Cli<global::Lokad.Cli.Ranges>(longArgWithValue.Value, @"--ranges");
                        arg0Seen = true;
                        break;
                    }
                    throw new global::Lokad.Cli.ParseException($"unexpected option `--{longArgWithValue.Name}`");
                case global::Lokad.Cli.LongArg longArg:
                    if (longArg.Name == @"ranges")
                    {
                        var valueToken = tokenized.Shift();
                        if (!(valueToken is global::Lokad.Cli.ValueArg optionValue))
                            throw new global::Lokad.Cli.ParseException(@"option `--ranges` expects a value");
                        arg0Value = ParseCustom__Cli<global::Lokad.Cli.Ranges>(optionValue.Value, @"--ranges");
                        arg0Seen = true;
                        break;
                    }
                    throw new global::Lokad.Cli.ParseException($"unexpected option `--{longArg.Name}`");
                case global::Lokad.Cli.ShortArgWithValue shortArgWithValue:
                    if (shortArgWithValue.Name == 'r')
                    {
                        arg0Value = ParseCustom__Cli<global::Lokad.Cli.Ranges>(shortArgWithValue.Value, @"-r");
                        arg0Seen = true;
                        break;
                    }
                    throw new global::Lokad.Cli.ParseException($"unexpected option `-{shortArgWithValue.Name}`");
                case global::Lokad.Cli.ShortArgSet shortArgSet:
                    foreach (var shortOption in shortArgSet.Options)
                    {
                        if (shortOption == 'r')
                        {
                            var valueToken = tokenized.Shift();
                            if (!(valueToken is global::Lokad.Cli.ValueArg optionValue))
                                throw new global::Lokad.Cli.ParseException(@"option `-r` expects a value");
                            arg0Value = ParseCustom__Cli<global::Lokad.Cli.Ranges>(optionValue.Value, @"-r");
                            arg0Seen = true;
                            continue;
                        }
                        throw new global::Lokad.Cli.ParseException($"unexpected option `-{shortOption}`");
                    }
                    break;
                case global::Lokad.Cli.NumericShortArg numericShortArg:
                    throw new global::Lokad.Cli.ParseException($"unexpected argument `-{numericShortArg.Value}`");
                case global::Lokad.Cli.ValueArg:
                case global::Lokad.Cli.LiteralArg:
                    var positionalValue = current is global::Lokad.Cli.ValueArg valueArg ? valueArg.Value : ((global::Lokad.Cli.LiteralArg)current).Value;
                    if (current is global::Lokad.Cli.ValueArg && tokenized.MaybeSkipPositional(positionalValue))
                        break;
                    throw new global::Lokad.Cli.ParseException($"unexpected argument `{positionalValue}`");
            }
        }

        if (!arg0Seen)
            throw new global::Lokad.Cli.ParseException(@"missing option `--ranges`");

        return new global::Test.RangeOptions()
        {
            Ranges = arg0Value,
        };
    }

    private static string DocumentCore__Cli(string commandName)
    {
        if (commandName == null) throw new global::System.ArgumentNullException(nameof(commandName));

        var lines = new global::System.Collections.Generic.List<string> { FormatDocumentationLine__Cli(commandName, @"Ranges command.") };
        lines.Add(@"  --ranges, -r: Line ranges.");
        return string.Join(global::System.Environment.NewLine, lines);
    }

    private static string FormatDocumentationLine__Cli(string commandName, string documentation)
    {
        if (documentation.Length == 0)
            return commandName;

        return commandName + ": " + documentation;
    }


    private static TValue ParseCustom__Cli<TValue>(string value, string label)
        where TValue : global::System.IParsable<TValue>
    {
        if (TryParseCustom__Cli<TValue>(value, out var parsed))
            return parsed;

        throw new global::Lokad.Cli.ParseException($"invalid value `{value}` for `{label}`");
    }

    private static bool TryParseCustom__Cli<TValue>(string value, [global::System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TValue parsed)
        where TValue : global::System.IParsable<TValue>
    {
        return TValue.TryParse(value, null, out parsed);
    }

}
