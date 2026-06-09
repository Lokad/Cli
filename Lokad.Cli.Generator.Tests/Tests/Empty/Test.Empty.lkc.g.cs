#nullable enable
namespace Test;
partial record Empty : global::Lokad.Cli.ICliParsed<global::Test.Empty>
{
    public static global::Test.Empty Parse(string[] arguments)
    {
        // Tokenization
        var tokenized = global::Lokad.Cli.ArgumentTokenizer.Tokenize(
            arguments,
            global::System.Array.Empty<char>(),
            global::System.Array.Empty<string>(),
            false);

        // Discrimination
        _ = tokenized;

        // Binding
        throw new global::System.NotImplementedException();
    }
}
