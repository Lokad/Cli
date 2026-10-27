using Xunit.Sdk;

namespace Lokad.Cli.Generator.Tests;

public sealed class RequiredMemberTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ChecksRequiredConstructorBindingsFromReferencedAssemblies(bool setsRequiredMembers)
    {
        var model = """
            using Lokad.Cli;
            namespace Model;
            [Arguments("Settings.")]
            public class Settings
            {
                [Argument("Value.", Long = "value")] public required int Value { get; init; }
                SETS_REQUIRED public Settings(int Value) { this.Value = Value; }
            }
            """.Replace("SETS_REQUIRED", setsRequiredMembers ? "[System.Diagnostics.CodeAnalysis.SetsRequiredMembers]" : "");
        var reference = GeneratorTestFixture.CompileReference("Model", model);
        const string source = """
            using Lokad.Cli;
            namespace Example;
            [Arguments("Group.")]
            public partial record Args([property: Argument("Settings.")] Model.Settings Settings);
            """;
        if (setsRequiredMembers)
            GeneratorTestFixture.AssertCompiles(source, new LokadCliGenerator(), new[] { reference });
        else
        {
            var error = Assert.ThrowsAny<XunitException>(() =>
                GeneratorTestFixture.AssertCompiles(source, new LokadCliGenerator(), new[] { reference }));
            Assert.Contains("LKC0024", error.Message);
            Assert.Contains("SetsRequiredMembers", error.Message);
        }
    }
}
