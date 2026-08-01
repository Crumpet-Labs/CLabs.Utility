using System;
using System.ComponentModel;
using System.Globalization;

namespace CLabs.Utility {
    /// <summary>
    /// Identifies whoever owns some piece of game state — an entity, a container, a player. The
    /// <see cref="TypeConverter"/> is load-bearing for persistence: the constructor is private and <see cref="ID"/> is
    /// get-only, so without it a serializer writes <c>{"ID":7}</c> and silently reads back <c>default</c>. Every package
    /// snapshot keyed by an owner (Equipment, Inventory, …) depends on this round-tripping as a bare number.
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(OwnerIdConverter))]
    public readonly struct OwnerId : IEquatable<OwnerId> {
        private readonly int m_Identifier;

        private OwnerId(int id) {
            m_Identifier = id;
        }

        public int ID => m_Identifier;

        public static implicit operator int(OwnerId id) => id.ID;
        public static implicit operator OwnerId(int id) => new(id);
        public static implicit operator OwnerId(char id) => new(id);

        public override string ToString() => m_Identifier.ToString();
        public override int GetHashCode() => m_Identifier.GetHashCode();

        public bool Equals(OwnerId other) => m_Identifier.Equals(other.m_Identifier);
        public override bool Equals(object obj) => obj is OwnerId other && Equals(other);

        public static bool operator ==(OwnerId left, OwnerId right) => left.Equals(right);
        public static bool operator !=(OwnerId left, OwnerId right) => !left.Equals(right);
    }

    internal sealed class OwnerIdConverter : TypeConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            return sourceType == typeof(string) || sourceType == typeof(int) || base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            return destinationType == typeof(string) || destinationType == typeof(int) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is int number) {
                return (OwnerId)number;
            }

            if (value is string text) {
                return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                    ? (OwnerId)parsed
                    : default(OwnerId);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            var owner = (OwnerId)value;

            if (destinationType == typeof(string)) {
                return owner.ID.ToString(CultureInfo.InvariantCulture);
            }

            if (destinationType == typeof(int)) {
                return owner.ID;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}