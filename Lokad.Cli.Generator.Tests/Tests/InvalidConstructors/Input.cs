using Lokad.Cli;

namespace Test;

[Arguments("Ambiguous constructors.")]
public partial class Overloads
{
    public Overloads([Argument("Number.", Long = "number")] int number) { }
    public Overloads([Argument("Text.", Long = "text")] string text) { }
}

[Arguments("Incomplete constructor binding.")]
public partial class Incomplete
{
    public Incomplete([Argument("Number.", Long = "number")] int number, string text) { }
}

[Arguments("Unbound constructor.")]
public partial class Unbound
{
    public Unbound(int number) { }
}

[Arguments("Ref constructor.")]
public partial class RefConstructor
{
    public RefConstructor([Argument("Number.", Long = "number")] ref int number) { }
}
