using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TreeStructure.EventManager;
using TreeStructure.Linq;

namespace TreeStructure {
    /// <summary>観測可能な多分木構造をなすノードを表す</summary>
    /// <typeparam name="TNode">各ノードの共通基底クラスとなる型</typeparam>
    [Serializable]
    public class ObservableTreeNodeCollection<TNode> : TreeNodeCollection<TNode>, IObservableTreeNode<TNode>, INotifyPropertyChanged
        where TNode : ObservableTreeNodeCollection<TNode> {
        public ObservableTreeNodeCollection() { this.PropertyChangeProxy = new PropertyChangeProxy(this); }
        public ObservableTreeNodeCollection(IEnumerable<TNode> collection) : this() {
            foreach (var item in collection) { this.AddChild(item); }
        }
        ReadOnlyObservableCollection<TNode>? _readonlyobservablecollection;
        public override ReadOnlyObservableCollection<TNode> Children => _readonlyobservablecollection ??= new ReadOnlyObservableCollection<TNode>(ChildNodes);
        protected override ObservableCollection<TNode> ChildNodes { get; } = new ObservableCollection<TNode>();

        /// <summary><inheritdoc/></summary>
        protected override Action<int, int, IEnumerable<TNode>> MoveAction => (oldIdx, newIdx, collection) =>
            ((ObservableCollection<TNode>)collection).Move(oldIdx, newIdx);


        protected IDisposable ShiftParentChangedNotification() {
            return UniqueExcutor.LateEvalute(parentchangedeventkey, () => Parent);
        }
        readonly string parentchangedeventkey = "in Library : " + nameof(ObservableTreeNodeCollection<TNode>)+ "." + nameof(Parent);
        readonly string disposedeventkey = "in Library : " + nameof(ObservableTreeNodeCollection<TNode>) + "." + nameof(Disposed);

        StructureChangedEventManager<TNode>? _uniqueExcutor;
        protected StructureChangedEventManager<TNode> UniqueExcutor {
            get {
                if(_uniqueExcutor == null) {
                    _uniqueExcutor = new StructureChangedEventManager<TNode>(Self);
                    _uniqueExcutor.Register(disposedeventkey, () => Disposed?.Invoke(Self,EventArgs.Empty));
                    _uniqueExcutor.Register(parentchangedeventkey, () => RaisePropertyChanged(nameof(Parent)));
                }
                return _uniqueExcutor;
            }
        }
        PropertyChangeProxy PropertyChangeProxy;
        protected void RaisePropertyChanged(string propName) {
            PropertyChangeProxy.Notify(propName);
        }
        protected virtual bool SetProperty<T>(ref T strage, T value, [CallerMemberName] string? propertyName = null) {
            return PropertyChangeProxy.SetWithNotify(ref strage, value, propertyName);
        }
        /// <summary><inheritdoc/></summary>
        public event PropertyChangedEventHandler? PropertyChanged {
            add { PropertyChangeProxy.Changed += value; }
            remove { PropertyChangeProxy.Changed -= value; }
        }

        public event EventHandler<StructureChangedEventArgs<TNode>>? StructureChanged;
        void IObservableTreeNode<TNode>.OnStructureChanged(StructureChangedEventArgs<TNode> e) {
            StructureChanged?.Invoke(this, e);
        }
        public event EventHandler? Disposed;
        protected override void Dispose(bool disposing) {
            IDisposable? dsp = null;
            if (disposing){
                if(Disposed != null) {
                    dsp = UniqueExcutor.ExecuteUnique(disposedeventkey);
                }
            }
            base.Dispose(disposing);
            dsp?.Dispose();
            Disposed = null;
        }

        protected override void AddChildProcess(TNode child) {
            using (child.UniqueExcutor.LateEvaluateTree())
            using (child.ShiftParentChangedNotification()) {
                base.AddChildProcess(child);
            }
        }
        protected override void InsertChildProcess(int index, TNode child) {
            using (child.UniqueExcutor.LateEvaluateTree())
            using (child.ShiftParentChangedNotification()) {
                base.InsertChildProcess(index, child);
            }
        }
        protected override void RemoveChildProcess(TNode child) {
            using (child?.UniqueExcutor.LateEvaluateTree()) 
            using (child?.ShiftParentChangedNotification()) {
                base.RemoveChildProcess(child);
            }
        }
        protected override void ClearChildProcess() {
            using (ChildNodes.Select(a => a?.UniqueExcutor.LateEvaluateTree()).OfType<IDisposable>().ToLumpDisposables()) 
            using (ChildNodes.Select(a => a?.ShiftParentChangedNotification()).OfType<IDisposable>().ToLumpDisposables()) {
                base.ClearChildProcess();
            }
        }
        protected override void MoveChildProcess(int oldIndex, int newIndex) {
            using (ChildNodes.ElementAt(oldIndex)?.UniqueExcutor.LateEvaluateTree()){
                base.MoveChildProcess(oldIndex, newIndex);
            }
        }

    }
}
