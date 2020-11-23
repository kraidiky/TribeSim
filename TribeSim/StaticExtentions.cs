using System;
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
    }
}