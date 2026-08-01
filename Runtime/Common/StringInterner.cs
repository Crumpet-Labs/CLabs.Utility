using System;
using System.Collections.Concurrent;

namespace CLabs.Utility {
    /// <summary>
    /// Thread-safe string pool that returns a single shared instance per distinct value, so callers can
    /// compare interned strings by reference and deduplicate memory. The span overloads intern without
    /// allocating when the value is already pooled.
    /// </summary>
    public interface IStringInterner {
        string Intern(string value);
        string Intern(ReadOnlySpan<char> span);
        void Remove(string value);
        void Remove(ReadOnlySpan<char> span);
    }

    public sealed class StringInterner : IStringInterner {
        private readonly ConcurrentDictionary<string, string> m_Pool = new();

        public string Intern(string value) {
            if (value is null) {
                return null;
            }

            return m_Pool.GetOrAdd(value, value);
        }

        public string Intern(ReadOnlySpan<char> span) {
            foreach (var kvp in m_Pool) {
                if (span.SequenceEqual(kvp.Key)) {
                    return kvp.Value;
                }
            }

            var newString = span.ToString();
            return m_Pool.GetOrAdd(newString, newString);
        }

        public void Remove(string value) {
            m_Pool.TryRemove(value, out _);
        }

        public void Remove(ReadOnlySpan<char> span) {
            foreach (var kvp in m_Pool) {
                if (span.SequenceEqual(kvp.Key)) {
                    m_Pool.TryRemove(kvp.Key, out _);
                    return;
                }
            }
        }
    }
}
