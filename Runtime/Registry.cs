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

        public IDisposable Register(TKey key, TValue value) {
            m_Entries[key] = value;
            OnRegistered?.Invoke(key, value);
            return new Disposable(() =>
            {
                OnUnregistered?.Invoke(key, value);
                m_Entries.Remove(key);
            });
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
}
