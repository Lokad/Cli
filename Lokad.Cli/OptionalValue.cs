using System;
using System.Diagnostics.CodeAnalysis;

namespace Lokad.Cli
{
    /// <summary>
    ///     A named option with an optional attached value. The default is absent;
    ///     <see cref="Bare"/> represents an occurrence without a value.
    /// </summary>
    /// <typeparam name="T">The type of an explicitly supplied value.</typeparam>
    public readonly struct OptionalValue<T>
    {
        private readonly SuppliedValue? _value;

        /// <summary>Creates a present option with an explicit value.</summary>
        public OptionalValue(T value)
        {
            IsPresent = true;
            _value = new SuppliedValue(value);
        }

        /// <summary>Whether the option occurred, with or without a value.</summary>
        public bool IsPresent { get; private init; }

        /// <summary>Whether the occurrence supplied an explicit value.</summary>
        public bool HasValue => _value.HasValue;

        /// <summary>A present occurrence without an explicit value.</summary>
        public static OptionalValue<T> Bare => new OptionalValue<T> { IsPresent = true };

        /// <summary>The explicit value. Throws when the option is absent or bare.</summary>
        public T Value => TryGetValue(out var value) ? value : throw new InvalidOperationException("The option has no explicit value.");

        /// <summary>Returns whether an explicit value exists and, if so, retrieves it.</summary>
        public bool TryGetValue([MaybeNullWhen(false)] out T value)
        {
            if (_value is SuppliedValue supplied)
            {
                value = supplied.Value;
                return true;
            }

            value = default;
            return false;
        }

        private readonly struct SuppliedValue
        {
            public SuppliedValue(T value) => Value = value;
            public T Value { get; }
        }
    }
}
