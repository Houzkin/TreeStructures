using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using TreeStructures.Collections;
using TreeStructures.Linq;
using TreeStructures.Results;
using System.Linq;
using System.Linq.Expressions;

namespace TreeStructures.Tree {
	//public static class PropertyHelper {
	//	public static Expression<Func<T, object>>[] ToExpression<T>(Expression<Func<T, object>> expression, params Expression<Func<T,object>>[] expressions) {
	//		return expressions.AddHead(expression).ToArray();
	//	}
	//	public static Expression<Func<T, object>>[] ToExpression<T>(this T self, Expression<Func<T, object>> expression, params Expression<Func<T,object>>[] expressions) {
	//		return expressions.AddHead(expression).ToArray();
	//	}
	//}
	public class ReactiveCategoryTree<TItm, TCtg> : CategoryTree<TItm, TCtg>, IDisposable {

		public ReactiveCategoryTree(ExpressionList<TItm> properties, Func<TItm, TCtg> lv1Category, params Func<TItm, TCtg>[] nestsLvCategories)
			: this(properties, EqualityComparer<TCtg>.Default, lv1Category, nestsLvCategories) { }
		public ReactiveCategoryTree(IEnumerable<Expression<Func<TItm, object>>> properties, Func<TItm, TCtg> lv1Category, params Func<TItm, TCtg>[] nestsLvCategories)
			: this(properties, EqualityComparer<TCtg>.Default, lv1Category, nestsLvCategories) { }

		public ReactiveCategoryTree(ExpressionList<TItm> properties, IEqualityComparer<TCtg> equality, Func<TItm, TCtg> lv1Category, params Func<TItm, TCtg>[] nestsLvCategories)
			: this(Array.Empty<TItm>(), properties, equality, lv1Category, nestsLvCategories) { }
		public ReactiveCategoryTree(IEnumerable<Expression<Func<TItm, object>>> properties, IEqualityComparer<TCtg> equality, Func<TItm, TCtg> lv1Category, params Func<TItm, TCtg>[] nestsLvCategories)
			: this(Array.Empty<TItm>(), properties, equality, lv1Category, nestsLvCategories) { }

		public ReactiveCategoryTree(IEnumerable<TItm> items,ExpressionList<TItm> properties,Func<TItm,TCtg> lv1Category,params Func<TItm,TCtg>[] nestsLvCategories)
			: this(items, properties, EqualityComparer<TCtg>.Default,lv1Category,nestsLvCategories) { }
		public ReactiveCategoryTree(IEnumerable<TItm> items, IEnumerable<Expression<Func<TItm, object>>> properties, Func<TItm, TCtg> lv1Category, params Func<TItm, TCtg>[] nestsLvCategories)
			: this(items, properties, EqualityComparer<TCtg>.Default, lv1Category, nestsLvCategories) { }

