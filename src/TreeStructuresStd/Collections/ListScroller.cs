using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures.Collections {

	/// <summary>
	/// Defines a mechanism to navigate through a collection while maintaining an index position.
	/// </summary>
	/// <typeparam name="T">The type of elements in the collection.</typeparam>
	/// <typeparam name="TList">The type implementing <see cref="IListScroller{T, TList}"/>.</typeparam>
	public interface IListScroller<T, TList> where TList : IListScroller<T, TList> {
		/// <summary>
		/// Resets the current index to its initial state. If an element exists, it is set to 0; otherwise, it is set to -1.
		/// </summary>
		/// <returns> Current instance with the index reset.</returns>
		TList Reset();

		/// <summary>
		/// Gets the current element at the current index.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the collection is empty.</exception>
		T Current { get; }

		/// <summary>
		/// Gets the current index position within the collection.
		/// </summary>
		/// <remarks>
		/// If the collection is empty, the value is <c>-1</c>.
		/// </remarks>
		int CurrentIndex { get; }

		/// <summary>
		/// Moves the current position to the specified index.
		/// </summary>
		/// <param name="index">The target index to move to.</param>
		/// <returns>Current instance of <typeparamref name="TList"/> with the updated position.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when the specified index is out of the valid range.
		/// </exception>
		TList MoveTo(int index);

		/// <summary>
		/// Determines whether the scroller can move to the specified index.
		/// </summary>
		/// <param name="index">The target index to check.</param>
		/// <returns><c>true</c> if the index is within a valid range; otherwise, <c>false</c>.</returns>
		bool CanMoveTo(int index);

		/// <summary>
		/// Gets the read-only list of items being scrolled through.
		/// </summary>
		IReadOnlyList<T> Items { get; }
	}
	/// <summary>
	/// Provides a scroller that allows navigation through a list of elements.
	/// </summary>
	/// <typeparam name="T">The type of elements in the list.</typeparam>
	public class ListScroller<T> : IListScroller<T, ListScroller<T>> {
		IReadOnlyList<T> _list;
		int curIdx;

		/// <summary>
		/// Initializes a new instance of the <see cref="ListScroller{T}"/> class with the specified collection.
		/// </summary>
		/// <param name="list">
		/// The collection of elements to be scrolled through.  
		/// If <c>null</c> is provided, an empty list will be used instead.
		/// </param>
		/// <remarks>
		/// If the provided collection contains elements, the initial index is set to <c>0</c>.  
		/// If the collection is empty, the initial index is set to <c>-1</c>.
		/// </remarks>
		public ListScroller(IEnumerable<T> list) {
			_list = list?.ToArray() ?? Array.Empty<T>();
			if (_list.Any()) curIdx = 0;
			else curIdx = -1;
		}
		/// <inheritdoc/>
		public T Current =>
			CurrentIndex < 0 || CurrentIndex >= _list.Count
			? throw new InvalidOperationException("No valid current element.")
			: _list[CurrentIndex];
		/// <inheritdoc/>
		public int CurrentIndex => curIdx;
		
		/// <inheritdoc/>
		public bool CanMoveTo(int index) {
			if (_list.Any()) {
				if (0 <= index && index < _list.Count) return true;
			} else {
				if(index == -1) return true;
			}
			return false;
		}
		/// <inheritdoc/>
		public ListScroller<T> MoveTo(int index) {
			if (!CanMoveTo(index)) throw new InvalidOperationException();
			curIdx = index;
			return this;
		}
		/// <inheritdoc/>
		public ListScroller<T> Reset() {
			if (_list.Any()) curIdx = 0;
			else curIdx = -1;
			return this;
		}
		/// <inheritdoc/>
		public IReadOnlyList<T> Items => _list;
	}
    
}
