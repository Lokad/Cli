using Lokad.Cli;

namespace Lokad.Cli.Generator.FullTests;

public sealed class UniqCommandTests
{
    [Fact]
    public void ParsesAllSupportedShortOptions()
    {
        var parsed = Parser.Parse<UniqArgs>(new[] { "-cdui" });

        Assert.True(parsed.Count);
        Assert.True(parsed.OnlyDuplicates);
        Assert.True(parsed.OnlyUnique);
        Assert.True(parsed.IgnoreCase);
    }
}

[Arguments("uniq command.")]
public partial record UniqArgs
{
    [Argument("Prefixes occurrence counts.", Short = 'c')]
    public bool Count { get; init; }

    [Argument("Emits duplicate groups.", Short = 'd')]
    public bool OnlyDuplicates { get; init; }

    [Argument("Emits unique groups.", Short = 'u')]
    public bool OnlyUnique { get; init; }

    [Argument("Ignores case.", Short = 'i')]
    public bool IgnoreCase { get; init; }
}
