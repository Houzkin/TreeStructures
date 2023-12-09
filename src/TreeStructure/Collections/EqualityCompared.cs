using System;
using System.Collections;
//using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructure.Collections {

    // Helper class for construction
    public static class EqualityCompared {
        public static IEqualityComparer<TSource> By<TSource, TKey>(Func<TSource, TKey> selector) {
            return new AnonymousEqualityComparer<TSource, TKey>(selector);
        }

        public static IEqualityComparer<TSource> By<TSource, TKey>(TSource ignored,Func<TSource, TKey> selector) {
            return new AnonymousEqualityComparer<TSource, TKey>(selector);
        }
        //public static IEqualityComparer<TSource> Between<TSource,TKey>(Func<TSource,TSource,bool> comparison) {
        //    //return new AnonymousEqualityComparer(x=>x,EqualityComparer<TSource>.)
        //}
    }

    public static class EqualityCompared<T> {
        public static IEqualityComparer<T> By<TKey>(Func<T, TKey> selector) {
            return new AnonymousEqualityComparer<T, TKey>(selector);
        }
        //public static IEqualityComparer<T> Default => System.Collections.Generic.EqualityComparer<T>.Default;
    }

    public class AnonymousEqualityComparer<TSource, TKey>: IEqualityComparer<TSource> {
        readonly Func<TSource, TKey> selector;
        readonly IEqualityComparer<TKey> comparer;

        public AnonymousEqualityComparer(Func<TSource, TKey> selector): this(selector, null) { }

        public AnonymousEqualityComparer(Func<TSource, TKey> selector, IEqualityComparer<TKey>? comparer) {
            this.comparer = comparer ?? System.Collections.Generic.EqualityComparer<TKey>.Default;
            this.selector = selector ?? throw new ArgumentNullException(nameof(selector));
        }

        public bool Equals(TSource? x, TSource? y) {
            if (x == null && y == null) {
                return true;
            }
            if (x == null || y == null) {
                return false;
            }
            return comparer.Equals(selector(x), selector(y));
        }

        public int GetHashCode(TSource obj) {
            if (obj == null) {
                throw new ArgumentNullException(nameof(obj));
            }
            return comparer.GetHashCode(selector(obj));
        }
    }
}
