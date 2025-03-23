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
	public class ListAligner<T, TList>  where TList : IList<T> {
		TList _editList;
		Action<TList, int, T> _insert;
		Action<TList, int, T> _replace;
		Action<TList, int, int> _move;
		Action<TList, int> _remove;
		Action<TList> _clear;
		IEqualityComparer<T> _comparer;
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
			Action<TList, int>? remove = null, Action<TList, int, int>? move = null, Action<TList>? clear = null, IEqualityComparer<T>? comparer = null) {
			_editList = editList;
			_insert = insert ??= (list, idx, item) => list.Insert(idx, item);
			_replace = replace ??= (list, idx, item) => list[idx] = item;
			_remove = remove ??= (list, idx) => list.RemoveAt(idx);
			_move = move ??= (list, ordIdx, newIdx) => {
				var tmp = list[ordIdx];
				_remove(list, ordIdx);
				_insert(list, newIdx, tmp);
			};
			_clear = clear ??= list => list.Clear();
			_comparer = comparer ?? EqualityComparer<T>.Default;
		}

		void setitem(int index,T item,IEnumerable<T> untils) {
			if (_editList.Skip(index).Contains(item, _comparer)) {
				//move
				var tgt =_editList.Skip(index).Select((v, i) => new { v, i }).First(a => _comparer.Equals(item, a.v)).i + index;
				_move(_editList, tgt, index);
			} else {
				if (_editList.Count <= index || untils.Contains(_editList[index], _comparer)) {
					//insert
					_insert(_editList, index, item);
				} else {
					//replace
					_replace(_editList, index, item);
				}
			}
		}
		/// <summary>
		/// Aligns the list according to the specified sequence.
		/// </summary>
		/// <param name="order">The sequence to mimic.</param>
		public void AlignBy(IEnumerable<T> order) {
			//var orders = order.Select((a, b) => new { Value = a, Key = b }).ToDictionary(a=>a.Key,a=>a.Value);
			if (!order.Any() && 1 < _editList.Count) { 
				_clear(_editList); return;
			}
			var orders = new Queue<T>(order);
			var queueCount = orders.Count;
			for(int i = 0; i <= queueCount-1; i++) {
				var item = orders.Dequeue();
				if (i < _editList.Count && _comparer.Equals(item, _editList[i])) continue;
				setitem(i, item, orders);
			}
			for (int i = _editList.Count - 1; queueCount <= i ; i--) {
				_remove(_editList, i);
			}
		}
	}
}
