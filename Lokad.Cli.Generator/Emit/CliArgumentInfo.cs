namespace Lokad.Cli.Generator.Emit
{
    public sealed class CliArgumentInfo
    {
        public CliArgumentInfo(
            string name,
            TypeName typeName,
            bool isNullable,
            bool isBoolean,
            bool isEnum,
            bool isArray,
            bool isConstructorParameter,
            bool isSettableMember,
            string documentation,
            string? documentationName,
            string? longOption,
            char shortOption,
            bool shortNumeric)
        {
            Name = name;
            TypeName = typeName;
            IsNullable = isNullable;
            IsBoolean = isBoolean;
            IsEnum = isEnum;
            IsArray = isArray;
            IsConstructorParameter = isConstructorParameter;
            IsSettableMember = isSettableMember;
            Documentation = documentation;
            DocumentationName = documentationName;
            LongOption = longOption;
            ShortOption = shortOption;
            ShortNumeric = shortNumeric;
        }

        public string Name { get; }

        public TypeName TypeName { get; }

        public bool IsNullable { get; }

        public bool IsBoolean { get; }

        public bool IsEnum { get; }

        public bool IsArray { get; }

        public bool IsConstructorParameter { get; }

        public bool IsSettableMember { get; }

        public string Documentation { get; }

        public string? DocumentationName { get; }

        public string? LongOption { get; }

        public char ShortOption { get; }

        public bool ShortNumeric { get; }

        public bool IsOption => LongOption != null || ShortOption != '\0' || ShortNumeric;
    }
}
