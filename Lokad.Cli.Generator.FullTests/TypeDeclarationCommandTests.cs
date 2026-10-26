using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests
{
    public sealed class TypeDeclarationCommandTests
    {
        [Fact]
        public void BindsMembersFromAnUnattributedPartialDeclaration()
        {
            Assert.Equal(7, Parser.Parse<SplitParsedArgs>(new[] { "--value=7" }).Value);
        }

        [Fact]
        public void ParsesATypeInTheGlobalNamespace()
        {
            Assert.Equal(7, Parser.Parse<global::GlobalParsedArgs>(new[] { "--value=7" }).Value);
        }
    }

    public partial record SplitParsedArgs
    {
        [Argument("Value.", Long = "value")] public int Value { get; init; }
    }

    [Arguments("Split parsed arguments.")]
    public partial record SplitParsedArgs;
}

[Arguments("Global parsed arguments.")]
public partial record GlobalParsedArgs([property: Argument("Value.", Long = "value")] int Value);
