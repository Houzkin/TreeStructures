using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.EventManagement;
using TreeStructures.Linq;

namespace TreeStructures.Tree {
    /// <summary>
    /// Builds a tree structure of date collections.
    /// </summary>
    public class DateTimeTree:DateTimeTree<DateTime> {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="collection">Collection of <see cref="DateTime"/>.</param>
        /// <param name="Lv1Classes">Classification at level 1.</param>
        /// <param name="nextsLvClasses">Each classifications at levels 2 and above.</param>
        public DateTimeTree(IEnumerable<DateTime> collection, Func<DateTime, int> Lv1Classes, params Func<DateTime, int>[] nextsLvClasses)
            : base(collection, x => x, Lv1Classes, nextsLvClasses) { }
    }
    /// <summary>
    /// Builds a tree structure from a collection of objects holding a date as a property.
    /// </summary>
    /// <typeparam name="T">Type of objects holding a date as a property.</typeparam>
    public class DateTimeTree<T> {
        private InnerNodeBase _root { get; }
        /// <summary>The constructed tree.</summary>
        public Node Root { get; }
        Dictionary<int, Func<DateTime, int>> selectChargeDic = new();
        Func<T, DateTime> SelectDateTime;
        IEnumerable<T> _collection;
        IDisposable? _dispo = null;
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeTree{T}"/> class.
        /// </summary>
        /// <param name="collection">A collection of objects that hold a date as a property.</param>
        /// <param name="dateTimeSelector">Function to extract the date from each object.</param>
        /// <param name="Lv1Classes">Classification at level 1.</param>
        /// <param name="nextsLvClasses">Each classification at levels 2 and above.</param>
        public DateTimeTree(IEnumerable<T> collection,Func<T,DateTime> dateTimeSelector,Func<DateTime,int> Lv1Classes,params Func<DateTime, int>[] nextsLvClasses) {
            _root = new InnerDateRoot();
            var levlst = new List<Func<DateTime, int>> { Lv1Classes };
            levlst.AddRange(nextsLvClasses);
            _collection = collection;
            SelectDateTime = dateTimeSelector; 
            for(int i = 0; i < levlst.Count; i++) {
                selectChargeDic[i+1] = levlst[i];
            }
            foreach(var itm in collection) AddDateTime(itm);
            if(collection is INotifyCollectionChanged notifir) {
                _dispo = new EventListener<NotifyCollectionChangedEventHandler>(
                    h => notifir.CollectionChanged += h, 
                    h => notifir.CollectionChanged -= h, 
                    CollectionChanged);
            }
            Root = new Node(_root);
        }

        private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
            if(e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace) {
                var adds = e.NewItems?.OfType<T>();
                if(adds != null) foreach(var itm in adds) AddDateTime(itm);
            }
            if(e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace) {
                var rmvs = e.OldItems?.OfType<T>();
                if(rmvs != null) foreach(var itm in rmvs) DeleteDateTime(itm);
            }
            if(e.Action == NotifyCollectionChangedAction.Reset) { ReCollect(_collection); }
        }
        /// <summary>
        /// Replaces the collection with a new one.
        /// </summary>
        /// <param name="collection">The new collection to replace the existing one.</param>
        public void Reset(IEnumerable<T> collection) {
            _dispo?.Dispose();
            if (collection is INotifyCollectionChanged notifir) {
                _dispo = new EventListener<NotifyCollectionChangedEventHandler>(
                    h => notifir.CollectionChanged += h,
                    h => notifir.CollectionChanged -= h,
                    CollectionChanged);
            }
            ReCollect(collection);
        }
        void ReCollect(IEnumerable<T> collection) {
            _root.ClearChildren();
            foreach(var itm in collection) AddDateTime(itm);
        }

