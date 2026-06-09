#nullable enable
#pragma warning disable 0162, 0219
namespace Test;
partial record Empty : global::Lokad.Cli.ICliParsed<global::Test.Empty>
{
    public static global::Test.Empty Parse(string[] arguments)
    {
        // Tokenization
        var tokenized = global::Lokad.Cli.ArgumentTokenizer.Tokenize(
            arguments,
            global::System.Array.Empty<char>(),
            global::System.Array.Empty<string>(),
            false);

        return ParseTokenizedCore(tokenized);
    }

    public static global::Test.Empty ParseTokenized(global::Lokad.Cli.TokenizedArguments arguments)
    {
        if (arguments == null) throw new global::System.ArgumentNullException(nameof(arguments));

        return ParseTokenizedCore(arguments);
    }

    private static global::Test.Empty ParseTokenizedCore(global::Lokad.Cli.TokenizedArguments tokenized)
    {
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

        return new global::Test.Empty();
    }


}
