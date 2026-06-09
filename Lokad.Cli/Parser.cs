namespace Lokad.Cli
{
    public static class Parser
    {
        public static T Parse<T>(string[] arguments)
            where T : ICliParsed<T>
        {
            return T.Parse(arguments);
        }
    }
}
