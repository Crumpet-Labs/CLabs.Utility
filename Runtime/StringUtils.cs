using System.Globalization;

namespace CLabs.Utility {
    public static class StringUtils {
        private static readonly TextInfo TextInfo;
        static StringUtils() {
            TextInfo = CultureInfo.CurrentCulture.TextInfo;
        }

        public static string ToPropertyName(this string value)
            => value.ToTitleCase().RemoveWhiteSpace();

        public static string ToTitleCase(this string value)
            => TextInfo.ToTitleCase(value);
        
        public static string RemoveWhiteSpace(this string value)
            => value.Replace(" ", "");
    }
}