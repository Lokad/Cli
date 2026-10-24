using Lokad.Cli.Generator.Analysis;
using Lokad.Cli.Generator.Emit;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Lokad.Cli.Generator
{
    [Generator]
    public sealed class LokadCliGenerator : BaseIncrementalGenerator
    {
        private static readonly SymbolDisplayFormat TypeDisplayFormat =
            SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
                SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions |
                SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

        private readonly TypeRecognizer _argumentAttribute =
            Recognize.Type("Lokad.Cli.ArgumentAttribute");

        private readonly TypeRecognizer _argumentsAttribute =
            Recognize.Type("Lokad.Cli.ArgumentsAttribute");

        private readonly TypeRecognizer _choiceAttribute =
            Recognize.Type("Lokad.Cli.ChoiceAttribute");

        private readonly TypeRecognizer _readOnlyList =
            Recognize.Type("System.Collections.Generic.IReadOnlyList`1");

        private readonly TypeRecognizer _readOnlyMemory =
            Recognize.Type("System.ReadOnlyMemory`1");

        private readonly TypeRecognizer _optionalValue =
            Recognize.Type("Lokad.Cli.OptionalValue`1");

        public LokadCliGenerator()
            : base("lkc")
        {
        }

        protected override ITypeGenerationStrategy? GetGenerationStrategyForSymbol(
            Compilation compilation,
            ITypeSymbol typeSymbol,
            CliTypeInfo type)
        {
            if (!HasCliAnnotation(compilation, typeSymbol))
                return null;

            return new ParserGenerationStrategy(CollectParserInfo(compilation, typeSymbol, type));
        }

        private bool HasCliAnnotation(Compilation compilation, ITypeSymbol typeSymbol) =>
            typeSymbol.TryGetAttribute(compilation, _argumentsAttribute) != null ||
            typeSymbol.TryGetAttribute(compilation, _choiceAttribute) != null;

        private CliTypeInfo CollectParserInfo(Compilation compilation, ITypeSymbol typeSymbol, CliTypeInfo type)
        {
            var shortOptions = new List<char>();
            var longOptions = new List<string>();
            var hasShortNumericArguments = false;
            var choices = new List<CliChoiceInfo>();
            var arguments = new List<CliArgumentInfo>();
            var documentation = default(string);
            var discriminatorOptions = new HashSet<string>(StringComparer.Ordinal);
            var tokenization = new CliTokenizationInfo();
            var collected = new Dictionary<ITypeSymbol, CollectedDeclaration>(SymbolEqualityComparer.Default);
            var discriminator = Collect(typeSymbol);

            var owners = new Dictionary<string, string>(StringComparer.Ordinal);
            var positional = arguments.FindAll(argument => !argument.IsOption);
            foreach (var argument in arguments)
            {
                if (argument.ConsumeUntilBoundary)
                {
                    var position = positional.IndexOf(argument);
                    if (!argument.IsCollection || position < 0 || position == positional.Count - 1)
                        throw new GeneratorException(GeneratorException.InvalidCollectionBoundary, argument.Name);
                    if (positional[position + 1].IsCollection)
                        throw new GeneratorException(GeneratorException.AdjacentCollectionBoundary, argument.Name, positional[position + 1].Name);
                }

                if (argument.AllowRepeated)
                {
                    if (!argument.IsBoolean || argument.IsCollection || argument.ShortNumeric || !argument.Options.HasNamedOption)
                        throw new GeneratorException(GeneratorException.InvalidRepeatedFlag, argument.Name);
                }

                foreach (var label in argument.Options.Labels)
                {
                    if (owners.TryGetValue(label, out var owner))
                        throw new GeneratorException(GeneratorException.OptionNameCollision, label, owner, argument.Name);
                    owners.Add(label, argument.Name);

                    if (argument.Options.HasAliases && discriminatorOptions.Contains(label))
                        throw new GeneratorException(GeneratorException.DiscriminatorAliases, argument.Name, label);
                }
            }

            tokenization.Validate();
            var builtBoundaries = new Dictionary<ITypeSymbol, CliCommandBoundaryInfo>(SymbolEqualityComparer.Default);
            var activeBoundaries = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var selectorDiagnostic = tokenization.RequiresBoundaries
                ? GeneratorException.AmbiguousCommandBoundary : GeneratorException.AmbiguousHelpSchema;
            var boundary = tokenization.RequiresBoundaries || tokenization.HasHelpPrefixes ? BuildBoundary(typeSymbol) : null;
            return new CliTypeInfo(
                type.TypeName,
                type.Kind,
                type.Location,
                shortOptions.ToArray(),
                longOptions.ToArray(),
                hasShortNumericArguments,
                choices.ToArray(),
                arguments.ToArray(),
                discriminator,
                documentation,
                tokenization,
                boundary);

            CliCommandBoundaryInfo BuildBoundary(ITypeSymbol declaration)
            {
                if (builtBoundaries.TryGetValue(declaration, out var cached))
                    return cached;
                if (!activeBoundaries.Add(declaration))
                    throw new GeneratorException(selectorDiagnostic, "cyclic choices");
                var info = collected[declaration];
                var operands = new List<CliArgumentInfo>();
                var stopShort = new List<char>();
                var stopLong = new List<string>();
                foreach (var argument in info.Arguments)
                {
                    if (!argument.IsOption)
                        operands.Add(argument);
                    if (argument.StopOptionParsing)
                    {
                        stopShort.AddRange(argument.Options.ShortOptions);
                        stopLong.AddRange(argument.Options.LongOptions);
                    }
                }
                if (stopShort.Count > 0 && tokenization.ShortOptionSyntax != CliShortOptionSyntax.AttachedValues)
                    throw new GeneratorException(GeneratorException.ShortBoundarySyntax);
                int? remainderAfter = null;
                for (var i = 0; i < operands.Count; i++)
                {
                    if (!operands[i].IsRemainder)
                        continue;
                    if (i != operands.Count - 1 || operands.Exists(a => a != operands[i] && a.IsCollection) ||
                        (i == 0 && stopShort.Count == 0 && stopLong.Count == 0 && !tokenization.StopAtFirstOperand))
                        throw new GeneratorException(GeneratorException.InvalidRemainderLayout, operands[i].Name);
                    remainderAfter = i;
                }

                var childBoundaries = new List<CliCommandBoundaryInfo>();
                var selectors = new HashSet<string>(StringComparer.Ordinal);
                var hasDefault = false;
                foreach (var choice in info.Choices)
                {
                    var child = BuildBoundary(choice);
                    var selector = child.Discriminator;
                    if (selector == null)
                    {
                        if (hasDefault || (tokenization.RequiresBoundaries && child.Choices.Count > 0))
                            throw new GeneratorException(selectorDiagnostic, "multiple defaults or a default with nested choices");
                        hasDefault = true;
                    }
                    else
                    {
                        if (selector.Prefix != null) AddSelector("prefix:" + selector.Prefix);
                        if (selector.LongOption != null) AddSelector("--" + selector.LongOption);
                        if (selector.ShortOption != '\0') AddSelector("-" + selector.ShortOption);
                    }
                    childBoundaries.Add(child);
                }
                var result = new CliCommandBoundaryInfo(info.Discriminator, childBoundaries, remainderAfter, stopShort, stopLong, info.IsHelpPrefix);
                activeBoundaries.Remove(declaration);
                builtBoundaries.Add(declaration, result);
                return result;

                void AddSelector(string selector)
                {
                    if (!selectors.Add(selector))
                        throw new GeneratorException(selectorDiagnostic, "duplicate selector " + selector);
                }
            }

            CliDiscriminatorInfo? Collect(ITypeSymbol declaration)
            {
                if (collected.TryGetValue(declaration, out var cached))
                    return cached.Discriminator;

                var isRoot = SymbolEqualityComparer.Default.Equals(declaration, typeSymbol);
                var ownDiscriminator = default(CliDiscriminatorInfo);
                var isHelpPrefix = false;
                foreach (var annotation in declaration.GetAttributes(compilation, _argumentsAttribute))
                {
                    ownDiscriminator = GetDiscriminator(annotation);
                    if (GetNamedBool(annotation, "IsHelp"))
                    {
                        if (ownDiscriminator == null)
                            throw new GeneratorException(GeneratorException.InvalidHelpChoice);
                        if (ownDiscriminator.Prefix != null)
                        {
                            isHelpPrefix = true;
                            tokenization.RequireHelpPrefix();
                        }
                        else
                            tokenization.AddHelpOptions(new CliOptionNames(
                                ownDiscriminator.LongOption is string name ? new[] { name } : Array.Empty<string>(),
                                ownDiscriminator.ShortOption != '\0' ? new[] { ownDiscriminator.ShortOption } : Array.Empty<char>()));
                    }
                    AddDiscriminatorOptions(ownDiscriminator, discriminatorOptions);
                    CollectArgumentNames(annotation, shortOptions, longOptions, ref hasShortNumericArguments);
                    if (isRoot)
                        documentation = GetConstructorString(annotation, 0);
                    foreach (var setting in annotation.NamedArguments)
                    {
                        if (setting.Key == "ShortOptionSyntax" && setting.Value.Value is int syntax)
                            tokenization.ConfigureShortOptions((CliShortOptionSyntax)syntax);
                        if (setting.Key == "AllowSingleDashLongOptions" && setting.Value.Value is bool allow)
                            tokenization.ConfigureSingleDashLongOptions(allow);
                        if (setting.Key == "StopAtFirstOperand" && setting.Value.Value is bool stop)
                            tokenization.ConfigureStopAtFirstOperand(stop);
                    }
                }

                var members = isRoot ? arguments : new List<CliArgumentInfo>();
                if (declaration.TypeKind != TypeKind.Interface)
                    CollectMembers();
                CollectTokenizationOptions(members, ownDiscriminator, tokenization);

                var choiceTypes = new List<ITypeSymbol>();
                foreach (var choice in declaration.GetAttributes(compilation, _choiceAttribute))
                {
                    if (choice.ConstructorArguments.Length == 0 ||
                        !(choice.ConstructorArguments[0].Value is ITypeSymbol choiceType))
                        continue;

                    choiceTypes.Add(choiceType);
                }
                // Cache before following choices and implemented interfaces, which can lead back here.
                collected.Add(declaration, new CollectedDeclaration(ownDiscriminator, members, choiceTypes, isHelpPrefix));
                foreach (var choiceType in choiceTypes)
                {
                    var choiceDiscriminator = Collect(choiceType);
                    if (isRoot)
                        choices.Add(new CliChoiceInfo(GetTypeName(choiceType), choiceDiscriminator));
                }

                foreach (var implementedInterface in declaration.AllInterfaces)
                    if (HasCliAnnotation(compilation, implementedInterface))
                        Collect(implementedInterface);

                return ownDiscriminator;

                void CollectMembers()
                {
                    var constructorBoundNames = new HashSet<string>();
                    foreach (var member in declaration.GetMembers())
                    {
                        if (!(member is IMethodSymbol method) || method.MethodKind != MethodKind.Constructor)
                            continue;

                        foreach (var parameter in method.Parameters)
                        {
                            var argument = parameter.TryGetAttribute(compilation, _argumentAttribute) ??
                                           FindMatchingPropertyArgument(parameter);
                            if (argument == null)
                                continue;

                            var names = CollectArgumentNames(argument, shortOptions, longOptions, ref hasShortNumericArguments);
                            constructorBoundNames.Add(parameter.Name);
                            if (TryGetBindableMemberArgument(compilation, parameter, argument, names,
                                isConstructorParameter: true, out var parameterArgument))
                                members.Add(parameterArgument);
                        }
                    }

                    foreach (var member in declaration.GetMembers())
                    {
                        if (constructorBoundNames.Contains(member.Name))
                            continue;
                        foreach (var argument in member.GetAttributes(compilation, _argumentAttribute))
                        {
                            var names = CollectArgumentNames(argument, shortOptions, longOptions, ref hasShortNumericArguments);
                            if (TryGetBindableMemberArgument(compilation, member, argument, names,
                                isConstructorParameter: false, out var memberArgument))
                                members.Add(memberArgument);
                        }
                    }

                    AttributeData? FindMatchingPropertyArgument(IParameterSymbol parameter)
                    {
                        foreach (var member in declaration.GetMembers(parameter.Name))
                            if (member is IPropertySymbol property)
                                return property.TryGetAttribute(compilation, _argumentAttribute);

                        return null;
                    }
                }

                static void CollectTokenizationOptions(
                    IReadOnlyList<CliArgumentInfo> arguments, CliDiscriminatorInfo? discriminator, CliTokenizationInfo tokenization)
                {
                    var owned = new HashSet<string>(StringComparer.Ordinal);
                    foreach (var argument in arguments)
                    {
                        if (argument.IsHelp)
                            tokenization.AddHelpOptions(argument.Options);
                        if (argument.IsRemainder || argument.StopOptionParsing)
                            tokenization.RequireBoundaries();
                        var mode = argument.IsOptionalValue ? CliOptionValueMode.OptionalAttached :
                            argument.IsBoolean ? CliOptionValueMode.None :
                            argument.ConsumeNextValueVerbatim ? CliOptionValueMode.Verbatim : CliOptionValueMode.Required;
                        tokenization.AddOptions(argument.Options, mode);
                        foreach (var label in argument.Options.Labels)
                            owned.Add(label);
                    }

                    if (discriminator == null)
                        return;
                    var longs = new List<string>();
                    var shorts = new List<char>();
                    if (discriminator.LongOption != null && !owned.Contains("--" + discriminator.LongOption))
                        longs.Add(discriminator.LongOption);
                    if (discriminator.ShortOption != '\0' && !owned.Contains("-" + discriminator.ShortOption))
                        shorts.Add(discriminator.ShortOption);
                    tokenization.AddOptions(new CliOptionNames(longs, shorts), CliOptionValueMode.None);
                }

                static void AddDiscriminatorOptions(CliDiscriminatorInfo? discriminator, ISet<string> options)
                {
                    if (discriminator == null)
                        return;
                    if (discriminator.LongOption != null)
                        options.Add("--" + discriminator.LongOption);
                    if (discriminator.ShortOption != '\0')
                        options.Add("-" + discriminator.ShortOption);
                }

                static CliDiscriminatorInfo? GetDiscriminator(AttributeData attribute)
                {
                    var prefix = GetNamedString(attribute, "Prefix");
                    if (prefix != null && prefix.Length > 0)
                        return new CliDiscriminatorInfo(prefix, prefix, null, '\0');

                    var longOption = GetNamedString(attribute, "Long");
                    var shortOption = GetNamedChar(attribute, "Short");
                    if (!string.IsNullOrEmpty(longOption) || shortOption != '\0')
                    {
                        var display = !string.IsNullOrEmpty(longOption) ? "--" + longOption : "-" + shortOption;
                        return new CliDiscriminatorInfo(display, null, longOption, shortOption);
                    }

                    return null;
                }
            }
        }

        private sealed class CollectedDeclaration
        {
            public CollectedDeclaration(CliDiscriminatorInfo? discriminator,
                IReadOnlyList<CliArgumentInfo> arguments, IReadOnlyList<ITypeSymbol> choices, bool isHelpPrefix)
            {
                Discriminator = discriminator;
                Arguments = arguments;
                Choices = choices;
                IsHelpPrefix = isHelpPrefix;
            }

            public CliDiscriminatorInfo? Discriminator { get; }
            public IReadOnlyList<CliArgumentInfo> Arguments { get; }
            public IReadOnlyList<ITypeSymbol> Choices { get; }
            public bool IsHelpPrefix { get; }
        }

        private bool TryGetBindableMemberArgument(
            Compilation compilation,
            ISymbol symbol,
            AttributeData attribute,
            CliOptionNames optionNames,
            bool isConstructorParameter,
            out CliArgumentInfo argument)
        {
            var type = default(ITypeSymbol);
            var isSettableMember = false;

            if (symbol is IPropertySymbol property)
            {
                type = property.Type;
                isSettableMember = property.SetMethod != null;
            }
            else if (symbol is IFieldSymbol field)
            {
                type = field.Type;
                isSettableMember = !field.IsReadOnly;
            }
            else if (symbol is IParameterSymbol parameter)
            {
                type = parameter.Type;
            }

            if (type == null)
            {
                argument = null!;
                return false;
            }

            var collectionKind = CliCollectionKind.None;
            var containerType = UnwrapNullable(type);
            var valueType = type;
            if (containerType is IArrayTypeSymbol array && array.Rank == 1)
            {
                collectionKind = CliCollectionKind.Array;
                valueType = array.ElementType;
            }
            else if (containerType is INamedTypeSymbol named && named.TypeArguments.Length == 1)
            {
                if (_readOnlyList(compilation, named.OriginalDefinition))
                    collectionKind = CliCollectionKind.ReadOnlyList;
                else if (_readOnlyMemory(compilation, named.OriginalDefinition))
                    collectionKind = CliCollectionKind.ReadOnlyMemory;

                if (collectionKind != CliCollectionKind.None)
                    valueType = named.TypeArguments[0];
            }

            var isOptionalValue = false;
            var optionalContainer = UnwrapNullable(valueType);
            if (optionalContainer is INamedTypeSymbol optional && _optionalValue(compilation, optional.OriginalDefinition))
            {
                if (!SymbolEqualityComparer.Default.Equals(optionalContainer, valueType))
                    throw new GeneratorException(GeneratorException.InvalidOptionalValue, symbol.Name);
                isOptionalValue = true;
                valueType = optional.TypeArguments[0];
                if (UnwrapNullable(valueType) is INamedTypeSymbol nested && _optionalValue(compilation, nested.OriginalDefinition))
                    throw new GeneratorException(GeneratorException.InvalidOptionalValue, symbol.Name);
            }

            var bindableType = UnwrapNullable(valueType);
            var isNullable = !SymbolEqualityComparer.Default.Equals(bindableType, valueType) ||
                             valueType.NullableAnnotation == NullableAnnotation.Annotated;
            argument = new CliArgumentInfo(
                symbol.Name,
                GetTypeName(bindableType),
                bindableType.IsReferenceType,
                isNullable,
                !isOptionalValue && collectionKind == CliCollectionKind.None && bindableType.SpecialType == SpecialType.System_Boolean,
                bindableType.TypeKind == TypeKind.Enum,
                collectionKind,
                isConstructorParameter,
                isSettableMember,
                GetConstructorString(attribute, 0) ?? string.Empty,
                GetNamedString(attribute, "Name"),
                optionNames,
                GetNamedBool(attribute, "ShortNumeric"),
                GetNamedBool(attribute, "AllowRepeated"),
                GetNamedBool(attribute, "ConsumeUntilBoundary"),
                GetNamedBool(attribute, "ConsumeNextValueVerbatim"),
                isOptionalValue,
                GetNamedBool(attribute, "Remainder"),
                GetNamedBool(attribute, "StopOptionParsing"),
                GetNamedBool(attribute, "IsHelp"));

            if (argument.IsHelp && (!argument.IsBoolean || argument.ShortNumeric || !optionNames.HasNamedOption))
                throw new GeneratorException(GeneratorException.InvalidHelpFlag, argument.Name);

            if (argument.IsRemainder && (argument.IsOption || !argument.IsCollection || isOptionalValue ||
                bindableType.SpecialType != SpecialType.System_String || argument.ConsumeUntilBoundary))
                throw new GeneratorException(GeneratorException.InvalidRemainder, argument.Name);

            if (argument.StopOptionParsing && (argument.IsBoolean || argument.IsCollection || isOptionalValue ||
                argument.ShortNumeric || !optionNames.HasNamedOption))
                throw new GeneratorException(GeneratorException.InvalidStopOption, argument.Name);

            if (isOptionalValue && (!optionNames.HasNamedOption || argument.ShortNumeric || argument.AllowRepeated || argument.ConsumeNextValueVerbatim))
                throw new GeneratorException(GeneratorException.InvalidOptionalValue, argument.Name);

            if (argument.ConsumeNextValueVerbatim &&
                (argument.IsBoolean || argument.ShortNumeric || !argument.Options.HasNamedOption))
                throw new GeneratorException(GeneratorException.InvalidVerbatimValue, argument.Name);

            return true;

            static ITypeSymbol UnwrapNullable(ITypeSymbol value)
            {
                if (value is INamedTypeSymbol nullable &&
                    nullable.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
                {
                    return nullable.TypeArguments[0];
                }

                return value;
            }
        }

        private static CliOptionNames CollectArgumentNames(
            AttributeData attribute,
            List<char> shortOptions,
            List<string> longOptions,
            ref bool hasShortNumericArguments)
        {
            var shortOption = GetNamedChar(attribute, "Short");
            var longOption = GetNamedString(attribute, "Long");
            var allShortOptions = new List<char>();
            var allLongOptions = new List<string>();
            if (shortOption != '\0')
                allShortOptions.Add(shortOption);
            if (longOption != null)
                allLongOptions.Add(longOption);

            AddNames(attribute, "Short", "ShortNames", allShortOptions,
                name => name != '-' && name != '=' && !char.IsWhiteSpace(name) &&
                        !char.IsControl(name) && !char.IsSurrogate(name));
            AddNames(attribute, "Long", "LongNames", allLongOptions, IsValidLongName);
            var names = new CliOptionNames(allLongOptions, allShortOptions);
            foreach (var name in names.ShortOptions)
                if (!shortOptions.Contains(name))
                    shortOptions.Add(name);
            foreach (var name in names.LongOptions)
                if (name.Length > 0 && !longOptions.Contains(name))
                    longOptions.Add(name);

            if (GetNamedBool(attribute, "ShortNumeric"))
                hasShortNumericArguments = true;
            return names;

            static void AddNames<T>(AttributeData annotation, string singular, string plural, List<T> names, Func<T, bool> isValid)
            {
                foreach (var setting in annotation.NamedArguments)
                {
                    if (setting.Key != plural)
                        continue;
                    foreach (var other in annotation.NamedArguments)
                        if (other.Key == singular)
                            throw new GeneratorException(GeneratorException.ConflictingOptionNames, singular, plural);
                    if (setting.Value.Kind != TypedConstantKind.Array || setting.Value.IsNull)
                        throw new GeneratorException(GeneratorException.InvalidOptionName, plural);
                    foreach (var item in setting.Value.Values)
                    {
                        if (!(item.Value is T name) || !isValid(name))
                            throw new GeneratorException(GeneratorException.InvalidOptionName, plural);
                        if (!names.Contains(name))
                            names.Add(name);
                    }
                }
            }

            static bool IsValidLongName(string name)
            {
                if (name.Length == 0 || name[0] == '-')
                    return false;
                foreach (var character in name)
                    if (character == '=' || char.IsWhiteSpace(character) || char.IsControl(character))
                        return false;
                return true;
            }
        }

        private static string? GetNamedString(AttributeData attribute, string name)
        {
            foreach (var argument in attribute.NamedArguments)
                if (argument.Key == name)
                    return argument.Value.Value as string;

            return null;
        }

        private static string? GetConstructorString(AttributeData attribute, int index)
        {
            if (attribute.ConstructorArguments.Length <= index)
                return null;

            return attribute.ConstructorArguments[index].Value as string;
        }

        private static char GetNamedChar(AttributeData attribute, string name)
        {
            foreach (var argument in attribute.NamedArguments)
                if (argument.Key == name && argument.Value.Value is char value)
                    return value;

            return '\0';
        }

        private static bool GetNamedBool(AttributeData attribute, string name)
        {
            foreach (var argument in attribute.NamedArguments)
                if (argument.Key == name && argument.Value.Value is bool value)
                    return value;

            return false;
        }

        private static TypeName GetTypeName(ITypeSymbol typeSymbol) =>
            new TypeName(
                typeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString(TypeDisplayFormat),
                typeSymbol.Name,
                typeSymbol.ContainingNamespace.ToDisplayString());
    }
}
