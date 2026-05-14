using System;

namespace CLabs.Utility {
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
}