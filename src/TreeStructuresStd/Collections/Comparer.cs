using System;
using System.Collections.Generic;
using System.Text;

namespace TreeStructures.Collections {
	/// <summary>
	/// Provides extension methods for <see cref="IComparer{T}"/>.
	/// </summary>
	public static class ComparerExtensions{

		/// <summary>比較結果を反転させる。</summary>
		public static InvertibleComparer<T> Invert<T>(this IComparer<T> self){
			if(self is InvertibleComparer<T> inv){
				inv.Invert();
				return inv;
			}
			return new InvertibleComparer<T>(self,true);
        }
    }
	/// <summary>
	/// Represents a comparer that can be inverted.
	/// </summary>
	/// <typeparam name="T">The type of elements to compare.</typeparam>
	public class InvertibleComparer<T> : Comparer<T> {
		int f;
		IComparer<T> _comparer;
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		/// <param name="comparer">The comparer to be inverted.</param>
		/// <param name="invert">True if the comparison results should be inverted upon initialization.</param>
		public InvertibleComparer(IComparer<T> comparer,bool invert = false) : base(){
			_comparer = comparer;
			f = invert ? -1 : 1;
		}
		/// <summary>
		/// Inverts the comparison result.
		/// </summary>
		public InvertibleComparer<T> Invert(){
			f *= -1;
			return this;
		}

		/// <inheritdoc/>
		public override int Compare(T? x, T? y) {
			return _comparer.Compare(x, y) * f;
		}
	}
}
