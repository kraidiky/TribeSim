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
            source = source.Trim();
            try {
                return Convert.ToDouble(source);
            } catch (FormatException e) {
                source = source.Replace(',', '.'); // Используется редко, так что пофиг на аллокации.
                return Convert.ToDouble(source, CultureInfo.InvariantCulture);
            }
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