		public ReactiveCategoryTree(IEnumerable<TItm> items, ExpressionList<TItm> properties, IEqualityComparer<TCtg> equality, Func<TItm, TCtg> lv1Category, params Func<TItm, TCtg>[] nestsLvCategories)
			: this(items, properties.AsEnumerable(), equality, lv1Category, nestsLvCategories) { }
		private ReactiveCategoryTree(IEnumerable<TItm> items, IEnumerable<Expression<Func<TItm,object>>> properties, IEqualityComparer<TCtg> equality, Func<TItm, TCtg> lv1Category, params Func<TItm, TCtg>[] nestsLvCategories)
			: base(equality, lv1Category, nestsLvCategories) {
			_items = new ObservableCollection<TItm>();
			_trackingCollection = new ReadOnlyObservableTrackingCollection<TItm>(_items);
			//var tlst = _trackingCollection.CreateTrackingList();
			//tlst.Register(properties);
			//tlst.AttachHandler(trackingPropertyChanged);

			//_trackingCollection.CreateTrackingList().Register(properties);
			//_trackingCollection.TrackingPropertyChanged += trackingPropertyChanged;

			_trackingCollection.TrackHandler(properties, trackingPropertyChanged);
			this.Add(items);
		}
		ObservableCollection<TItm> _items;
		ReadOnlyObservableTrackingCollection<TItm> _trackingCollection;
		private bool disposedValue;
		void trackingPropertyChanged(TItm sender, ChainedPropertyChangedEventArgs<object> e) {
			var tgtNodes = Root.LevelOrder()
				.SkipWhile(x => x.Depth() <= CategorySelectors.Items.Count)
				.Where(x => x.HasItem && Equality<TItm>.ValueOrReferenceComparer.Equals(x.Item, sender))//同値、可能であれば同一のインスタンスを動かしたい
				.ToDictionary(x => x, x => x.Upstream().Select(y => y.Category).Reverse()); 
			var newPath = CategorySelectors.Items.Select(x=>x(sender)).AddHead(Root.Category);
			if (tgtNodes.Any(x => !x.Value.SequenceEqual(newPath, CategoryEquality))) {
				var tgt = tgtNodes.FirstOrDefault(x => !x.Value.SequenceEqual(newPath, CategoryEquality));
				var tgtP = tgt.Key.Parent;
				base.AddItem(tgt.Key.Item, (x, y) => tgt.Key);
				foreach(var nd in tgtP?.Upstream() ?? Array.Empty<Node>()) {
					if (!nd.Children.Any()) nd.Parent?.Remove(nd);
					else break;
				}
			}
		}
		protected override void AddItem(TItm item, Func<TItm, TCtg, Node>? create = null) {
			_items.Add(item);
			base.AddItem(item, create);
		}
		protected override void RemoveItem(TItm item) {
			_items.Remove(item);
			base.RemoveItem(item);
		}

