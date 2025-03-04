using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using TreeStructures.Internals;
using TreeStructures.Tree;

namespace TreeStructures.Collections {
	public class ReadOnlyObservableItemCollection<T> : ReadOnlyObservableCollection<T>, IDisposable {

		HashSet<ExpressionListenerPair> _ExpLstnrPairs = new();
		ImitableCollection<T, ObservedPropertyTree<T>> _trees;
		IEnumerable<T> _originList;
		ListAligner<T, ObservableCollection<T>> _listAligner;

		public ReadOnlyObservableItemCollection(IEnumerable<T> list) : base(new ObservableCollection<T>()) {
			_originList = list;
			if (_originList is INotifyCollectionChanged notify) notify.CollectionChanged += CollectionChanged;

			var dic = new Dictionary<List<IDisposable>, Expression<Func<T,object>>>();

			_trees = ImitableCollection.Create(_originList,
				x => {
					var opt = new ObservedPropertyTree<T>(x);
					foreach(var inits in _ExpLstnrPairs){
						foreach(var prop in inits.FuncExpressions){
							var subsc = opt.Subscribe(prop, ItemPropertyChanged);
							inits.AddTrigger(subsc);
							inits.AddInitTrigger(subsc);
						}
					}
					return opt;
				}, x => {
					foreach(var inits in _ExpLstnrPairs){
						inits.RemoveTrigger();
					}
					x.Dispose();
				});
		}
		protected virtual ListAligner<T, ObservableCollection<T>> ListAligner 
			=> _listAligner ??= new ListAligner<T, ObservableCollection<T>>((this.Items as ObservableCollection<T>)!, move:(list,ord,to)=> { list.Move(ord, to); });
		protected virtual void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e){

		}
		protected virtual void ItemPropertyChanged<TVal>(object? sender,ChainedPropertyChangedEventArgs<TVal> e){

		}
		protected ExpressionListenerPair AcquireNewExpressionListenerPair(){
			var etp = new ExpressionListenerPair();
			this._ExpLstnrPairs.Add(etp);
			return etp;
		}

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
