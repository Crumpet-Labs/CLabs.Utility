using System;
using System.Collections.Generic;

namespace CLabs.Utility {
    public abstract class Registry<TKey, TValue> {
        private readonly Dictionary<TKey, TValue> m_Entries = new();

        public IEnumerable<TValue> Values => m_Entries.Values;
        public IEnumerable<TKey> Keys => m_Entries.Keys;
        public int Count => m_Entries.Count;

        public event Action<TKey, TValue> OnRegistered;
        public event Action<TKey, TValue> OnUnregistered;

        /// <summary>
        /// Registers a value and hands back the handle that removes it. Disposal IS the removal, with no
        /// unregister counterpart, and the handle can only ever remove the registration IT made: disposing it
        /// twice does nothing the second time, and a stale handle whose key has since been re-registered leaves
        /// the current occupant alone. That last part is what makes pooled entities safe, where ids get reused
        /// and a previous occupant's handle may outlive its registration.
        /// </summary>
        public IDisposable Register(TKey key, TValue value) {
            m_Entries[key] = value;
            OnRegistered?.Invoke(key, value);

            return new RegistryRegistration<TKey, TValue>(this, key, value);
        }

        internal bool Unregister(TKey key, TValue value) {
            if (false == m_Entries.TryGetValue(key, out var current)) {
                return false;
            }

            if (false == EqualityComparer<TValue>.Default.Equals(current, value)) {
                return false;
            }

            OnUnregistered?.Invoke(key, value);
            m_Entries.Remove(key);

            return true;
        }

        public TValue Get(TKey key) {
            return m_Entries[key];
        }

        public bool TryGet(TKey key, out TValue value) {
            return m_Entries.TryGetValue(key, out value);
        }

        public bool TryGetValue(TKey key, out TValue value) {
            return m_Entries.TryGetValue(key, out value);
        }

        public IEnumerable<TValue> Get(IEnumerable<TKey> keys) {
            foreach (var key in keys) {
                if (m_Entries.TryGetValue(key, out var value)) {
                    yield return value;
                }
            }
        }

        public bool Contains(TKey key) {
            return m_Entries.ContainsKey(key);
        }
    }

    /// <summary>
    /// The handle returned by <see cref="Registry{TKey,TValue}.Register"/>. Holds the key and the value it
    /// registered rather than closing over them, so removal can check it is still the current occupant before
    /// evicting anything. A closure would happily remove whoever holds the key by the time it runs.
    /// </summary>
    internal sealed class RegistryRegistration<TKey, TValue> : IDisposable {
        private readonly Registry<TKey, TValue> m_Registry;
        private readonly TKey m_Key;
        private readonly TValue m_Value;
        private bool m_Disposed;

        internal RegistryRegistration(Registry<TKey, TValue> registry, TKey key, TValue value) {
            m_Registry = registry;
            m_Key = key;
            m_Value = value;
            m_Disposed = false;
        }

        public void Dispose() {
            if (m_Disposed) {
                return;
            }

            m_Disposed = true;
            m_Registry.Unregister(m_Key, m_Value);
        }
    }
}
