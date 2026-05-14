using System;
using System.Globalization;

namespace CLabs.Utility {
    /// <summary>
    /// Engine-agnostic colour. Internal representation is RGBA as floats in 0..1.
    /// Construct via <see cref="FromRgb"/>, <see cref="FromRgb255"/>, <see cref="FromHex"/>,
    /// <see cref="FromHsv"/>, <see cref="FromHsl"/>, or <see cref="FromCmyk"/>. Convert to
    /// engine-native colour types via platform adapters (e.g. CLabs.Utility.Unity's
    /// <c>ToUnityColor()</c>).
    /// </summary>
    public readonly struct Color : IEquatable<Color> {
        public readonly float R;
        public readonly float G;
        public readonly float B;
        public readonly float A;

        public Color(float r, float g, float b, float a = 1f) {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public byte R8 => (byte)Math.Clamp(MathF.Round(R * 255f), 0f, 255f);
        public byte G8 => (byte)Math.Clamp(MathF.Round(G * 255f), 0f, 255f);
        public byte B8 => (byte)Math.Clamp(MathF.Round(B * 255f), 0f, 255f);
        public byte A8 => (byte)Math.Clamp(MathF.Round(A * 255f), 0f, 255f);

        // --- Factories ---

        public static Color FromRgb(float r, float g, float b, float a = 1f) =>
            new(r, g, b, a);

        public static Color FromRgb255(int r, int g, int b, int a = 255) =>
            new(r / 255f, g / 255f, b / 255f, a / 255f);

        /// <summary>
        /// Parses hex strings: "#RGB", "#RGBA", "#RRGGBB", "#RRGGBBAA" (with or without leading '#').
        /// </summary>
        public static Color FromHex(string hex) {
            if (string.IsNullOrWhiteSpace(hex))
                throw new ArgumentException("Hex string is null or empty.", nameof(hex));

            var span = hex.AsSpan().Trim();
            if (span[0] == '#') span = span[1..];

            int r, g, b, a = 255;
            switch (span.Length) {
                case 3:
                    r = ParseNibble(span[0]) * 17;
                    g = ParseNibble(span[1]) * 17;
                    b = ParseNibble(span[2]) * 17;
                    break;
                case 4:
                    r = ParseNibble(span[0]) * 17;
                    g = ParseNibble(span[1]) * 17;
                    b = ParseNibble(span[2]) * 17;
                    a = ParseNibble(span[3]) * 17;
                    break;
                case 6:
                    r = ParseByte(span[..2]);
                    g = ParseByte(span.Slice(2, 2));
                    b = ParseByte(span.Slice(4, 2));
                    break;
                case 8:
                    r = ParseByte(span[..2]);
                    g = ParseByte(span.Slice(2, 2));
                    b = ParseByte(span.Slice(4, 2));
                    a = ParseByte(span.Slice(6, 2));
                    break;
                default:
                    throw new FormatException($"Unrecognised hex colour length: '{hex}'. Expected 3, 4, 6, or 8 hex digits.");
            }

            return FromRgb255(r, g, b, a);
        }

        /// <summary>Hue in degrees [0, 360), saturation/value in [0, 1].</summary>
        public static Color FromHsv(float h, float s, float v, float a = 1f) {
            h = ((h % 360f) + 360f) % 360f;
            s = Math.Clamp(s, 0f, 1f);
            v = Math.Clamp(v, 0f, 1f);

            var c = v * s;
            var x = c * (1f - Math.Abs((h / 60f) % 2f - 1f));
            var m = v - c;

            float r, g, b;
            if (h < 60f)       { r = c; g = x; b = 0; }
            else if (h < 120f) { r = x; g = c; b = 0; }
            else if (h < 180f) { r = 0; g = c; b = x; }
            else if (h < 240f) { r = 0; g = x; b = c; }
            else if (h < 300f) { r = x; g = 0; b = c; }
            else               { r = c; g = 0; b = x; }

            return new Color(r + m, g + m, b + m, a);
        }

        /// <summary>Hue in degrees [0, 360), saturation/lightness in [0, 1].</summary>
        public static Color FromHsl(float h, float s, float l, float a = 1f) {
            h = ((h % 360f) + 360f) % 360f;
            s = Math.Clamp(s, 0f, 1f);
            l = Math.Clamp(l, 0f, 1f);

            var c = (1f - Math.Abs(2f * l - 1f)) * s;
            var x = c * (1f - Math.Abs((h / 60f) % 2f - 1f));
            var m = l - c / 2f;

            float r, g, b;
            if (h < 60f)       { r = c; g = x; b = 0; }
            else if (h < 120f) { r = x; g = c; b = 0; }
            else if (h < 180f) { r = 0; g = c; b = x; }
            else if (h < 240f) { r = 0; g = x; b = c; }
            else if (h < 300f) { r = x; g = 0; b = c; }
            else               { r = c; g = 0; b = x; }

            return new Color(r + m, g + m, b + m, a);
        }

        /// <summary>CMYK components in [0, 1].</summary>
        public static Color FromCmyk(float c, float m, float y, float k, float a = 1f) {
            c = Math.Clamp(c, 0f, 1f);
            m = Math.Clamp(m, 0f, 1f);
            y = Math.Clamp(y, 0f, 1f);
            k = Math.Clamp(k, 0f, 1f);

            var r = (1f - c) * (1f - k);
            var g = (1f - m) * (1f - k);
            var b = (1f - y) * (1f - k);
            return new Color(r, g, b, a);
        }

        // --- Conversions out ---

        /// <summary>"#RRGGBBAA" form.</summary>
        public string ToHex() => $"#{R8:X2}{G8:X2}{B8:X2}{A8:X2}";

        /// <summary>"#RRGGBB" form (alpha dropped).</summary>
        public string ToHexRgb() => $"#{R8:X2}{G8:X2}{B8:X2}";

        /// <summary>Returns (H [0,360), S [0,1], V [0,1]).</summary>
        public (float H, float S, float V) ToHsv() {
            var max = MathF.Max(R, MathF.Max(G, B));
            var min = MathF.Min(R, MathF.Min(G, B));
            var delta = max - min;

            float h;
            if (delta == 0f) h = 0f;
            else if (max == R) h = 60f * (((G - B) / delta) % 6f);
            else if (max == G) h = 60f * ((B - R) / delta + 2f);
            else               h = 60f * ((R - G) / delta + 4f);
            if (h < 0f) h += 360f;

            var s = max == 0f ? 0f : delta / max;
            return (h, s, max);
        }

        /// <summary>Returns (H [0,360), S [0,1], L [0,1]).</summary>
        public (float H, float S, float L) ToHsl() {
            var max = MathF.Max(R, MathF.Max(G, B));
            var min = MathF.Min(R, MathF.Min(G, B));
            var delta = max - min;
            var l = (max + min) / 2f;

            float h, s;
            if (delta == 0f) {
                h = 0f;
                s = 0f;
            } else {
                s = l < 0.5f ? delta / (max + min) : delta / (2f - max - min);

                if (max == R)      h = 60f * (((G - B) / delta) % 6f);
                else if (max == G) h = 60f * ((B - R) / delta + 2f);
                else               h = 60f * ((R - G) / delta + 4f);
                if (h < 0f) h += 360f;
            }

            return (h, s, l);
        }

        /// <summary>Returns (C, M, Y, K) all in [0, 1].</summary>
        public (float C, float M, float Y, float K) ToCmyk() {
            var k = 1f - MathF.Max(R, MathF.Max(G, B));
            if (k >= 1f) return (0f, 0f, 0f, 1f);
            var c = (1f - R - k) / (1f - k);
            var m = (1f - G - k) / (1f - k);
            var y = (1f - B - k) / (1f - k);
            return (c, m, y, k);
        }

        // --- Named colours ---

        public static Color White       => new(1f, 1f, 1f, 1f);
        public static Color Black       => new(0f, 0f, 0f, 1f);
        public static Color Transparent => new(0f, 0f, 0f, 0f);
        public static Color Red         => new(1f, 0f, 0f, 1f);
        public static Color Green       => new(0f, 1f, 0f, 1f);
        public static Color Blue        => new(0f, 0f, 1f, 1f);
        public static Color Yellow      => new(1f, 1f, 0f, 1f);
        public static Color Cyan        => new(0f, 1f, 1f, 1f);
        public static Color Magenta     => new(1f, 0f, 1f, 1f);
        public static Color Grey        => new(0.5f, 0.5f, 0.5f, 1f);

        // --- Equality ---

        public bool Equals(Color other) =>
            R == other.R && G == other.G && B == other.B && A == other.A;

        public override bool Equals(object obj) => obj is Color c && Equals(c);

        public override int GetHashCode() =>
            HashCode.Combine(R, G, B, A);

        public static bool operator ==(Color a, Color b) => a.Equals(b);
        public static bool operator !=(Color a, Color b) => !a.Equals(b);

        public override string ToString() =>
            $"RGBA({R:0.##}, {G:0.##}, {B:0.##}, {A:0.##})";

        // --- Internals ---

        private static int ParseNibble(char c) {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'a' && c <= 'f') return c - 'a' + 10;
            if (c >= 'A' && c <= 'F') return c - 'A' + 10;
            throw new FormatException($"Invalid hex digit: '{c}'.");
        }

        private static int ParseByte(ReadOnlySpan<char> s) =>
            int.Parse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
    }
}
