namespace Lokad.Cli.Generator.Emit
{
    public static class EmitParserType
    {
        public static string EmitFor(CliTypeInfo type)
        {
            return
$@"#nullable enable
namespace {type.TypeName.Namespace};
partial {GetTypeDeclaration(type)} {type.TypeName.Name} : global::Lokad.Cli.ICliParsed<{type.TypeName.FullName}>
{{
    public static {type.TypeName.FullName} Parse(string[] arguments)
    {{
        throw new global::System.NotImplementedException();
    }}
}}
";
        }

        private static string GetTypeDeclaration(CliTypeInfo type)
        {
            switch (type.Kind)
            {
                case CliTypeKind.Class:
                    return "class";
                case CliTypeKind.Struct:
                    return "struct";
                case CliTypeKind.Interface:
                    return "interface";
                case CliTypeKind.RecordStruct:
                    return "record struct";
                default:
                    return "record";
            }
        }
    }
}
