using System;

namespace Lokad.Cli.Tests;

public sealed class OptionalValueTests
{
    [Fact]
    public void DistinguishesAbsentAndBareOccurrences()
    {
        var absent = default(OptionalValue<string>);
        var bare = OptionalValue<string>.Bare;

        Assert.False(absent.IsPresent);
        Assert.True(bare.IsPresent);
        Assert.False(absent.HasValue);
        Assert.False(bare.HasValue);
        Assert.False(absent.TryGetValue(out _));
        Assert.False(bare.TryGetValue(out _));
        Assert.Throws<InvalidOperationException>(() => absent.Value);
        Assert.Throws<InvalidOperationException>(() => bare.Value);
    }

    [Fact]
    public void RetainsExplicitDefaultValues()
    {
        var zero = new OptionalValue<int>(0);
        Assert.True(zero.IsPresent && zero.HasValue);
        Assert.True(zero.TryGetValue(out var value));
        Assert.Equal(0, value);
        Assert.Equal(0, zero.Value);
    }

    [Fact]
    public void RetainsAnExplicitNull()
    {
        var supplied = new OptionalValue<string?>(null);
        Assert.True(supplied.IsPresent && supplied.HasValue);
        Assert.True(supplied.TryGetValue(out var value));
        Assert.Null(value);
        Assert.Null(supplied.Value);
    }
}
