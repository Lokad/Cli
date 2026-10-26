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

[Arguments("Ambiguous optional constructors.")]
public partial class AmbiguousOptional
{
    public AmbiguousOptional(int x = 1) { }
    public AmbiguousOptional(int x = 1, int y = 2) { }
}

[Arguments("Ambiguous params constructors.")]
public partial class AmbiguousParams
{
    public AmbiguousParams(params int[] values) { }
    public AmbiguousParams(params string[] values) { }
}

[Arguments("Ambiguous group constructor.")]
public partial record AmbiguousConstructorGroup([property: Argument("Options.")] AmbiguousOptional Options);
