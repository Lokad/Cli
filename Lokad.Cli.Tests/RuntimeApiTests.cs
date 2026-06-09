namespace Lokad.Cli.Tests;

public class RuntimeApiTests
{
    [Fact]
    public void ParserDelegatesToStaticInterfaceMembers()
    {
        Assert.Throws<System.NotImplementedException>(() => Parser.Parse<Parsed>(System.Array.Empty<string>()));
        Assert.Throws<System.NotImplementedException>(() => Parser.Document<Parsed>("cmd"));
    }

    private sealed class Parsed : ICliParsed<Parsed>
    {
        public static Parsed Parse(string[] arguments)
        {
            throw new System.NotImplementedException();
        }

        public static Parsed ParseTokenized(TokenizedArguments arguments)
        {
            throw new System.NotImplementedException();
        }

        public static string Document(string commandName)
        {
            throw new System.NotImplementedException();
        }
    }
}
