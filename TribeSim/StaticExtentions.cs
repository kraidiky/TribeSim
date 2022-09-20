using System;
using System.Collections.Generic;
using System.Globalization;

namespace TribeSim
{
    public static class StaticExtentions
    {
        public static double ConvertToDouble(this string source)
        {
            if (source == null)
                throw new ArgumentException("Can't convert null to double");
            if (double.TryParse(source, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                return d;
            var replaced = source.Replace(',', '.'); // Используется редко, так что пофиг на аллокации.
            if (double.TryParse(replaced, NumberStyles.Float, CultureInfo.InvariantCulture, out d))
                return d;
            throw new Exception($"Something wrong with number format: {source}");
        }

        public static List<string> significantNumbersFormat = new List<string>();

        public static string ToSignificantNumbers(this float number, int digits = 3)
        {
            if (number == 0)
                return "0";
            var size = Math.Log10(Math.Abs(number));
            int format = Math.Max(0, digits - 1 - (int) Math.Floor(size));
            if (format > significantNumbersFormat.Count)
                for (int i = significantNumbersFormat.Count; i <= format; i++)
                    significantNumbersFormat.Add($"f{i}");
            return number.ToString(significantNumbersFormat[format], CultureInfo.InvariantCulture);
        }

        public static string ToSignificantNumbers(this double number, int digits = 3)
        {
            if (number == 0)
                return "0";
            var size = Math.Log10(Math.Abs(number));
            int format = Math.Max(0, digits - 1 - (int)Math.Floor(size));
            if (format >= significantNumbersFormat.Count)
                for (int i = significantNumbersFormat.Count; i <= format; i++)
                    significantNumbersFormat.Add($"f{i}");
            return number.ToString(significantNumbersFormat[format], CultureInfo.InvariantCulture);
        }
    }
}