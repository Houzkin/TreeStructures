using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using TreeStructures.Collections;
using TreeStructures.Events;
using TreeStructures.Linq;
using TreeStructures.Results;
using TreeStructures.Utilities;

namespace TreeStructures.Tree {
    /// <summary>
    /// Builds a tree structure from a collection of items holding a value as a property.
    /// </summary>
    /// <typeparam name="TItm">Type of items.</typeparam>
    /// <typeparam name="TVal"></typeparam>
    /// <typeparam name="TClass"></typeparam>
    public class PartitionedTree<TItm, TVal, TClass> {
        private InnerNodeBase _innerRoot { get; }
        private Node? _root;
        /// <summary>The constructed tree.</summary>
        public Node Root
            => _root ??= InitializeRoot(_innerRoot);

        ///// <summary>
        ///// </summary>
        //public bool IsValidWhenItemMoved { get; set; } = true;//書き掛け

        Dictionary<int, Func<TVal, TClass>> selectChargeDic = new();
        Func<TItm, TVal> SelectValue;
        IEnumerable<TItm> _collection;
        IDisposable? _dispo = null;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="collection">A collection of objects that hold a date as a property.</param>
        /// <param name="valueSelector">Function to extract the date from each object.</param>
        /// <param name="Lv1Classes">Classification at level 1.</param>
        /// <param name="nextsLvClasses">Each classification at levels 2 and above.</param>
        public PartitionedTree(IEnumerable<TItm> collection, Func<TItm, TVal> valueSelector, Func<TVal, TClass> Lv1Classes, params Func<TVal, TClass>[] nextsLvClasses) {
            _innerRoot = new InnerDateRoot();
            var levlst = new List<Func<TVal, TClass>> { Lv1Classes };
            levlst.AddRange(nextsLvClasses);
            _collection = collection;
            SelectValue = valueSelector;
            for (int i = 0; i < levlst.Count; i++) {
                selectChargeDic[i + 1] = levlst[i];
            }
            foreach (var itm in collection) AddItem(itm);
            if (collection is INotifyCollectionChanged notifir) {
                _dispo = new EventListener<NotifyCollectionChangedEventHandler>(
                    h => notifir.CollectionChanged += h,
                    h => notifir.CollectionChanged -= h,
                    CollectionChanged);
            }
            //Root = new Node(_innerRoot);
        }

