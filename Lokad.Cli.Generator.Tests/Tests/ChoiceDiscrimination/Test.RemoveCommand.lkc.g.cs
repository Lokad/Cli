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

    public static string Document(string commandName)
    {
        return DocumentCore(commandName);
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

    private static string DocumentCore(string commandName)
    {
        if (commandName == null) throw new global::System.ArgumentNullException(nameof(commandName));

        var lines = new global::System.Collections.Generic.List<string> { FormatDocumentationLine(commandName + @" -r", @"Remove command.") };
        lines.Add(@"  -i: Identifier to remove.");
        return string.Join(global::System.Environment.NewLine, lines);
    }

    private static string FormatDocumentationLine(string commandName, string documentation)
    {
        if (documentation.Length == 0)
            return commandName;

        return commandName + ": " + documentation;
    }

    private static int ParseInt32(string value, string label)
    {
        if (global::System.Int32.TryParse(value, global::System.Globalization.NumberStyles.Integer, global::System.Globalization.CultureInfo.InvariantCulture, out var parsed))
            return parsed;

        throw new global::Lokad.Cli.ParseException($"invalid value `{value}` for `{label}`");
    }

}
