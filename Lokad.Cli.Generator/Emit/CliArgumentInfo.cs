using System.Collections.Generic;
using Microsoft.CodeAnalysis;

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
            bool isHelp,
            IReadOnlyList<CliArgumentInfo>? groupMembers,
            string path,
            Location? location)
        {
            Name = name;
            TypeName = typeName;
            IsReferenceType = isReferenceType;
            IsNullable = isNullable;
            IsBoolean = isBoolean;
            IsEnum = isEnum;
            CollectionKind = collectionKind;
            IsConstructorParameter = isConstructorParameter;
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
            GroupMembers = groupMembers;
            Path = path;
            Location = location;
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

        /// <summary>Members used to construct a nested group, or null for a scalar or collection argument.</summary>
        public IReadOnlyList<CliArgumentInfo>? GroupMembers { get; }

        public string Path { get; }

        public Location? Location { get; }

        public bool IsOption => Options.Labels.Count > 0 || ShortNumeric;

        public bool NeedsSeen => !IsOptionalValue && !IsCollection && !IsReferenceType && (!IsBoolean || !AllowRepeated || IsNullable);
    }
}
