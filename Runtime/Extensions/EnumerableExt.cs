using System;
using System.Collections.Generic;

namespace CLabs.Utility {
    public static class EnumerableExt {
        public static void Complete<TSource>(this IEnumerable<TSource> source, Action<TSource> selector) {
            foreach(var s in source) selector.Invoke(s);
        }
    }
}