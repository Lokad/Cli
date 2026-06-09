namespace Lokad.Cli.Generator.Emit
{
    public sealed class CliChoiceInfo
    {
        public CliChoiceInfo(TypeName typeName, CliDiscriminatorInfo? discriminator)
        {
            TypeName = typeName;
            Discriminator = discriminator;
        }

        public TypeName TypeName { get; }

        public CliDiscriminatorInfo? Discriminator { get; }
    }
}
