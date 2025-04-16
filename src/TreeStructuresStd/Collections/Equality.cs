using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
//using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures.Collections {

    // Helper class for construction
    /// <summary>Supports equality comparison.</summary>
    public static class Equality {
        /// <summary>Selects the key for performing equality comparison.</summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static EqualityComparer<TSource> ComparerBy<TSource, TKey>(Func<TSource, TKey> selector) {
            return new AnonymousEqualityComparer<TSource, TKey>(selector);
        }
        /// <summary>Returns a reference equality comparer.</summary>
        public static IEqualityComparer ReferenceComparer => Equality<object>.ReferenceComparer;

		/// <summary>
		/// Returns a comparer that compares by value if the type is a value type,
		/// and by reference if the type is a reference type.
		/// </summary>
		public static IEqualityComparer ValueOrReferenceComparer { get; } = new AnonymousEqualityComparer<object>(
            (a, b) => {
                if (a == null || b == null) return ReferenceEquals(a, b);
                var typeA = a.GetType();
                var typeB = b.GetType();
                Type underA = Nullable.GetUnderlyingType(typeA) ?? typeA;
                Type underB = Nullable.GetUnderlyingType(typeB) ?? typeB;
                if(underA.IsValueType && typeA == typeB) return a.Equals(b);
                else return ReferenceEquals(a, b);
            });
    }


    /// <summary>Supports equality comparison.</summary>
    public static class Equality<T> {
        /// <summary>Selects the key for performing equality comparison.</summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="selector"></param>
        /// <param name="keyComparer"></param>
        /// <returns></returns>
        public static EqualityComparer<T> ComparerBy<TKey>(Func<T, TKey> selector, IEqualityComparer<TKey>? keyComparer = null) {
            return new AnonymousEqualityComparer<T, TKey>(selector, keyComparer);
        }
        /// <summary>Selects the sequence for performing equality comparison.</summary>
        /// <typeparam name="U">The type of elements in the sequence.</typeparam>
        /// <param name="sequence"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static EqualityComparer<T> ComparerBySequence<U>(Func<T,IEnumerable<U>> sequence, IEqualityComparer<U>? comparer = null)  {
            return new AnonymousEqualityComparer<T, IEnumerable<U>>(sequence, new SequenceEqualityComparer<IEnumerable<U>,U>(comparer));
        }
        /// <summary>
        /// Returns a reference equality comparer for the type specified by generic argument.
        /// </summary>
        public static EqualityComparer<T> ReferenceComparer { get; } = new ReferenceEqualityComparer<T>();
		/// <summary>
		/// Returns a comparer that compares by value if T is a value type,
		/// and by reference if T is a reference type.
		/// </summary>
		public static EqualityComparer<T> ValueOrReferenceComparer { get; } = new AnonymousEqualityComparer<T>(
            (a, b) => typeof(T).IsValueType ? EqualityComparer<T>.Default.Equals(a, b) : ReferenceEquals(a, b));
    }
    public class SequenceEqualityComparer<TSeq,TItm> :EqualityComparer<TSeq> where TSeq : IEnumerable<TItm> {
        readonly IEqualityComparer<TItm> comparer;
        public SequenceEqualityComparer(IEqualityComparer<TItm>? itemEquality = null) {
            comparer = itemEquality ?? EqualityComparer<TItm>.Default;
        }
		///<inheritdoc/> 
		public override bool Equals(TSeq? x, TSeq? y) {
			if (x == null && y == null) { return true; }
			if (x == null || y == null) { return false; }
            return x.SequenceEqual(y, comparer);
		}
		///<inheritdoc/> 
		public override int GetHashCode(TSeq obj) {
			if (obj == null) return 0;
            var hash = new HashCode();
            foreach (var itm in obj) hash.Add(itm);
            return hash.ToHashCode();
		}
	}
    /// <summary>An object that supports equality comparison.</summary>
    public class AnonymousEqualityComparer<T> : EqualityComparer<T> {
        Func<T, T, bool> _equality;
        Func<T, int> _getHash;
        /// <summary>Initializes an instance for performing equality comparison with the specified function.</summary>
        public AnonymousEqualityComparer(Func<T, T, bool> equality, Func<T, int>? getHash = null) {
            _equality = equality;
            _getHash = getHash ?? new(x => RuntimeHelpers.GetHashCode(x));
        }
        /// <inheritdoc/>
		public override bool Equals(T? x, T? y) {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return _equality(x, y);
		}
        /// <inheritdoc/>
		public override int GetHashCode(T obj) {
            return _getHash(obj);
            //return RuntimeHelpers.GetHashCode(obj);
		}
	}
    /// <summary>An object that supports equality comparison.</summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class AnonymousEqualityComparer<TSource, TKey>:EqualityComparer<TSource>{
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

        ///<inheritdoc/> 
        public override bool Equals(TSource? x, TSource? y) {
            if (x == null && y == null) {
                return true;
            }
            if (x == null || y == null) {
                return false;
            }
            return comparer.Equals(selector(x), selector(y));
        }

        ///<inheritdoc/> 
		public override int GetHashCode(TSource obj) {
            if (obj == null) {
                return 0;
            }
            return comparer.GetHashCode(selector(obj));
        }
	}
    /// <summary>
    /// A generic object comparer that would only use object's reference,
    /// ingnoring any <see cref="IEquatable{T}"/> or <see cref="object.Equals(object?)"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ReferenceEqualityComparer<T> : EqualityComparer<T>{
        
        ///<inheritdoc/> 
		public override bool Equals(T? x, T? y) {
            return ReferenceEquals(x, y);
		}
        ///<inheritdoc/> 
		public override int GetHashCode(T obj) {
            return RuntimeHelpers.GetHashCode(obj);
		}
	}
}