        internal void AddDateTime(T item) {
            var addlst = selectChargeDic.Select(x => x.Value(SelectDateTime(item))).ToArray();
            var nd = _root;
            for(int i = 0; i < addlst.Length; i++) {
                var ele = nd.Children.FirstOrDefault(x => x.NodeClass == addlst[i]);
                if (ele == null) {
                    ele = new InnerDateNode(addlst[i]);
                    nd.AddChild(ele);
                }
                if (i+1 == addlst.Length) {
                    ele.AddChild(new InnerDateLeaf<T>(addlst[i], SelectDateTime(item),item));
                    break;
                } else {
                    nd = ele;
                }
            }
        }
        internal void DeleteDateTime(T item) {
            var dt = SelectDateTime(item);
            var tgt = _root.Leafs().OfType<InnerDateLeaf<T>>().FirstOrDefault(x => object.Equals(x,item));
            if (tgt != null) {
                var ans = tgt.Ancestors().ToArray();
                tgt.TryRemoveOwn();
                foreach(var nd  in ans) {
                    if (!nd.Leafs().OfType<InnerDateLeaf<T>>().Any()) nd.TryRemoveOwn();
                }
            }
        }
        /// <summary>Base node that forms the tree.</summary>
        public class InnerNodeBase : GeneralTreeNode<InnerNodeBase> {
            /// <inheritdoc/>
            protected override IEnumerable<DateTimeTree<T>.InnerNodeBase> SetupInnerChildCollection()
                => new SortedObableList();
            /// <inheritdoc/>
            protected override IEnumerable<DateTimeTree<T>.InnerNodeBase> SetupPublicChildCollection(IEnumerable<DateTimeTree<T>.InnerNodeBase> innerCollection)
                => new ReadOnlyObservableCollection<DateTimeTree<T>.InnerNodeBase>((innerCollection as ObservableCollection<DateTimeTree<T>.InnerNodeBase>)!);

            internal InnerNodeBase(int chargedValue) {
                NodeClass = chargedValue;
            }
            /// <summary>The number assigned for the class at the corresponding level.</summary>
            public int NodeClass { get; }
            public override string ToString() {
                return NodeClass.ToString();
            }

            private class SortedObableList : ObservableCollection<DateTimeTree<T>.InnerNodeBase> {
                static readonly IComparer<DateTimeTree<T>.InnerNodeBase> comp = Comparer<DateTimeTree<T>.InnerNodeBase>.Create((a, b) => {
                    if (a is InnerDateLeaf<T> leafA && b is InnerDateLeaf<T> leafB) {
                        return leafA.DateTimeValue.CompareTo(leafB.DateTimeValue);
                    } else {
                        return a.NodeClass - b.NodeClass;
                    }
                });
                public SortedObableList() {
                }
                //int FirstIndexOf(DateTimeTree<T>.DateNodeBase other) => this.IndexOf(this.FirstOrDefault(x => comparer.Compare(x, other) >= 0));
                int LastIndexOf(DateTimeTree<T>.InnerNodeBase other) => this.IndexOf(this.LastOrDefault(x=>comp.Compare(x, other) <= 0));
                protected override void InsertItem(int _, DateTimeTree<T>.InnerNodeBase item) {
                    var idx = LastIndexOf(item) + 1;
                    base.InsertItem(idx, item);
                }
            }
        }
        private class InnerDateNode :InnerNodeBase{
            public InnerDateNode(int chargedValue) : base(chargedValue) { }
        }
        private class InnerDateLeaf<TItem> : InnerNodeBase {
            public InnerDateLeaf(int chargedValue,DateTime datetime,TItem item) : base(chargedValue) {
                DateTimeValue = datetime;
                Item = item;
            }
            public DateTime DateTimeValue { get; }
            public TItem Item { get; }
            public override string ToString() {
                return DateTimeValue.ToString();
            }
        }
        private class InnerDateRoot : InnerNodeBase {
            public InnerDateRoot() : base(0) {

            }
        }
        /// <summary>Represents a node with classification or value.</summary>
        public sealed class Node : TreeNodeWrapper<InnerNodeBase, Node> {
            internal Node(DateTimeTree<T>.InnerNodeBase sourceNode) : base(sourceNode) { }
            /// <inheritdoc/>
            protected override DateTimeTree<T>.Node GenerateChild(DateTimeTree<T>.InnerNodeBase sourceChildNode) {
                return new Node(sourceChildNode);
            }
            /// <summary>The number assigned for the class at the corresponding level.</summary>
            public int NodeClass => this.SourceNode.NodeClass;
            public bool HasValue  => (this.SourceNode is InnerDateLeaf<T>);
            /// <summary>This property is not null when <see cref="HasValue"/> is true.</summary>
            public DateTime? DateTimeValue => (this.SourceNode is InnerDateLeaf<T> leaf) ? leaf.DateTimeValue : null;
            /// <summary>
            /// Returns the value of the element if <see cref="HasValue"/> is true; otherwise, returns the default value.
            /// </summary>
            public T Value => (this.SourceNode is InnerDateLeaf<T> leaf) ? leaf.Item : default;
            /// <inheritdoc/>
            public override string ToString() {
                return this.SourceNode.ToString();
            }
        }

    }

}
