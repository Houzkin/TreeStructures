using System;
using System.Collections.Generic;
using System.Text;

namespace System.Linq {
	internal static class LinqCompat {
	/// <summary>prepend</summary>
		internal static IEnumerable<T> AddHead<T>(this IEnumerable<T> source, T element) {
#if NETSTANDARD2_0
			if (source == null) throw new ArgumentNullException(nameof(source));
			yield return element;
			foreach (var item in source)
				yield return item;
#else
			return source.Prepend(element);
#endif
		}

			/// <summary>append</summary>
		internal static IEnumerable<T> AddTail<T>(this IEnumerable<T> source, T element) {
#if NETSTANDARD2_0
			if (source == null) throw new ArgumentNullException(nameof(source));
			foreach (var item in source)
				yield return item;
			yield return element;
#else
			return source.Append(element);
#endif
		}


	}
}
