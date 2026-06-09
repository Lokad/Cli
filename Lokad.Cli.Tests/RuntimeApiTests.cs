namespace Lokad.Cli.Tests;

public class RuntimeApiTests
{
    [Fact]
    public void ParserIsNotImplementedYet()
    {
        Assert.Throws<System.NotImplementedException>(() => Parser.Parse<Parsed>(System.Array.Empty<string>()));
    }

    private sealed class Parsed : ICliParsed<Parsed>
    {
        public static Parsed Parse(string[] arguments)
        {
            throw new System.NotImplementedException();
        }
    }
}
