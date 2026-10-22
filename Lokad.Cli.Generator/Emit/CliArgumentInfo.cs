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
            CliOptionNames options,
            bool shortNumeric,
            bool allowRepeated)
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
            Options = options;
            ShortNumeric = shortNumeric;
            AllowRepeated = allowRepeated;
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

        public CliOptionNames Options { get; }

        public bool ShortNumeric { get; }

        public bool AllowRepeated { get; }

        public bool IsOption => Options.Labels.Count > 0 || ShortNumeric;

        public bool NeedsSeen => !IsArray && (!IsBoolean || !AllowRepeated || IsNullable);
    }
}
