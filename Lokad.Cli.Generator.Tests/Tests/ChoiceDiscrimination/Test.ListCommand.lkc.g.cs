#nullable enable
#pragma warning disable 0162, 0219
namespace Test;
partial record ListCommand : global::Lokad.Cli.ICliParsed<global::Test.ListCommand>
{
    public static global::Test.ListCommand Parse(string[] arguments)
    {
        // Tokenization
        var tokenized = global::Lokad.Cli.ArgumentTokenizer.Tokenize(
            arguments,
            new char[] { 'r', 'i' },
            new string[] { @"add", @"name" },
            false);

        return ParseTokenizedCore(tokenized);
    }

    public static global::Test.ListCommand ParseTokenized(global::Lokad.Cli.TokenizedArguments arguments)
    {
        if (arguments == null) throw new global::System.ArgumentNullException(nameof(arguments));

        return ParseTokenizedCore(arguments);
    }

    public static string Document(string commandName)
    {
        return DocumentCore(commandName);
    }

    private static global::Test.ListCommand ParseTokenizedCore(global::Lokad.Cli.TokenizedArguments tokenized)
    {
        tokenized.SkipPositional(@"list");

        // Discrimination
        _ = tokenized;

        // Binding
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

        return new global::Test.ListCommand();
    }

    private static string DocumentCore(string commandName)
    {
        if (commandName == null) throw new global::System.ArgumentNullException(nameof(commandName));

        var lines = new global::System.Collections.Generic.List<string> { FormatDocumentationLine(commandName + @" list", @"List command.") };
        return string.Join(global::System.Environment.NewLine, lines);
    }

    private static string FormatDocumentationLine(string commandName, string documentation)
    {
        if (documentation.Length == 0)
            return commandName;

        return commandName + ": " + documentation;
    }
}
