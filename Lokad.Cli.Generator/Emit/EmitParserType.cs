using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Lokad.Cli.Generator.Emit
{
    public static class EmitParserType
    {
        public static string EmitFor(CliTypeInfo type)
        {
            return
$@"#nullable enable
{GetExternAliasDeclarations(type)}{GetNamespaceDeclaration(type)}partial {GetTypeDeclaration(type)} {FormatIdentifier(type.TypeName.Name)} : global::Lokad.Cli.ICliParsed<{type.TypeName.FullName}>
{{
{EmitOptionNames(type)}

    {GetParserMethodDeclaration(CliParserMethods.Parse, type.TypeName.FullName, "global::System.Collections.Generic.IReadOnlyList<string> arguments")}
    {{
        return ParseTokenizedCore{type.ImplementationSuffix}(TokenizeCore{type.ImplementationSuffix}(arguments));
    }}

    {GetParserMethodDeclaration(CliParserMethods.Tokenize, "global::Lokad.Cli.TokenizedArguments", "global::System.Collections.Generic.IReadOnlyList<string> arguments")}
    {{
        return TokenizeCore{type.ImplementationSuffix}(arguments);
    }}

    {GetHelpMethodDeclaration(type)}
    {{
        if (arguments == null) throw new global::System.ArgumentNullException(nameof(arguments));

        {GetHelpInspection(type)}
    }}

    {GetParserMethodDeclaration(CliParserMethods.ParseTokenized, type.TypeName.FullName, "global::Lokad.Cli.TokenizedArguments arguments")}
    {{
        if (arguments == null) throw new global::System.ArgumentNullException(nameof(arguments));

        return ParseTokenizedCore{type.ImplementationSuffix}(arguments);
    }}

    {GetParserMethodDeclaration(CliParserMethods.Document, "string", "string commandName")}
    {{
        return DocumentCore{type.ImplementationSuffix}(commandName);
    }}

    private static global::Lokad.Cli.TokenizedArguments TokenizeCore{type.ImplementationSuffix}(global::System.Collections.Generic.IReadOnlyList<string> arguments)
    {{
        return global::Lokad.Cli.ArgumentTokenizer.Tokenize(
            arguments,
            OptionNames{type.ImplementationSuffix}.Short,
            OptionNames{type.ImplementationSuffix}.Long,
            {FormatBoolean(type.HasShortNumericArguments)}{GetLexicalPolicies(type)});
    }}

    private static {type.TypeName.FullName} ParseTokenizedCore{type.ImplementationSuffix}(global::Lokad.Cli.TokenizedArguments tokenized)
    {{{EmitTokenizedParseArguments(type)}
        // Discrimination
{EmitDiscrimination(type)}

        // Binding
{EmitBinding(type)}
    }}

{EmitParseHelpers(type)}
}}
";

            static string GetExternAliasDeclarations(CliTypeInfo parserType) => parserType.ExternAliases.Count == 0
                ? string.Empty : string.Join(Environment.NewLine, System.Linq.Enumerable.Select(
                    parserType.ExternAliases, alias => "extern alias @" + alias + ";")) + Environment.NewLine;

            static string GetNamespaceDeclaration(CliTypeInfo parserType) => parserType.TypeName.Namespace.Length == 0
                ? string.Empty : "namespace " + parserType.TypeName.Namespace + ";" + Environment.NewLine;

            string GetParserMethodDeclaration(CliParserMethods method, string returnType, string parameters)
            {
                var implementation = type.Kind == CliTypeKind.Interface || (type.ExplicitParserMethods & method) != 0
                    ? "static " + returnType + " global::Lokad.Cli.ICliParsed<" + type.TypeName.FullName + ">."
                    : "public static " + returnType + " ";
                return implementation + method + "(" + parameters + ")";
            }

            static string GetHelpMethodDeclaration(CliTypeInfo parserType) =>
                "static bool global::Lokad.Cli.ICliParsed<" + parserType.TypeName.FullName + ">.IsHelpRequested(global::Lokad.Cli.TokenizedArguments arguments)";

            static string GetHelpInspection(CliTypeInfo parserType) => parserType.Tokenization.HasHelp
                ? "return arguments.IsHelpRequested(OptionNames" + parserType.ImplementationSuffix + ".HelpShort, OptionNames" + parserType.ImplementationSuffix + ".HelpLong, " +
                    (parserType.CommandBoundary != null ? "OptionNames" + parserType.ImplementationSuffix + ".Boundary" : "global::Lokad.Cli.CommandBoundary.Empty") + ");"
                : "return false;";

            static string EmitOptionNames(CliTypeInfo parserType)
            {
                var shortDeclaration = FormatOptions("char", "Short", parserType.Tokenization.ShortOptions, FormatCharLiteral);
                var longDeclaration = FormatOptions("string", "Long", parserType.Tokenization.LongOptions, FormatStringLiteral);
                var helpDeclarations = parserType.Tokenization.HasHelp
                    ? "        public static readonly " + FormatOptions("char", "HelpShort", parserType.Tokenization.HelpShortOptions, FormatCharLiteral) + ";" + Environment.NewLine +
                      "        public static readonly " + FormatOptions("string", "HelpLong", parserType.Tokenization.HelpLongOptions, FormatStringLiteral) + ";" + Environment.NewLine
                    : string.Empty;
                return $@"    // A separate holder allows parsing from the argument type's static initializers.
    private static class OptionNames{parserType.ImplementationSuffix}
    {{
        public static readonly {shortDeclaration};
        public static readonly {longDeclaration};
{helpDeclarations}{EmitBoundaries()}    }}";

                string EmitBoundaries()
                {
                    if (parserType.CommandBoundary == null)
                        return string.Empty;
                    var lines = new System.Text.StringBuilder();
                    var names = new System.Collections.Generic.Dictionary<CliCommandBoundaryInfo, string>();
                    Visit(parserType.CommandBoundary);
                    return lines.ToString();

                    string Visit(CliCommandBoundaryInfo boundary)
                    {
                        if (names.TryGetValue(boundary, out var cached))
                            return cached;
                        var name = boundary == parserType.CommandBoundary ? "Boundary" : "Boundary" + names.Count;
                        names.Add(boundary, name);
                        var children = new System.Collections.Generic.List<string>();
                        foreach (var child in boundary.Choices)
                            children.Add(Visit(child));
                        lines.Append("        public static readonly global::Lokad.Cli.CommandBoundary ").Append(name)
                            .Append(" = new global::Lokad.Cli.CommandBoundary(")
                            .Append(boundary.Discriminator?.Prefix is string prefix ? FormatStringLiteral(prefix) : "null").Append(", ")
                            .Append(boundary.Discriminator?.LongOption is string option ? FormatStringLiteral(option) : "null").Append(", ")
                            .Append(FormatCharLiteral(boundary.Discriminator?.ShortOption ?? '\0')).Append(", ")
                            .Append("new global::Lokad.Cli.CommandBoundary[] { ").Append(string.Join(", ", children)).Append(" }, ")
                            .Append(boundary.RemainderAfterOperands?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "null").Append(", ")
                            .Append(FormatArray(boundary.StopAfterShortOptions, "char", FormatCharLiteral)).Append(", ")
                            .Append(FormatArray(boundary.StopAfterLongOptions, "string", FormatStringLiteral)).Append(", ")
                            .Append(FormatBoolean(boundary.IsHelpPrefix)).AppendLine(");");
                        return name;

                        static string FormatArray<T>(
                            System.Collections.Generic.IReadOnlyList<T> values, string typeName, Func<T, string> format) => values.Count == 0
                            ? "global::System.Array.Empty<" + typeName + ">()"
                            : "new " + typeName + "[] { " + string.Join(", ", System.Linq.Enumerable.Select(values, format)) + " }";
                    }
                }

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

            static string GetLexicalPolicies(CliTypeInfo parserType) =>
                "," + Environment.NewLine + "            global::Lokad.Cli.ShortOptionSyntax." + parserType.Tokenization.ShortOptionSyntax +
                  "," + Environment.NewLine + "            " + FormatBoolean(parserType.Tokenization.AllowSingleDashLongOptions)
                  + (parserType.CommandBoundary == null ? string.Empty : "," + Environment.NewLine + "            " +
                      FormatBoolean(parserType.Tokenization.StopAtFirstOperand) + ", OptionNames" + parserType.ImplementationSuffix + ".Boundary");

            static string GetTypeDeclaration(CliTypeInfo parserType)
            {
                switch (parserType.Kind)
                {
                    case CliTypeKind.Class: return "class";
                    case CliTypeKind.Struct: return "struct";
                    case CliTypeKind.Interface: return "interface";
                    case CliTypeKind.RecordStruct: return "record struct";
                    default: return "record";
                }
            }
        }

        private static string FormatBoolean(bool value) => value ? "true" : "false";

        private static string EmitTokenizedParseArguments(CliTypeInfo type)
        {
            if (type.Discriminator == null)
                return string.Empty;

            var lines = new System.Text.StringBuilder();
            lines.AppendLine();
            if (type.Discriminator.Prefix is string prefix)
            {
                lines.Append("        if (tokenized.HasPositionalPrefix(").Append(FormatStringLiteral(prefix)).AppendLine("))");
                lines.Append("            tokenized.SkipPositional(").Append(FormatStringLiteral(prefix)).AppendLine(");");
            }
            else
                lines.Append("        tokenized.SelectDiscriminator(")
                    .Append(type.Discriminator.LongOption is string name ? FormatStringLiteral(name) : "null")
                    .Append(", ").Append(FormatCharLiteral(type.Discriminator.ShortOption)).AppendLine(");");

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
                lines.AppendLine("            if (!(current is global::Lokad.Cli.ValueArg) && !(current is global::Lokad.Cli.LiteralArg))");
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
            lines.AppendLine("                case global::Lokad.Cli.ValueArg:");
            lines.AppendLine("                case global::Lokad.Cli.LiteralArg:");
            lines.AppendLine("                    var positionalValue = current is global::Lokad.Cli.ValueArg valueArg ? valueArg.Value : ((global::Lokad.Cli.LiteralArg)current).Value;");
            lines.Append(EmitPositionalBinding(type, positional, useCollectionBoundaries));
            lines.AppendLine("                default:");
            lines.AppendLine("                    throw new global::Lokad.Cli.ParseException($\"unexpected token type `{current.GetType().Name}`\");");
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
                    if (!argument.IsRequired)
                        continue;

                    lines.AppendLine();
                    lines.Append("        if (")
                        .Append(argument.IsReferenceType ? GetValueName(i) + " is null" : "!" + GetSeenName(i))
                        .AppendLine(")");
                    if (argument.IsOption)
                    {
                        lines.Append("            throw new global::Lokad.Cli.ParseException(")
                            .Append(FormatStringLiteral("missing option `" + argument.PrimaryLabel + "`"))
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
                    return "global::System.Collections.Generic.List<" + GetBoundValueType(argument) + ">";

                if (argument.IsOptionalValue)
                    return GetBoundValueType(argument);

                if (argument.IsReferenceType)
                    return type + "?";

                return type;
            }

            static string EmitPositionalBinding(CliTypeInfo type, CliArgumentInfo[] positional, bool useCollectionBoundaries)
            {
                var lines = new System.Text.StringBuilder();
                lines.AppendLine("                    if (current is global::Lokad.Cli.ValueArg && tokenized.MaybeSkipPositional(positionalValue))");
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
                    if (positional[p].IsRemainder)
                    {
                        lines.AppendLine("                        if (!(current is global::Lokad.Cli.LiteralArg))");
                        lines.AppendLine("                            throw new global::Lokad.Cli.ParseException(\"expected an option boundary before the remainder\");");
                    }
                    if (positional[p].ConsumeUntilBoundary)
                    {
                        var method = GetParseMethod(type, positional[p]);
                        if (method.Length == 0)
                        {
                            lines.AppendLine("                        tokenized.Shift();");
                            lines.Append("                        ").Append(GetValueName(argumentIndex)).AppendLine(".Add(positionalValue);");
                        }
                        else
                        {
                            lines.Append("                        if (Try").Append(method).AppendLine("(positionalValue, out var parsedElement))");
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
                    lines.Append(EmitAssign(type, argumentIndex, positional[p], "positionalValue", "\"argument\"", "                        "));
                    if (!positional[p].IsCollection || p < positional.Length - 1)
                        lines.AppendLine("                        positionalIndex++;");
                    lines.AppendLine("                        break;");
                    lines.AppendLine("                    }");
                }

                if (useCollectionBoundaries)
                    lines.AppendLine("                    tokenized.Shift();");
                lines.AppendLine("                    throw new global::Lokad.Cli.ParseException($\"unexpected argument `{positionalValue}`\");");
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
                lines.Append(EmitAssign(type, i, argument, "longArgWithValue.Value", FormatStringLiteral(GetOptionLabel(argument, argument.Options.LongOptions, "--")), "                        "));
                lines.AppendLine("                        break;");
                lines.AppendLine("                    }");
            }

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
                if (argument.IsOptionalValue)
                {
                    lines.Append(EmitStore(i, argument, GetBoundValueType(argument) + ".Bare", "                        "));
                }
                else if (argument.IsBoolean)
                {
                    lines.Append(EmitBooleanFlag(type, i, argument, GetOptionLabel(argument, argument.Options.LongOptions, "--"), "                        "));
                }
                else
                {
                    lines.AppendLine("                        var valueToken = tokenized.Shift();");
                    lines.AppendLine("                        if (!(valueToken is global::Lokad.Cli.ValueArg optionValue))");
                    lines.Append("                            throw new global::Lokad.Cli.ParseException(")
                        .Append(FormatStringLiteral("option `" + GetOptionLabel(argument, argument.Options.LongOptions, "--") + "` expects a value"))
                        .AppendLine(");");
                    lines.Append(EmitAssign(type, i, argument, "optionValue.Value", FormatStringLiteral(GetOptionLabel(argument, argument.Options.LongOptions, "--")), "                        "));
                }

                lines.AppendLine("                        break;");
                lines.AppendLine("                    }");
            }

            lines.AppendLine("                    if (tokenized.IsSelectedLongOption(longArg.Name))");
            lines.AppendLine("                        break;");
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
                lines.Append(EmitAssign(type, i, argument, "shortArgWithValue.Value", FormatStringLiteral(GetOptionLabel(argument, argument.Options.ShortOptions, "-")), "                        "));
                lines.AppendLine("                        break;");
                lines.AppendLine("                    }");
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
                if (argument.IsOptionalValue)
                {
                    lines.Append(EmitStore(i, argument, GetBoundValueType(argument) + ".Bare", "                            "));
                }
                else if (argument.IsBoolean)
                {
                    lines.Append(EmitBooleanFlag(type, i, argument, GetOptionLabel(argument, argument.Options.ShortOptions, "-"), "                            "));
                }
                else
                {
                    lines.AppendLine("                            var valueToken = tokenized.Shift();");
                    lines.AppendLine("                            if (!(valueToken is global::Lokad.Cli.ValueArg optionValue))");
                    lines.Append("                                throw new global::Lokad.Cli.ParseException(")
                        .Append(FormatStringLiteral("option `" + GetOptionLabel(argument, argument.Options.ShortOptions, "-") + "` expects a value"))
                        .AppendLine(");");
                    lines.Append(EmitAssign(type, i, argument, "optionValue.Value", FormatStringLiteral(GetOptionLabel(argument, argument.Options.ShortOptions, "-")), "                            "));
                }

                lines.AppendLine("                            continue;");
                lines.AppendLine("                        }");
            }

            lines.AppendLine("                        if (tokenized.IsSelectedShortOption(shortOption))");
            lines.AppendLine("                            continue;");

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
            argument.Options.HasAliases ? argument.PrimaryLabel : prefix + names[0];

        private static string EmitBooleanFlag(CliTypeInfo type, int index, CliArgumentInfo argument, string label, string indent)
        {
            var lines = new System.Text.StringBuilder();
            if (!argument.AllowRepeated)
            {
                lines.Append(indent).Append("if (").Append(GetSeenName(index)).AppendLine(")");
                lines.Append(indent).Append("    throw new global::Lokad.Cli.ParseException(")
                    .Append(FormatStringLiteral("duplicate option `" + label + "`"))
                    .AppendLine(");");
            }

            lines.Append(EmitAssign(type, index, argument, "true", FormatStringLiteral(label), indent));
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
                var label = argument.Options.HasAliases ? FormatStringLiteral(argument.PrimaryLabel) : "\"-\" + numericShortArg.Value";
                lines.Append(EmitAssign(type, i, argument, "numericShortArg.Value", label, "                        "));
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
            lines.Append("        return ").Append(Construct(type.TypeName, type.Members, "        ")).AppendLine(";");
            return lines.ToString();

            string Construct(TypeName typeName, System.Collections.Generic.IReadOnlyList<CliArgumentInfo> members, string indent)
            {
                var expression = new System.Text.StringBuilder("new " + typeName.FullName + "(");
                var first = true;
                foreach (var member in members)
                {
                    if (!(member.ConstructorParameterType is string parameterType))
                        continue;
                    if (!first)
                        expression.Append(", ");
                    expression.Append('(').Append(parameterType).Append(")(")
                        .Append(GetResultValueExpression(member, indent)).Append(')');
                    first = false;
                }
                expression.Append(')');
                var hasInitializer = false;
                foreach (var member in members)
                {
                    if (member.IsConstructorParameter)
                        continue;
                    if (!hasInitializer)
                    {
                        expression.AppendLine().Append(indent).AppendLine("{");
                        hasInitializer = true;
                    }
                    expression.Append(indent).Append("    ").Append(FormatIdentifier(member.Name)).Append(" = ")
                        .Append(GetResultValueExpression(member, indent + "    ")).AppendLine(",");
                }
                if (hasInitializer)
                    expression.Append(indent).Append('}');
                return expression.ToString();
            }

            string GetResultValueExpression(CliArgumentInfo argument, string indent)
            {
                if (argument.GroupMembers is System.Collections.Generic.IReadOnlyList<CliArgumentInfo> members)
                    return Construct(argument.TypeName, members, indent);
                var index = Array.IndexOf(type.Arguments, argument);
                var value = GetValueName(index);
                var valueType = argument.TypeName.FullName;
                if (argument.IsCollection)
                {
                    var array = value + ".ToArray()";
                    return argument.CollectionKind == CliCollectionKind.ReadOnlyMemory
                        ? "new global::System.ReadOnlyMemory<" + GetBoundValueType(argument) + ">(" + array + ")"
                        : array;
                }

                if (argument.IsOptionalValue || argument.IsReferenceType || !argument.IsNullable)
                    return value;

                return GetSeenName(index) + " ? " + value + " : default(global::System.Nullable<" + valueType + ">)";
            }
        }

        private static string EmitAssign(CliTypeInfo type, int index, CliArgumentInfo argument, string valueExpression, string labelExpression, string indent)
        {
            if (argument.IsBoolean)
            {
                var lines = new System.Text.StringBuilder();
                if (argument.NeedsSeen)
                    lines.Append(indent).Append(GetSeenName(index)).AppendLine(" = true;");
                lines.Append(indent).Append(GetValueName(index)).AppendLine(" = true;");
                return lines.ToString();
            }

            var expression = GetParseExpression(type, argument, valueExpression, labelExpression);
            if (argument.IsOptionalValue)
                expression = "new " + GetBoundValueType(argument) + "(" + expression + ")";
            return EmitStore(index, argument, expression, indent);

            static string GetParseExpression(CliTypeInfo type, CliArgumentInfo argument, string valueExpression, string labelExpression)
            {
                var method = GetParseMethod(type, argument);
                if (method == string.Empty)
                    return valueExpression;

                return method + "(" + valueExpression + ", " + labelExpression + ")";
            }
        }

        private static string EmitStore(int index, CliArgumentInfo argument, string expression, string indent)
        {
            var lines = new System.Text.StringBuilder();
            lines.Append(indent).Append(GetValueName(index));
            if (argument.IsCollection)
                lines.Append(".Add(").Append(expression).AppendLine(");");
            else
                lines.Append(" = ").Append(expression).AppendLine(";");
            if (argument.NeedsSeen)
                lines.Append(indent).Append(GetSeenName(index)).AppendLine(" = true;");
            return lines.ToString();
        }

        private static string GetParseMethod(CliTypeInfo type, CliArgumentInfo argument)
        {
            var method = GetBuiltInParseMethod(argument.SpecialType) ??
                (argument.IsEnum ? "ParseEnum<" + argument.TypeName.FullName + ">" : "ParseCustom<" + argument.TypeName.FullName + ">");
            return method.Length == 0 ? method : GetImplementationName(type, method);
        }

        private static string GetImplementationName(CliTypeInfo type, string method)
        {
            var generic = method.IndexOf('<');
            return generic < 0 ? method + type.ImplementationSuffix : method.Insert(generic, type.ImplementationSuffix);
        }

        private static string? GetBuiltInParseMethod(SpecialType type)
        {
            switch (type)
            {
                case SpecialType.System_String:
                    return string.Empty;
                case SpecialType.System_Int32:
                    return "ParseInt32";
                case SpecialType.System_UInt32:
                    return "ParseUInt32";
                case SpecialType.System_Int64:
                    return "ParseInt64";
                case SpecialType.System_UInt64:
                    return "ParseUInt64";
                case SpecialType.System_Double:
                    return "ParseDouble";
                case SpecialType.System_DateTime:
                    return "ParseDateTime";
                default:
                    return null;
            }
        }

        private static string GetBoundValueType(CliArgumentInfo argument)
        {
            var type = argument.TypeName.FullName;
            if (argument.SpecialType == SpecialType.System_String)
                type = "string";

            if (argument.IsNullable)
                type += "?";
            return argument.IsOptionalValue ? "global::Lokad.Cli.OptionalValue<" + type + ">" : type;
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

                lines.AppendLine($@"    private static TChoice ParseChoice{type.ImplementationSuffix}<TChoice>(global::Lokad.Cli.TokenizedArguments arguments)
        where TChoice : global::Lokad.Cli.ICliParsed<TChoice>
    {{
        return TChoice.ParseTokenized(arguments);
    }}");

            }

            if (type.Arguments.Length == 0)
                return lines.ToString().TrimEnd();

            var helpers = new System.Text.StringBuilder();

            if (GetTypeName(SpecialType.System_Int32) != null)
                helpers.Append(EmitValueParser("int", "ParseInt32",
                    "global::System.Int32.TryParse(value, global::System.Globalization.NumberStyles.Integer, global::System.Globalization.CultureInfo.InvariantCulture, out parsed)",
                    string.Empty, string.Empty));

            if (GetTypeName(SpecialType.System_UInt32) != null)
                helpers.Append(EmitValueParser("uint", "ParseUInt32",
                    "global::System.UInt32.TryParse(value, global::System.Globalization.NumberStyles.Integer, global::System.Globalization.CultureInfo.InvariantCulture, out parsed)",
                    string.Empty, string.Empty));

            if (GetTypeName(SpecialType.System_Int64) != null)
                helpers.Append(EmitValueParser("long", "ParseInt64",
                    "global::System.Int64.TryParse(value, global::System.Globalization.NumberStyles.Integer, global::System.Globalization.CultureInfo.InvariantCulture, out parsed)",
                    string.Empty, string.Empty));

            if (GetTypeName(SpecialType.System_UInt64) != null)
                helpers.Append(EmitValueParser("ulong", "ParseUInt64",
                    "global::System.UInt64.TryParse(value, global::System.Globalization.NumberStyles.Integer, global::System.Globalization.CultureInfo.InvariantCulture, out parsed)",
                    string.Empty, string.Empty));

            if (GetTypeName(SpecialType.System_Double) != null)
                helpers.Append(EmitValueParser("double", "ParseDouble",
                    "global::System.Double.TryParse(value, global::System.Globalization.NumberStyles.Float | global::System.Globalization.NumberStyles.AllowThousands, global::System.Globalization.CultureInfo.InvariantCulture, out parsed)",
                    string.Empty, string.Empty));

            if (GetTypeName(SpecialType.System_DateTime) is string dateTimeType)
                helpers.Append(EmitValueParser(dateTimeType, "ParseDateTime",
                    dateTimeType + ".TryParse(value, global::System.Globalization.CultureInfo.InvariantCulture, global::System.Globalization.DateTimeStyles.None, out parsed)",
                    string.Empty, string.Empty));

            if (Array.Exists(type.Arguments, argument => argument.IsEnum))
                helpers.Append(EmitValueParser("TEnum", "ParseEnum<TEnum>",
                    "global::System.Enum.TryParse<TEnum>(value, ignoreCase: false, out parsed)",
                    "where TEnum : struct, global::System.Enum", string.Empty));

            if (Array.Exists(type.Arguments, argument => !argument.IsBoolean && !argument.IsEnum &&
                    GetBuiltInParseMethod(argument.SpecialType) == null))
                helpers.Append(EmitValueParser("TValue", "ParseCustom<TValue>",
                    "TValue.TryParse(value, null, out parsed)", "where TValue : global::System.IParsable<TValue>",
                    "[global::System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] "));

            if (helpers.Length == 0)
                return lines.ToString().TrimEnd();

            if (lines.Length > 0)
                lines.AppendLine().AppendLine();

            lines.Append(helpers);
            return lines.ToString();

            string? GetTypeName(SpecialType specialType)
            {
                foreach (var argument in type.Arguments)
                    if (argument.SpecialType == specialType)
                        return argument.TypeName.FullName;
                return null;
            }

            string EmitValueParser(string returnType, string method, string tryExpression, string constraints, string outAnnotation)
            {
                method = GetImplementationName(type, method);
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

            lines.Append("    private static string DocumentCore").Append(type.ImplementationSuffix).AppendLine("(string commandName)");
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
                    lines.Append("        lines.Add(").Append(GetDocumentationLineExpression("documentedCommandName")).AppendLine(");");
                }

                lines.AppendLine("        string documentation;");
                for (var i = 0; i < type.Choices.Length; i++)
                {
                    lines.Append("        documentation = DocumentChoice").Append(type.ImplementationSuffix).Append('<')
                        .Append(type.Choices[i].TypeName.FullName)
                        .AppendLine(">(documentedCommandName);");
                    lines.AppendLine("        if (documentation.Length > 0)");
                    lines.AppendLine("            lines.Add(documentation);");
                }

                lines.AppendLine("        return string.Join(global::System.Environment.NewLine, lines);");
            }
            else
            {
                lines.Append("        var lines = new global::System.Collections.Generic.List<string> { ")
                    .Append(GetDocumentationLineExpression(GetDocumentedCommandNameExpression(type, "commandName")))
                    .AppendLine(" };");

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
                lines.AppendLine($@"    private static string DocumentChoice{type.ImplementationSuffix}<TChoice>(string commandName)
        where TChoice : global::Lokad.Cli.ICliParsed<TChoice>
    {{
        return TChoice.Document(commandName);
    }}");
            }

            return lines.ToString().TrimEnd();

            string GetDocumentationLineExpression(string commandNameExpression) => string.IsNullOrEmpty(type.Documentation)
                ? commandNameExpression
                : commandNameExpression + " + " + FormatStringLiteral(": " + type.Documentation);
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
                if (argument.IsOptionalValue)
                {
                    var names = new System.Collections.Generic.List<string>();
                    var valueName = GetDocumentationName(argument);
                    foreach (var label in argument.Options.Labels)
                        names.Add(label + (label.StartsWith("--", StringComparison.Ordinal) ? "[=" : "[") + valueName + "]");
                    return string.Join(", ", names);
                }
                var labels = string.Join(", ", argument.Options.Labels);
                return argument.ShortNumeric ? labels + ", " + CliArgumentInfo.NumericShortLabel : labels;
            }

            if (argument.ShortNumeric)
                return CliArgumentInfo.NumericShortLabel;

            return GetPositionalDocumentationPlaceholder(argument);
        }

        private static string GetPositionalDocumentationPlaceholder(CliArgumentInfo argument)
        {
            var name = GetDocumentationName(argument);
            if (argument.IsCollection)
                return "<" + name + "...>";

            if (argument.IsNullable)
                return "<" + name + "?>";

            return "<" + name + ">";
        }

        private static string GetDocumentationName(CliArgumentInfo argument)
        {
            var name = argument.DocumentationName;
            return name == null || name.Length == 0 ? argument.Name : name;
        }

        private static string EmitDiscrimination(CliTypeInfo type)
        {
            if (type.Choices.Length == 0)
                return "        _ = tokenized;";

            var lines = new System.Text.StringBuilder();
            lines.AppendLine("        var choice = -1;");
            if (Array.Exists(type.Choices, choice => choice.Discriminator != null))
                lines.AppendLine("        string? choiceDiscriminator = null;");

            for (var i = 0; i < type.Choices.Length; i++)
            {
                var discriminator = type.Choices[i].Discriminator;
                if (discriminator == null)
                    continue;

                lines.Append("        if (").Append(FormatDiscriminatorTest(discriminator)).AppendLine(")");
                lines.AppendLine("        {");
                lines.AppendLine("            if (choice >= 0)");
                lines.Append("                throw new global::Lokad.Cli.ParseException(")
                    .Append(FormatStringLiteral("incompatible options `"))
                    .Append(" + choiceDiscriminator + ")
                    .Append(FormatStringLiteral("` and `" + discriminator.Display + "`"))
                    .AppendLine(");");
                lines.Append("            choice = ").Append(i).AppendLine(";");
                lines.Append("            choiceDiscriminator = ").Append(FormatStringLiteral(discriminator.Display)).AppendLine(";");
                lines.AppendLine("        }");
            }

            var defaultChoice = Array.FindIndex(type.Choices, choice => choice.Discriminator == null);
            if (defaultChoice >= 0)
            {
                lines.AppendLine();
                lines.AppendLine("        if (choice < 0)");
                lines.Append("            choice = ").Append(defaultChoice).AppendLine(";");
            }
            else
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
                lines.Append("                return ParseChoice").Append(type.ImplementationSuffix).Append('<').Append(type.Choices[i].TypeName.FullName).AppendLine(">(tokenized);");
            }

            lines.AppendLine("        }");
            return lines.ToString().TrimEnd();

            static string FormatDiscriminatorTest(CliDiscriminatorInfo discriminator)
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

            static string FormatExpectedDiscriminators(CliTypeInfo type)
            {
                var discriminators = new System.Collections.Generic.List<CliDiscriminatorInfo>();
                foreach (var choice in type.Choices)
                    if (choice.Discriminator is CliDiscriminatorInfo discriminator)
                        discriminators.Add(discriminator);

                if (discriminators.Count == 1)
                    return "`" + discriminators[0].Display + "`";

                if (discriminators.Count == 2)
                    return "`" + discriminators[0].Display + "` or `" + discriminators[1].Display + "`";

                var text = new System.Text.StringBuilder();
                for (var i = 0; i < discriminators.Count; i++)
                {
                    if (i > 0)
                        text.Append(i == discriminators.Count - 1 ? " or " : ", ");

                    text.Append('`').Append(discriminators[i].Display).Append('`');
                }

                return text.ToString();
            }
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

        private static string FormatIdentifier(string name) =>
            SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None || SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.None
                ? "@" + name : name;
    }
}
