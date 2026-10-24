using System;
using System.Collections.Generic;

namespace Lokad.Cli.Generator.Emit
{
    // The generator targets netstandard2.0 independently of the runtime library.
    public enum CliOptionValueMode { None, Required, Verbatim, OptionalAttached }
    public enum CliShortOptionSyntax { GroupedValues, AttachedValues }

    public sealed class CliTokenizationInfo
    {
        private readonly Dictionary<char, CliOptionValueMode> _shortOptions = new Dictionary<char, CliOptionValueMode>();
        private readonly Dictionary<string, CliOptionValueMode> _longOptions = new Dictionary<string, CliOptionValueMode>(StringComparer.Ordinal);
        private readonly SortedSet<string> _ambiguousOptions = new SortedSet<string>(StringComparer.Ordinal);
        private CliShortOptionSyntax? _shortOptionSyntax;
        private bool? _allowSingleDashLongOptions;
        private bool _hasVerbatimOrOptionalValues;
        private bool _hasOptionalShortValues;
        private bool? _stopAtFirstOperand;
        private bool _hasDeclaredBoundaries;
        private readonly SortedSet<char> _helpShortOptions = new SortedSet<char>();
        private readonly SortedSet<string> _helpLongOptions = new SortedSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<char, CliOptionValueMode> _helpShortModes = new Dictionary<char, CliOptionValueMode>();
        private readonly Dictionary<string, CliOptionValueMode> _helpLongModes = new Dictionary<string, CliOptionValueMode>(StringComparer.Ordinal);

        public IReadOnlyDictionary<char, CliOptionValueMode> ShortOptions => _shortOptions;
        public IReadOnlyDictionary<string, CliOptionValueMode> LongOptions => _longOptions;
        public CliShortOptionSyntax ShortOptionSyntax => _shortOptionSyntax ?? CliShortOptionSyntax.GroupedValues;
        public bool AllowSingleDashLongOptions => _allowSingleDashLongOptions ?? true;
        public bool StopAtFirstOperand => _stopAtFirstOperand ?? false;
        public bool RequiresBoundaries => StopAtFirstOperand || _hasDeclaredBoundaries;
        public bool HasHelp { get; private set; }
        public bool HasHelpPrefixes { get; private set; }
        public IReadOnlyDictionary<char, CliOptionValueMode> HelpShortOptions => _helpShortModes;
        public IReadOnlyDictionary<string, CliOptionValueMode> HelpLongOptions => _helpLongModes;
        public bool UsesArity => ShortOptionSyntax == CliShortOptionSyntax.AttachedValues || _hasVerbatimOrOptionalValues || RequiresBoundaries || HasHelp;
        public bool RequiresSchema => UsesArity || !AllowSingleDashLongOptions;

        public void ConfigureShortOptions(CliShortOptionSyntax syntax)
        {
            if (syntax != CliShortOptionSyntax.GroupedValues && syntax != CliShortOptionSyntax.AttachedValues)
                throw new GeneratorException(GeneratorException.InvalidTokenizationPolicy, "ShortOptionSyntax");
            if (_shortOptionSyntax.HasValue && _shortOptionSyntax.Value != syntax)
                throw new GeneratorException(GeneratorException.ConflictingTokenizationPolicy, "ShortOptionSyntax");
            _shortOptionSyntax = syntax;
        }

        public void ConfigureSingleDashLongOptions(bool allow)
        {
            if (_allowSingleDashLongOptions.HasValue && _allowSingleDashLongOptions.Value != allow)
                throw new GeneratorException(GeneratorException.ConflictingTokenizationPolicy, "AllowSingleDashLongOptions");
            _allowSingleDashLongOptions = allow;
        }

        public void ConfigureStopAtFirstOperand(bool stop)
        {
            if (_stopAtFirstOperand.HasValue && _stopAtFirstOperand.Value != stop)
                throw new GeneratorException(GeneratorException.ConflictingTokenizationPolicy, "StopAtFirstOperand");
            _stopAtFirstOperand = stop;
        }

        public void RequireBoundaries() => _hasDeclaredBoundaries = true;

        public void AddHelpOptions(CliOptionNames names)
        {
            HasHelp = true;
            foreach (var name in names.ShortOptions)
                _helpShortOptions.Add(name);
            foreach (var name in names.LongOptions)
                _helpLongOptions.Add(name);
        }

        public void RequireHelpPrefix()
        {
            HasHelp = true;
            HasHelpPrefixes = true;
        }

        public void AddOptions(CliOptionNames names, CliOptionValueMode mode)
        {
            _hasVerbatimOrOptionalValues |= mode == CliOptionValueMode.Verbatim || mode == CliOptionValueMode.OptionalAttached;
            _hasOptionalShortValues |= mode == CliOptionValueMode.OptionalAttached && names.ShortOptions.Count > 0;
            foreach (var name in names.ShortOptions)
                Add(_shortOptions, name, "-" + name);
            foreach (var name in names.LongOptions)
                Add(_longOptions, name, "--" + name);

            void Add<T>(Dictionary<T, CliOptionValueMode> options, T name, string label)
            {
                if (options.TryGetValue(name, out var previous) && previous != mode)
                {
                    _ambiguousOptions.Add(label);
                    options[name] = CliOptionValueMode.None;
                }
                else if (!_ambiguousOptions.Contains(label))
                    options[name] = mode;
            }
        }

        public void Validate()
        {
            if (_hasOptionalShortValues && ShortOptionSyntax != CliShortOptionSyntax.AttachedValues)
                throw new GeneratorException(GeneratorException.OptionalShortSyntax);
            if (UsesArity && _ambiguousOptions.Count > 0)
                throw new GeneratorException(GeneratorException.AmbiguousOptionArity, _ambiguousOptions.Min);
            foreach (var name in _helpShortOptions)
                _helpShortModes[name] = _shortOptions[name];
            foreach (var name in _helpLongOptions)
                _helpLongModes[name] = _longOptions[name];
        }
    }
}
