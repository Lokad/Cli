using Lokad.Cli.Generator.Analysis;
using Lokad.Cli.Generator.Emit;
using Microsoft.CodeAnalysis;
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

            foreach (var argumentsAttribute in typeSymbol.GetAttributes(compilation, _argumentsAttribute))
            {
                discriminator = GetDiscriminator(argumentsAttribute);
                documentation = GetConstructorString(argumentsAttribute, 0);
                CollectArgumentNames(argumentsAttribute, shortOptions, longOptions, ref hasShortNumericArguments);
            }

            foreach (var choice in typeSymbol.GetAttributes(compilation, _choiceAttribute))
                CollectChoice(compilation, choice, shortOptions, longOptions, ref hasShortNumericArguments, choices);

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
                    new HashSet<string>());
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
            List<CliChoiceInfo> choices)
        {
            if (attribute.ConstructorArguments.Length == 0 ||
                !(attribute.ConstructorArguments[0].Value is ITypeSymbol choiceType))
            {
                return;
            }

            var discriminator = default(CliDiscriminatorInfo);
            foreach (var arguments in choiceType.GetAttributes(compilation, _argumentsAttribute))
            {
                discriminator = GetDiscriminator(arguments);
                CollectArgumentNames(arguments, shortOptions, longOptions, ref hasShortNumericArguments);
            }

            CollectReachableArgumentNames(
                compilation,
                choiceType,
                shortOptions,
                longOptions,
                ref hasShortNumericArguments,
                new HashSet<string>());

            choices.Add(new CliChoiceInfo(GetTypeName(choiceType), discriminator));
        }

        private void CollectReachableArgumentNames(
            Compilation compilation,
            ITypeSymbol typeSymbol,
            List<char> shortOptions,
            List<string> longOptions,
            ref bool hasShortNumericArguments,
            HashSet<string> visitedTypes)
        {
            if (!visitedTypes.Add(typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))
                return;

            foreach (var arguments in typeSymbol.GetAttributes(compilation, _argumentsAttribute))
                CollectArgumentNames(arguments, shortOptions, longOptions, ref hasShortNumericArguments);

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
                    visitedTypes);
            }
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

                    CollectArgumentNames(argument, shortOptions, longOptions, ref hasShortNumericArguments);
                    constructorBoundNames.Add(parameter.Name);

                    if (arguments != null &&
                        TryGetBindableMemberArgument(
                            parameter,
                            argument,
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
                    CollectArgumentNames(argument, shortOptions, longOptions, ref hasShortNumericArguments);
                    if (arguments != null &&
                        TryGetBindableMemberArgument(member, argument, isConstructorParameter: false, out var memberArgument))
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

        private static bool TryGetBindableMemberArgument(
            ISymbol symbol,
            AttributeData attribute,
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

            argument = new CliArgumentInfo(
                symbol.Name,
                GetBindableTypeName(type, out var isNullable, out var isArray),
                isNullable,
                IsBooleanType(type),
                GetBindableType(type).TypeKind == TypeKind.Enum,
                isArray,
                isConstructorParameter,
                isSettableMember,
                GetConstructorString(attribute, 0) ?? string.Empty,
                GetNamedString(attribute, "Name"),
                GetNamedString(attribute, "Long"),
                GetNamedChar(attribute, "Short"),
                GetNamedBool(attribute, "ShortNumeric"));
            return true;
        }

        private static TypeName GetBindableTypeName(ITypeSymbol typeSymbol)
        {
            var bindableType = GetBindableType(typeSymbol);
            return GetTypeName(bindableType);
        }

        private static TypeName GetBindableTypeName(ITypeSymbol typeSymbol, out bool isNullable, out bool isArray)
        {
            var bindableType = GetBindableType(typeSymbol);
            isArray = typeSymbol is IArrayTypeSymbol;
            isNullable = !isArray &&
                         !SymbolEqualityComparer.Default.Equals(bindableType, typeSymbol) ||
                         typeSymbol.NullableAnnotation == NullableAnnotation.Annotated;
            return GetTypeName(bindableType);
        }

        private static ITypeSymbol GetBindableType(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is INamedTypeSymbol named &&
                named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T &&
                named.TypeArguments.Length == 1)
            {
                return named.TypeArguments[0];
            }

            if (typeSymbol is IArrayTypeSymbol array && array.Rank == 1)
                return GetBindableType(array.ElementType);

            return typeSymbol;
        }

        private static bool IsBooleanType(ITypeSymbol typeSymbol) =>
            GetBindableType(typeSymbol).SpecialType == SpecialType.System_Boolean;

        private static void CollectArgumentNames(
            AttributeData attribute,
            List<char> shortOptions,
            List<string> longOptions,
            ref bool hasShortNumericArguments)
        {
            var shortOption = GetNamedChar(attribute, "Short");
            if (shortOption != '\0' && !shortOptions.Contains(shortOption))
                shortOptions.Add(shortOption);

            var longOption = GetNamedString(attribute, "Long");
            if (longOption != null && longOption.Length > 0 && !longOptions.Contains(longOption))
                longOptions.Add(longOption);

            if (GetNamedBool(attribute, "ShortNumeric"))
                hasShortNumericArguments = true;
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