		/// <summary></summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					// TODO: マネージド状態を破棄します (マネージド オブジェクト)
					_trackingCollection.Dispose();
				}
				// TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
				// TODO: 大きなフィールドを null に設定します
				disposedValue = true;
			}
		}
		/// <inheritdoc/>
		public void Dispose() {
			// このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
	public class CategoryTree<TItm, TCtg> {

		public CategoryTree(Func<TItm, TCtg> lv1Category, params Func<TItm, TCtg>[] nestsLvCategories)
			: this(Array.Empty<TItm>(), lv1Category, nestsLvCategories) { }
		public CategoryTree(IEqualityComparer<TCtg> equality, Func<TItm, TCtg> lv1Category, params Func<TItm, TCtg>[] nestsLvCategories)
			: this(Array.Empty<TItm>(), equality, lv1Category, nestsLvCategories) { }
		public CategoryTree(IEnumerable<TItm> items, Func<TItm, TCtg> lv1Category, params Func<TItm, TCtg>[] nestsLvCategories)
			: this(items, EqualityComparer<TCtg>.Default, lv1Category, nestsLvCategories) { }
		public CategoryTree(IEnumerable<TItm> items, IEqualityComparer<TCtg> equality, Func<TItm, TCtg> lv1Category, params Func<TItm, TCtg>[] nestsLvCategories) {
			CategorySelectors = nestsLvCategories.AddHead(lv1Category).ToListScroller();
			CategoryEquality = equality ?? EqualityComparer<TCtg>.Default;
			this.Add(items);
		}
		Node? _root;
		protected IEqualityComparer<TCtg> CategoryEquality;
		protected ListScroller<Func<TItm, TCtg>> CategorySelectors;
		protected virtual Node CreateRoot() => new Node();
		protected virtual Node CreateNode(TCtg ctg) => new Node(ctg);
		protected virtual Node CreateLeaf(TItm item, TCtg ctg) => new Node(item, ctg);
		public Node Root => _root ??= CreateRoot();
		protected virtual void AddItem(TItm item,Func<TItm,TCtg,Node>? create = null) {
			create ??= new Func<TItm, TCtg, Node>((itm, ctg) => CreateLeaf(itm, ctg));
			var nd = this.Root;
			CategorySelectors.First().DoWhile(x => x.TryNext(), scroller => {
				var cn = scroller.Current(item);
				var n = nd.Children.FirstOrDefault(a=>CategoryEquality.Equals(a.Category,scroller.Current(item)));
				if(n == null) {
					n = CreateNode(scroller.Current(item));
					nd.Add(n);
				}
				if(scroller.IsLast()) n.Add(create(item,scroller.Current(item)));
				nd = n;
			});
		}
		protected virtual void RemoveItem(TItm item) {
			var tgt = this.Root.LevelOrder()
				.SkipWhile(x => x.Depth() <= CategorySelectors.Items.Count)
				.Where(x => x.HasItem)
				.FirstOrDefault(x => EqualityComparer<TItm>.Default.Equals(x.Item, item));//同値であれば、同一でなくてもよい
			if (tgt == null) return;
			var ans = tgt.Ancestors().ToArray();
			Action<Node> remove = cld => cld.Parent?.Remove(cld);
			remove(tgt);
			foreach (var n in ans) {
				if (!n.Children.Any()) {
					remove(n);
				} else {
					break;
				}
			}
		}
		public void Add(TItm item) {
			this.AddItem(item);
		}
		public void Add(IEnumerable<TItm> items) {
			foreach (var item in items) this.AddItem(item);
		}
		public void Remove(TItm item) {
			this.RemoveItem(item);
		}
		public void Remove(IEnumerable<TItm> items) {
			foreach (var item in items) this.RemoveItem(item);
		}

		public class Node : TreeNodeBase<Node> {
			/// <summary>Initialize as root.</summary>
			public Node() {
				this.HasItem = false;
				this.Item = default;
			}
			/// <summary>Initialize as class node.</summary>
			public Node(TCtg nodeClass) {
				this.Category = nodeClass;
				this.HasItem = false;
				this.Item = default;
			}

			/// <summary>Initialize as leaf.</summary>
			public Node(TItm item, TCtg nodeClass) {
				this.Category = nodeClass;
				this.Item = item;
				this.HasItem = true;
			}
			public TCtg Category { get; }
			public bool HasItem { get; }

			public TItm Item { get; }

			ImitableCollection<Node>? _childnodes;
			IComparer<Node> comparer = Comparer<Node>.Create((a, b) => {
				//if (a is IComparable<Node> cmpCN) return cmpCN.CompareTo(b);
				if (a.HasItem && b.HasItem) {
					if (a.Item is IComparable<TItm> cmpVal) return cmpVal.CompareTo(b.Item);
					if (a.Item is IComparable cmpval) return cmpval.CompareTo(b.Item);
				} else {
					if (a.Category is IComparable<TCtg> cmpValcls) return cmpValcls.CompareTo(b.Category);
					if (a.Category is IComparable cmpvalcls) return cmpvalcls.CompareTo(b.Category);
				}
				return 0;
			});
			protected override IEnumerable<Node> SetupInnerChildCollection() {
				return new HashSet<Node>();
			}
			protected override IEnumerable<Node> SetupPublicChildCollection(IEnumerable<Node> innerCollection) {
				//return innerCollection;
				_childnodes = innerCollection.ToImitable(a => a, null, true);
				var lst = _childnodes.ToReadOnlyObservableFilterSort();
				lst.SortBy(x => x, comparer);
				return lst;
			}
			protected override void InsertChildProcess(int index, Node child, Action<IEnumerable<Node>, int, Node>? action = null) {
				action ??= (list, idx, node) => ((ICollection<Node>)list).Add(node);
				base.InsertChildProcess(index, child, action);
				_childnodes?.Imitate();
			}
			internal void Add(Node child) {
				this.InsertChildProcess(0, child);
			}
			protected override void RemoveChildProcess(Node child, Action<IEnumerable<Node>, Node>? action = null) {
				base.RemoveChildProcess(child, action);
				_childnodes?.Imitate();
			}
			internal void Remove(Node child) {
				this.RemoveChildProcess(child);
			}
		}
	}
}
