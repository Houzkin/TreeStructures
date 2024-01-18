using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures.Collections {

	/// <summary>
	/// Supports aligning the list.
	/// </summary>
	/// <typeparam name="T">The type of elements.</typeparam>
	/// <typeparam name="TList">The type of the list.</typeparam>
	public class ListAligner<T,TList> : ISequenceScroller<T, ListAligner<T,TList>> where TList : IList<T>{
		TList lst;
		Action<TList, int, T> _insert;
		Action<TList, int, T> _replace;
		Action<TList, int, int> _move;
		Action<TList, int> _remove;
		Action<TList> _clear;
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		/// <param name="editList">The collection to operate on.</param>
		/// <param name="insert">The insertion operation.</param>
		/// <param name="replace">The replacement operation.</param>
		/// <param name="remove">The removal operation.</param>
		/// <param name="move">The movement operation.</param>
		/// <param name="clear">The clearing operation.</param>
		public ListAligner(TList editList,
			Action<TList, int, T>? insert = null, Action<TList, int, T>? replace = null,
			Action<TList, int>? remove = null, Action<TList, int, int>? move = null, Action<TList>? clear = null) {
			lst = editList;
			if (lst.Any()) curIdx = 0;
			_insert = insert ??= (list, idx, item) => list.Insert(idx, item);
			_replace = replace ??= (list, idx, item) => list[idx] = item;
			_remove = remove ??= (list, idx) => list.RemoveAt(idx);
			_move = move ??= (list, ordIdx, newIdx) => {
				var tmp = list[ordIdx];
				_remove(list, ordIdx);
				_insert(list, newIdx, tmp);
			};
			_clear = clear ??= list => list.Clear();
		}
		/// <inheritdoc/>
		public T Current => lst[curIdx];

		int curIdx = -1;
		/// <inheritdoc/>
		public int CurrentIndex => curIdx;

		/// <inheritdoc/>
		public bool CanMove(int moveCount) {
			var cnt = CurrentIndex + moveCount;
			if (cnt < 0 || lst.Count <= cnt) return false;
			return true;
        }

		/// <inheritdoc/>
		public IEnumerable<T> GetSequence() {
			return lst.ToImmutableList();
		}

		/// <inheritdoc/>
		ListAligner<T, TList> ISequenceScroller<T, ListAligner<T, TList>>.Move(int moveCount) {
			var cnt=CurrentIndex + moveCount;
			if (cnt < 0 || lst.Count <= cnt) throw new ArgumentException();
			curIdx = cnt;
			return this;
		}

		/// <inheritdoc/>
		ListAligner<T, TList> ISequenceScroller<T, ListAligner<T, TList>>.MoveTo(T element) {
			var idx = lst.IndexOf(element);
			if(idx < 0) throw new ArgumentException();
			curIdx = idx;
			return this;
		}
		/// <summary></summary>
		public ListAligner<T,TList> ResetIndex(){
			if (lst.Any()) curIdx = 0;
			else curIdx = -1;
			return this;
		}
		/// <summary>
		/// Aligns the list according to the specified sequence.
		/// </summary>
		/// <param name="org">The sequence to mimic.</param>
		/// <param name="equality"></param>
		public void AlignBy(IEnumerable<T> org,IEqualityComparer<T>? equality = null){
			int orgCount = org.Count();
			this.ResetIndex();
			if (CurrentIndex == -1) {
				for (int i = 0; i < orgCount; i++) { _insert(lst, i, org.ElementAt(i)); }
				return;
			}
			if(!org.Any()){
				_clear(lst); return;
			}
			equality ??= EqualityComparer<T>.Default;

			for(int i = 0; i < orgCount; i++) {
				this.TryMoveTo(i).When(
				o => {
					if (!equality.Equals(this.Current, org.ElementAt(i))) {
						if (this.TryNext(x => equality.Equals(x, org.ElementAt(i)))) {
							_move(lst, this.CurrentIndex, i);
						} else if (org.Skip(i).Any(x => equality.Equals(x, this.Current))) {
							_insert(lst, i, org.ElementAt(i));
						} else {
							_replace(lst, i, org.ElementAt(i));
						}
					}
				},
				x => _insert(lst, i, org.ElementAt(i)));
			}
			for(int i = lst.Count - 1; orgCount <= i; i--) { _remove(lst, i); }
        }
	}

}
