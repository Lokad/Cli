using System;
using System.Collections.Generic;

namespace Lokad.Cli
{
    /// <summary>Owned by one tokenization pass; resolves selectors without binding or converting values.</summary>
    internal sealed class ArgumentBoundaryState
    {
        private readonly CommandBoundary _root;
        private readonly bool _stopAtFirstOperand;
        private readonly Dictionary<string, int> _longOptions = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly Dictionary<char, int> _shortOptions = new Dictionary<char, int>();
        private readonly Dictionary<CommandBoundary, Selection> _prefixes = new Dictionary<CommandBoundary, Selection>();
        private bool _rootPrefixPending;
        private int _order;
        private int _operands;

        public ArgumentBoundaryState(CommandBoundary root, bool stopAtFirstOperand)
        {
            _root = root;
            _stopAtFirstOperand = stopAtFirstOperand;
            _rootPrefixPending = root.Prefix != null;
        }

        public bool IsLiteral { get; private set; }

        public void ObserveOption(ITokenizedArguments token)
        {
            var before = GetCommand();
            switch (token)
            {
                case LongArg option: RememberLong(option.Name); break;
                case LongArgWithValue option: RememberLong(option.Name); break;
                case ShortArgWithValue option: RememberShort(option.Name); break;
                case ShortArgSet group:
                    foreach (var name in group.Options)
                        RememberShort(name);
                    break;
            }
            var after = GetCommand();
            if (_operands > 0 && after != before && after != null &&
                (_stopAtFirstOperand || after.HasBoundaries))
                throw new ParseException("select the command before its positional operands");

            void RememberLong(string name)
            {
                if (!_longOptions.ContainsKey(name))
                    _longOptions.Add(name, _order++);
            }

            void RememberShort(char name)
            {
                if (!_shortOptions.ContainsKey(name))
                    _shortOptions.Add(name, _order++);
            }
        }

        public bool TryConsumePrefix(string value)
        {
            if (_operands > 0)
                return false;
            if (_rootPrefixPending)
            {
                _rootPrefixPending = false;
                if (string.Equals(_root.Prefix, value, StringComparison.Ordinal))
                    return true;
                if (_root.Choices.Count > 0)
                    throw new ParseException($"expected `{_root.Prefix}` before command arguments");
            }

            var node = _root;
            while (node.Choices.Count > 0)
            {
                var selected = GetExplicitChoice(node);
                if (selected != null)
                {
                    node = selected;
                    continue;
                }
                foreach (var choice in node.Choices)
                {
                    if (choice.Prefix == null || !string.Equals(choice.Prefix, value, StringComparison.Ordinal))
                        continue;
                    _prefixes.Add(node, new Selection(choice, _order++));
                    return true;
                }
                var fallback = GetDefaultChoice(node);
                if (fallback == null)
                    return false;
                node = fallback;
            }
            return false;
        }

        public bool PositionalValue(bool optionsTerminated)
        {
            var command = GetCommand();
            if (command == null && !_stopAtFirstOperand)
                throw new ParseException("select the command before its positional operands");
            var startsRemainder = command?.RemainderAfterOperands == 0 && (_stopAtFirstOperand || optionsTerminated);
            _operands++;
            IsLiteral = _stopAtFirstOperand || startsRemainder ||
                (command?.RemainderAfterOperands is int count && count > 0 && _operands >= count);
            return startsRemainder;
        }

        public void OptionValueConsumed(ITokenizedArguments option)
        {
            var command = GetCommand();
            if (command == null || (_rootPrefixPending && _root.Choices.Count > 0))
            {
                var visited = new HashSet<CommandBoundary>();
                if (HasBoundaryOption(_root))
                    throw new ParseException("select the command before an option ends option parsing");
                return;

                bool HasBoundaryOption(CommandBoundary node)
                {
                    if (!visited.Add(node))
                        return false;
                    if (IsBoundaryOption(node))
                        return true;
                    foreach (var child in node.Choices)
                        if (HasBoundaryOption(child))
                            return true;
                    return false;
                }
            }
            if (IsBoundaryOption(command))
                IsLiteral = true;

            bool IsBoundaryOption(CommandBoundary node)
            {
                foreach (var name in node.StopAfterLongOptions)
                    if (option is LongArgWithValue longArg && name == longArg.Name)
                        return true;
                foreach (var name in node.StopAfterShortOptions)
                    if (option is ShortArgWithValue shortArg && name == shortArg.Name)
                        return true;
                return false;
            }
        }

        private CommandBoundary? GetCommand()
        {
            var node = _root;
            while (node.Choices.Count > 0)
            {
                var selected = GetExplicitChoice(node) ?? GetDefaultChoice(node);
                if (selected == null)
                    return null;
                node = selected;
            }
            return node;
        }

        private CommandBoundary? GetExplicitChoice(CommandBoundary node)
        {
            CommandBoundary? selected = null;
            var order = int.MaxValue;
            // The first selector fixes lexical context. Keep all tokens so binding can report conflicting selectors.
            if (_prefixes.TryGetValue(node, out var prefix))
            {
                selected = prefix.Command;
                order = prefix.Order;
            }
            foreach (var choice in node.Choices)
            {
                if (choice.LongOption != null && _longOptions.TryGetValue(choice.LongOption, out var longOrder) && longOrder < order)
                {
                    selected = choice;
                    order = longOrder;
                }
                if (choice.ShortOption != '\0' && _shortOptions.TryGetValue(choice.ShortOption, out var shortOrder) && shortOrder < order)
                {
                    selected = choice;
                    order = shortOrder;
                }
            }
            return selected;
        }

        private static CommandBoundary? GetDefaultChoice(CommandBoundary node)
        {
            foreach (var choice in node.Choices)
                if (choice.IsDefault)
                    return choice;
            return null;
        }

        private readonly struct Selection
        {
            public Selection(CommandBoundary command, int order)
            {
                Command = command;
                Order = order;
            }
            public CommandBoundary Command { get; }
            public int Order { get; }
        }
    }
}
