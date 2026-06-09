namespace Lokad.Cli
{
    public interface ICliParsed<T>
    {
        static abstract T Parse(string[] arguments);
    }
}
