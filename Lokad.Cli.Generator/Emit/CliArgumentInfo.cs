namespace Lokad.Cli.Generator.Emit
{
    public sealed class CliArgumentInfo
    {
        public CliArgumentInfo(
            string name,
            TypeName typeName,
            bool isReferenceType,
            bool isNullable,
            bool isBoolean,
            bool isEnum,
            CliCollectionKind collectionKind,
            bool isConstructorParameter,
            bool isSettableMember,
            string documentation,
            string? documentationName,
            CliOptionNames options,
            bool shortNumeric,
            bool allowRepeated,
            bool consumeUntilBoundary,
            bool consumeNextValueVerbatim,
            bool isOptionalValue,
            bool isRemainder,
            bool stopOptionParsing,
            bool isHelp)
        {
            Name = name;
            TypeName = typeName;
            IsReferenceType = isReferenceType;
            IsNullable = isNullable;
            IsBoolean = isBoolean;
            IsEnum = isEnum;
            CollectionKind = collectionKind;
            IsConstructorParameter = isConstructorParameter;
            IsSettableMember = isSettableMember;
            Documentation = documentation;
            DocumentationName = documentationName;
            Options = options;
            ShortNumeric = shortNumeric;
            AllowRepeated = allowRepeated;
            ConsumeUntilBoundary = consumeUntilBoundary;
            ConsumeNextValueVerbatim = consumeNextValueVerbatim;
            IsOptionalValue = isOptionalValue;
            IsRemainder = isRemainder;
            StopOptionParsing = stopOptionParsing;
            IsHelp = isHelp;
        }

        public string Name { get; }

        public TypeName TypeName { get; }

        /// <summary>Whether the converted input value is a reference type.</summary>
        public bool IsReferenceType { get; }

        /// <summary>Whether the converted input value is nullable.</summary>
        public bool IsNullable { get; }

        /// <summary>Whether the argument is a scalar boolean flag.</summary>
        public bool IsBoolean { get; }

        public bool IsEnum { get; }

        public CliCollectionKind CollectionKind { get; }

        public bool IsCollection => CollectionKind != CliCollectionKind.None;

        public bool IsConstructorParameter { get; }

        public bool IsSettableMember { get; }

        public string Documentation { get; }

        public string? DocumentationName { get; }

        public CliOptionNames Options { get; }

        public bool ShortNumeric { get; }

        public bool AllowRepeated { get; }

        public bool ConsumeUntilBoundary { get; }

        public bool ConsumeNextValueVerbatim { get; }

        public bool IsOptionalValue { get; }

        public bool IsRemainder { get; }

        public bool StopOptionParsing { get; }

        public bool IsHelp { get; }

        public bool IsOption => Options.Labels.Count > 0 || ShortNumeric;

        public bool NeedsSeen => !IsOptionalValue && !IsCollection && !IsReferenceType && (!IsBoolean || !AllowRepeated || IsNullable);
    }
}
