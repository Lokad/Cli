using Lokad.Cli.Generator.Analysis;
using Lokad.Cli.Generator.Emit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace Lokad.Cli.Generator
{
    [Generator]
    public sealed class LokadCliGenerator : BaseIncrementalGenerator
    {
        private readonly TypeRecognizer _argumentAttribute =
            Recognize.CliType("Lokad.Cli.ArgumentAttribute");

        private readonly TypeRecognizer _argumentsAttribute =
            Recognize.CliType("Lokad.Cli.ArgumentsAttribute");

        private readonly TypeRecognizer _choiceAttribute =
            Recognize.CliType("Lokad.Cli.ChoiceAttribute");

        private readonly TypeRecognizer _readOnlyList =
            Recognize.Type("System.Collections.Generic.IReadOnlyList`1");

        private readonly TypeRecognizer _readOnlyMemory =
            Recognize.Type("System.ReadOnlyMemory`1");

        private readonly TypeRecognizer _optionalValue =
            Recognize.CliType("Lokad.Cli.OptionalValue`1");

        public LokadCliGenerator()
            : base("lkc")
        {
        }

        protected override ITypeGenerationStrategy GetGenerationStrategyForSymbol(
            Compilation compilation,
            ITypeSymbol typeSymbol)
        {
            if (typeSymbol.ContainingType != null)
                throw Unsupported("nested types are not supported");
            if (typeSymbol is INamedTypeSymbol named && named.Arity != 0)
                throw Unsupported("generic types are not supported");
            if (typeSymbol is INamedTypeSymbol refLike && refLike.IsRefLikeType)
                throw Unsupported("ref-like types cannot implement the parser contract");
            if (typeSymbol.IsStatic)
                throw Unsupported("static types cannot implement the parser contract");
            if (typeSymbol.TypeKind == TypeKind.Interface)
            {
                if (typeSymbol.TryGetAttribute(compilation, _choiceAttribute) == null)
                    throw Unsupported("interfaces require at least one choice");
            }
            else if (typeSymbol.IsAbstract)
                throw Unsupported("argument types must be concrete");
            foreach (var reference in typeSymbol.DeclaringSyntaxReferences)
            {
                if (!(reference.GetSyntax() is TypeDeclarationSyntax declaration))
                    continue;
                if (!System.Linq.Enumerable.Any(declaration.Modifiers, modifier => modifier.IsKind(SyntaxKind.PartialKeyword)))
                    throw Unsupported("argument types must be partial");
                if (System.Linq.Enumerable.Any(declaration.Modifiers, modifier => modifier.ValueText == "file"))
                    throw Unsupported("file-local types cannot be extended from generated source");
            }
            return new ParserGenerationStrategy(CollectParserInfo(compilation, typeSymbol));

            GeneratorException Unsupported(string reason) =>
                new GeneratorException(GeneratorException.UnsupportedArgumentType, typeSymbol.Name, reason);
        }

        protected override bool HasCliAnnotation(Compilation compilation, ITypeSymbol typeSymbol) =>
            typeSymbol.TryGetAttribute(compilation, _argumentsAttribute) != null ||
            typeSymbol.TryGetAttribute(compilation, _choiceAttribute) != null;

        private CliTypeInfo CollectParserInfo(Compilation compilation, ITypeSymbol typeSymbol)
        {
            var typeNames = new TypeNameFormatter(compilation);
            var hasShortNumericArguments = false;
            var choices = new List<CliChoiceInfo>();
            var rootMembers = Array.Empty<CliArgumentInfo>();
            var documentation = default(string);
            var discriminatorOptions = new HashSet<string>(StringComparer.Ordinal);
            var tokenization = new CliTokenizationInfo();
            var collected = new Dictionary<ITypeSymbol, CollectedDeclaration>(SymbolEqualityComparer.Default);
            var activeGroups = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var configuredPolicies = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var discriminator = Collect(typeSymbol);
            var arguments = collected[typeSymbol].Arguments;

            var owners = new Dictionary<string, string>(StringComparer.Ordinal);
            var positional = Array.FindAll(arguments, argument => !argument.IsOption);
            foreach (var argument in arguments)
            {
                if (argument.ConsumeUntilBoundary)
                {
                    var position = Array.IndexOf(positional, argument);
                    if (!argument.IsCollection || position < 0 || position == positional.Length - 1)
                        throw new GeneratorException(GeneratorException.InvalidCollectionBoundary, argument.Name);
                    if (positional[position + 1].IsCollection)
                        throw new GeneratorException(GeneratorException.AdjacentCollectionBoundary, argument.Name, positional[position + 1].Name);
                }

                if (argument.AllowRepeated)
                {
                    if (!argument.IsBoolean || argument.IsCollection || argument.ShortNumeric || !argument.Options.HasNamedOption)
                        throw new GeneratorException(GeneratorException.InvalidRepeatedFlag, argument.Name);
                }

                if (argument.IsBoolean && (!argument.Options.HasNamedOption || argument.ShortNumeric))
                    throw new GeneratorException(argument.Location, GeneratorException.InvalidBooleanFlag, argument.Path);

                if (argument.ShortNumeric)
                    AddOwner("-<number>", argument);
                foreach (var label in argument.Options.Labels)
                    AddOwner(label, argument);
            }

            if (hasShortNumericArguments)
                foreach (var name in tokenization.ShortOptions.Keys)
                    if (name >= '0' && name <= '9')
                        throw new GeneratorException(GeneratorException.NumericShortCollision, name);

            var commandSchemas = new Dictionary<ITypeSymbol, CliCommandBoundaryInfo>(SymbolEqualityComparer.Default);
            var activeCommands = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var selectorDiagnostic = tokenization.RequiresBoundaries
                ? GeneratorException.AmbiguousCommandBoundary
                : tokenization.HasHelpPrefixes ? GeneratorException.AmbiguousHelpSchema : GeneratorException.InvalidChoiceSchema;
            var schema = BuildCommandSchema(typeSymbol);
            tokenization.Validate();
            var boundary = tokenization.RequiresBoundaries || tokenization.HasHelpPrefixes ? schema : null;
            var existingNames = new HashSet<string>(StringComparer.Ordinal);
            for (var current = typeSymbol; current != null; current = current.BaseType)
                ReserveNames(current);
            foreach (var inherited in typeSymbol.AllInterfaces)
                ReserveNames(inherited);
            var explicitParserMethods = CliParserMethods.None;
            foreach (var method in new[] { CliParserMethods.Parse, CliParserMethods.Tokenize, CliParserMethods.ParseTokenized, CliParserMethods.Document })
                if (existingNames.Contains(method.ToString()))
                    explicitParserMethods |= method;
            var implementationSuffix = "__Cli";
            while (System.Linq.Enumerable.Any(existingNames, name => name.EndsWith(implementationSuffix, StringComparison.Ordinal)))
                implementationSuffix += "_";
            return new CliTypeInfo(
                typeNames.FromSymbol(typeSymbol),
                GetTypeKind(typeSymbol),
                explicitParserMethods,
                implementationSuffix,
                hasShortNumericArguments,
                choices.ToArray(),
                rootMembers,
                arguments,
                discriminator,
                documentation,
                tokenization,
                boundary,
                typeNames.ExternAliases);

            void ReserveNames(ITypeSymbol declaration)
            {
                existingNames.Add(declaration.Name);
                foreach (var member in declaration.GetMembers())
                    existingNames.Add(member.Name);
            }

            static CliTypeKind GetTypeKind(ITypeSymbol symbol)
            {
                if (symbol.TypeKind == TypeKind.Interface)
                    return CliTypeKind.Interface;
                if (symbol.IsRecord)
                    return symbol.TypeKind == TypeKind.Struct ? CliTypeKind.RecordStruct : CliTypeKind.Record;
                return symbol.TypeKind == TypeKind.Struct ? CliTypeKind.Struct : CliTypeKind.Class;
            }

            void AddOwner(string label, CliArgumentInfo argument)
            {
                if (owners.TryGetValue(label, out var owner))
                    throw new GeneratorException(argument.Location, GeneratorException.OptionNameCollision, label, owner, argument.Path);
                owners.Add(label, argument.Path);
            }

            CliCommandBoundaryInfo BuildCommandSchema(ITypeSymbol declaration)
            {
                if (commandSchemas.TryGetValue(declaration, out var cached))
                    return cached;
                if (!activeCommands.Add(declaration))
                    throw new GeneratorException(selectorDiagnostic, "cyclic choices");
                var info = collected[declaration];
                if (info.Discriminator?.Prefix != null)
                    tokenization.RequirePositionalPrefix();
                var operands = new List<CliArgumentInfo>();
                var stopShort = new List<char>();
                var stopLong = new List<string>();
                foreach (var argument in info.Arguments)
                {
                    if (argument.Options.HasAliases)
                        foreach (var label in argument.Options.Labels)
                            if (discriminatorOptions.Contains(label))
                                throw new GeneratorException(argument.Location, GeneratorException.DiscriminatorAliases, argument.Name, label);
                    if (!argument.IsOption)
                        operands.Add(argument);
                    if (argument.StopOptionParsing)
                    {
                        stopShort.AddRange(argument.Options.ShortOptions);
                        stopLong.AddRange(argument.Options.LongOptions);
                    }
                }
                CliArgumentInfo? optionalOperand = null;
                foreach (var operand in operands)
                {
                    if (operand.IsRequired && optionalOperand != null)
                        throw new GeneratorException(operand.Location, GeneratorException.InvalidPositionalOrder,
                            optionalOperand.Path, operand.Path);
                    if (!operand.IsCollection && operand.IsNullable)
                        optionalOperand ??= operand;
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
                    var child = BuildCommandSchema(choice);
                    var selector = child.Discriminator;
                    if (selector == null)
                    {
                        if (hasDefault)
                            throw new GeneratorException(selectorDiagnostic, "multiple default choices");
                        if (tokenization.RequiresBoundaries && child.Choices.Count > 0)
                            throw new GeneratorException(selectorDiagnostic, "a default choice with nested choices");
                        hasDefault = true;
                    }
                    else
                    {
                        if (selector.Prefix != null)
                            AddSelector("prefix:" + selector.Prefix);
                        if (selector.LongOption != null) AddSelector("--" + selector.LongOption);
                        if (selector.ShortOption != '\0') AddSelector("-" + selector.ShortOption);
                    }
                    childBoundaries.Add(child);
                }
                var result = new CliCommandBoundaryInfo(info.Discriminator, childBoundaries, remainderAfter, stopShort, stopLong, info.IsHelpPrefix);
                activeCommands.Remove(declaration);
                commandSchemas.Add(declaration, result);
                return result;

                void AddSelector(string selector)
                {
                    if (!selectors.Add(selector))
                        throw new GeneratorException(selectorDiagnostic, "duplicate selector " + selector);
                }
            }

            void ConfigureTokenization(ITypeSymbol declaration)
            {
                Configure(declaration);
                foreach (var implementedInterface in declaration.AllInterfaces)
                    Configure(implementedInterface);

                void Configure(ITypeSymbol source)
                {
                    if (!configuredPolicies.Add(source))
                        return;
                    foreach (var annotation in source.GetAttributes(compilation, _argumentsAttribute))
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
            }

            CliArgumentInfo[] CollectMembers(ITypeSymbol declaration, string path, AttributeData? groupUsage, ITypeSymbol constructionContext)
            {
                for (var parent = declaration.BaseType; parent != null &&
                    parent.SpecialType != SpecialType.System_Object && parent.SpecialType != SpecialType.System_ValueType;
                    parent = parent.BaseType)
                {
                    if (!HasCliAnnotation(compilation, parent) &&
                        !System.Linq.Enumerable.Any(parent.GetMembers(), member => member.TryGetAttribute(compilation, _argumentAttribute) != null))
                        continue;
                    const string reason = "inherited CLI annotations are not supported; use an unnamed argument group";
                    if (groupUsage != null)
                        throw GroupError(groupUsage, path, reason);
                    throw new GeneratorException(GeneratorException.UnsupportedArgumentType, declaration.Name, reason);
                }
                var isGroup = groupUsage != null;
                activeGroups.Add(declaration.OriginalDefinition);
                var members = new List<CliArgumentInfo>();
                var constructorBoundMembers = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
                IMethodSymbol? boundConstructor = null;
                var constructorArguments = new List<(IParameterSymbol Parameter, AttributeData Annotation, ISymbol? Member)>();
                foreach (var member in declaration.GetMembers())
                {
                    if (!(member is IMethodSymbol method) || method.MethodKind != MethodKind.Constructor)
                        continue;
                    // Record copy constructors do not describe command-line inputs.
                    if (method.IsImplicitlyDeclared && method.Parameters.Length > 0)
                        continue;
                    var parameters = new List<(IParameterSymbol Parameter, AttributeData Annotation, ISymbol? Member)>();
                    foreach (var parameter in method.Parameters)
                    {
                        var matchingMember = FindMatchingMember(parameter);
                        var annotation = parameter.TryGetAttribute(compilation, _argumentAttribute) ??
                                         (matchingMember is IPropertySymbol ? matchingMember.TryGetAttribute(compilation, _argumentAttribute) : null);
                        if (annotation == null)
                            continue;
                        if (parameter.RefKind != RefKind.None)
                            throw ConstructorError("bound parameters cannot use ref, in or out");
                        parameters.Add((parameter, annotation, matchingMember));
                    }
                    if (parameters.Count == 0)
                        continue;
                    if (boundConstructor != null)
                        throw ConstructorError("only one constructor may bind arguments");
                    if (parameters.Count != method.Parameters.Length)
                        throw ConstructorError("every parameter of the bound constructor must be annotated or match an annotated property");
                    if (isGroup && !compilation.IsSymbolAccessibleWithin(method, constructionContext, declaration))
                        throw GroupError(parameters[0].Annotation, path, "its constructor is inaccessible from the containing command");
                    boundConstructor = method;
                    constructorArguments = parameters;
                }
                if (boundConstructor == null && declaration is INamedTypeSymbol named)
                {
                    var constructorCount = 0;
                    foreach (var constructor in named.InstanceConstructors)
                    {
                        if (!compilation.IsSymbolAccessibleWithin(constructor, constructionContext, declaration) ||
                            !System.Linq.Enumerable.All(constructor.Parameters, parameter => parameter.IsOptional || parameter.IsParams))
                            continue;
                        if (constructor.Parameters.Length == 0)
                        {
                            constructorCount = 1;
                            break;
                        }
                        constructorCount++;
                    }
                    if (constructorCount == 0)
                    {
                        if (groupUsage != null)
                            throw GroupError(groupUsage, path, "no constructor callable without arguments is accessible from the containing command");
                        throw ConstructorError("a constructor callable without arguments or a fully annotated constructor is required");
                    }
                    if (constructorCount > 1 && HasAmbiguousZeroArgumentCall())
                    {
                        if (groupUsage != null)
                            throw GroupError(groupUsage, path, "its constructor call without arguments is ambiguous");
                        throw ConstructorError("the constructor call without arguments is ambiguous");
                    }
                }
                var constructorSetsRequiredMembers = boundConstructor != null && System.Linq.Enumerable.Any(
                    boundConstructor.GetAttributes(), attribute =>
                        attribute.AttributeClass?.ToDisplayString() == "System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute");
                foreach (var parameter in constructorArguments)
                {
                    if (parameter.Member != null)
                    {
                        if (!constructorSetsRequiredMembers && IsRequiredMember(parameter.Member))
                        {
                            var reason = "constructor-bound required member '" + parameter.Member.Name +
                                         "' requires SetsRequiredMembers on its constructor";
                            if (groupUsage != null)
                                throw GroupError(groupUsage, path, reason);
                            throw new GeneratorException(GetAttributeLocation(parameter.Annotation),
                                GeneratorException.InvalidArgumentConstructor, declaration.Name, reason);
                        }
                        constructorBoundMembers.Add(parameter.Member);
                    }
                    AddMember(parameter.Parameter, parameter.Annotation, true);
                }
                foreach (var member in declaration.GetMembers())
                {
                    if (constructorBoundMembers.Contains(member))
                        continue;
                    foreach (var annotation in member.GetAttributes(compilation, _argumentAttribute))
                    {
                        var assignment = member is IPropertySymbol property ? property.SetMethod : member;
                        if (isGroup && assignment != null && !compilation.IsSymbolAccessibleWithin(assignment, constructionContext, declaration))
                            throw GroupError(annotation, path, "a bound member is inaccessible from the containing command");
                        AddMember(member, annotation, false);
                    }
                }
                activeGroups.Remove(declaration.OriginalDefinition);
                return members.ToArray();

                GeneratorException ConstructorError(string reason) =>
                    new GeneratorException(GeneratorException.InvalidArgumentConstructor, declaration.Name, reason);

                static bool IsRequiredMember(ISymbol symbol) =>
                    symbol is IPropertySymbol property && property.IsRequired || symbol is IFieldSymbol field && field.IsRequired;

                bool HasAmbiguousZeroArgumentCall()
                {
                    TypeDeclarationSyntax? context = null;
                    foreach (var reference in constructionContext.DeclaringSyntaxReferences)
                    {
                        if (!compilation.ContainsSyntaxTree(reference.SyntaxTree) || !(reference.GetSyntax() is TypeDeclarationSyntax part))
                            continue;
                        context = part;
                        if (part.OpenBraceToken.RawKind != 0 && !part.OpenBraceToken.IsMissing)
                            break;
                    }
                    // Referenced choices construct themselves; this generator emits no constructor call for them.
                    if (context == null)
                        return false;
                    var position = context.OpenBraceToken.RawKind == 0 || context.OpenBraceToken.IsMissing
                        ? context.Identifier.Span.End : context.OpenBraceToken.Span.End;
                    var creation = SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName(typeNames.FromSymbol(declaration).FullName))
                        .WithArgumentList(SyntaxFactory.ArgumentList());
                    var info = compilation.GetSemanticModel(context.SyntaxTree).GetSpeculativeSymbolInfo(
                        position, creation, SpeculativeBindingOption.BindAsExpression);
                    return info.CandidateReason == CandidateReason.OverloadResolutionFailure;
                }

                void AddMember(ISymbol member, AttributeData annotation, bool isConstructorParameter)
                {
                    var names = CollectArgumentNames(annotation, ref hasShortNumericArguments);
                    var memberPath = path.Length == 0 ? member.Name : path + "." + member.Name;
                    if (member.IsStatic || !member.CanBeReferencedByName)
                        throw InvalidTarget();
                    var type = default(ITypeSymbol);
                    var isSettableMember = false;

                    if (member is IPropertySymbol property)
                    {
                        type = property.Type;
                        isSettableMember = property.SetMethod != null;
                    }
                    else if (member is IFieldSymbol field)
                    {
                        type = field.Type;
                        isSettableMember = !field.IsReadOnly;
                    }
                    else if (member is IParameterSymbol parameter)
                    {
                        type = parameter.Type;
                    }

                    if (type == null)
                        return;

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
                            throw new GeneratorException(GeneratorException.InvalidOptionalValue, member.Name);
                        isOptionalValue = true;
                        valueType = optional.TypeArguments[0];
                        if (UnwrapNullable(valueType) is INamedTypeSymbol nested && _optionalValue(compilation, nested.OriginalDefinition))
                            throw new GeneratorException(GeneratorException.InvalidOptionalValue, member.Name);
                    }

                    var bindableType = UnwrapNullable(valueType);
                    var isNullable = !SymbolEqualityComparer.Default.Equals(bindableType, valueType) ||
                                     valueType.NullableAnnotation == NullableAnnotation.Annotated;
                    IReadOnlyList<CliArgumentInfo>? groupMembers = null;
                    if (HasCliAnnotation(compilation, bindableType))
                    {
                        if (names.HasNamedOption || isNullable || collectionKind != CliCollectionKind.None || isOptionalValue ||
                            GetNamedBool(annotation, "ShortNumeric") || GetNamedBool(annotation, "AllowRepeated") ||
                            GetNamedBool(annotation, "ConsumeUntilBoundary") || GetNamedBool(annotation, "ConsumeNextValueVerbatim") ||
                            GetNamedBool(annotation, "Remainder") || GetNamedBool(annotation, "StopOptionParsing") || GetNamedBool(annotation, "IsHelp"))
                            throw GroupError(annotation, memberPath, "use a non-nullable positional member without a collection or scalar binding options");
                        if (!isConstructorParameter && !isSettableMember)
                            throw GroupError(annotation, memberPath, "the group must be constructor-bound or writable");
                        groupMembers = CollectGroup(bindableType, memberPath, annotation, constructionContext);
                    }
                    else if (!isConstructorParameter && !isSettableMember)
                        throw InvalidTarget();
                    var argument = new CliArgumentInfo(
                        member.Name,
                        typeNames.FromSymbol(bindableType),
                        bindableType.SpecialType,
                        bindableType.IsReferenceType,
                        isNullable,
                        !isOptionalValue && collectionKind == CliCollectionKind.None && bindableType.SpecialType == SpecialType.System_Boolean,
                        bindableType.TypeKind == TypeKind.Enum,
                        collectionKind,
                        isConstructorParameter ? typeNames.FormatSymbol(type) : null,
                        GetConstructorString(annotation, 0) ?? string.Empty,
                        GetNamedString(annotation, "Name"),
                        names,
                        GetNamedBool(annotation, "ShortNumeric"),
                        GetNamedBool(annotation, "AllowRepeated"),
                        GetNamedBool(annotation, "ConsumeUntilBoundary"),
                        GetNamedBool(annotation, "ConsumeNextValueVerbatim"),
                        isOptionalValue,
                        GetNamedBool(annotation, "Remainder"),
                        GetNamedBool(annotation, "StopOptionParsing"),
                        GetNamedBool(annotation, "IsHelp"),
                        groupMembers,
                        memberPath,
                        GetAttributeLocation(annotation));

                    if (argument.IsHelp && (!argument.IsBoolean || argument.ShortNumeric || !names.HasNamedOption))
                        throw new GeneratorException(GeneratorException.InvalidHelpFlag, argument.Name);

                    if (argument.IsRemainder && (argument.IsOption || !argument.IsCollection || isOptionalValue ||
                        bindableType.SpecialType != SpecialType.System_String || argument.ConsumeUntilBoundary))
                        throw new GeneratorException(GeneratorException.InvalidRemainder, argument.Name);

                    if (argument.StopOptionParsing && (argument.IsBoolean || argument.IsCollection || isOptionalValue ||
                        argument.ShortNumeric || !names.HasNamedOption))
                        throw new GeneratorException(GeneratorException.InvalidStopOption, argument.Name);

                    if (isOptionalValue && (!names.HasNamedOption || argument.ShortNumeric || argument.AllowRepeated || argument.ConsumeNextValueVerbatim))
                        throw new GeneratorException(GeneratorException.InvalidOptionalValue, argument.Name);

                    if (argument.ConsumeNextValueVerbatim &&
                        (argument.IsBoolean || argument.ShortNumeric || !argument.Options.HasNamedOption))
                        throw new GeneratorException(GeneratorException.InvalidVerbatimValue, argument.Name);

                    members.Add(argument);

                    GeneratorException InvalidTarget() =>
                        new GeneratorException(GetAttributeLocation(annotation), GeneratorException.InvalidBindingTarget, memberPath);

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

                ISymbol? FindMatchingMember(IParameterSymbol parameter)
                {
                    foreach (var member in declaration.GetMembers(parameter.Name))
                        if ((member is IPropertySymbol property && SymbolEqualityComparer.Default.Equals(property.Type, parameter.Type)) ||
                            (member is IFieldSymbol field && SymbolEqualityComparer.Default.Equals(field.Type, parameter.Type)))
                            return member;
                    return null;
                }
            }

            IReadOnlyList<CliArgumentInfo> CollectGroup(ITypeSymbol declaration, string path, AttributeData usage, ITypeSymbol constructionContext)
            {
                var annotation = declaration.TryGetAttribute(compilation, _argumentsAttribute);
                if (annotation == null || declaration.TryGetAttribute(compilation, _choiceAttribute) != null ||
                    declaration.TypeKind == TypeKind.Interface || declaration.IsAbstract || GetDiscriminator(annotation) != null)
                    throw GroupError(usage, path, "use a concrete [Arguments] type without choices or a discriminator");
                if (activeGroups.Contains(declaration.OriginalDefinition))
                    throw GroupError(usage, path, "recursive argument groups are not supported");
                ConfigureTokenization(declaration);
                return CollectMembers(declaration, path, usage, constructionContext);
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
                    CollectArgumentNames(annotation, ref hasShortNumericArguments);
                    if (isRoot)
                        documentation = GetConstructorString(annotation, 0);
                }

                ConfigureTokenization(declaration);
                var members = declaration.TypeKind == TypeKind.Interface
                    ? Array.Empty<CliArgumentInfo>() : CollectMembers(declaration, "", null, declaration);
                if (isRoot)
                    rootMembers = members;
                var leaves = new List<CliArgumentInfo>();
                AppendLeaves(members);
                var flattened = leaves.ToArray();
                CollectTokenizationOptions(flattened, ownDiscriminator, tokenization);

                var choiceTypes = new List<ITypeSymbol>();
                foreach (var choice in declaration.GetAttributes(compilation, _choiceAttribute))
                {
                    var choiceType = choice.ConstructorArguments.Length == 0
                        ? null : choice.ConstructorArguments[0].Value as ITypeSymbol;
                    if (choiceType == null)
                        throw InvalidChoice("the target type must not be null");

                    if (!compilation.ClassifyCommonConversion(choiceType, declaration).IsImplicit)
                        throw InvalidChoice("the target must implement the choice interface");
                    if (!HasCliAnnotation(compilation, choiceType))
                        throw InvalidChoice("the target requires Arguments or Choice annotations");
                    if (!compilation.IsSymbolAccessibleWithin(choiceType, declaration))
                        throw InvalidChoice("the target must be accessible from the choice interface");
                    choiceTypes.Add(choiceType);

                    GeneratorException InvalidChoice(string reason) =>
                        new GeneratorException(GetAttributeLocation(choice), GeneratorException.InvalidChoiceTarget,
                            choiceType?.Name ?? "null", declaration.Name, reason);
                }
                // Cache before following choices so command-schema validation can diagnose cycles.
                collected.Add(declaration, new CollectedDeclaration(ownDiscriminator, flattened, choiceTypes, isHelpPrefix));
                foreach (var choiceType in choiceTypes)
                {
                    var choiceDiscriminator = Collect(choiceType);
                    if (isRoot)
                        choices.Add(new CliChoiceInfo(typeNames.FromSymbol(choiceType), choiceDiscriminator));
                }

                return ownDiscriminator;

                void AppendLeaves(IReadOnlyList<CliArgumentInfo> nested)
                {
                    foreach (var member in nested)
                        if (member.GroupMembers is IReadOnlyList<CliArgumentInfo> children)
                            AppendLeaves(children);
                        else
                            leaves.Add(member);
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
            }
        }

        private static CliDiscriminatorInfo? GetDiscriminator(AttributeData attribute)
        {
            var prefix = GetNamedString(attribute, "Prefix");
            var longOption = GetNamedString(attribute, "Long");
            var shortOption = GetNamedChar(attribute, "Short");
            var hasNamedDiscriminator = !string.IsNullOrEmpty(longOption) || shortOption != '\0';
            if (prefix != null && prefix.Length > 0)
            {
                if (hasNamedDiscriminator)
                    throw new GeneratorException(GetAttributeLocation(attribute), GeneratorException.ConflictingDiscriminators);
                return new CliDiscriminatorInfo(prefix, prefix, null, '\0');
            }
            if (hasNamedDiscriminator)
            {
                var display = !string.IsNullOrEmpty(longOption) ? "--" + longOption : "-" + shortOption;
                return new CliDiscriminatorInfo(display, null, longOption, shortOption);
            }
            return null;
        }

        private static GeneratorException GroupError(AttributeData usage, string path, string reason) =>
            new GeneratorException(GetAttributeLocation(usage), GeneratorException.InvalidArgumentGroup, path, reason);

        private static Location? GetAttributeLocation(AttributeData annotation) =>
            annotation.ApplicationSyntaxReference?.GetSyntax().GetLocation();

        private sealed class CollectedDeclaration
        {
            public CollectedDeclaration(CliDiscriminatorInfo? discriminator,
                CliArgumentInfo[] arguments, IReadOnlyList<ITypeSymbol> choices, bool isHelpPrefix)
            {
                Discriminator = discriminator;
                Arguments = arguments;
                Choices = choices;
                IsHelpPrefix = isHelpPrefix;
            }

            public CliDiscriminatorInfo? Discriminator { get; }
            public CliArgumentInfo[] Arguments { get; }
            public IReadOnlyList<ITypeSymbol> Choices { get; }
            public bool IsHelpPrefix { get; }
        }

        private static CliOptionNames CollectArgumentNames(
            AttributeData attribute,
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
                foreach (var name in names)
                    if (!isValid(name))
                        throw new GeneratorException(GeneratorException.InvalidOptionName, singular);
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

    }
}
