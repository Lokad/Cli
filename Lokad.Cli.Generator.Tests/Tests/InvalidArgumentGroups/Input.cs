#nullable enable
using System;
using System.Collections.Generic;
using Lokad.Cli;

namespace Test;

[Arguments("Count.")]
public partial record CountGroup([property: Argument("Count.", Long = "count")] int Count) : ICountGroupPolicy;

[Arguments("Conflicting policy.", ShortOptionSyntax = ShortOptionSyntax.AttachedValues)]
public partial record ConflictingGroup([property: Argument("Group.")] CountGroup Options);

[Arguments("Named group.")]
public partial record NamedGroup([property: Argument("Group.", Long = "group")] CountGroup Options);

[Arguments("Nullable group.")]
public partial record NullableGroup([property: Argument("Group.")] CountGroup? Options);

[Arguments("Array group.")]
public partial record ArrayGroup([property: Argument("Group.")] CountGroup[] Options);

[Arguments("List group.")]
public partial record ListGroup([property: Argument("Group.")] IReadOnlyList<CountGroup> Options);

[Arguments("Memory group.")]
public partial record MemoryGroup([property: Argument("Group.")] ReadOnlyMemory<CountGroup> Options);

[Arguments("Optional group.")]
public partial record OptionalGroup([property: Argument("Group.", Long = "group")] OptionalValue<CountGroup> Options);

[Arguments("Scalar policy on a group.")]
public partial record VerbatimGroup([property: Argument("Group.", ConsumeNextValueVerbatim = true)] CountGroup Options);

[Arguments("Prefix group.", Prefix = "mode")]
public partial record PrefixOptions : IChoiceGroup;

[Arguments("Discriminated group.")]
public partial record DiscriminatedGroup([property: Argument("Group.")] PrefixOptions Options);

[Choice(typeof(PrefixOptions))]
public partial interface IChoiceGroup { }

[Arguments("Choice group.")]
public partial record ChoiceGroup([property: Argument("Group.")] IChoiceGroup Options);

[Arguments("Self cycle.")]
public partial record SelfGroup([property: Argument("Group.")] SelfGroup Options);

[Arguments("Mutual cycle.")]
public partial record FirstGroup([property: Argument("Group.")] SecondGroup Options);

[Arguments("Mutual cycle.")]
public partial record SecondGroup([property: Argument("Group.")] FirstGroup Options);

[Arguments("Outer collision.")]
public partial record CollisionGroup(
    [property: Argument("Count.", Long = "count")] int Count,
    [property: Argument("Group.")] CountGroup Options);

[Arguments("Repeated group collision.")]
public partial record RepeatedGroup(
    [property: Argument("First.")] CountGroup First,
    [property: Argument("Second.")] CountGroup Second);

[Arguments("Read-only group.")]
public partial record ReadOnlyGroup
{
    [Argument("Group.")] public CountGroup Options { get; } = new(1);
}

[Arguments("Private constructor.")]
public partial class PrivateOptions
{
    private PrivateOptions() { }
}

[Arguments("Inaccessible group constructor.")]
public partial record PrivateGroup([property: Argument("Group.")] PrivateOptions Options);

[Arguments("Private setter.")]
public partial class PrivateSetterOptions
{
    [Argument("Count.", Long = "count")] public int Count { get; private set; }
}

[Arguments("Inaccessible group setter.")]
public partial record PrivateSetterGroup([property: Argument("Group.")] PrivateSetterOptions Options);

[Arguments("Protected constructor.")]
public partial class ProtectedOptions
{
    protected ProtectedOptions() { }
}

[Arguments("Protected group constructor.")]
public partial class ProtectedGroup : ProtectedOptions
{
    [Argument("Group.")] public required ProtectedOptions Options { get; init; }
}

[Arguments("Expanding recursive group.")]
public partial record ExpandingGroup<T>(
    [property: Argument("Group.")] ExpandingGroup<ExpandingGroup<T>> Options);

[Arguments("Count policy.", ShortOptionSyntax = ShortOptionSyntax.GroupedValues)]
[Choice(typeof(CountGroup))]
public partial interface ICountGroupPolicy { }
