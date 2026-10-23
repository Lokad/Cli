using System;
using System.Collections;
using System.Collections.Generic;

namespace Lokad.Cli.Tests;

public sealed class ArgumentBoundaryTests
{
    [Fact]
    public void SearchesSharedChoiceNodesWithoutRepeatingEveryPath()
    {
        var names = new CountedNames();
        var left = Node("left", Array.Empty<CommandBoundary>(), names);
        var right = Node("right", Array.Empty<CommandBoundary>(), names);
        var nodeCount = 2;
        for (var i = 0; i < 16; i++)
        {
            var children = new[] { left, right };
            left = Node("left", children, Array.Empty<string>());
            right = Node("right", children, Array.Empty<string>());
            nodeCount += 2;
        }
        var root = Node(null, new[] { left, right }, Array.Empty<string>());
        nodeCount++;
        var options = new Dictionary<string, OptionValueMode>
        {
            ["code"] = OptionValueMode.Required, ["output"] = OptionValueMode.Required
        };
        var arguments = new[] { "--output=one", "--output=two" };
        var tokens = ArgumentTokenizer.Tokenize(arguments, new Dictionary<char, OptionValueMode>(),
            options, false, ShortOptionSyntax.GroupedValues, true, false, root);

        Assert.Equal(new LongArgWithValue("output", "one"), tokens.Shift());
        Assert.Equal(new LongArgWithValue("output", "two"), tokens.Shift());
        Assert.Null(tokens.Peek());
        Assert.InRange(names.Enumerations, 0, nodeCount * arguments.Length);
        Assert.Throws<ParseException>(() => ArgumentTokenizer.Tokenize(new[] { "--code=x" },
            new Dictionary<char, OptionValueMode>(), options, false, ShortOptionSyntax.GroupedValues, true, false, root));

        static CommandBoundary Node(string? prefix, IReadOnlyList<CommandBoundary> choices, IReadOnlyList<string> stopOptions) =>
            new CommandBoundary(prefix, null, '\0', choices, null, Array.Empty<char>(), stopOptions);
    }

    private sealed class CountedNames : IReadOnlyList<string>
    {
        public int Enumerations { get; private set; }
        public int Count => 1;
        public string this[int index] => index == 0 ? "code" : throw new ArgumentOutOfRangeException(nameof(index));

        public IEnumerator<string> GetEnumerator()
        {
            Enumerations++;
            yield return "code";
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
