using System;

namespace Lokad.Cli.Generator.Emit
{
    public static class EmitParserType
    {
        public static string EmitFor(CliTypeInfo type)
        {
            return
$@"#nullable enable
namespace {type.TypeName.Namespace};
partial {GetTypeDeclaration(type)} {type.TypeName.Name} : global::Lokad.Cli.ICliParsed<{type.TypeName.FullName}>
{{
{EmitOptionNames(type)}

    {GetParseMethodDeclaration(type)}
    {{
        return ParseTokenizedCore(TokenizeCore(arguments));
    }}

    {GetTokenizeMethodDeclaration(type)}
    {{
        return TokenizeCore(arguments);
    }}

    {GetParseTokenizedMethodDeclaration(type)}
    {{
        if (arguments == null) throw new global::System.ArgumentNullException(nameof(arguments));

        return ParseTokenizedCore(arguments);
    }}

    {GetDocumentMethodDeclaration(type)}
    {{
        return DocumentCore(commandName);
    }}

    private static global::Lokad.Cli.TokenizedArguments TokenizeCore(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {{
        return global::Lokad.Cli.ArgumentTokenizer.Tokenize(
            arguments,
            __CliOptionNames.Short,
            __CliOptionNames.Long,
            {FormatBoolean(type.HasShortNumericArguments)}{GetLexicalPolicies(type)});
    }}

    private static {type.TypeName.FullName} ParseTokenizedCore(global::Lokad.Cli.TokenizedArguments tokenized)
    {{{EmitTokenizedParseArguments(type)}
        // Discrimination
{EmitDiscrimination(type)}

        // Binding
{EmitBinding(type)}
    }}

{EmitParseHelpers(type)}
}}
";

            static string GetTokenizeMethodDeclaration(CliTypeInfo parserType)
            {
                if (parserType.Kind == CliTypeKind.Interface)
                    return "static global::Lokad.Cli.TokenizedArguments global::Lokad.Cli.ICliParsed<" + parserType.TypeName.FullName + ">.Tokenize(global::System.Collections.Generic.IReadOnlyList<string> arguments)";

                return "public static global::Lokad.Cli.TokenizedArguments Tokenize(global::System.Collections.Generic.IReadOnlyList<string> arguments)";
            }

            static string EmitOptionNames(CliTypeInfo parserType)
            {
                string shortDeclaration;
                string longDeclaration;
                if (parserType.Tokenization.RequiresSchema)
                {
                    shortDeclaration = FormatOptions("char", "Short", parserType.Tokenization.ShortOptions, FormatCharLiteral);
                    longDeclaration = FormatOptions("string", "Long", parserType.Tokenization.LongOptions, FormatStringLiteral);
                }
                else
                {
                    shortDeclaration = "char[] Short = " + FormatCharArray(parserType.ShortOptions);
                    longDeclaration = "string[] Long = " + FormatStringArray(parserType.LongOptions);
                }
                return $@"    // A separate holder allows parsing from the argument type's static initializers.
    private static class __CliOptionNames
    {{
        public static readonly {shortDeclaration};
        public static readonly {longDeclaration};
    }}";

                string FormatOptions<T>(string keyType, string name, System.Collections.Generic.IReadOnlyDictionary<T, CliOptionValueMode> options, Func<T, string> format)
                {
                    var entries = new System.Collections.Generic.List<string>();
                    foreach (var option in options)
                    {
                        var mode = parserType.Tokenization.UsesArity ? option.Value : CliOptionValueMode.None;
                        entries.Add("[" + format(option.Key) + "] = global::Lokad.Cli.OptionValueMode." + mode);
                    }
                    return "global::System.Collections.Generic.IReadOnlyDictionary<" + keyType + ", global::Lokad.Cli.OptionValueMode> " + name +
                        " = new global::System.Collections.Generic.Dictionary<" + keyType + ", global::Lokad.Cli.OptionValueMode> { " + string.Join(", ", entries) + " }";
                }
            }

            static string GetLexicalPolicies(CliTypeInfo parserType) => parserType.Tokenization.RequiresSchema
                ? "," + Environment.NewLine + "            global::Lokad.Cli.ShortOptionSyntax." + parserType.Tokenization.ShortOptionSyntax +
                  "," + Environment.NewLine + "            " + FormatBoolean(parserType.Tokenization.AllowSingleDashLongOptions)
                : string.Empty;
        }

        private static string GetTypeDeclaration(CliTypeInfo type)
        {
            switch (type.Kind)
            {
                case CliTypeKind.Class:
                    return "class";
                case CliTypeKind.Struct:
                    return "struct";
                case CliTypeKind.Interface:
                    return "interface";
                case CliTypeKind.RecordStruct:
                    return "record struct";
                default:
                    return "record";
            }
        }

        private static string GetParseMethodDeclaration(CliTypeInfo type)
        {
            if (type.Kind == CliTypeKind.Interface)
                return "static " + type.TypeName.FullName + " global::Lokad.Cli.ICliParsed<" + type.TypeName.FullName + ">.Parse(global::System.Collections.Generic.IReadOnlyList<string> arguments)";

            return "public static " + type.TypeName.FullName + " Parse(global::System.Collections.Generic.IReadOnlyList<string> arguments)";
        }

        private static string GetParseTokenizedMethodDeclaration(CliTypeInfo type)
        {
            if (type.Kind == CliTypeKind.Interface)
                return "static " + type.TypeName.FullName + " global::Lokad.Cli.ICliParsed<" + type.TypeName.FullName + ">.ParseTokenized(global::Lokad.Cli.TokenizedArguments arguments)";

            return "public static " + type.TypeName.FullName + " ParseTokenized(global::Lokad.Cli.TokenizedArguments arguments)";
        }

        private static string GetDocumentMethodDeclaration(CliTypeInfo type)
        {
            if (type.Kind == CliTypeKind.Interface)
                return "static string global::Lokad.Cli.ICliParsed<" + type.TypeName.FullName + ">.Document(string commandName)";

            return "public static string Document(string commandName)";
        }

        private static string FormatCharArray(char[] values)
        {
            if (values.Length == 0)
                return "global::System.Array.Empty<char>()";

            return "new char[] { " + string.Join(", ", Array.ConvertAll(values, FormatCharLiteral)) + " }";
        }

        private static string FormatStringArray(string[] values)
        {
            if (values.Length == 0)
                return "global::System.Array.Empty<string>()";

            return "new string[] { " + string.Join(", ", Array.ConvertAll(values, FormatStringLiteral)) + " }";
        }

        private static string FormatBoolean(bool value) => value ? "true" : "false";

        private static string EmitTokenizedParseArguments(CliTypeInfo type)
        {
            if (type.Discriminator?.Prefix == null)
                return string.Empty;

            var lines = new System.Text.StringBuilder();
            lines.AppendLine();
            lines.Append("        tokenized.SkipPositional(")
                .Append(FormatStringLiteral(type.Discriminator.Prefix))
                .AppendLine(");");

            return lines.ToString();
        }

        private static string EmitBinding(CliTypeInfo type)
        {
            if (type.Choices.Length > 0)
                return "        throw new global::System.InvalidOperationException();";

            var lines = new System.Text.StringBuilder();
            var positional = GetPositionalArguments(type);
            var useCollectionBoundaries = Array.Exists(positional, argument => argument.ConsumeUntilBoundary);

            for (var i = 0; i < type.Arguments.Length; i++)
            {
                var argument = type.Arguments[i];
                if (argument.NeedsSeen)
                    lines.Append("        var ").Append(GetSeenName(i)).AppendLine(" = false;");
                lines.Append("        ").Append(GetLocalType(argument)).Append(' ').Append(GetValueName(i));
                if (argument.IsCollection)
                    lines.AppendLine(" = new();");
                else
                    lines.AppendLine(" = default;");
            }

            if (positional.Length > 0)
                lines.AppendLine("        var positionalIndex = 0;");
            lines.AppendLine("        global::Lokad.Cli.ITokenizedArguments? current;");
            lines.AppendLine(useCollectionBoundaries
                ? "        while ((current = tokenized.Peek()) != null)"
                : "        while ((current = tokenized.Shift()) != null)");
            lines.AppendLine("        {");
            if (useCollectionBoundaries)
            {
                lines.AppendLine("            if (!(current is global::Lokad.Cli.ValueArg))");
                lines.AppendLine("                tokenized.Shift();");
                lines.AppendLine("            if (current is global::Lokad.Cli.LongArg || current is global::Lokad.Cli.LongArgWithValue ||");
                lines.AppendLine("                current is global::Lokad.Cli.ShortArgSet || current is global::Lokad.Cli.ShortArgWithValue ||");
                lines.AppendLine("                current is global::Lokad.Cli.NumericShortArg)");
                lines.AppendLine("            {");
                for (var p = 0; p < positional.Length; p++)
                {
                    if (!positional[p].ConsumeUntilBoundary)
                        continue;
                    var argumentIndex = Array.IndexOf(type.Arguments, positional[p]);
                    lines.Append("                if (positionalIndex == ").Append(p)
                        .Append(" && ").Append(GetValueName(argumentIndex)).AppendLine(".Count > 0)");
                    lines.AppendLine("                    positionalIndex++;");
                }
                lines.AppendLine("            }");
            }
            lines.AppendLine("            switch (current)");
            lines.AppendLine("            {");
            lines.AppendLine("                case global::Lokad.Cli.LongArgWithValue longArgWithValue:");
            lines.Append(EmitLongWithValueBinding(type));
            lines.AppendLine("                case global::Lokad.Cli.LongArg longArg:");
            lines.Append(EmitLongBinding(type));
            lines.AppendLine("                case global::Lokad.Cli.ShortArgWithValue shortArgWithValue:");
            lines.Append(EmitShortWithValueBinding(type));
            lines.AppendLine("                case global::Lokad.Cli.ShortArgSet shortArgSet:");
            lines.Append(EmitShortBinding(type));
            lines.AppendLine("                    break;");
            lines.AppendLine("                case global::Lokad.Cli.NumericShortArg numericShortArg:");
            lines.Append(EmitNumericShortBinding(type));
            lines.AppendLine("                case global::Lokad.Cli.ValueArg valueArg:");
            lines.Append(EmitPositionalBinding(type, positional, useCollectionBoundaries));
            lines.AppendLine("            }");
            lines.AppendLine("        }");

            lines.Append(EmitRequiredChecks(type));
            lines.Append(EmitConstruction(type));
            return lines.ToString().TrimEnd();

            static string EmitRequiredChecks(CliTypeInfo type)
            {
                var lines = new System.Text.StringBuilder();
                for (var i = 0; i < type.Arguments.Length; i++)
                {
                    var argument = type.Arguments[i];
                    if (argument.IsNullable || argument.IsBoolean || argument.IsCollection)
                        continue;

                    lines.AppendLine();
                    lines.Append("        if (")
                        .Append(argument.IsReferenceType ? GetValueName(i) + " is null" : "!" + GetSeenName(i))
                        .AppendLine(")");
                    if (argument.IsOption)
                    {
                        lines.Append("            throw new global::Lokad.Cli.ParseException(")
                            .Append(FormatStringLiteral("missing option `" + argument.Options.PrimaryLabel + "`"))
                            .AppendLine(");");
                    }
                    else
                    {
                        lines.AppendLine("            throw new global::Lokad.Cli.ParseException(\"missing argument\");");
                    }
                }

                return lines.ToString();
            }

            static string GetLocalType(CliArgumentInfo argument)
            {
                var type = argument.TypeName.FullName;
                if (argument.IsCollection)
                    return "global::System.Collections.Generic.List<" + GetValueType(argument) + ">";

                if (argument.IsReferenceType)
                    return type + "?";

                return type;
            }

            static string EmitPositionalBinding(CliTypeInfo type, CliArgumentInfo[] positional, bool useCollectionBoundaries)
            {
                var lines = new System.Text.StringBuilder();
                lines.AppendLine("                    if (tokenized.MaybeSkipPositional(valueArg.Value))");
                if (useCollectionBoundaries)
                {
                    lines.AppendLine("                    {");
                    lines.AppendLine("                        tokenized.Shift();");
                }
                lines.AppendLine("                        break;");
                if (useCollectionBoundaries)
                    lines.AppendLine("                    }");

                for (var p = 0; p < positional.Length; p++)
                {
                    var argumentIndex = Array.IndexOf(type.Arguments, positional[p]);
                    lines.Append("                    if (positionalIndex == ").Append(p).AppendLine(")");
                    lines.AppendLine("                    {");
                    if (positional[p].ConsumeUntilBoundary)
                    {
                        var method = GetParseMethod(positional[p]);
                        if (method.Length == 0)
                        {
                            lines.AppendLine("                        tokenized.Shift();");
                            lines.Append("                        ").Append(GetValueName(argumentIndex)).AppendLine(".Add(valueArg.Value);");
                        }
                        else
                        {
                            lines.Append("                        if (Try").Append(method).AppendLine("(valueArg.Value, out var parsedElement))");
                            lines.AppendLine("                        {");
                            lines.AppendLine("                            tokenized.Shift();");
                            lines.Append("                            ").Append(GetValueName(argumentIndex)).AppendLine(".Add(parsedElement);");
                            lines.AppendLine("                            break;");
                            lines.AppendLine("                        }");
                            lines.AppendLine("                        positionalIndex++;");
                        }
                        lines.AppendLine("                        break;");
                        lines.AppendLine("                    }");
                        continue;
                    }
                    if (useCollectionBoundaries)
                        lines.AppendLine("                        tokenized.Shift();");
                    lines.Append(EmitAssign(argumentIndex, positional[p], "valueArg.Value", "\"argument\"", "                        "));
                    if (!positional[p].IsCollection || p < positional.Length - 1)
                        lines.AppendLine("                        positionalIndex++;");
                    lines.AppendLine("                        break;");
                    lines.AppendLine("                    }");
                }

                if (useCollectionBoundaries)
                    lines.AppendLine("                    tokenized.Shift();");
                lines.AppendLine("                    throw new global::Lokad.Cli.ParseException($\"unexpected argument `{valueArg.Value}`\");");
                return lines.ToString();
            }
        }

        private static string EmitLongWithValueBinding(CliTypeInfo type)
        {
            var lines = new System.Text.StringBuilder();
            for (var i = 0; i < type.Arguments.Length; i++)
            {
                var argument = type.Arguments[i];
                if (argument.Options.LongOptions.Count == 0 || argument.IsBoolean)
                    continue;

                lines.Append("                    if (")
                    .Append(FormatOptionMatch("longArgWithValue.Name", argument.Options.LongOptions, FormatStringLiteral))
                    .AppendLine(")");
                lines.AppendLine("                    {");
                lines.Append(EmitAssign(i, argument, "longArgWithValue.Value", FormatStringLiteral(GetOptionLabel(argument, argument.Options.LongOptions, "--")), "                        "));
                lines.AppendLine("                        break;");
                lines.AppendLine("                    }");
            }

            lines.Append(EmitLongDiscriminatorSkip(type, "longArgWithValue.Name", "                    "));
            lines.AppendLine("                    throw new global::Lokad.Cli.ParseException($\"unexpected option `--{longArgWithValue.Name}`\");");
            return lines.ToString();
        }

        private static string EmitLongBinding(CliTypeInfo type)
        {
            var lines = new System.Text.StringBuilder();
            for (var i = 0; i < type.Arguments.Length; i++)
            {
                var argument = type.Arguments[i];
                if (argument.Options.LongOptions.Count == 0)
                    continue;

                lines.Append("                    if (")
                    .Append(FormatOptionMatch("longArg.Name", argument.Options.LongOptions, FormatStringLiteral))
                    .AppendLine(")");
                lines.AppendLine("                    {");
                if (argument.IsBoolean)
                {
                    lines.Append(EmitBooleanFlag(i, argument, GetOptionLabel(argument, argument.Options.LongOptions, "--"), "                        "));
                }
                else
                {
                    lines.AppendLine("                        var valueToken = tokenized.Shift();");
                    lines.AppendLine("                        if (!(valueToken is global::Lokad.Cli.ValueArg optionValue))");
                    lines.Append("                            throw new global::Lokad.Cli.ParseException(")
                        .Append(FormatStringLiteral("option `" + GetOptionLabel(argument, argument.Options.LongOptions, "--") + "` expects a value"))
                        .AppendLine(");");
                    lines.Append(EmitAssign(i, argument, "optionValue.Value", FormatStringLiteral(GetOptionLabel(argument, argument.Options.LongOptions, "--")), "                        "));
                }

                lines.AppendLine("                        break;");
                lines.AppendLine("                    }");
            }

            lines.Append(EmitLongDiscriminatorSkip(type, "longArg.Name", "                    "));
            lines.AppendLine("                    throw new global::Lokad.Cli.ParseException($\"unexpected option `--{longArg.Name}`\");");
            return lines.ToString();
        }

        private static string EmitShortWithValueBinding(CliTypeInfo type)
        {
            var lines = new System.Text.StringBuilder();
            for (var i = 0; i < type.Arguments.Length; i++)
            {
                var argument = type.Arguments[i];
                if (argument.Options.ShortOptions.Count == 0 || argument.IsBoolean)
                    continue;

                lines.Append("                    if (")
                    .Append(FormatOptionMatch("shortArgWithValue.Name", argument.Options.ShortOptions, FormatCharLiteral))
                    .AppendLine(")");
                lines.AppendLine("                    {");
                lines.Append(EmitAssign(i, argument, "shortArgWithValue.Value", FormatStringLiteral(GetOptionLabel(argument, argument.Options.ShortOptions, "-")), "                        "));
                lines.AppendLine("                        break;");
                lines.AppendLine("                    }");
            }

            if (type.Discriminator?.ShortOption != '\0' && type.Discriminator?.ShortOption != null)
            {
                lines.Append("                    if (shortArgWithValue.Name == ")
                    .Append(FormatCharLiteral(type.Discriminator.ShortOption))
                    .AppendLine(")");
                lines.AppendLine("                        break;");
            }

            lines.AppendLine("                    throw new global::Lokad.Cli.ParseException($\"unexpected option `-{shortArgWithValue.Name}`\");");
            return lines.ToString();
        }

        private static string EmitShortBinding(CliTypeInfo type)
        {
            var lines = new System.Text.StringBuilder();
            lines.AppendLine("                    foreach (var shortOption in shortArgSet.Options)");
            lines.AppendLine("                    {");
            for (var i = 0; i < type.Arguments.Length; i++)
            {
                var argument = type.Arguments[i];
                if (argument.Options.ShortOptions.Count == 0)
                    continue;

                lines.Append("                        if (").Append(FormatOptionMatch("shortOption", argument.Options.ShortOptions, FormatCharLiteral)).AppendLine(")");
                lines.AppendLine("                        {");
                if (argument.IsBoolean)
                {
                    lines.Append(EmitBooleanFlag(i, argument, GetOptionLabel(argument, argument.Options.ShortOptions, "-"), "                            "));
                }
                else
                {
                    lines.AppendLine("                            var valueToken = tokenized.Shift();");
                    lines.AppendLine("                            if (!(valueToken is global::Lokad.Cli.ValueArg optionValue))");
                    lines.Append("                                throw new global::Lokad.Cli.ParseException(")
                        .Append(FormatStringLiteral("option `" + GetOptionLabel(argument, argument.Options.ShortOptions, "-") + "` expects a value"))
                        .AppendLine(");");
                    lines.Append(EmitAssign(i, argument, "optionValue.Value", FormatStringLiteral(GetOptionLabel(argument, argument.Options.ShortOptions, "-")), "                            "));
                }

                lines.AppendLine("                            continue;");
                lines.AppendLine("                        }");
            }

            if (type.Discriminator?.ShortOption != '\0' && type.Discriminator?.ShortOption != null)
            {
                lines.Append("                        if (shortOption == ")
                    .Append(FormatCharLiteral(type.Discriminator.ShortOption))
                    .AppendLine(")");
                lines.AppendLine("                            continue;");
            }

            lines.AppendLine("                        throw new global::Lokad.Cli.ParseException($\"unexpected option `-{shortOption}`\");");
            lines.AppendLine("                    }");
            return lines.ToString();
        }

        private static string FormatOptionMatch<T>(string expression, System.Collections.Generic.IReadOnlyList<T> names, Func<T, string> format)
        {
            var tests = new string[names.Count];
            for (var i = 0; i < names.Count; i++)
                tests[i] = expression + " == " + format(names[i]);
            return string.Join(" || ", tests);
        }

        private static string GetOptionLabel<T>(CliArgumentInfo argument, System.Collections.Generic.IReadOnlyList<T> names, string prefix) =>
            argument.Options.HasAliases ? argument.Options.PrimaryLabel : prefix + names[0];

        private static string EmitBooleanFlag(int index, CliArgumentInfo argument, string label, string indent)
        {
            var lines = new System.Text.StringBuilder();
            if (!argument.AllowRepeated)
            {
                lines.Append(indent).Append("if (").Append(GetSeenName(index)).AppendLine(")");
                lines.Append(indent).Append("    throw new global::Lokad.Cli.ParseException(")
                    .Append(FormatStringLiteral("duplicate option `" + label + "`"))
                    .AppendLine(");");
            }

            lines.Append(EmitAssign(index, argument, "true", FormatStringLiteral(label), indent));
            return lines.ToString();
        }

        private static string EmitNumericShortBinding(CliTypeInfo type)
        {
            var lines = new System.Text.StringBuilder();
            for (var i = 0; i < type.Arguments.Length; i++)
            {
                var argument = type.Arguments[i];
                if (!argument.ShortNumeric)
                    continue;

                lines.AppendLine("                    {");
                var label = argument.Options.HasAliases ? FormatStringLiteral(argument.Options.PrimaryLabel) : "\"-\" + numericShortArg.Value";
                lines.Append(EmitAssign(i, argument, "numericShortArg.Value", label, "                        "));
                lines.AppendLine("                        break;");
                lines.AppendLine("                    }");
                return lines.ToString();
            }

            lines.AppendLine("                    throw new global::Lokad.Cli.ParseException($\"unexpected argument `-{numericShortArg.Value}`\");");
            return lines.ToString();
        }

        private static string EmitConstruction(CliTypeInfo type)
        {
            var lines = new System.Text.StringBuilder();
            lines.AppendLine();
            lines.Append("        return new ").Append(type.TypeName.FullName).Append('(');

            var first = true;
            for (var i = 0; i < type.Arguments.Length; i++)
            {
                if (!type.Arguments[i].IsConstructorParameter)
                    continue;

                if (!first)
                    lines.Append(", ");

                lines.Append(GetResultValueExpression(i, type.Arguments[i]));
                first = false;
            }

            lines.Append(')');
            var hasInitializer = false;
            for (var i = 0; i < type.Arguments.Length; i++)
            {
                var argument = type.Arguments[i];
                if (argument.IsConstructorParameter || !argument.IsSettableMember)
                    continue;

                if (!hasInitializer)
                {
                    lines.AppendLine();
                    lines.AppendLine("        {");
                    hasInitializer = true;
                }

                lines.Append("            ").Append(argument.Name).Append(" = ").Append(GetResultValueExpression(i, argument)).AppendLine(",");
            }

            if (hasInitializer)
            {
                lines.Append("        }");
            }

            lines.AppendLine(";");
            return lines.ToString();

            static string GetResultValueExpression(int index, CliArgumentInfo argument)
            {
                var value = GetValueName(index);
                var type = argument.TypeName.FullName;
                if (argument.IsCollection)
                {
                    var array = value + ".ToArray()";
                    return argument.CollectionKind == CliCollectionKind.ReadOnlyMemory
                        ? "new global::System.ReadOnlyMemory<" + GetValueType(argument) + ">(" + array + ")"
                        : array;
                }

                if (argument.IsReferenceType || !argument.IsNullable)
                    return value;

                return GetSeenName(index) + " ? " + value + " : default(global::System.Nullable<" + type + ">)";
            }
        }

        private static string EmitAssign(int index, CliArgumentInfo argument, string valueExpression, string labelExpression, string indent)
        {
            var lines = new System.Text.StringBuilder();
            if (argument.IsBoolean)
            {
                if (argument.NeedsSeen)
                    lines.Append(indent).Append(GetSeenName(index)).AppendLine(" = true;");
                lines.Append(indent).Append(GetValueName(index)).AppendLine(" = true;");
                return lines.ToString();
            }

            lines.Append(indent).Append(GetValueName(index));
            if (argument.IsCollection)
            {
                lines.Append(".Add(")
                    .Append(GetParseExpression(argument, valueExpression, labelExpression))
                    .AppendLine(");");
            }
            else
            {
                lines.Append(" = ")
                    .Append(GetParseExpression(argument, valueExpression, labelExpression))
                    .AppendLine(";");
            }
            if (argument.NeedsSeen)
                lines.Append(indent).Append(GetSeenName(index)).AppendLine(" = true;");
            return lines.ToString();

            static string GetParseExpression(CliArgumentInfo argument, string valueExpression, string labelExpression)
            {
                var method = GetParseMethod(argument);
                if (method == string.Empty)
                    return valueExpression;

                return method + "(" + valueExpression + ", " + labelExpression + ")";
            }
        }

        private static string GetParseMethod(CliArgumentInfo argument) =>
            GetBuiltInParseMethod(argument.TypeName.FullName) ??
            (argument.IsEnum ? "ParseEnum<" + argument.TypeName.FullName + ">" : "ParseCustom<" + argument.TypeName.FullName + ">");

        private static string? GetBuiltInParseMethod(string type)
        {
            switch (type)
            {
                case "string":
                case "global::System.String":
                    return string.Empty;
                case "int":
                case "global::System.Int32":
                    return "ParseInt32";
                case "uint":
                case "global::System.UInt32":
                    return "ParseUInt32";
                case "long":
                case "global::System.Int64":
                    return "ParseInt64";
                case "ulong":
                case "global::System.UInt64":
                    return "ParseUInt64";
                case "double":
                case "global::System.Double":
                    return "ParseDouble";
                case "global::System.DateTime":
                    return "ParseDateTime";
                default:
                    return null;
            }
        }

        private static string GetValueType(CliArgumentInfo argument)
        {
            var type = argument.TypeName.FullName;
            if (type == "string" || type == "global::System.String")
                type = "string";

            return argument.IsNullable ? type + "?" : type;
        }

        private static string EmitLongDiscriminatorSkip(CliTypeInfo type, string nameExpression, string indent)
        {
            if (type.Discriminator?.LongOption == null)
                return string.Empty;

            return indent + "if (" + nameExpression + " == " + FormatStringLiteral(type.Discriminator.LongOption) + ")" + System.Environment.NewLine +
                   indent + "    break;" + System.Environment.NewLine;
        }

        private static CliArgumentInfo[] GetPositionalArguments(CliTypeInfo type) =>
            Array.FindAll(type.Arguments, argument => !argument.IsOption);

        private static string GetSeenName(int index) => "arg" + index + "Seen";

        private static string GetValueName(int index) => "arg" + index + "Value";

        private static string EmitParseHelpers(CliTypeInfo type)
        {
            var lines = new System.Text.StringBuilder();
            lines.Append(EmitDocumentHelpers(type));

            if (type.Choices.Length > 0)
            {
                if (lines.Length > 0)
                    lines.AppendLine().AppendLine();

                lines.AppendLine(@"    private static TChoice ParseChoice<TChoice>(global::Lokad.Cli.TokenizedArguments arguments)
        where TChoice : global::Lokad.Cli.ICliParsed<TChoice>
    {
        return TChoice.ParseTokenized(arguments);
    }");

            }

            if (type.Arguments.Length == 0)
                return lines.ToString().TrimEnd();

            var helpers = new System.Text.StringBuilder();

            if (UsesType(type, "int", "global::System.Int32"))
                helpers.Append(EmitValueParser("int", "ParseInt32",
                    "global::System.Int32.TryParse(value, global::System.Globalization.NumberStyles.Integer, global::System.Globalization.CultureInfo.InvariantCulture, out parsed)",
                    string.Empty, string.Empty));

            if (UsesType(type, "uint", "global::System.UInt32"))
                helpers.Append(EmitValueParser("uint", "ParseUInt32",
                    "global::System.UInt32.TryParse(value, global::System.Globalization.NumberStyles.Integer, global::System.Globalization.CultureInfo.InvariantCulture, out parsed)",
                    string.Empty, string.Empty));

            if (UsesType(type, "long", "global::System.Int64"))
                helpers.Append(EmitValueParser("long", "ParseInt64",
                    "global::System.Int64.TryParse(value, global::System.Globalization.NumberStyles.Integer, global::System.Globalization.CultureInfo.InvariantCulture, out parsed)",
                    string.Empty, string.Empty));

            if (UsesType(type, "ulong", "global::System.UInt64"))
                helpers.Append(EmitValueParser("ulong", "ParseUInt64",
                    "global::System.UInt64.TryParse(value, global::System.Globalization.NumberStyles.Integer, global::System.Globalization.CultureInfo.InvariantCulture, out parsed)",
                    string.Empty, string.Empty));

            if (UsesType(type, "double", "global::System.Double"))
                helpers.Append(EmitValueParser("double", "ParseDouble",
                    "global::System.Double.TryParse(value, global::System.Globalization.NumberStyles.Float | global::System.Globalization.NumberStyles.AllowThousands, global::System.Globalization.CultureInfo.InvariantCulture, out parsed)",
                    string.Empty, string.Empty));

            if (UsesType(type, "global::System.DateTime"))
                helpers.Append(EmitValueParser("global::System.DateTime", "ParseDateTime",
                    "global::System.DateTime.TryParse(value, global::System.Globalization.CultureInfo.InvariantCulture, global::System.Globalization.DateTimeStyles.None, out parsed)",
                    string.Empty, string.Empty));

            if (UsesEnum(type))
                helpers.Append(EmitValueParser("TEnum", "ParseEnum<TEnum>",
                    "global::System.Enum.TryParse<TEnum>(value, ignoreCase: false, out parsed)",
                    "where TEnum : struct, global::System.Enum", string.Empty));

            if (Array.Exists(type.Arguments, argument => !argument.IsBoolean && !argument.IsEnum &&
                    GetBuiltInParseMethod(argument.TypeName.FullName) == null))
                helpers.Append(EmitValueParser("TValue", "ParseCustom<TValue>",
                    "TValue.TryParse(value, null, out parsed)", "where TValue : global::System.IParsable<TValue>",
                    "[global::System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] "));

            if (helpers.Length == 0)
                return lines.ToString().TrimEnd();

            if (lines.Length > 0)
                lines.AppendLine().AppendLine();

            lines.Append(helpers);
            return lines.ToString();

            static string EmitValueParser(string returnType, string method, string tryExpression, string constraints, string outAnnotation)
            {
                var constraintLine = constraints.Length == 0 ? string.Empty : "        " + constraints + Environment.NewLine;
                return $@"
    private static {returnType} {method}(string value, string label)
{constraintLine}    {{
        if (Try{method}(value, out var parsed))
            return parsed;

        throw new global::Lokad.Cli.ParseException($""invalid value `{{value}}` for `{{label}}`"");
    }}

    private static bool Try{method}(string value, {outAnnotation}out {returnType} parsed)
{constraintLine}    {{
        return {tryExpression};
    }}
";
            }
        }

        private static string EmitDocumentHelpers(CliTypeInfo type)
        {
            var lines = new System.Text.StringBuilder();

            lines.AppendLine("    private static string DocumentCore(string commandName)");
            lines.AppendLine("    {");
            lines.AppendLine("        if (commandName == null) throw new global::System.ArgumentNullException(nameof(commandName));");
            lines.AppendLine();

            if (type.Choices.Length > 0)
            {
                lines.Append("        var documentedCommandName = ")
                    .Append(GetDocumentedCommandNameExpression(type, "commandName"))
                    .AppendLine(";");
                lines.AppendLine("        var lines = new global::System.Collections.Generic.List<string>();");
                if (type.Documentation != null)
                {
                    lines.Append("        lines.Add(FormatDocumentationLine(documentedCommandName, ")
                        .Append(FormatStringLiteral(type.Documentation))
                        .AppendLine("));");
                }

                for (var i = 0; i < type.Choices.Length; i++)
                {
                    lines.Append("        AddDocumentation(lines, DocumentChoice<")
                        .Append(type.Choices[i].TypeName.FullName)
                        .AppendLine(">(documentedCommandName));");
                }

                lines.AppendLine("        return string.Join(global::System.Environment.NewLine, lines);");
            }
            else
            {
                lines.Append("        var lines = new global::System.Collections.Generic.List<string> { FormatDocumentationLine(")
                    .Append(GetDocumentedCommandNameExpression(type, "commandName"))
                    .Append(", ")
                    .Append(FormatStringLiteral(type.Documentation ?? string.Empty))
                    .AppendLine(") };");

                foreach (var argument in type.Arguments)
                {
                    lines.Append("        lines.Add(")
                        .Append(FormatStringLiteral("  " + GetArgumentDocumentationLabel(argument) + ": " + argument.Documentation))
                        .AppendLine(");");
                }

                lines.AppendLine("        return string.Join(global::System.Environment.NewLine, lines);");
            }

            lines.AppendLine("    }");

            if (type.Choices.Length > 0)
            {
                lines.AppendLine();
                lines.AppendLine(@"    private static string DocumentChoice<TChoice>(string commandName)
        where TChoice : global::Lokad.Cli.ICliParsed<TChoice>
    {
        return TChoice.Document(commandName);
    }

    private static void AddDocumentation(global::System.Collections.Generic.List<string> lines, string documentation)
    {
        if (documentation.Length == 0)
            return;

        var split = documentation.Split(new[] { global::System.Environment.NewLine }, global::System.StringSplitOptions.None);
        foreach (var line in split)
            lines.Add(line);
    }");
            }

            lines.AppendLine();
            lines.AppendLine(@"    private static string FormatDocumentationLine(string commandName, string documentation)
    {
        if (documentation.Length == 0)
            return commandName;

        return commandName + "": "" + documentation;
    }");

            return lines.ToString().TrimEnd();
        }

        private static string GetDocumentedCommandNameExpression(CliTypeInfo type, string commandNameExpression)
        {
            var text = new System.Text.StringBuilder(commandNameExpression);
            if (type.Discriminator == null)
            {
                AppendPositionalDocumentationNames(type, text);
                return text.ToString();
            }

            text.Append(" + ").Append(FormatStringLiteral(" " + type.Discriminator.Display));
            AppendPositionalDocumentationNames(type, text);
            return text.ToString();
        }

        private static void AppendPositionalDocumentationNames(CliTypeInfo type, System.Text.StringBuilder text)
        {
            foreach (var argument in type.Arguments)
            {
                if (argument.IsOption)
                    continue;

                text.Append(" + ")
                    .Append(FormatStringLiteral(" " + GetPositionalDocumentationPlaceholder(argument)));
            }
        }

        private static string GetArgumentDocumentationLabel(CliArgumentInfo argument)
        {
            if (argument.Options.Labels.Count > 0)
            {
                var labels = string.Join(", ", argument.Options.Labels);
                return argument.ShortNumeric && argument.Options.HasAliases ? labels + ", -<number>" : labels;
            }

            if (argument.ShortNumeric)
                return "-<number>";

            return GetPositionalDocumentationPlaceholder(argument);
        }

        private static string GetPositionalDocumentationPlaceholder(CliArgumentInfo argument)
        {
            var name = string.IsNullOrEmpty(argument.DocumentationName) ? argument.Name : argument.DocumentationName!;
            if (argument.IsCollection)
                return "<" + name + "...>";

            if (argument.IsNullable)
                return "<" + name + "?>";

            return "<" + name + ">";
        }

        private static bool UsesType(CliTypeInfo type, params string[] typeNames)
        {
            foreach (var argument in type.Arguments)
                foreach (var typeName in typeNames)
                    if (argument.TypeName.FullName == typeName)
                        return true;

            return false;
        }

        private static bool UsesEnum(CliTypeInfo type)
        {
            foreach (var argument in type.Arguments)
                if (argument.IsEnum)
                    return true;

            return false;
        }

        private static string EmitDiscrimination(CliTypeInfo type)
        {
            if (type.Choices.Length == 0)
                return "        _ = tokenized;";

            var lines = new System.Text.StringBuilder();
            lines.AppendLine("        var choice = -1;");
            lines.AppendLine("        string? choiceDiscriminator = null;");

            for (var i = 0; i < type.Choices.Length; i++)
            {
                var discriminator = type.Choices[i].Discriminator;
                if (discriminator == null)
                    continue;

                lines.Append("        if (").Append(FormatDiscriminatorTest(discriminator)).AppendLine(")");
                lines.AppendLine("        {");
                lines.AppendLine("            if (choice >= 0)");
                lines.Append("                throw new global::Lokad.Cli.ParseException($\"incompatible options `")
                    .Append("{choiceDiscriminator}")
                    .Append("` and `")
                    .Append(EscapeInterpolatedStringText(discriminator.Display))
                    .AppendLine("`\");");
                lines.Append("            choice = ").Append(i).AppendLine(";");
                lines.Append("            choiceDiscriminator = ").Append(FormatStringLiteral(discriminator.Display)).AppendLine(";");
                lines.AppendLine("        }");
            }

            var defaultChoice = GetDefaultChoice(type);
            if (defaultChoice >= 0)
            {
                lines.AppendLine();
                lines.AppendLine("        if (choice < 0)");
                lines.Append("            choice = ").Append(defaultChoice).AppendLine(";");
            }
            else if (AllChoicesHaveDiscriminators(type))
            {
                lines.AppendLine();
                lines.AppendLine("        if (choice < 0)");
                lines.Append("            throw new global::Lokad.Cli.ParseException(")
                    .Append(FormatStringLiteral("expected " + FormatExpectedDiscriminators(type)))
                    .AppendLine(");");
            }

            lines.AppendLine();
            lines.AppendLine("        switch (choice)");
            lines.AppendLine("        {");
            for (var i = 0; i < type.Choices.Length; i++)
            {
                lines.Append("            case ").Append(i).AppendLine(":");
                lines.Append("                return ParseChoice<").Append(type.Choices[i].TypeName.FullName).AppendLine(">(tokenized);");
            }

            lines.AppendLine("        }");
            return lines.ToString().TrimEnd();
        }

        private static string FormatDiscriminatorTest(CliDiscriminatorInfo discriminator)
        {
            var tests = new System.Collections.Generic.List<string>();

            if (discriminator.ShortOption != '\0')
                tests.Add("tokenized.HasShortOption(" + FormatCharLiteral(discriminator.ShortOption) + ")");

            if (discriminator.LongOption != null)
                tests.Add("tokenized.HasLongOption(" + FormatStringLiteral(discriminator.LongOption) + ")");

            if (discriminator.Prefix != null)
                tests.Add("tokenized.HasPositionalPrefix(" + FormatStringLiteral(discriminator.Prefix) + ")");

            if (tests.Count == 0)
                throw new InvalidOperationException("Discriminator has no value.");

            return string.Join(" || ", tests);
        }

        private static bool AllChoicesHaveDiscriminators(CliTypeInfo type)
        {
            foreach (var choice in type.Choices)
                if (choice.Discriminator == null)
                    return false;

            return true;
        }

        private static int GetDefaultChoice(CliTypeInfo type)
        {
            for (var i = 0; i < type.Choices.Length; i++)
                if (type.Choices[i].Discriminator == null)
                    return i;

            return -1;
        }

        private static string FormatExpectedDiscriminators(CliTypeInfo type)
        {
            var discriminators = Array.FindAll(
                Array.ConvertAll(type.Choices, choice => choice.Discriminator),
                discriminator => discriminator != null);

            if (discriminators.Length == 1)
                return "`" + discriminators[0]!.Display + "`";

            if (discriminators.Length == 2)
                return "`" + discriminators[0]!.Display + "` or `" + discriminators[1]!.Display + "`";

            var text = new System.Text.StringBuilder();
            for (var i = 0; i < discriminators.Length; i++)
            {
                if (i > 0)
                    text.Append(i == discriminators.Length - 1 ? " or " : ", ");

                text.Append('`').Append(discriminators[i]!.Display).Append('`');
            }

            return text.ToString();
        }

        private static string FormatCharLiteral(char value)
        {
            switch (value)
            {
                case '\'':
                    return "'\\''";
                case '\\':
                    return "'\\\\'";
                case '\0':
                    return "'\\0'";
                case '\r':
                    return "'\\r'";
                case '\n':
                    return "'\\n'";
                case '\t':
                    return "'\\t'";
                default:
                    return "'" + value + "'";
            }
        }

        private static string FormatStringLiteral(string value) =>
            "@\"" + value.Replace("\"", "\"\"") + "\"";

        private static string EscapeInterpolatedStringText(string value) =>
            value.Replace("{", "{{").Replace("}", "}}").Replace("\"", "\\\"");
    }
}
