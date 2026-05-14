using System;
using System.Collections.Generic;

namespace CLabs.Utility {
    public sealed class Disposable : IDisposable {
        private readonly Action m_DisposeAction;

        public Disposable(Action disposeAction) {
            m_DisposeAction = disposeAction;
        }

        public void Dispose() => m_DisposeAction?.Invoke();
    }

    public sealed class DisposableCollection : IDisposable {
        private readonly List<IDisposable> m_Disposables;

        public DisposableCollection(params IDisposable[] disposables) {
            m_Disposables = new List<IDisposable>(disposables);
        }

        public DisposableCollection(IEnumerable<IDisposable> disposables) {
            m_Disposables = new List<IDisposable>(disposables);
        }

        public void Add(IDisposable disposable) => m_Disposables.Add(disposable);

        public void Dispose() {
            foreach (var disposable in m_Disposables) {
                disposable?.Dispose();
            }

            m_Disposables.Clear();
        }
    }
}
