using System;
using System.Collections.Generic;
//using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures.Collections {

	/// <summary>
	/// Supports aligning the list.
	/// </summary>
	/// <typeparam name="T">The type of elements.</typeparam>
	/// <typeparam name="TList">The type of the list.</typeparam>
	public class ListAligner<T, TList> : ListAligner<T,T,TList>  where TList : IList<T> {
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		/// <param name="editList">The collection to operate on.</param>
		/// <param name="insert">The insertion operation.</param>
		/// <param name="replace">The replacement operation.</param>
		/// <param name="remove">The removal operation.</param>
		/// <param name="move">The movement operation.</param>
		/// <param name="clear">The clearing operation.</param>
		/// <param name="comparer"></param>
		public ListAligner(TList editList,
			Action<TList, int, T>? insert = null, Action<TList, int, T>? replace = null,
			Action<TList, int>? remove = null, Action<TList, int, int>? move = null, Action<TList>? clear = null, IEqualityComparer<T>? comparer = null)
			: base(editList, x => x, (x, y) => comparer?.Equals(x, y) ?? EqualityComparer<T>.Default.Equals(x, y), insert, replace, move, remove, clear) {

		}
	}
	/// <summary>
	/// A utility class that manipulates a given list to align its element order with a specified sequence.
	/// </summary>
	/// <typeparam name="T">The type of elements in the list.</typeparam>
	/// <typeparam name="U">The type of elements used for comparison.</typeparam>
	/// <typeparam name="TList">The type of the target list, which must implement <see cref="IList{T}"/>.</typeparam>
	public class ListAligner<T,U,TList> where TList : IList<T> {
		TList _editList;
		Action<int, T> _insert;
		Action<int, T> _replace;
		Action<int, int> _move;
		Action<int> _remove;
		Action _clear;
		Func<T, U, bool> _equality;
		Func<U, T> _convert;
		/// <summary>
		/// Initializes a new instance of the <see cref="ListAligner{T, U, TList}"/> class to reorder a list based on a given sequence.
		/// </summary>
		/// <param name="editList">The target list to be aligned.</param>
		/// <param name="convert">A function to convert an element of type <typeparamref name="U"/> to type <typeparamref name="T"/>.</param>
		/// <param name="equality">A function that determines whether an element of type <typeparamref name="T"/> is equal to an element of type <typeparamref name="U"/>.</param>
		/// <param name="insert">A function to insert an element into the list. By default, <see cref="IList{T}.Insert(int, T)"/> is used.</param>
		/// <param name="replace">A function to replace an element in the list. By default, assignment via the indexer of <see cref="IList{T}"/> is used.</param>
		/// <param name="move">A function to move an element within the list. By default, this is handled via manual insertion and deletion.</param>
		/// <param name="remove">A function to remove an element from the list. By default, <see cref="IList{T}.RemoveAt(int)"/> is used.</param>
		/// <param name="clear">A function to clear the list. By default, <see cref="IList{T}.Clear()"/> is used.</param>
		public ListAligner(TList editList, Func<U, T> convert, Func<T, U, bool> equality, Action<TList, int, T>? insert = null, Action<TList, int, T>? replace = null, Action<TList, int, int>? move = null, Action<TList, int>? remove=null, Action<TList>? clear=null) {
			_editList = editList;
			_equality = equality;
			_convert = convert;

			_insert = (insert != null) ? (i, t) => insert(_editList, i, t) : (i, t) => _editList.Insert(i, t);
			_replace = (replace != null) ? (i, t) => replace(_editList, i, t) : (i, t) => _editList[i] = t;
			_remove = (remove != null) ? i => remove(_editList, i) : i => _editList.RemoveAt(i);
			_move = (move != null) ? (ordi, newi) => move(_editList, ordi, newi)
			: (ordi, newi) => {
				var tmp = _editList[ordi];
				_remove(ordi);
				_insert(newi, tmp);
			};
			_clear = (clear != null) ? () => clear(_editList) : () => _editList.Clear();
		}
		void setitem(int index,U item,IEnumerable<U> untils) {
			if (_editList.Skip(index).Any(x => _equality(x, item))) {
				var tgt = _editList.Skip(index).Select((v, i) => new { v, i }).First(a => _equality(a.v, item)).i + index;
				if (!untils.Any(x => _equality(_editList[index], x))) {
					_remove(index);
					tgt--;
					if(tgt != index) _move(tgt, index);
				} else {
					//move
					_move(tgt, index);
				}
				//move
				//var tgt = _editList.Skip(index).Select((v, i) => new { v, i }).First(a => _equality(a.v, item)).i + index;
				//_move(tgt, index);
			} else {
				if(_editList.Count <= index || untils.Any(x => _equality(_editList[index], x))) {
					//insert
					_insert(index, _convert(item));
				} else {
					//replace
					_replace(index, _convert(item));
				}
			}
		}
		/// <summary>
		/// Aligns the list according to the specified order.
		/// </summary>
		/// <param name="order">A collection defining the desired order of elements in the list.</param>
		public void AlignBy(IEnumerable<U> order) {
			if (!order.Any() && 1 < _editList.Count) { 
				_clear(); return;
			}
			var orders = new Queue<U>(order);
			var queueCount = orders.Count;
			for(int i = 0; i <= queueCount-1; i++) {
				var item = orders.Dequeue();
				if (i < _editList.Count && _equality(_editList[i], item)) continue; //_comparer.Equals(item, _editList[i])) continue;
				setitem(i, item, orders);
			}
			for (int i = _editList.Count - 1; queueCount <= i ; i--) {
				_remove(i);
			}
		}
	}
}
