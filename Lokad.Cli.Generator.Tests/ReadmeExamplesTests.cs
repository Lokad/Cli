using System.Text.RegularExpressions;

#pragma warning disable RS1035 // This test reads repository documentation, not analyzer input.

namespace Lokad.Cli.Generator.Tests;

public sealed class ReadmeExamplesTests
{
    [Fact]
    public void CompilesEveryCSharpExample()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "Lokad.Cli.slnx")))
            directory = directory.Parent;
        if (directory == null)
            throw new DirectoryNotFoundException("Could not locate the repository README.");

        var readme = File.ReadAllText(Path.Combine(directory.FullName, "README.md"));
        var examples = Regex.Matches(readme, "```csharp\\r?\\n([\\s\\S]*?)```")
            .Select(match => match.Groups[1].Value.Trim()).ToArray();
        Assert.NotEmpty(examples);

        foreach (var example in examples)
        {
            var source = "using System; using Lokad.Cli; namespace ReadmeExample;\n";
            if (example.StartsWith("var ", StringComparison.Ordinal))
                source += examples[0] + "\npublic static class Usage { public static void Run() { " + example + " } }";
            else if (example.StartsWith("public int Main", StringComparison.Ordinal))
                source += examples[0] + "\npublic class Usage { " + example + " private int Operate(MyArgs args) => 0; }";
            else if (example.StartsWith("[Argument(", StringComparison.Ordinal))
                source += "[Arguments(\"Example.\")] public partial record Example { " + example + " }";
            else
                source += example;

            GeneratorTestFixture.AssertCompiles(source);
        }
    }
}
