#nullable enable
namespace Test;
partial record RangeOptions : global::Lokad.Cli.ICliParsed<global::Test.RangeOptions>
{
    public static global::Test.RangeOptions Parse(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        // Tokenization
        var tokenized = global::Lokad.Cli.ArgumentTokenizer.Tokenize(
            arguments,
            new char[] { 'r' },
            new string[] { @"ranges" },
            false);

        return ParseTokenizedCore(tokenized);
    }

    public static global::Test.RangeOptions ParseTokenized(global::Lokad.Cli.TokenizedArguments arguments)
    {
        if (arguments == null) throw new global::System.ArgumentNullException(nameof(arguments));

        return ParseTokenizedCore(arguments);
    }

    public static string Document(string commandName)
    {
        return DocumentCore(commandName);
    }

    private static global::Test.RangeOptions ParseTokenizedCore(global::Lokad.Cli.TokenizedArguments tokenized)
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
                        arg0Value = global::Lokad.Cli.Ranges.Parse(longArgWithValue.Value, null);
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
                        arg0Value = global::Lokad.Cli.Ranges.Parse(optionValue.Value, null);
                        arg0Seen = true;
                        break;
                    }
                    throw new global::Lokad.Cli.ParseException($"unexpected option `--{longArg.Name}`");
                case global::Lokad.Cli.ShortArgWithValue shortArgWithValue:
                    if (shortArgWithValue.Name == 'r')
                    {
                        arg0Value = global::Lokad.Cli.Ranges.Parse(shortArgWithValue.Value, null);
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
                            arg0Value = global::Lokad.Cli.Ranges.Parse(optionValue.Value, null);
                            arg0Seen = true;
                            continue;
                        }
                        throw new global::Lokad.Cli.ParseException($"unexpected option `-{shortOption}`");
                    }
                    break;
                case global::Lokad.Cli.NumericShortArg numericShortArg:
                    throw new global::Lokad.Cli.ParseException($"unexpected argument `-{numericShortArg.Value}`");
                case global::Lokad.Cli.ValueArg valueArg:
                    if (tokenized.MaybeSkipPositional(valueArg.Value))
                        break;
                    throw new global::Lokad.Cli.ParseException($"unexpected argument `{valueArg.Value}`");
            }
        }

        if (!arg0Seen)
            throw new global::Lokad.Cli.ParseException(@"missing option `--ranges`");

        return new global::Test.RangeOptions()
        {
            Ranges = arg0Value,
        };
    }

    private static string DocumentCore(string commandName)
    {
        if (commandName == null) throw new global::System.ArgumentNullException(nameof(commandName));

        var lines = new global::System.Collections.Generic.List<string> { FormatDocumentationLine(commandName, @"Ranges command.") };
        lines.Add(@"  --ranges, -r: Line ranges.");
        return string.Join(global::System.Environment.NewLine, lines);
    }

    private static string FormatDocumentationLine(string commandName, string documentation)
    {
        if (documentation.Length == 0)
            return commandName;

        return commandName + ": " + documentation;
    }
}
