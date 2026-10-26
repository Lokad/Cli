#nullable enable
namespace Test;
partial record AddCommand : global::Lokad.Cli.ICliParsed<global::Test.AddCommand>
{
    // A separate holder allows parsing from the argument type's static initializers.
    private static class OptionNames__Cli
    {
        public static readonly global::System.Collections.Generic.IReadOnlyDictionary<char, global::Lokad.Cli.OptionValueMode> Short = new global::System.Collections.Generic.Dictionary<char, global::Lokad.Cli.OptionValueMode> {  };
        public static readonly global::System.Collections.Generic.IReadOnlyDictionary<string, global::Lokad.Cli.OptionValueMode> Long = new global::System.Collections.Generic.Dictionary<string, global::Lokad.Cli.OptionValueMode> { [@"name"] = global::Lokad.Cli.OptionValueMode.None, [@"add"] = global::Lokad.Cli.OptionValueMode.None };
    }

    public static global::Test.AddCommand Parse(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        return ParseTokenizedCore__Cli(TokenizeCore__Cli(arguments));
    }

    public static global::Lokad.Cli.TokenizedArguments Tokenize(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        return TokenizeCore__Cli(arguments);
    }

    static bool global::Lokad.Cli.ICliParsed<global::Test.AddCommand>.IsHelpRequested(global::Lokad.Cli.TokenizedArguments arguments)
    {
        if (arguments == null) throw new global::System.ArgumentNullException(nameof(arguments));

        return false;
    }

    public static global::Test.AddCommand ParseTokenized(global::Lokad.Cli.TokenizedArguments arguments)
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

    private static global::Test.AddCommand ParseTokenizedCore__Cli(global::Lokad.Cli.TokenizedArguments tokenized)
    {
        tokenized.SelectDiscriminator(@"add", '\0');

        // Discrimination
        _ = tokenized;

        // Binding
        string? arg0Value = default;
        global::Lokad.Cli.ITokenizedArguments? current;
        while ((current = tokenized.Shift()) != null)
        {
            switch (current)
            {
                case global::Lokad.Cli.LongArgWithValue longArgWithValue:
                    if (longArgWithValue.Name == @"name")
                    {
                        arg0Value = longArgWithValue.Value;
                        break;
                    }
                    throw new global::Lokad.Cli.ParseException($"unexpected option `--{longArgWithValue.Name}`");
                case global::Lokad.Cli.LongArg longArg:
                    if (longArg.Name == @"name")
                    {
                        var valueToken = tokenized.Shift();
                        if (!(valueToken is global::Lokad.Cli.ValueArg optionValue))
                            throw new global::Lokad.Cli.ParseException(@"option `--name` expects a value");
                        arg0Value = optionValue.Value;
                        break;
                    }
                    if (tokenized.IsSelectedLongOption(longArg.Name))
                        break;
                    throw new global::Lokad.Cli.ParseException($"unexpected option `--{longArg.Name}`");
                case global::Lokad.Cli.ShortArgWithValue shortArgWithValue:
                    throw new global::Lokad.Cli.ParseException($"unexpected option `-{shortArgWithValue.Name}`");
                case global::Lokad.Cli.ShortArgSet shortArgSet:
                    foreach (var shortOption in shortArgSet.Options)
                    {
                        if (tokenized.IsSelectedShortOption(shortOption))
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

        return new global::Test.AddCommand()
        {
            Name = arg0Value,
        };
    }

    private static string DocumentCore__Cli(string commandName)
    {
        if (commandName == null) throw new global::System.ArgumentNullException(nameof(commandName));

        var lines = new global::System.Collections.Generic.List<string> { FormatDocumentationLine__Cli(commandName + @" --add", @"Add command.") };
        lines.Add(@"  --name: Name to add.");
        return string.Join(global::System.Environment.NewLine, lines);
    }

    private static string FormatDocumentationLine__Cli(string commandName, string documentation)
    {
        if (documentation.Length == 0)
            return commandName;

        return commandName + ": " + documentation;
    }
}
