using System;
using System.Collections;
using System.Collections.Generic;

namespace Lokad.Cli.Tests;

public sealed class ArgumentBoundaryTests
{
    [Fact]
    public void InspectsSharedHelpCommandsWithoutRepeatingEveryPath()
    {
        var left = Node("help", null, Array.Empty<CommandBoundary>(), true);
        var right = Node("other", null, Array.Empty<CommandBoundary>(), false);
        var layers = new List<CountedList<CommandBoundary>>();
        var nodeCount = 2;
        for (var i = 0; i < 16; i++)
        {
            var children = new CountedList<CommandBoundary>(new[] { left, right });
            layers.Add(children);
            left = Node(null, "left", children, false);
            right = Node(null, "right", children, false);
            nodeCount += 2;
        }
        var root = Node(null, null, new[] { left, right }, false);
        nodeCount++;
        foreach (var layer in layers)
            layer.Enumerations = 0;
        var tokens = new TokenizedArguments(new ITokenizedArguments[]
        {
            new LongArg("left"), new LongArg("right"), new ValueArg("not-help")
        });
        var helpShort = new Dictionary<char, OptionValueMode>();
        var helpLong = new Dictionary<string, OptionValueMode>();
        Assert.False(tokens.IsHelpRequested(helpShort, helpLong, root));
        var enumerations = 0;
        foreach (var layer in layers)
            enumerations += layer.Enumerations;
        Assert.InRange(enumerations, 0, nodeCount * 4);

        tokens = new TokenizedArguments(new ITokenizedArguments[]
        {
            new LongArg("left"), new LongArg("right"), new ValueArg("help")
        });
        Assert.True(tokens.IsHelpRequested(helpShort, helpLong, root));

        static CommandBoundary Node(string? prefix, string? option, IReadOnlyList<CommandBoundary> choices, bool help) =>
            new CommandBoundary(prefix, option, '\0', choices, null, Array.Empty<char>(), Array.Empty<string>(), help);
    }

    [Fact]
    public void SearchesSharedChoiceNodesWithoutRepeatingEveryPath()
    {
        var names = new CountedList<string>(new[] { "code" });
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
            new CommandBoundary(prefix, null, '\0', choices, null, Array.Empty<char>(), stopOptions, false);
    }

    private sealed class CountedList<T> : IReadOnlyList<T>
    {
        private readonly IReadOnlyList<T> _items;

        public CountedList(IReadOnlyList<T> items)
        {
            _items = items;
        }

        public int Enumerations { get; set; }
        public int Count => _items.Count;
        public T this[int index] => _items[index];

        public IEnumerator<T> GetEnumerator()
        {
            Enumerations++;
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
