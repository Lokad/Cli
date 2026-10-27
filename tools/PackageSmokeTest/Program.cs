using Lokad.Cli;

namespace PackageConsumer;

public static class Program
{
    public static void Main()
    {
        var tokens = Parser.Tokenize<CopyArgs>(new[]
        {
            "copy", "source.txt", "--tag=first", "--label=second", "-vv"
        });
        if (!tokens.HasLongOption("tag") || Parser.IsHelpRequested<CopyArgs>(tokens))
            throw new InvalidOperationException("Generated token inspection failed.");

        var parsed = Parser.ParseTokenized<CopyArgs>(tokens);
        if (parsed.Source != "source.txt" || !parsed.Verbose || parsed.Help ||
            !parsed.Tags.SequenceEqual(new[] { "first", "second" }) || tokens.Peek() != null)
            throw new InvalidOperationException("Generated argument binding failed.");

        if (!Parser.IsHelpRequested<CopyArgs>(new[] { "--help" }) ||
            !Parser.Document<CopyArgs>("app").Contains("--tag", StringComparison.Ordinal))
            throw new InvalidOperationException("Generated help failed.");

        Console.WriteLine("Package consumer passed on " + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
    }
}

[Arguments("Copy a source.", Prefix = "copy")]
public partial record CopyArgs(
    [property: Argument("Source.")] string Source,
    [property: Argument("Tags.", LongNames = new[] { "tag", "label" })] IReadOnlyList<string> Tags,
    [property: Argument("Verbose.", Short = 'v', Long = "verbose", AllowRepeated = true)] bool Verbose,
    [property: Argument("Help.", Long = "help", IsHelp = true)] bool Help);