        protected virtual Node InitializeRoot(InnerNodeBase innerNode) {
            return new Node(innerNode);
        }
        private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace) {
                var adds = e.NewItems?.OfType<TItm>();
                if (adds != null) foreach (var itm in adds) AddItem(itm);
            }
            if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace) {
                var rmvs = e.OldItems?.OfType<TItm>();
                if (rmvs != null) foreach (var itm in rmvs) DeleteItem(itm);
            }
            if (e.Action == NotifyCollectionChangedAction.Reset) { ReCollect(_collection); }
        }
        /// <summary>
        /// Replaces the collection with a new one.
        /// </summary>
        /// <param name="collection">The new collection to replace the existing one.</param>
        public void Reset(IEnumerable<TItm> collection) {
            _dispo?.Dispose();
            if (collection is INotifyCollectionChanged notifir) {
                _dispo = new EventListener<NotifyCollectionChangedEventHandler>(
                    h => notifir.CollectionChanged += h,
                    h => notifir.CollectionChanged -= h,
                    CollectionChanged);
            }
            ReCollect(collection);
        }
        void ReCollect(IEnumerable<TItm> collection) {
            _innerRoot.ClearChildren();
            foreach (var itm in collection) AddItem(itm);
        }
        public void Reflect(){
            foreach(var ele in this._collection) AdjustItem(ele);
        }

        internal void AddItem(TItm item) {
            AddItemNode(new InnerDateLeaf<TItm>(default, default, item));
        }
        void AddItemNode(InnerDateLeaf<TItm> itemNode){
            var addlst = selectChargeDic.Select(x => x.Value(SelectValue(itemNode.Item))).ToArray();//追加ノードのクラスを取得
            Stack<InnerNodeBase> stack = new Stack<InnerNodeBase>();
            stack.Push(_innerRoot);//巡回用リストにルートをセットする

            for (int i = 0; i < addlst.Length; i++) {//ノードを巡回
                var ele = stack.Peek().Children.FirstOrDefault(x => object.Equals(x.NodeClass, addlst[i]));//該当クラスと一致するノードを取得
                ele ??= new InnerDateNode(addlst[i]);//取得できなかった場合、生成
                stack.Push(ele);//巡回用リストにプッシュ
                if (i + 1 == addlst.Length) {//終端だった場合、リーフを生成して巡回用リストに追加
                    itemNode.NodeClass = addlst[i];
                    itemNode.SelectedValue = SelectValue(itemNode.Item);
                    stack.Push(itemNode);
                    break;
                }
            }
            bool next = true;
            do {
                ResultWith<InnerNodeBase>.Of(stack.TryPop).When(
                    o => {
                        ResultWith<InnerNodeBase>.Of(stack.TryPeek).When(
                            oo => next = oo.TryAddChild(o),
                            ox => next = false);
                    },
                    x => next = false);
            } while (next);
        }
        internal void DeleteItem(TItm item) {
            //var dt = SelectValue(item);//削除する要素から値を取得
            var tgt = _innerRoot.Leafs().OfType<InnerDateLeaf<TItm>>().FirstOrDefault(x => object.Equals(x.Item, item));//該当ノードを取得
            if (tgt != null) {
                var ans = tgt.Ancestors().ToArray();//該当ノードの祖先を取得
                tgt.TryRemoveOwn();//該当ノードを削除
                foreach (var nd in ans) {//祖先ノードで、リーフを持たないノードを削除
                    if (!nd.Leafs().OfType<InnerDateLeaf<TItm>>().Any()) nd.TryRemoveOwn();
                }
            }
        }
        internal void AdjustItem(TItm item) {
            var tgtnode = _innerRoot.Leafs().OfType<InnerDateLeaf<TItm>>().FirstOrDefault(x => object.Equals(x.Item, item));//該当ノードを取得
            if (tgtnode == null) return;
            var ans = tgtnode.Ancestors().ToArray();
            tgtnode.TryRemoveOwn();
            AddItemNode(tgtnode);

            foreach (var nd in ans) {
                if (!nd.Leafs().OfType<InnerDateLeaf<TItm>>().Any()) {
                    nd.TryRemoveOwn();
                } else { break; }
            }
        }
        /// <summary>Base node that forms the tree.</summary>
        public class InnerNodeBase : GeneralTreeNode<InnerNodeBase> {
            /// <inheritdoc/>
            protected override IEnumerable<InnerNodeBase> SetupInnerChildCollection()
                => new ObservableCollection<InnerNodeBase>(); // new SortedObableList();
            /// <inheritdoc/>
            protected override IEnumerable<InnerNodeBase> SetupPublicChildCollection(IEnumerable<InnerNodeBase> innerCollection)
                => new ReadOnlyObservableCollection<InnerNodeBase>((innerCollection as ObservableCollection<InnerNodeBase>)!);

            internal InnerNodeBase(TClass chargedClassValue) {
                NodeClass = chargedClassValue;
            }
            /// <summary>The number assigned for the class at the corresponding level.</summary>
            public TClass NodeClass { get; internal set; }
            /// <inheritdoc/>
            public override string ToString() {
                return NodeClass?.ToString() ?? "";
            }

        }
        private class InnerDateNode : InnerNodeBase {
            public InnerDateNode(TClass chargedValue) : base(chargedValue) { }
        }
        private class InnerDateLeaf<TItem> : InnerNodeBase {
            public InnerDateLeaf(TClass selectedClass, TVal selectedValue, TItem item) : base(selectedClass) {
                SelectedValue = selectedValue;
                Item = item;
            }
            public TVal SelectedValue { get; set; }
            public TItem Item { get; }
            public override string ToString() {
                return SelectedValue.ToString();
            }
        }
        private class InnerDateRoot : InnerNodeBase {
            public InnerDateRoot() : base(default) {

            }
        }
        /// <summary>Represents a node with classification or value.</summary>
        public class Node : BindableTreeNodeWrapper<InnerNodeBase, Node> {
            /// <inheritdoc/>
            protected internal Node(InnerNodeBase sourceNode) : base(sourceNode) { }
            readonly IComparer<Node> comparer = Comparer<Node>.Create((a, b) => {
                if (a.HasItemAndValue && b.HasItemAndValue) {
                    return Comparer<TVal>.Default.Compare(a.Value, b.Value);
                } else {
                    return Comparer<TClass>.Default.Compare(a.NodeClass, b.NodeClass);
                }                            
            });
            /// <inheritdoc/>
			protected override IEnumerable<Node> SetupPublicChildCollection(CombinableChildrenProxyCollection<Node> children) {
                var list = new ReadOnlyObservableFilterSortCollection<Node>(children);
                list.SortBy(x => x, comparer);
                return list;//.AsReadOnlyObservableCollection();
			}
			/// <inheritdoc/>
			protected override Node GenerateChild(InnerNodeBase sourceChildNode) {
                return new Node(sourceChildNode);
            }
            /// <summary>The number assigned for the class at the corresponding level.</summary>
            public TClass NodeClass => this.Source.NodeClass;
            /// <summary>
            /// Return true when this node is leaf.
            /// </summary>
            public bool HasItemAndValue => (this.Source is InnerDateLeaf<TItm>);
            /// <summary>This property is not null when <see cref="HasItemAndValue"/> is true.</summary>
            public TVal? Value => (this.Source is InnerDateLeaf<TItm> leaf) ? leaf.SelectedValue : default;
            /// <summary>
            /// Returns the value of the element if <see cref="HasItemAndValue"/> is true; otherwise, returns the default value.
            /// </summary>
            public TItm Item => (this.Source is InnerDateLeaf<TItm> leaf) ? leaf.Item : default;
            /// <inheritdoc/>
            public override string ToString() {
                return this.Source.ToString();
            }
        }

    }

}
