#nullable enable
namespace Test;
partial record ListCommand : global::Lokad.Cli.ICliParsed<global::Test.ListCommand>
{
    // A separate holder allows parsing from the argument type's static initializers.
    private static class __CliOptionNames
    {
        public static readonly char[] Short = new char[] { 'r', 'i' };
        public static readonly string[] Long = new string[] { @"add", @"name" };
    }

    public static global::Test.ListCommand Parse(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        return ParseTokenizedCore(TokenizeCore(arguments));
    }

    public static global::Lokad.Cli.TokenizedArguments Tokenize(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        return TokenizeCore(arguments);
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

    private static global::Lokad.Cli.TokenizedArguments TokenizeCore(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        return global::Lokad.Cli.ArgumentTokenizer.Tokenize(
            arguments,
            __CliOptionNames.Short,
            __CliOptionNames.Long,
            false);
    }

    private static global::Test.ListCommand ParseTokenizedCore(global::Lokad.Cli.TokenizedArguments tokenized)
    {
        tokenized.SkipPositional(@"list");

        // Discrimination
        _ = tokenized;

        // Binding
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
                    throw new global::Lokad.Cli.ParseException($"unexpected option `-{shortArgWithValue.Name}`");
                case global::Lokad.Cli.ShortArgSet shortArgSet:
                    foreach (var shortOption in shortArgSet.Options)
                    {
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
