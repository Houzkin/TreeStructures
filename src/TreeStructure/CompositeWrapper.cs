using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TreeStructures.Collections;
using TreeStructures.EventManagement;
using TreeStructures.Linq;

namespace TreeStructures {
    /// <summary>
    /// Wraps the Composite pattern as a tree structure.
    /// References are only propagated in the descendant direction.
    /// Intended for exposing a mutable structure as a read-only or restricted-change data structure.
    /// </summary>
    /// <typeparam name="TSrc">Type representing the Composite pattern</typeparam>
    /// <typeparam name="TWrpr">Type of the wrapper node</typeparam>
    public abstract class CompositeWrapper<TSrc,TWrpr> : ITreeNode<TWrpr> ,INotifyPropertyChanged, IDisposable
        where TSrc : class
        where TWrpr:CompositeWrapper<TSrc,TWrpr> {

        /// <summary>The wrapped node</summary>
        protected TSrc SourceNode { get; }

        /// <summary>Initializes a new instance</summary>
        /// <param name="sourceNode">The node to be wrapped</param>
        protected CompositeWrapper(TSrc sourceNode) { 
            SourceNode = sourceNode;
        }
        #region NotifyPropertyChanged
        PropertyChangeProxy? _propChangeProxy;
        PropertyChangeProxy PropChangeProxy => _propChangeProxy ??= new PropertyChangeProxy(this);
        /// <summary><inheritdoc/></summary>
        public event PropertyChangedEventHandler? PropertyChanged {
            add { this.PropChangeProxy.Changed += value; }
            remove { this.PropChangeProxy.Changed -= value; }
        }
        /// <summary>
        /// Performs the change of value and issues a change notification.
        /// </summary>
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null) =>
            PropChangeProxy.SetWithNotify(ref storage, value, propertyName);
        /// <summary>
        ///  Issues a property change notification.
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropChangeProxy.Notify(propertyName);
        #endregion

        TWrpr? _parent;
        /// <summary><inheritdoc/></summary>
        public TWrpr? Parent { 
            get { return _parent; }
            private protected set { SetProperty(ref _parent, value); }
        }
        /// <summary>Specifies a reference to a child node collection that implements <see cref="INotifyCollectionChanged"/>.</summary>
        protected abstract IEnumerable<TSrc>? SourceNodeChildren { get; }

        private protected ImitableCollection<TWrpr>? _innerChildren;
        private ImitableCollection<TWrpr> InnerChildren => 
            _innerChildren ??= ImitableCollection.Create(this.SourceNodeChildren ?? new ObservableCollection<TSrc>(), GenerateAndSetupChild, ManageRemovedChild,IsImitating);

        private IEnumerable<TWrpr>? _children;
        //public virtual IEnumerable<TWrpr> Children => 
        //    _innerChildren ??= ImitableCollection.Create(this.SourceNodeChildren ?? new ObservableCollection<TSrc>(), GenerateAndSetupChild, ManageRemovedChild);
        /// <inheritdoc/>
        public IEnumerable<TWrpr> Children => _children ??= SetupPublicChildCollection(InnerChildren);
        /// <summary>Sets the collection to be exposed externally.</summary>
        /// <remarks>From the base class, it returns a wrapped <see cref="ImitableCollection{TSrc, TConv}"/> of <see cref="SourceNodeChildren"/>.</remarks>
        protected virtual IEnumerable<TWrpr> SetupPublicChildCollection(ImitableCollection<TWrpr> children) => children;
        /// <summary>Conversion function applied to child nodes, converting from <typeparamref name="TSrc"/> to <typeparamref name="TWrpr"/>.</summary>
        /// <param name="sourceChildNode">Child node to be wrapped</param>
        /// <returns>Wrapped child node</returns>
        protected abstract TWrpr GenerateChild(TSrc sourceChildNode);
        private TWrpr GenerateAndSetupChild(TSrc sourceChildNode) {
            ThrowExceptionIfDisposed();
            TWrpr? cld = null;
            try {
                cld = GenerateChild(sourceChildNode);
            } catch(NullReferenceException e) {
                string msg = $"{nameof(GenerateChild)}メソッドで{nameof(NullReferenceException)}が発生しました。";
                if(sourceChildNode is null) { msg += $"{nameof(sourceChildNode)}は null です。"; }
                throw new NullReferenceException( msg, e);
            }
            if(cld != null) {
                cld.Parent = this as TWrpr;
                cld.IsImitating = true;
                cld._innerChildren?.Imitate();
            }
            return cld;
        }
        /// <summary>Processing for removed child nodes.</summary>
        /// <remarks>Invoked by the <see cref="Dispose"/> method in the base class.</remarks>
        /// <param name="removedNode">Removed child node</param>
        protected virtual void ManageRemovedChild(TWrpr removedNode) {
            (removedNode as IDisposable)?.Dispose();
        }
        bool _isImitating = true;
        /// <summary>Parameter indicating the state of whether the child node collection is in the imitating state when generating an instance.</summary>
        private protected bool IsImitating {
            get { return _isImitating; }
            set { if (_isImitating != value) _isImitating = value; }
        }
        private bool isDisposed;
        /// <summary></summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                this.Parent = null;
                var nd = this
                    .Evolve(a => {
                        a.IsImitating = false;
                        return a.Children;
                    }, (a, b, c) => b.Prepend(a).Concat(c))
                    .Skip(1).Reverse().OfType<IDisposable>().ToArray();
                _innerChildren?.Dispose();
                foreach (var n in nd) n.Dispose();
            }
            isDisposed = true;
        }
        /// <summary>Prohibits operations on an instance that has already been disposed.</summary>
        protected void ThrowExceptionIfDisposed() {
            if (isDisposed) throw new ObjectDisposedException(this.ToString(), "既に破棄されたインスタンスが操作されました。");
        }
        void IDisposable.Dispose() {
            if (isDisposed) return;
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}
