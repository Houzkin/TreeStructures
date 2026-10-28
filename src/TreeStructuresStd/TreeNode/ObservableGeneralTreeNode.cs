using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Events;
using TreeStructures.Internals;
using TreeStructures.Linq;
using TreeStructures.Results;

namespace TreeStructures {
    /// <summary>Represents a mutable node forming an observable general tree structure.</summary>
    /// <typeparam name="TNode">The common base type for each node.</typeparam>
    [Serializable]
    public class ObservableGeneralTreeNode<TNode> : GeneralTreeNode<TNode>, IObservableTreeNode<TNode>, INotifyPropertyChanged
        where TNode : ObservableGeneralTreeNode<TNode> {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ObservableGeneralTreeNode() { this.Properties = new PropertyAccessor(this, () => this.PropertyChanged); }

        /// <summary>Initializes a new instance.</summary>
        /// <param name="collection">The collection.</param>
        public ObservableGeneralTreeNode(IEnumerable<TNode> collection) : this() {
            foreach (var item in collection) { this.AddChild(item); }
        }

        /// <inheritdoc/>
        protected override IEnumerable<TNode> SetupInnerChildCollection() => new ObservableCollection<TNode>();
        /// <inheritdoc/>
        protected override IEnumerable<TNode> SetupPublicChildCollection(IEnumerable<TNode> innerCollection) 
            => new ReadOnlyObservableCollection<TNode>((innerCollection as ObservableCollection<TNode>)!);
        
        IDisposable DeferParentChangedNotification() {
            return UniqueExcutor.LateEvaluate(parentchangedeventkey, () => Parent);
        }
        [NonSerialized]
        readonly string parentchangedeventkey = "in Library : " + nameof(ObservableGeneralTreeNode<TNode>)+ "." + nameof(Parent);
        //[NonSerialized]
        //readonly string disposedeventkey = "in Library : " + nameof(ObservableGeneralTreeNode<TNode>) + "." + nameof(Disposed);

        [NonSerialized]
        StructureChangedEventExecutor<TNode>? _uniqueExcutor;
        private StructureChangedEventExecutor<TNode> UniqueExcutor {
            get {
                if(_uniqueExcutor == null) {
                    _uniqueExcutor = new StructureChangedEventExecutor<TNode>(Self);
                    //_uniqueExcutor.Register(disposedeventkey, () => Disposed?.Invoke(Self,EventArgs.Empty));
                    _uniqueExcutor.Register(parentchangedeventkey, () => Properties.Notify(nameof(Parent)));
                }
                return _uniqueExcutor;
            }
        }
		/// <summary> Gets the <see cref="PropertyAccessor"/> used to manage property values and raise property change notifications. </summary>
		protected PropertyAccessor Properties { get; }
        /// <summary>Raises the property changed notification.</summary>
        /// <param name="propName">The property name.</param>
        [Obsolete]
        protected PropertyAccessor OnPropertyChanged([CallerMemberName] string? propName = null)
            => Properties.Notify(propName);

        /// <summary>Performs value change and raises the change notification.</summary>
        [Obsolete]
        protected ResultWithValue<PropertyAccessor> SetProperty<T>(ref T strage, T value, [CallerMemberName] string? propertyName = null) 
            => Properties.TrySetField(ref strage, value, propertyName);
        
        /// <inheritdoc/>
        [field: NonSerialized]
        public event PropertyChangedEventHandler? PropertyChanged;
        //{
        //    add { PropertyChangeProxy.Changed += value; }
        //    remove { PropertyChangeProxy.Changed -= value; }
        //}
        /// <summary>Occurs when the tree structure changes.</summary>
        [field: NonSerialized]
        public event EventHandler<StructureChangedEventArgs<TNode>>? StructureChanged;

        void IObservableTreeNode<TNode>.OnStructureChanged(StructureChangedEventArgs<TNode> e) {
            StructureChanged?.Invoke(this, e);
        }

        ///// <summary>Occurs when the instance is disposed.</summary>
        //[field: NonSerialized]
        //public event EventHandler? Disposed;
        ///// <inheritdoc/>
        //protected override void Dispose(bool disposing) {
        //    IDisposable? dsp = null;
        //    if (disposing){
        //        if(Disposed != null) {
        //            dsp = UniqueExcutor.ExecuteUnique(disposedeventkey);
        //        }
        //    }
        //    base.Dispose(disposing);
        //    dsp?.Dispose();
        //    Disposed = null;
        //}
        /// <inheritdoc/>
        protected override void AddChildCore(TNode child, Action<IEnumerable<TNode>, TNode>? action = null) {
            using (child.UniqueExcutor.LateEvaluateTree())
            using (child.DeferParentChangedNotification()) {
                base.AddChildCore(child,action);
            }
        }
        /// <inheritdoc/>
        protected override void InsertChildCore(int index, TNode child,Action<IEnumerable<TNode>,int,TNode>? action = null) {
            using (child.UniqueExcutor.LateEvaluateTree())
            using (child.DeferParentChangedNotification()) {
                base.InsertChildCore(index, child,action);
            }
        }
        /// <inheritdoc/>
        protected override void SetChildCore(int index, TNode child, Action<IEnumerable<TNode>, int, TNode>? action = null) {
            var rmv = ChildNodes.ElementAt(index);
            using (child.UniqueExcutor.LateEvaluateTree())
            using(child.DeferParentChangedNotification())
            using(rmv?.UniqueExcutor.LateEvaluateTree())
            using (rmv?.DeferParentChangedNotification())
                base.SetChildCore(index, child,action);
        }
        /// <inheritdoc/>
        protected override void RemoveChildCore(TNode child, Action<IEnumerable<TNode>, TNode>? action = null) {
            using (child?.UniqueExcutor.LateEvaluateTree()) 
            using (child?.DeferParentChangedNotification()) {
                base.RemoveChildCore(child,action);
            }
        }
        /// <inheritdoc/>
        protected override void ClearChildCore(Action<IEnumerable<TNode>>? action = null) {
            using (ChildNodes.Select(a => a?.UniqueExcutor.LateEvaluateTree()).OfType<IDisposable>().CombineDisposables()) 
            using (ChildNodes.Select(a => a?.DeferParentChangedNotification()).OfType<IDisposable>().CombineDisposables()) {
                base.ClearChildCore(action);
            }
        }
        /// <summary>Executes the process of moving a child node within the collection.</summary>
        /// <param name="oldIndex">The index of the child node to be moved.</param>
        /// <param name="newIndex">The index to which the child node will be moved.</param>
        /// <param name="action">Specifies the collection operation. The default operation in the base class is as follows:<br/>
        /// <code>(collection, idx1, idx2) =>
        ///     ((ObservableCollection&lt;<typeparamref name="TNode"/>&gt;)collection).Move(idx1, idx2);
        /// </code>
        /// </param>
        protected override void ShiftChildCore(int oldIndex, int newIndex,Action<IEnumerable<TNode>,int,int>? action = null) {
            action ??= (collection, idx1, idx2) =>
                ((ObservableCollection<TNode>)collection).Move(idx1, idx2);
            
            using (ChildNodes.ElementAt(oldIndex)?.UniqueExcutor.LateEvaluateTree()){
                base.ShiftChildCore(oldIndex, newIndex,action);
            }
        }

    }
}
