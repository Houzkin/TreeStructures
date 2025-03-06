using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using TreeStructures.Internals;
using TreeStructures.Tree;
using TreeStructures.Utility;

namespace TreeStructures.Collections {
	public class ReadOnlyObservableItemCollection<T> : ReadOnlyObservableCollection<T>, IDisposable {

		HashSet<ExpressionListenerPair> _ExpLstnrPairs = new();
		ImitableCollection<ObservedPropertyTree<T>> _trees;
		IEnumerable<T> _originList;
		ListAligner<T, ObservableCollection<T>> _listAligner;

		List<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>,IDisposable>>> dim3 = new();


		public ReadOnlyObservableItemCollection(IEnumerable<T> list) : base(new ObservableCollection<T>()) {
			_originList = list;
			if (_originList is INotifyCollectionChanged notify) notify.CollectionChanged += CollectionChanged;

			var dic = new Dictionary<List<IDisposable>, Expression<Func<T, object>>>();

			_trees = ImitableCollection.Create(_originList,
				x => {
					var opt = new ObservedPropertyTree<T>(x);
					//foreach(var inits in _ExpLstnrPairs){
					//	foreach(var prop in inits.FuncExpressions){
					//		var subsc = opt.Subscribe(prop, ItemPropertyChanged);
					//		inits.AddTrigger(subsc);
					//		inits.AddInitTrigger(subsc);
					//	}
					//}
					foreach (var d3 in dim3) {
						foreach (var d2 in d3.Keys) {
							var sbsc = opt.Subscribe(d2, ItemPropertyChanged);
							//d3[d2].Add(new TreeListenerPair(opt,sbsc));
							d3[d2][opt] = sbsc;
						}
					}
					return opt;
				}, x => {
					//foreach (var inits in _ExpLstnrPairs) {
					//	inits.RemoveTrigger();
					//}
					//x.Dispose();

					foreach (var d3 in dim3) {
						foreach (var d2 in d3.Keys) {
							ResultWith<IDisposable>.Of(d3[d2].Remove,x).When(o => o.Dispose());
						}
					}

					//foreach (var lst in x.Listener) lst.Dispose();
				});

			//dim3 = new List<Dictionary<Expression<Func<T, object>>, List<IDisposable>>>();
			//var ddim = new List<List<Tuple<Expression<Func<T, object>>, List<IDisposable>>>>();
		}
		void addExpressions(Dictionary<Expression<Func<T,object>>,Dictionary<ObservedPropertyTree<T>,IDisposable>> area,IEnumerable<Expression<Func<T,object>>> exps) {
			foreach(var key in exps) {
				if (area.ContainsKey(key)) continue;
				foreach(var tr in _trees) {
					area[key].Add(tr, tr.Subscribe(key, this.ItemPropertyChanged));
				}
			}
		}
		void removeExpressions(Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>> area,IEnumerable<Expression<Func<T,object>>> exps) {
			foreach (var key in exps) {
				ResultWith<Dictionary<ObservedPropertyTree<T>, IDisposable>>.Of(area.Remove, key).When(
					o => {
						foreach (var t in o.Values) t.Dispose();
					});
			}
		}
		void clearExpressions(Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>> area) {
			foreach (var disp in area.Values.SelectMany(x=>x.Values)) { disp.Dispose(); }
		}
		void dispExpressions(Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>> area) {
			if (dim3.Remove(area)) {
				clearExpressions(area);
			}

		}
		protected ExpressionsCollection AcquireNewExpressionList(IEnumerable<Expression<Func<T,object>>> exps){
			var dic = new Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>();
			return new ExpressionsCollection(dic, addExpressions, removeExpressions, clearExpressions, dispExpressions);
				
		}

