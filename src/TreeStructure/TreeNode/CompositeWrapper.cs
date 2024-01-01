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
    public abstract class CompositeWrapper<TSrc,TWrpr> : ITreeNode<TWrpr> ,INotifyPropertyChanged, IDisposable,IEquatable<CompositeWrapper<TSrc,TWrpr>>
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
            add { this.PropChangeProxy.PropertyChanged += value; }
            remove { this.PropChangeProxy.PropertyChanged -= value; }
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
        /// <summary>Specifies a reference to the source's child node collection. Implementing <see cref="INotifyCollectionChanged"/> is not required if synchronization is not intended.</summary>
        protected abstract IEnumerable<TSrc>? SourceNodeChildren { get; }

        private ImitableCollection<TWrpr>? _innerChildren;
        private protected ImitableCollection<TWrpr> InnerChildren => 
            _innerChildren ??= ImitableCollection.Create(this.SourceNodeChildren ?? new ObservableCollection<TSrc>(), GenerateAndSetupChild, ManageRemovedChild,IsImitating);

        private IEnumerable<TWrpr>? _children;
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
                string msg = $"{nameof(GenerateChild)} method threw a {nameof(NullReferenceException)}.";
                if(sourceChildNode is null) { msg += $"{nameof(sourceChildNode)} is null."; }
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
                        return a.InnerChildren;
                    }, (a, b, c) => b.Prepend(a).Concat(c))
                    .Skip(1).Reverse().OfType<IDisposable>().ToArray();
                InnerChildren.Dispose();
                foreach (var n in nd) n.Dispose();
            }
            isDisposed = true;
        }
        /// <summary>Prohibits operations on an instance that has already been disposed.</summary>
        protected void ThrowExceptionIfDisposed() {
            if (isDisposed) throw new ObjectDisposedException(this.ToString(), "The instance has already been disposed and cannot be operated on.");
        }
        void IDisposable.Dispose() {
            if (isDisposed) return;
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public bool Equals(CompositeWrapper<TSrc, TWrpr>? other) {
            if (other is null || other.SourceNode is null) return false;
            return object.ReferenceEquals(this.SourceNode, other.SourceNode);
        }
        public override bool Equals(object? obj) {
            return this.Equals(obj as CompositeWrapper<TSrc, TWrpr>);
        }
        public override int GetHashCode() {
            return this.SourceNode?.GetHashCode() ?? base.GetHashCode();
        }
        public static bool operator==(CompositeWrapper<TSrc,TWrpr> obj1,CompositeWrapper<TSrc, TWrpr> obj2) {
            if (obj1 is null && obj2 is null) return true;
            if(obj1 is null ||  obj2 is null) return false;
            return obj1.Equals(obj2);
        }
        public static bool operator!=(CompositeWrapper<TSrc,TWrpr> obj1,CompositeWrapper<TSrc, TWrpr> obj2) {
            return !(obj1 == obj2);
        }
    }

}
