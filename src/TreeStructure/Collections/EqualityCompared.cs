using System;
using System.Collections;
//using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructure.Collections {

    // Helper class for construction
    /// <summary>
    /// 等価比較をサポートする
    /// </summary>
    public static class EqualityCompared {
        /// <summary>等価比較を行うキーを選択する</summary>
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

    /// <summary>
    /// 等価比較をサポートする
    /// </summary>
    public static class EqualityCompared<T> {
        /// <summary>等価比較を行うキーを選択する</summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IEqualityComparer<T> By<TKey>(Func<T, TKey> selector) {
            return new AnonymousEqualityComparer<T, TKey>(selector);
        }
        //public static IEqualityComparer<T> Default => System.Collections.Generic.EqualityComparer<T>.Default;
    }
    /// <summary>等価比較をサポートするオブジェクト</summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class AnonymousEqualityComparer<TSource, TKey>: IEqualityComparer<TSource> {
        readonly Func<TSource, TKey> selector;
        readonly IEqualityComparer<TKey> comparer;
        /// <summary>指定したキーのデフォルトの等価比較を行うインスタンスを初期化する</summary>
        /// <param name="selector"></param>
        public AnonymousEqualityComparer(Func<TSource, TKey> selector): this(selector, null) { }
        /// <summary>キーを指定して等価比較を行うインスタンスを初期化する</summary>
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
