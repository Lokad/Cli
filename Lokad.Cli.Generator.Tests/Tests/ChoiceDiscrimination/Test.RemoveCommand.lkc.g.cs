#nullable enable
namespace Test;
partial record RemoveCommand : global::Lokad.Cli.ICliParsed<global::Test.RemoveCommand>
{
    // A separate holder allows parsing from the argument type's static initializers.
    private static class OptionNames__Cli
    {
        public static readonly char[] Short = new char[] { 'r', 'i' };
        public static readonly string[] Long = new string[] { @"add", @"name" };
    }

    public static global::Test.RemoveCommand Parse(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        return ParseTokenizedCore__Cli(TokenizeCore__Cli(arguments));
    }

    public static global::Lokad.Cli.TokenizedArguments Tokenize(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        return TokenizeCore__Cli(arguments);
    }

    static bool global::Lokad.Cli.ICliParsed<global::Test.RemoveCommand>.IsHelpRequested(global::Lokad.Cli.TokenizedArguments arguments)
    {
        if (arguments == null) throw new global::System.ArgumentNullException(nameof(arguments));

        return false;
    }

    public static global::Test.RemoveCommand ParseTokenized(global::Lokad.Cli.TokenizedArguments arguments)
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
            false);
    }

    private static global::Test.RemoveCommand ParseTokenizedCore__Cli(global::Lokad.Cli.TokenizedArguments tokenized)
    {
        // Discrimination
        _ = tokenized;

        // Binding
        var arg0Seen = false;
        int arg0Value = default;
        global::Lokad.Cli.ITokenizedArguments? current;
        while ((current = tokenized.Shift()) != null)
        {
            switch (current)
            {
                case global::Lokad.Cli.LongArgWithValue longArgWithValue:
                    throw new global::Lokad.Cli.ParseException($"unexpected option `--{longArgWithValue.Name}`");
                case global::Lokad.Cli.LongArg longArg:
                    throw new global::Lokad.Cli.ParseException($"unexpected option `--{longArg.Name}`");
                case global::Lokad.Cli.ShortArgWithValue shortArgWithValue:
                    if (shortArgWithValue.Name == 'i')
                    {
                        arg0Value = ParseInt32__Cli(shortArgWithValue.Value, @"-i");
                        arg0Seen = true;
                        break;
                    }
                    throw new global::Lokad.Cli.ParseException($"unexpected option `-{shortArgWithValue.Name}`");
                case global::Lokad.Cli.ShortArgSet shortArgSet:
                    foreach (var shortOption in shortArgSet.Options)
                    {
                        if (shortOption == 'i')
                        {
                            var valueToken = tokenized.Shift();
                            if (!(valueToken is global::Lokad.Cli.ValueArg optionValue))
                                throw new global::Lokad.Cli.ParseException(@"option `-i` expects a value");
                            arg0Value = ParseInt32__Cli(optionValue.Value, @"-i");
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
                case global::Lokad.Cli.ValueArg:
                case global::Lokad.Cli.LiteralArg:
                    var positionalValue = current is global::Lokad.Cli.ValueArg valueArg ? valueArg.Value : ((global::Lokad.Cli.LiteralArg)current).Value;
                    if (current is global::Lokad.Cli.ValueArg && tokenized.MaybeSkipPositional(positionalValue))
                        break;
                    throw new global::Lokad.Cli.ParseException($"unexpected argument `{positionalValue}`");
            }
        }

        return new global::Test.RemoveCommand()
        {
            Id = arg0Seen ? arg0Value : default(global::System.Nullable<int>),
        };
    }

    private static string DocumentCore__Cli(string commandName)
    {
        if (commandName == null) throw new global::System.ArgumentNullException(nameof(commandName));

        var lines = new global::System.Collections.Generic.List<string> { FormatDocumentationLine__Cli(commandName + @" -r", @"Remove command.") };
        lines.Add(@"  -i: Identifier to remove.");
        return string.Join(global::System.Environment.NewLine, lines);
    }

    private static string FormatDocumentationLine__Cli(string commandName, string documentation)
    {
        if (documentation.Length == 0)
            return commandName;

        return commandName + ": " + documentation;
    }


    private static int ParseInt32__Cli(string value, string label)
    {
        if (TryParseInt32__Cli(value, out var parsed))
            return parsed;

        throw new global::Lokad.Cli.ParseException($"invalid value `{value}` for `{label}`");
    }

    private static bool TryParseInt32__Cli(string value, out int parsed)
    {
        return global::System.Int32.TryParse(value, global::System.Globalization.NumberStyles.Integer, global::System.Globalization.CultureInfo.InvariantCulture, out parsed);
    }

}
