namespace Lokad.Cli.Generator.Emit
{
    public sealed class CliTypeInfo
    {
        public CliTypeInfo(TypeName typeName, CliTypeKind kind, SourceLocationInfo? location)
        {
            TypeName = typeName;
            Kind = kind;
            Location = location;
        }

        public TypeName TypeName { get; }

        public CliTypeKind Kind { get; }

        public SourceLocationInfo? Location { get; }
    }
}
