using System.Collections.Generic;

namespace Lokad.Cli.Generator.Emit
{
    public sealed class CliTypeInfo
    {
        public CliTypeInfo(
            TypeName typeName,
            CliTypeKind kind,
            CliParserMethods explicitParserMethods,
            char[] shortOptions,
            string[] longOptions,
            bool hasShortNumericArguments,
            CliChoiceInfo[] choices,
            IReadOnlyList<CliArgumentInfo> members,
            CliArgumentInfo[] arguments,
            CliDiscriminatorInfo? discriminator,
            string? documentation,
            CliTokenizationInfo tokenization,
            CliCommandBoundaryInfo? commandBoundary)
        {
            TypeName = typeName;
            Kind = kind;
            ExplicitParserMethods = explicitParserMethods;
            ShortOptions = shortOptions;
            LongOptions = longOptions;
            HasShortNumericArguments = hasShortNumericArguments;
            Choices = choices;
            Members = members;
            Arguments = arguments;
            Discriminator = discriminator;
            Documentation = documentation;
            Tokenization = tokenization;
            CommandBoundary = commandBoundary;
        }

        public TypeName TypeName { get; }

        public CliTypeKind Kind { get; }

        public CliParserMethods ExplicitParserMethods { get; }

        public char[] ShortOptions { get; }

        public string[] LongOptions { get; }

        public bool HasShortNumericArguments { get; }

        public CliChoiceInfo[] Choices { get; }

        public CliArgumentInfo[] Arguments { get; }

        public IReadOnlyList<CliArgumentInfo> Members { get; }

        public CliDiscriminatorInfo? Discriminator { get; }

        public string? Documentation { get; }

        public CliTokenizationInfo Tokenization { get; }

        public CliCommandBoundaryInfo? CommandBoundary { get; }
    }
}