		public class ExpressionsCollection : IEnumerable<Expression<Func<T,object>>>, IDisposable {
			//List<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>> _dim3;
			Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>> _area;
			//IReadOnlyList<ObservedPropertyTree<T>> _tree;
			//internal ExpressionsCollection(IReadOnlyList<ObservedPropertyTree<T>> tree, List<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>,IDisposable>>> dim3,
			//	Dictionary<Expression<Func<T,object>>,Dictionary<ObservedPropertyTree<T>,IDisposable>> area) {
			//	_tree = tree;
			//	_dim3 = dim3;
			//	_area = area;
			//}
			Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>,IEnumerable<Expression<Func<T, object>>>> _addAction;
			Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>,IEnumerable<Expression<Func<T, object>>>> _removeAction;
			Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>> _clearAction;
			Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>> _dispAction;
			internal ExpressionsCollection(Dictionary<Expression<Func<T,object>>,Dictionary<ObservedPropertyTree<T>,IDisposable>> area,
				Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>, IEnumerable<Expression<Func<T, object>>>> addAction,
				Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>, IEnumerable<Expression<Func<T, object>>>> removeAction,
				Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>> clearAction,
				Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>>  dispAction) {
				_area = area;
				_addAction = addAction;
				_removeAction = removeAction;
				_clearAction = clearAction;
				_dispAction = dispAction;
			}
			public void Add(IEnumerable<Expression<Func<T, object>>> expressions) {
				//foreach (var key in expressions) {
				//	if (_area.ContainsKey(key)) continue;
				//	foreach(var tr in _tree) {
				//		//_area[key].Add(tr,)
				//	}
				//}
				_addAction(_area,expressions);
			}
			public void Remove(IEnumerable<Expression<Func<T, object>>> expressions) { _removeAction(_area, expressions); }
			public void Clear() { _clearAction(_area); }
			/// <inheritdoc/>
			public void Dispose() {
				_dispAction(_area);
			}
			/// <inheritdoc/>
			public IEnumerator<Expression<Func<T, object>>> GetEnumerator() { throw new NotImplementedException(); }
			IEnumerator IEnumerable.GetEnumerator() { throw new NotImplementedException(); }
		}


		protected virtual ListAligner<T, ObservableCollection<T>> ListAligner 
			=> _listAligner ??= new ListAligner<T, ObservableCollection<T>>((this.Items as ObservableCollection<T>)!, move:(list,ord,to)=> { list.Move(ord, to); });
		protected virtual void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e){

		}
		protected virtual void ItemPropertyChanged<TVal>(object? sender,ChainedPropertyChangedEventArgs<TVal> e){

		}
		//protected ExpressionListenerPair AcquireNewExpressionListenerPair(){
		//	var etp = new ExpressionListenerPair();
		//	this._ExpLstnrPairs.Add(etp);
		//	return etp;
		//}

		bool isDisposed = false;
		public void Dispose() {
			if (isDisposed) { return; }
			if(_originList is INotifyCollectionChanged notify) notify.CollectionChanged -= CollectionChanged;
			foreach(var triggers in _ExpLstnrPairs.Select(x=>x.NotifyTriggers)){
				triggers.Dispose();
			}
			_trees.Dispose();
			isDisposed = true;
		}

		internal class TreeListenerPair {
			public TreeListenerPair(ObservedPropertyTree<T> tree, IDisposable listener) {
				this.Tree = tree;
				this.Listener = listener;
			}
			public ObservedPropertyTree<T> Tree { get; }
			public IDisposable Listener { get; }
		}

		protected class ExpressionListenerPair {
			LumpedDisopsables _trgs = new LumpedDisopsables();
			List<Expression<Func<T, object>>> _exp = new();
			public IDisposable NotifyTriggers => _trgs;
			public IEnumerable<Expression<Func<T, object>>> FuncExpressions => _exp;
			internal List<IDisposable> InitTriggers { get; } = new List<IDisposable>();
			public void AddExpressions(IEnumerable<Expression<Func<T,object>>> expressions) { _exp.AddRange(expressions); }
			//public void ClearExpressions() { _exp.Clear(); }
			//public void DisposeTrigger() { _trgs.Dispose(); }
			public void AddTrigger(IDisposable trigger) { }
			public void RemoveTrigger() { _trgs.Remove(InitTriggers); }
			public void Reset(IEnumerable<Expression<Func<T,object>>> expressions,IEnumerable<IDisposable> listener){
				_exp.Clear();
				_trgs.Dispose();
				_exp.AddRange(expressions);
				foreach(var lst in listener)_trgs.Add(lst);
			}
			internal void AddInitTrigger(IDisposable trigger) { }
		}
	}
}
