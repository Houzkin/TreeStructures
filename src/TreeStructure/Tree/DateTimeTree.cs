using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.EventManagement;
using TreeStructures.Linq;

namespace TreeStructures.Tree {
    /// <summary>
    /// 日付のコレクションをツリー構造として構築する
    /// </summary>
    public class DateTimeTree:DateTimeTree<DateTime> {

        /// <summary>新規インスタンスを初期化する</summary>
        /// <param name="collection"><see cref="DateTime"/>型のコレクション</param>
        /// <param name="Lv1Classes">レベル１での分類</param>
        /// <param name="nextsLvClasses">レベル２以降での各分類</param>
        public DateTimeTree(IEnumerable<DateTime> collection, Func<DateTime, int> Lv1Classes, params Func<DateTime, int>[] nextsLvClasses)
            : base(collection, x => x, Lv1Classes, nextsLvClasses) { }
    }
    /// <summary>日付をプロパティとして保持するオブジェクトのコレクションをツリー構造として構築する</summary>
    /// <typeparam name="T">日付をプロパティとして保持するオブジェクト</typeparam>
    public class DateTimeTree<T> {
        private InnerNodeBase _root { get; }
        /// <summary>構築されたツリー</summary>
        public DateTimeNode Root { get; }
        Dictionary<int, Func<DateTime, int>> selectChargeDic = new();
        Func<T, DateTime> SelectDateTime;
        IEnumerable<T> _collection;
        IDisposable? _dispo = null;
        /// <summary>新規インスタンスを初期化する</summary>
        /// <param name="collection">日付をプロパティとして保持するオブジェクトのコレクション</param>
        /// <param name="dateTimeSelector">日付を示す</param>
        /// <param name="Lv1Classes">レベル１での分類</param>
        /// <param name="nextsLvClasses">レベル２以降での各分類</param>
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
            Root = new DateTimeNode(_root);
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
        /// <summary>コレクションを入替える</summary>
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
        /// <summary>ツリーをなすBaseNode</summary>
        public class InnerNodeBase : TreeNodeCollection<InnerNodeBase> {
            protected override IEnumerable<DateTimeTree<T>.InnerNodeBase> SetupInnerChildCollection()
                => new SortedObableList();
            protected override IEnumerable<DateTimeTree<T>.InnerNodeBase> SetupPublicChildCollection(IEnumerable<DateTimeTree<T>.InnerNodeBase> innerCollection)
                => new ReadOnlyObservableCollection<DateTimeTree<T>.InnerNodeBase>((innerCollection as ObservableCollection<DateTimeTree<T>.InnerNodeBase>)!);

            internal InnerNodeBase(int chargedValue) {
                NodeClass = chargedValue;
            }
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
        /// <summary>分類または値を持ったノードを表す</summary>
        public sealed class DateTimeNode : TreeNodeWrapper<InnerNodeBase, DateTimeNode> {
            internal DateTimeNode(DateTimeTree<T>.InnerNodeBase sourceNode) : base(sourceNode) { }

            protected override DateTimeTree<T>.DateTimeNode GenerateChild(DateTimeTree<T>.InnerNodeBase sourceChildNode) {
                return new DateTimeNode(sourceChildNode);
            }
            public int NodeClass => this.SourceNode.NodeClass;
            public bool HasValue  => (this.SourceNode is InnerDateLeaf<T>);
            
            public DateTime? DateTimeValue => (this.SourceNode is InnerDateLeaf<T> leaf) ? leaf.DateTimeValue : null;
            
            public T Value => (this.SourceNode is InnerDateLeaf<T> leaf) ? leaf.Item : default;
        }

    }

}
