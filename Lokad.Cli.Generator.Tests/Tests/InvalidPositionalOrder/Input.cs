using Lokad.Cli;

namespace Test;

[Arguments("Optional number before required file.")]
public partial record OptionalNumberFirst(
    [property: Argument("Limit.")] int? Limit,
    [property: Argument("File.")] string File);

[Arguments("Optional string before required file.")]
public partial record OptionalStringFirst(
    [property: Argument("Prefix.")] string? Prefix,
    [property: Argument("File.")] string File);

[Arguments("Nullable group leaf.")]
public partial record NullableGroup([property: Argument("Limit.")] int? Limit);

[Arguments("Optional group leaf before required file.")]
public partial record OptionalGroupFirst(
    [property: Argument("Options.")] NullableGroup Options,
    [property: Argument("File.")] string File);
