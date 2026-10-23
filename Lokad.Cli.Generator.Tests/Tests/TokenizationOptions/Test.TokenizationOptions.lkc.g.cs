#nullable enable
namespace Test;
partial record TokenizationOptions : global::Lokad.Cli.ICliParsed<global::Test.TokenizationOptions>
{
    // A separate holder allows parsing from the argument type's static initializers.
    private static class __CliOptionNames
    {
        public static readonly char[] Short = new char[] { 'm', 'c', 'p', 'v', 'V', 'f' };
        public static readonly string[] Long = new string[] { @"mode", @"from-ctor", @"raw-param", @"verbose", @"chatty", @"field", @"optional" };
    }

    public static global::Test.TokenizationOptions Parse(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        return ParseTokenizedCore(TokenizeCore(arguments));
    }

    public static global::Lokad.Cli.TokenizedArguments Tokenize(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {
        return TokenizeCore(arguments);
    }

    public static global::Test.TokenizationOptions ParseTokenized(global::Lokad.Cli.TokenizedArguments arguments)
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
            true);
    }

    private static global::Test.TokenizationOptions ParseTokenizedCore(global::Lokad.Cli.TokenizedArguments tokenized)
    {
        // Discrimination
        _ = tokenized;

        // Binding
        global::Test.SnapshotReferenceValue<string?>? arg0Value = default;
        global::System.Collections.Generic.List<int> arg1Value = new();
        bool arg2Value = default;
        var arg3Seen = false;
        int arg3Value = default;
        global::System.Collections.Generic.List<string> arg4Value = new();
        global::Test.SnapshotReferenceValue<string?>? arg5Value = default;
        global::Lokad.Cli.ITokenizedArguments? current;
        while ((current = tokenized.Shift()) != null)
        {
            switch (current)
            {
                case global::Lokad.Cli.LongArgWithValue longArgWithValue:
                    if (longArgWithValue.Name == @"from-ctor")
                    {
                        arg0Value = ParseCustom<global::Test.SnapshotReferenceValue<string?>>(longArgWithValue.Value, @"--from-ctor");
                        break;
                    }
                    if (longArgWithValue.Name == @"raw-param")
                    {
                        arg1Value.Add(ParseInt32(longArgWithValue.Value, @"--raw-param"));
                        break;
                    }
                    if (longArgWithValue.Name == @"field")
                    {
                        arg4Value.Add(longArgWithValue.Value);
                        break;
                    }
                    if (longArgWithValue.Name == @"optional")
                    {
                        arg5Value = ParseCustom<global::Test.SnapshotReferenceValue<string?>>(longArgWithValue.Value, @"--optional");
                        break;
                    }
                    if (longArgWithValue.Name == @"mode")
                        break;
                    throw new global::Lokad.Cli.ParseException($"unexpected option `--{longArgWithValue.Name}`");
                case global::Lokad.Cli.LongArg longArg:
                    if (longArg.Name == @"from-ctor")
                    {
                        var valueToken = tokenized.Shift();
                        if (!(valueToken is global::Lokad.Cli.ValueArg optionValue))
                            throw new global::Lokad.Cli.ParseException(@"option `--from-ctor` expects a value");
                        arg0Value = ParseCustom<global::Test.SnapshotReferenceValue<string?>>(optionValue.Value, @"--from-ctor");
                        break;
                    }
                    if (longArg.Name == @"raw-param")
                    {
                        var valueToken = tokenized.Shift();
                        if (!(valueToken is global::Lokad.Cli.ValueArg optionValue))
                            throw new global::Lokad.Cli.ParseException(@"option `--raw-param` expects a value");
                        arg1Value.Add(ParseInt32(optionValue.Value, @"--raw-param"));
                        break;
                    }
                    if (longArg.Name == @"verbose" || longArg.Name == @"chatty")
                    {
                        arg2Value = true;
                        break;
                    }
                    if (longArg.Name == @"field")
                    {
                        var valueToken = tokenized.Shift();
                        if (!(valueToken is global::Lokad.Cli.ValueArg optionValue))
                            throw new global::Lokad.Cli.ParseException(@"option `--field` expects a value");
                        arg4Value.Add(optionValue.Value);
                        break;
                    }
                    if (longArg.Name == @"optional")
                    {
                        var valueToken = tokenized.Shift();
                        if (!(valueToken is global::Lokad.Cli.ValueArg optionValue))
                            throw new global::Lokad.Cli.ParseException(@"option `--optional` expects a value");
                        arg5Value = ParseCustom<global::Test.SnapshotReferenceValue<string?>>(optionValue.Value, @"--optional");
                        break;
                    }
                    if (longArg.Name == @"mode")
                        break;
                    throw new global::Lokad.Cli.ParseException($"unexpected option `--{longArg.Name}`");
                case global::Lokad.Cli.ShortArgWithValue shortArgWithValue:
                    if (shortArgWithValue.Name == 'c')
                    {
                        arg0Value = ParseCustom<global::Test.SnapshotReferenceValue<string?>>(shortArgWithValue.Value, @"-c");
                        break;
                    }
                    if (shortArgWithValue.Name == 'p')
                    {
                        arg1Value.Add(ParseInt32(shortArgWithValue.Value, @"-p"));
                        break;
                    }
                    if (shortArgWithValue.Name == 'f')
                    {
                        arg4Value.Add(shortArgWithValue.Value);
                        break;
                    }
                    if (shortArgWithValue.Name == 'm')
                        break;
                    throw new global::Lokad.Cli.ParseException($"unexpected option `-{shortArgWithValue.Name}`");
                case global::Lokad.Cli.ShortArgSet shortArgSet:
                    foreach (var shortOption in shortArgSet.Options)
                    {
                        if (shortOption == 'c')
                        {
                            var valueToken = tokenized.Shift();
                            if (!(valueToken is global::Lokad.Cli.ValueArg optionValue))
                                throw new global::Lokad.Cli.ParseException(@"option `-c` expects a value");
                            arg0Value = ParseCustom<global::Test.SnapshotReferenceValue<string?>>(optionValue.Value, @"-c");
                            continue;
                        }
                        if (shortOption == 'p')
                        {
                            var valueToken = tokenized.Shift();
                            if (!(valueToken is global::Lokad.Cli.ValueArg optionValue))
                                throw new global::Lokad.Cli.ParseException(@"option `-p` expects a value");
                            arg1Value.Add(ParseInt32(optionValue.Value, @"-p"));
                            continue;
                        }
                        if (shortOption == 'v' || shortOption == 'V')
                        {
                            arg2Value = true;
                            continue;
                        }
                        if (shortOption == 'f')
                        {
                            var valueToken = tokenized.Shift();
                            if (!(valueToken is global::Lokad.Cli.ValueArg optionValue))
                                throw new global::Lokad.Cli.ParseException(@"option `-f` expects a value");
                            arg4Value.Add(optionValue.Value);
                            continue;
                        }
                        if (shortOption == 'm')
                            continue;
                        throw new global::Lokad.Cli.ParseException($"unexpected option `-{shortOption}`");
                    }
                    break;
                case global::Lokad.Cli.NumericShortArg numericShortArg:
                    {
                        arg3Value = ParseInt32(numericShortArg.Value, "-" + numericShortArg.Value);
                        arg3Seen = true;
                        break;
                    }
                case global::Lokad.Cli.ValueArg valueArg:
                    if (tokenized.MaybeSkipPositional(valueArg.Value))
                        break;
                    throw new global::Lokad.Cli.ParseException($"unexpected argument `{valueArg.Value}`");
            }
        }

        if (arg0Value is null)
            throw new global::Lokad.Cli.ParseException(@"missing option `--from-ctor`");

        return new global::Test.TokenizationOptions(arg0Value, new global::System.ReadOnlyMemory<int>(arg1Value.ToArray()))
        {
            Verbose = arg2Value,
            Depth = arg3Seen ? arg3Value : default(global::System.Nullable<int>),
            Field = arg4Value.ToArray(),
            Optional = arg5Value,
        };
    }

    private static string DocumentCore(string commandName)
    {
        if (commandName == null) throw new global::System.ArgumentNullException(nameof(commandName));

        var lines = new global::System.Collections.Generic.List<string> { FormatDocumentationLine(commandName + @" --mode", @"Tokenization mode.") };
        lines.Add(@"  --from-ctor, -c: Constructor property value.");
        lines.Add(@"  --raw-param, -p: Raw constructor parameter.");
        lines.Add(@"  --verbose, --chatty, -v, -V: Enable verbose output.");
        lines.Add(@"  -<number>: Numeric depth.");
        lines.Add(@"  --field, -f: Field value.");
        lines.Add(@"  --optional: Optional reference value.");
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

    private static TValue ParseCustom<TValue>(string value, string label)
        where TValue : global::System.IParsable<TValue>
    {
        if (TValue.TryParse(value, null, out var parsed))
            return parsed;

        throw new global::Lokad.Cli.ParseException($"invalid value `{value}` for `{label}`");
    }

}
