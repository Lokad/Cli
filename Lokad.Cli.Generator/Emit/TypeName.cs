namespace Lokad.Cli.Generator.Emit
{
    public sealed class TypeName
    {
        public TypeName(string fullName, string name, string ns)
        {
            FullName = fullName;
            Name = name;
            Namespace = ns;
        }

        public string FullName { get; }

        public string Name { get; }

        public string Namespace { get; }
    }
}
