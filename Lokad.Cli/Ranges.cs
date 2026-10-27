using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Lokad.Cli
{
    /// <summary>A sequence of integer ranges, parsed from comma-separated elements such as <c>1,4,5-6,-10,20-</c>.</summary>
    /// <remarks>Open bounds use <see cref="int.MinValue"/> or <see cref="int.MaxValue"/>. The default value is an empty list.</remarks>
    public readonly struct Ranges : IReadOnlyList<(int First, int Last)>, IParsable<Ranges>
    {
        private readonly (int First, int Last)[] _ranges;

        /// <summary>Copies the supplied range elements.</summary>
        /// <param name="ranges">Range elements to store.</param>
        public Ranges(IEnumerable<(int First, int Last)> ranges)
        {
            if (ranges == null) throw new ArgumentNullException(nameof(ranges));
            _ranges = ToArray(ranges);
        }

        private Ranges((int First, int Last)[] ranges)
        {
            _ranges = ranges;
        }

        /// <summary>The number of stored range elements.</summary>
        public int Count => (_ranges ?? Array.Empty<(int First, int Last)>()).Length;

        /// <summary>Gets the stored lower and upper bounds of a range element.</summary>
        /// <param name="index">Zero-based element index.</param>
        public (int First, int Last) this[int index] => (_ranges ?? Array.Empty<(int First, int Last)>())[index];

        /// <summary>Parses comma-separated single values, closed ranges and open bounds.</summary>
        /// <param name="s">Range text to parse.</param>
        /// <param name="provider">Unused; integer bounds use invariant culture.</param>
        /// <returns>The parsed range elements.</returns>
        /// <exception cref="FormatException">The text is empty or contains an invalid range element.</exception>
        public static Ranges Parse(string s, IFormatProvider? provider)
        {
            if (TryParse(s, provider, out var result))
                return result;

            throw new FormatException("Invalid ranges value.");
        }

        /// <summary>Attempts to parse comma-separated single values, closed ranges and open bounds.</summary>
        /// <param name="s">Range text to parse; null and whitespace-only text are invalid.</param>
        /// <param name="provider">Unused; integer bounds use invariant culture.</param>
        /// <param name="result">The parsed ranges on success; otherwise the empty default value.</param>
        /// <returns>Whether every range element was parsed successfully.</returns>
        public static bool TryParse(string? s, IFormatProvider? provider, out Ranges result)
        {
            result = default;

            if (string.IsNullOrWhiteSpace(s))
                return false;

            var elements = s.Split(',');
            var ranges = new (int First, int Last)[elements.Length];
            for (var i = 0; i < elements.Length; i++)
            {
                if (!TryParseElement(elements[i].Trim(), out ranges[i]))
                    return false;
            }

            result = new Ranges(ranges);
            return true;
        }

        /// <summary>Enumerates the stored range elements.</summary>
        /// <returns>An enumerator over the range elements.</returns>
        public IEnumerator<(int First, int Last)> GetEnumerator() =>
            ((IEnumerable<(int First, int Last)>)(_ranges ?? Array.Empty<(int First, int Last)>())).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private static bool TryParseElement(string element, out (int First, int Last) range)
        {
            range = default;

            if (element.Length == 0)
                return false;

            var separator = element.IndexOf('-');
            if (separator < 0)
            {
                if (!TryParseInt32(element, out var value))
                    return false;

                range = (value, value);
                return true;
            }

            if (element.IndexOf('-', separator + 1) >= 0)
                return false;

            var firstPart = element.Substring(0, separator);
            var lastPart = element.Substring(separator + 1);
            if (firstPart.Length == 0 && lastPart.Length == 0)
                return false;

            var first = int.MinValue;
            if (firstPart.Length > 0 && !TryParseInt32(firstPart, out first))
                return false;

            var last = int.MaxValue;
            if (lastPart.Length > 0 && !TryParseInt32(lastPart, out last))
                return false;

            range = (first, last);
            return true;
        }

        private static bool TryParseInt32(string value, out int parsed) =>
            int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed);

        private static (int First, int Last)[] ToArray(IEnumerable<(int First, int Last)> ranges)
        {
            if (ranges is (int First, int Last)[] array)
                return ((int First, int Last)[])array.Clone();

            var list = new List<(int First, int Last)>();
            foreach (var range in ranges)
                list.Add(range);

            return list.ToArray();
        }
    }
}
