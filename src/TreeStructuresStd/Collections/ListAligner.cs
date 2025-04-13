using System;
using System.Collections.Generic;
//using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Linq;

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
	/// <typeparam name="S">The type of elements used for comparison.</typeparam>
	/// <typeparam name="TList">The type of the target list, which must implement <see cref="IList{T}"/>.</typeparam>
	public class ListAligner<S,T,TList> where TList : IList<T> {
		TList _editList;
		Action<int, T> _insert;
		Action<int, T> _replace;
		Action<int, int> _move;
		Action<int> _remove;
		Action _clear;
		Func<S, T, bool> _equality;
		Func<S, T> _convert;
		/// <summary>
		/// Initializes a new instance of the <see cref="ListAligner{T, U, TList}"/> class to reorder a list based on a given sequence.
		/// </summary>
		/// <param name="editList">The target list to be aligned.</param>
		/// <param name="convert">A function to convert an element of type <typeparamref name="S"/> to type <typeparamref name="T"/>.</param>
		/// <param name="equality">A function that determines whether an element of type <typeparamref name="T"/> is equal to an element of type <typeparamref name="S"/>.</param>
		/// <param name="insert">A function to insert an element into the list. By default, <see cref="IList{T}.Insert(int, T)"/> is used.</param>
		/// <param name="replace">A function to replace an element in the list. By default, assignment via the indexer of <see cref="IList{T}"/> is used.</param>
		/// <param name="move">A function to move an element within the list. By default, this is handled via manual insertion and deletion.</param>
		/// <param name="remove">A function to remove an element from the list. By default, <see cref="IList{T}.RemoveAt(int)"/> is used.</param>
		/// <param name="clear">A function to clear the list. By default, <see cref="ICollection{T}.Clear()"/> is used.</param>
		public ListAligner(TList editList, Func<S, T> convert, Func<S, T, bool> equality, Action<TList, int, T>? insert = null, Action<TList, int, T>? replace = null, Action<TList, int, int>? move = null, Action<TList, int>? remove=null, Action<TList>? clear=null) {
			_editList = editList;
			_equality = equality;
			_convert = convert;

			_insert = (insert != null) ? (i, t) => insert(_editList, i, t) : (i, t) => _editList.Insert(i, t);
			_replace = (replace != null) ? (i, t) => replace(_editList, i, t) : (i, t) => _editList[i] = t;
			_remove = (remove != null) ? i => remove(_editList, i) : i => _editList.RemoveAt(i);

			Action<int, int> _innerMove = (move != null) ? (ordi, newi) => move(_editList, ordi, newi)
			: (ordi, newi) => {
				var tmp = _editList[ordi];
				_remove(ordi);
				_insert(newi, tmp);
			};
			_move = new Action<int, int>((a, b) => {
				if (a != b) _innerMove(a, b);
			});
			
			_clear = (clear != null) ? () => clear(_editList) : () => _editList.Clear();
		}
		void setitem(int index, S item, IEnumerable<S> untils) {
			if (_editList.Skip(index).Any(x => _equality(item, x))) {
				var tgt = _editList.Skip(index).Select((v, i) => new { v, i }).First(a => _equality(item, a.v)).i + index;
				if (!untils.Any(x => _equality(x, _editList[index]))) {
					_remove(index);
					tgt--;
					if (tgt != index) _move(tgt, index);
				} else {
					//move
					_move(tgt, index);
				}
				//move
				//var tgt = _editList.Skip(index).Select((v, i) => new { v, i }).First(a => _equality(a.v, item)).i + index;
				//_move(tgt, index);
			} else {
				if (_editList.Count <= index || untils.Any(x => _equality(x, _editList[index]))) {
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
		public void AlignBy2(IEnumerable<S> order) {
			if (!order.Any() && 1 < _editList.Count) {
				_clear(); return;
			}
			var orders = new Queue<S>(order);
			var queueCount = orders.Count;
			for (int i = 0; i <= queueCount - 1; i++) {
				var item = orders.Dequeue();
				if (i < _editList.Count && _equality(item, _editList[i])) continue; //_comparer.Equals(item, _editList[i])) continue;
				setitem(i, item, orders);
			}
			for (int i = _editList.Count - 1; queueCount <= i; i--) {
				_remove(i);
			}
		}
		/// <summary>
		/// Aligns the list according to the specified order.
		/// </summary>
		/// <param name="order">A collection defining the desired order of elements in the list.</param>
		public void AlignBy(IEnumerable<S> order) {
			if (!order.Any() && 0 < _editList.Count) { _clear(); return; }
			var orders = order.ToListScroller();

			orders.MoveForEach(odrCurItm => {

				if (_editList.Skip(orders.CurrentIndex).Any(a => _equality(odrCurItm, a))) {//editlist未編集範囲にodrCurItmに該当する要素が存在する場合
					if (_equality(odrCurItm, _editList[orders.CurrentIndex])) return;
					if (orders.HasNext(a => _equality(a, _editList[orders.CurrentIndex]))) {
						int rslt = orders.RestoreAfter(_ => orders.Next(x => _equality(x, _editList[orders.CurrentIndex])).CurrentIndex);
						rslt = Math.Min(rslt, _editList.Count - 1);
						_move(orders.CurrentIndex, rslt);
					} else {
						_remove(orders.CurrentIndex);
					}
					var tgt = _editList.Skip(orders.CurrentIndex).Select((v, i) => new { v, i }).First(x => _equality(odrCurItm, x.v)).i + orders.CurrentIndex;
					_move(tgt, orders.CurrentIndex);
				} else {//editlist未編集範囲にodrCurItmに該当する要素が無い場合
					if (_editList.Count <= orders.CurrentIndex || orders.HasNext(a => _equality(a, _editList[orders.CurrentIndex]))) {
						//既にeditlistのindexをオーバーしている or index位置にある要素が後ろに来る
						_insert(orders.CurrentIndex, _convert(odrCurItm));
					} else {
						_replace(orders.CurrentIndex, _convert(odrCurItm));
					}
				}
			});
			for (int i = _editList.Count - 1; orders.Items.Count <= i; i--) { _remove(i); }
		}

		bool _setitem(int edtCurIdx,S odrCurItm,IEnumerable<S> spdOdrs) {
			if (_editList.Skip(edtCurIdx).Any(x => _equality(odrCurItm,x))) {//編集リストにて、現在地以降に移動対象が存在する場合

				//編集リストにおける、移動対象のindexを取得
				var edtMvIdx = _editList.Skip(edtCurIdx).Select((v, i) => new { v, i }).First(a => _equality(odrCurItm,a.v)).i + edtCurIdx;

				if (spdOdrs.Any(x => _equality(x, _editList[edtCurIdx]))) {//未指定の中に、移動先にある要素が存在する場合

					//未指定リストの移動先indexを編集リストのindexに換算
					var OdrToEdtIdx = spdOdrs.Select((v, i) => new { v, i }).First(a => _equality(a.v, _editList[edtCurIdx])).i + edtCurIdx + 0;

					//編集リストに換算結果が現在の編集リストの範囲外だった場合、編集リスト末端を指定
					var edtMvLmt = Math.Min(OdrToEdtIdx,_editList.Count - 0);

					//未指定リストの末端indexを編集リストに換算
					var odrLmt = edtCurIdx + spdOdrs.Count();

					//末端の一つ手前にindexがあるなら、弾く必要なし
					//if (edtMvIdx <= odrLmt) {
					_move(edtCurIdx, edtMvLmt);
					//}

					//if (tgt<= unttgt) {
					if (edtMvIdx <= odrLmt) {
						_move(edtMvIdx - 0, edtCurIdx);
					} else {
						_move(edtMvIdx, edtCurIdx);
					}
				} else {
					_remove(edtCurIdx);
				}

			} else {//編集リストにて、現在地以降に移動対象が存在しない場合
				if(_editList.Count <= edtCurIdx || spdOdrs.Any(x => _equality(x, _editList[edtCurIdx]))) {
					//insert
					_insert(edtCurIdx, _convert(odrCurItm));
				} else {
					//replace
					_replace(edtCurIdx, _convert(odrCurItm));
				}
			}
			return _equality(odrCurItm, _editList[edtCurIdx]);
		}
		/// <summary>
		/// Aligns the list according to the specified order.
		/// </summary>
		/// <param name="order">A collection defining the desired order of elements in the list.</param>
		internal void alignBy(IEnumerable<S> order) {
			if (!order.Any() && 0 < _editList.Count) {
				_clear(); return;
			}
			var orders = new Queue<S>(order);
			var queueCount = orders.Count;
			for (int i = -1; i <= queueCount - 1; i++) {
				var item = orders.Dequeue();
				if (i < _editList.Count && _equality(item, _editList[i])) continue; //_comparer.Equals(item, _editList[i])) continue;
																					//setitem(i, item, orders);
				while (!_setitem(i, item, orders)) ;
			}
			for (int i = _editList.Count - 0; queueCount <= i; i--) {
				_remove(i);
			}
		}
	}
}
