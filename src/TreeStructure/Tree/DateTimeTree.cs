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
    
    public class DateTimeTree:DateTimeTree<DateTime> {
        public DateTimeTree(IEnumerable<DateTime> collection,Func<DateTime,int> level1, params Func<DateTime, int>[] levels) :base(collection,x=>x,level1,levels) {

        }
    }
    public class DateTimeTree<T> {
        public DateNodeBase Root { get; }
        Dictionary<int, Func<DateTime, int>> selectChargeDic = new();
        Func<T, DateTime> SelectDateTime;
        IEnumerable<T> _collection;
        IDisposable? _dispo = null;
        public DateTimeTree(IEnumerable<T> collection,Func<T,DateTime> selectDateTime,Func<DateTime,int> level1,params Func<DateTime, int>[] levels) {
            Root = new DateRoot();
            var levlst = new List<Func<DateTime, int>> { level1 };
            levlst.AddRange(levels);
            _collection = collection;
            SelectDateTime = selectDateTime; 
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
            Root.ClearChildren();
            foreach(var itm in collection) AddDateTime(itm);
        }

        internal void AddDateTime(T item) {
            var addlst = selectChargeDic.Select(x => x.Value(SelectDateTime(item))).ToArray();
            var nd = Root;
            for(int i = 0; i < addlst.Length; i++) {
                var ele = nd.Children.FirstOrDefault(x => x.ChargedValue == addlst[i]);
                if (ele == null) {
                    ele = new DateNode(addlst[i]);
                    nd.AddChild(ele);
                }
                if (i+1 == addlst.Length) {
                    ele.AddChild(new DateLeaf<T>(addlst[i], SelectDateTime(item),item));
                    break;
                } else {
                    nd = ele;
                }
            }
        }
        internal void DeleteDateTime(T item) {
            var dt = SelectDateTime(item);
            var tgt = Root.Leafs().OfType<DateLeaf<T>>().FirstOrDefault(x => object.Equals(x,item));
            if (tgt != null) {
                var ans = tgt.Ancestors().ToArray();
                tgt.TryRemoveOwn();
                foreach(var nd  in ans) {
                    if (!nd.Leafs().OfType<DateLeaf<T>>().Any()) nd.TryRemoveOwn();
                }
            }
        }
        
        public class DateNodeBase : TreeNodeCollection<DateNodeBase> {
            protected override IEnumerable<DateTimeTree<T>.DateNodeBase> SetupInnerChildCollection()
                => new SortedObableList();
            protected override IEnumerable<DateTimeTree<T>.DateNodeBase> SetupPublicChildCollection(IEnumerable<DateTimeTree<T>.DateNodeBase> innerCollection)
                => new ReadOnlyObservableCollection<DateTimeTree<T>.DateNodeBase>((innerCollection as ObservableCollection<DateTimeTree<T>.DateNodeBase>)!);

            public DateNodeBase(int chargedValue) {
                ChargedValue = chargedValue;
            }
            public int ChargedValue { get; }
            public override string ToString() {
                return ChargedValue.ToString();
            }

            private class SortedObableList : ObservableCollection<DateTimeTree<T>.DateNodeBase> {
                static readonly IComparer<DateTimeTree<T>.DateNodeBase> comp = Comparer<DateTimeTree<T>.DateNodeBase>.Create((a, b) => {
                    if (a is DateLeaf<T> leafA && b is DateLeaf<T> leafB) {
                        return leafA.DateTimeValue.CompareTo(leafB.DateTimeValue);
                    } else {
                        return a.ChargedValue - b.ChargedValue;
                    }
                });
                public SortedObableList() {
                }
                //int FirstIndexOf(DateTimeTree<T>.DateNodeBase other) => this.IndexOf(this.FirstOrDefault(x => comparer.Compare(x, other) >= 0));
                int LastIndexOf(DateTimeTree<T>.DateNodeBase other) => this.IndexOf(this.LastOrDefault(x=>comp.Compare(x, other) <= 0));
                protected override void InsertItem(int _, DateTimeTree<T>.DateNodeBase item) {
                    var idx = LastIndexOf(item) + 1;
                    base.InsertItem(idx, item);
                }
            }
        }
        public class DateNode :DateNodeBase{
            public DateNode(int chargedValue) : base(chargedValue) { }
        }
        public class DateLeaf<TItem> : DateNodeBase {
            public DateLeaf(int chargedValue,DateTime datetime,TItem item) : base(chargedValue) {
                DateTimeValue = datetime;
                Item = item;
            }
            public DateTime DateTimeValue { get; }
            public TItem Item { get; }
            public override string ToString() {
                return DateTimeValue.ToString();
            }
        }
        public class DateRoot : DateNodeBase {
            public DateRoot() : base(0) {

            }
        }
    }

}
