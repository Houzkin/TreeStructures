using System;
using System.Collections;
//using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures.Collections {

    // Helper class for construction
    /// <summary>Supports equality comparison.</summary>
    public static class EqualityCompared {
        /// <summary>Selects the key for performing equality comparison.</summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IEqualityComparer<TSource> By<TSource, TKey>(Func<TSource, TKey> selector) {
            return new AnonymousEqualityComparer<TSource, TKey>(selector);
        }

        //public static IEqualityComparer<TSource> By<TSource, TKey>(TSource ignored,Func<TSource, TKey> selector) {
        //    return new AnonymousEqualityComparer<TSource, TKey>(selector);
        //}
    }


    /// <summary>Supports equality comparison.</summary>
    public static class EqualityCompared<T> {
        /// <summary>Selects the key for performing equality comparison.</summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IEqualityComparer<T> By<TKey>(Func<T, TKey> selector) {
            return new AnonymousEqualityComparer<T, TKey>(selector);
        }
        //public static IEqualityComparer<T> Default => System.Collections.Generic.EqualityComparer<T>.Default;
    }
    /// <summary>An object that supports equality comparison.</summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class AnonymousEqualityComparer<TSource, TKey>: IEqualityComparer<TSource> {
        readonly Func<TSource, TKey> selector;
        readonly IEqualityComparer<TKey> comparer;
        /// <summary>Initializes an instance for performing default equality comparison with the specified key.</summary>
        /// <param name="selector"></param>
        public AnonymousEqualityComparer(Func<TSource, TKey> selector): this(selector, null) { }

        /// <summary>Initializes an instance for performing equality comparison with the specified key.</summary>
        /// <param name="selector"></param>
        /// <param name="comparer"></param>
        /// <exception cref="ArgumentNullException"></exception>
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
