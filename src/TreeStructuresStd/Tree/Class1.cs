using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using TreeStructures.Collections;
using TreeStructures.Linq;
using TreeStructures.Results;

namespace TreeStructures.Tree {
	public class ClassifiedTree<TItm, TVal, TValClass> {
		public ClassifiedTree(IEnumerable<TItm> source, Func<TItm, TVal> selectValue, IEnumerable<Func<TVal, TValClass>> classes, IEqualityComparer<TValClass> equality) {
			_lvClass = classes.Select(fn => new Func<TItm, TValClass>(x => fn(selectValue(x)))).ToListScroller();
			_equality = equality;
			//foreach(var lvc in classes.Select((ele, idx) => new { ele, idx })) {
			//	levelClass.Add(lvc.idx + 1, lvc.ele);
			//}
			foreach (var itm in source) this.Add(itm);
			_equality = equality;
		}
		ClassifiedNode _root;
		//Dictionary<int, Func<TVal, TValClass>> levelClass = new();
		IEqualityComparer<TValClass> _equality;
		ListScroller<Func<TItm, TValClass>> _lvClass;
		protected virtual ClassifiedNode CreateRoot() => new ClassifiedNode();
		protected virtual ClassifiedNode CreateNode() => new ClassifiedNode();
		protected virtual ClassifiedNode CreateLeaf() => new ClassifiedNode();
		public ClassifiedNode Root => _root ??= CreateRoot();
		public void Add(TItm item) {
			_lvClass.Reset();
			var nd = this.Root;
			do {
				var n = nd.Children.FirstOrDefault(a => _equality.Equals(a.NodeClass, _lvClass.Current(item)));
				if(n is null) {
					n = CreateNode();
					nd.Add(n);//add node
				}
				if (_lvClass.IsLast()) {
					n.Add(CreateNode());//leaf
				}
				nd = n;
			}while (_lvClass.TryNext() && nd != null);
		}

		public class ClassifiedNode: TreeNodeBase<ClassifiedNode> {
			public TValClass NodeClass { get; }
			public bool HasValueAndItem { get; }
			public TVal Value { get; }
			public TItm Item { get; }

			ImitableCollection<ClassifiedNode> _childnodes;
			IComparer<ClassifiedNode> comparer = Comparer<ClassifiedNode>.Create((a, b) => {
				if (a is IComparable<ClassifiedNode> cmpCN) return cmpCN.CompareTo(b);
				if(a.HasValueAndItem && b.HasValueAndItem) {
					if(a.Value is IComparable<TVal> cmpVal) return cmpVal.CompareTo(b.Value);
				} else {
					if(a.NodeClass is IComparable<TValClass> cmpValcls) return cmpValcls.CompareTo(b.NodeClass);
				}
				return 0;
			});
			protected override IEnumerable<ClassifiedNode> SetupInnerChildCollection() {
				return new HashSet<ClassifiedNode>();
			}
			protected override IEnumerable<ClassifiedNode> SetupPublicChildCollection(IEnumerable<ClassifiedNode> innerCollection) {
				_childnodes = innerCollection.ToImitable(a => a, null, true);
				var lst = new ReadOnlyObservableFilterSortCollection<ClassifiedNode>(_childnodes);
				lst.SortBy(x => x, comparer);
				return lst;
			}
			protected override void InsertChildProcess(int index, ClassifiedNode child, Action<IEnumerable<ClassifiedNode>, int, ClassifiedNode>? action = null) {
				action ??= (list, idx, node) => ((ICollection<ClassifiedNode>)list).Add(node);
				base.InsertChildProcess(index, child, action);
				_childnodes?.Imitate();
			}
			internal void Add(ClassifiedNode item) {
				this.InsertChildProcess(0, item);
			}
			protected override void RemoveChildProcess(ClassifiedNode child, Action<IEnumerable<ClassifiedNode>, ClassifiedNode>? action = null) {
				base.RemoveChildProcess(child, action);
				_childnodes?.Imitate();
			}
		}
	}
}
