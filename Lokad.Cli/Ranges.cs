using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Lokad.Cli
{
    public readonly struct Ranges : IReadOnlyList<(int First, int Last)>, IParsable<Ranges>
    {
        private readonly (int First, int Last)[] _ranges;

        public Ranges(IEnumerable<(int First, int Last)> ranges)
        {
            if (ranges == null) throw new ArgumentNullException(nameof(ranges));
            _ranges = ToArray(ranges);
        }

        private Ranges((int First, int Last)[] ranges)
        {
            _ranges = ranges;
        }

        public int Count => (_ranges ?? Array.Empty<(int First, int Last)>()).Length;

        public (int First, int Last) this[int index] => (_ranges ?? Array.Empty<(int First, int Last)>())[index];

        public static Ranges Parse(string s, IFormatProvider? provider)
        {
            if (TryParse(s, provider, out var result))
                return result;

            throw new FormatException("Invalid ranges value.");
        }

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
