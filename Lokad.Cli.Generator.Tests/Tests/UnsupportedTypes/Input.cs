using Lokad.Cli;

namespace Test;

[Arguments("Generic.")]
public partial record GenericArgs<T>([property: Argument("Value.")] T Value);

public class Host
{
    [Arguments("Nested.")]
    public partial record NestedArgs([property: Argument("Value.")] int Value);
}

[Arguments("Ref-like.")]
public ref partial struct RefArgs { }

[Arguments("Non-partial.")]
public record NonPartialArgs;

[Arguments("File-local.")]
file partial record FileArgs;

[Arguments("Abstract.")]
public abstract partial record AbstractArgs;

[Arguments("Static.")]
public static partial class StaticArgs { }

[Arguments("Interface without choices.")]
public partial interface IEmptyArgs { }

public class AnnotatedMemberBase
{
    [Argument("Count.", Long = "count")] public int Count { get; init; }
}

[Arguments("Inherited member.")]
public partial class InheritedMemberArgs : AnnotatedMemberBase { }

[Arguments("Base parser.")]
public partial class ParserBase { }

[Arguments("Inherited parser.")]
public partial class InheritedParserArgs : ParserBase { }

[Arguments("Inherited group.")]
public partial record InheritedGroupArgs([property: Argument("Options.")] InheritedMemberArgs Options);
