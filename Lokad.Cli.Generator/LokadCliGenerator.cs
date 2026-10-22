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
            var discriminator = default(CliDiscriminatorInfo);
            var documentation = default(string);
            var discriminatorOptions = new HashSet<string>(StringComparer.Ordinal);

            foreach (var argumentsAttribute in typeSymbol.GetAttributes(compilation, _argumentsAttribute))
            {
                discriminator = GetDiscriminator(argumentsAttribute);
                AddDiscriminatorOptions(discriminator, discriminatorOptions);
                documentation = GetConstructorString(argumentsAttribute, 0);
                CollectArgumentNames(argumentsAttribute, shortOptions, longOptions, ref hasShortNumericArguments);
            }

            foreach (var choice in typeSymbol.GetAttributes(compilation, _choiceAttribute))
                CollectChoice(compilation, choice, shortOptions, longOptions, ref hasShortNumericArguments, choices, discriminatorOptions);

            CollectMemberArgumentNames(
                compilation,
                typeSymbol,
                shortOptions,
                longOptions,
                ref hasShortNumericArguments,
                arguments);

            foreach (var implementedInterface in typeSymbol.AllInterfaces)
            {
                if (!HasCliAnnotation(compilation, implementedInterface))
                    continue;

                CollectReachableArgumentNames(
                    compilation,
                    implementedInterface,
                    shortOptions,
                    longOptions,
                    ref hasShortNumericArguments,
                    new HashSet<string>(),
                    discriminatorOptions);
            }

            var owners = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var argument in arguments)
            {
                if (argument.AllowRepeated)
                {
                    var hasNamedOption = argument.Options.ShortOptions.Count > 0;
                    foreach (var name in argument.Options.LongOptions)
                        hasNamedOption |= name.Length > 0;

                    if (!argument.IsBoolean || argument.IsCollection || argument.ShortNumeric || !hasNamedOption)
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
                documentation);
        }

        private void CollectChoice(
            Compilation compilation,
            AttributeData attribute,
            List<char> shortOptions,
            List<string> longOptions,
            ref bool hasShortNumericArguments,
            List<CliChoiceInfo> choices,
            ISet<string> discriminatorOptions)
        {
            if (attribute.ConstructorArguments.Length == 0 ||
                !(attribute.ConstructorArguments[0].Value is ITypeSymbol choiceType))
            {
                return;
            }

            var discriminator = CollectReachableArgumentNames(
                compilation,
                choiceType,
                shortOptions,
                longOptions,
                ref hasShortNumericArguments,
                new HashSet<string>(),
                discriminatorOptions);

            choices.Add(new CliChoiceInfo(GetTypeName(choiceType), discriminator));
        }

        private CliDiscriminatorInfo? CollectReachableArgumentNames(
            Compilation compilation,
            ITypeSymbol typeSymbol,
            List<char> shortOptions,
            List<string> longOptions,
            ref bool hasShortNumericArguments,
            HashSet<string> visitedTypes,
            ISet<string> discriminatorOptions)
        {
            if (!visitedTypes.Add(typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))
                return null;

            var discriminator = default(CliDiscriminatorInfo);
            foreach (var arguments in typeSymbol.GetAttributes(compilation, _argumentsAttribute))
            {
                discriminator = GetDiscriminator(arguments);
                CollectArgumentNames(arguments, shortOptions, longOptions, ref hasShortNumericArguments);
                AddDiscriminatorOptions(discriminator, discriminatorOptions);
            }

            CollectMemberArgumentNames(compilation, typeSymbol, shortOptions, longOptions, ref hasShortNumericArguments);

            foreach (var choice in typeSymbol.GetAttributes(compilation, _choiceAttribute))
            {
                if (choice.ConstructorArguments.Length == 0 ||
                    !(choice.ConstructorArguments[0].Value is ITypeSymbol choiceType))
                {
                    continue;
                }

                CollectReachableArgumentNames(
                    compilation,
                    choiceType,
                    shortOptions,
                    longOptions,
                    ref hasShortNumericArguments,
                    visitedTypes,
                    discriminatorOptions);
            }

            return discriminator;
        }

        private void CollectMemberArgumentNames(
            Compilation compilation,
            ITypeSymbol typeSymbol,
            List<char> shortOptions,
            List<string> longOptions,
            ref bool hasShortNumericArguments,
            List<CliArgumentInfo>? arguments = null)
        {
            var constructorBoundNames = new HashSet<string>();

            foreach (var member in typeSymbol.GetMembers())
            {
                if (!(member is IMethodSymbol method) || method.MethodKind != MethodKind.Constructor)
                    continue;

                foreach (var parameter in method.Parameters)
                {
                    var argument = parameter.TryGetAttribute(compilation, _argumentAttribute) ??
                                   FindMatchingPropertyArgument(compilation, typeSymbol, parameter);
                    if (argument == null)
                        continue;

                    var optionNames = CollectArgumentNames(argument, shortOptions, longOptions, ref hasShortNumericArguments);
                    constructorBoundNames.Add(parameter.Name);

                    if (arguments != null &&
                        TryGetBindableMemberArgument(
                            compilation,
                            parameter,
                            argument,
                            optionNames,
                            isConstructorParameter: true,
                            out var parameterArgument))
                    {
                        arguments.Add(parameterArgument);
                    }
                }
            }

            foreach (var member in typeSymbol.GetMembers())
            {
                if (constructorBoundNames.Contains(member.Name))
                    continue;

                foreach (var argument in member.GetAttributes(compilation, _argumentAttribute))
                {
                    var optionNames = CollectArgumentNames(argument, shortOptions, longOptions, ref hasShortNumericArguments);
                    if (arguments != null &&
                        TryGetBindableMemberArgument(compilation, member, argument, optionNames, isConstructorParameter: false, out var memberArgument))
                    {
                        arguments.Add(memberArgument);
                    }
                }
            }
        }

        private AttributeData? FindMatchingPropertyArgument(
            Compilation compilation,
            ITypeSymbol typeSymbol,
            IParameterSymbol parameter)
        {
            foreach (var member in typeSymbol.GetMembers(parameter.Name))
                if (member is IPropertySymbol property)
                    return property.TryGetAttribute(compilation, _argumentAttribute);

            return null;
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

            var bindableType = UnwrapNullable(valueType);
            var isNullable = !SymbolEqualityComparer.Default.Equals(bindableType, valueType) ||
                             valueType.NullableAnnotation == NullableAnnotation.Annotated;
            argument = new CliArgumentInfo(
                symbol.Name,
                GetTypeName(bindableType),
                isNullable,
                collectionKind == CliCollectionKind.None && bindableType.SpecialType == SpecialType.System_Boolean,
                bindableType.TypeKind == TypeKind.Enum,
                collectionKind,
                isConstructorParameter,
                isSettableMember,
                GetConstructorString(attribute, 0) ?? string.Empty,
                GetNamedString(attribute, "Name"),
                optionNames,
                GetNamedBool(attribute, "ShortNumeric"),
                GetNamedBool(attribute, "AllowRepeated"));

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

        private static void AddDiscriminatorOptions(CliDiscriminatorInfo? discriminator, ISet<string> options)
        {
            if (discriminator == null)
                return;
            if (discriminator.LongOption != null)
                options.Add("--" + discriminator.LongOption);
            if (discriminator.ShortOption != '\0')
                options.Add("-" + discriminator.ShortOption);
        }

        private static CliDiscriminatorInfo? GetDiscriminator(AttributeData attribute)
        {
            var prefix = GetNamedString(attribute, "Prefix");
            if (!string.IsNullOrEmpty(prefix))
                return new CliDiscriminatorInfo(prefix!, prefix, null, '\0');

            var longOption = GetNamedString(attribute, "Long");
            var shortOption = GetNamedChar(attribute, "Short");
            if (!string.IsNullOrEmpty(longOption) || shortOption != '\0')
            {
                var display = !string.IsNullOrEmpty(longOption) ? "--" + longOption : "-" + shortOption;
                return new CliDiscriminatorInfo(display, null, longOption, shortOption);
            }

            return null;
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
                typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                typeSymbol.Name,
                typeSymbol.ContainingNamespace.ToDisplayString());
    }
}
