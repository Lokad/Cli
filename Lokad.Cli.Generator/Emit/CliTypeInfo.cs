namespace Lokad.Cli.Generator.Emit
{
    public sealed class CliTypeInfo
    {
        public CliTypeInfo(TypeName typeName, CliTypeKind kind, SourceLocationInfo? location)
            : this(
                typeName,
                kind,
                location,
                new char[0],
                new string[0],
                false,
                new CliChoiceInfo[0],
                new CliArgumentInfo[0],
                null,
                null,
                new CliTokenizationInfo(),
                null)
        {
        }

        public CliTypeInfo(
            TypeName typeName,
            CliTypeKind kind,
            SourceLocationInfo? location,
            char[] shortOptions,
            string[] longOptions,
            bool hasShortNumericArguments,
            CliChoiceInfo[] choices,
            CliArgumentInfo[] arguments,
            CliDiscriminatorInfo? discriminator,
            string? documentation,
            CliTokenizationInfo tokenization,
            CliCommandBoundaryInfo? commandBoundary)
        {
            TypeName = typeName;
            Kind = kind;
            Location = location;
            ShortOptions = shortOptions;
            LongOptions = longOptions;
            HasShortNumericArguments = hasShortNumericArguments;
            Choices = choices;
            Arguments = arguments;
            Discriminator = discriminator;
            Documentation = documentation;
            Tokenization = tokenization;
            CommandBoundary = commandBoundary;
        }

        public TypeName TypeName { get; }

        public CliTypeKind Kind { get; }

        public SourceLocationInfo? Location { get; }

        public char[] ShortOptions { get; }

        public string[] LongOptions { get; }

        public bool HasShortNumericArguments { get; }

        public CliChoiceInfo[] Choices { get; }

        public CliArgumentInfo[] Arguments { get; }

        public CliDiscriminatorInfo? Discriminator { get; }

        public string? Documentation { get; }

        public CliTokenizationInfo Tokenization { get; }

        public CliCommandBoundaryInfo? CommandBoundary { get; }
    }
}
