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
    /// Wraps the hierarchy object as a tree structure.
    /// References are only propagated in the descendant direction.
    /// Intended for exposing a mutable structure as a read-only or restricted-change data structure.
    /// </summary>
    /// <typeparam name="TSrc">Type representing the hierarchy object</typeparam>
    /// <typeparam name="TWrpr">Type of the wrapper node</typeparam>
    public abstract class HierarchyWrapper<TSrc,TWrpr> : ITreeNode<TWrpr> , IEquatable<HierarchyWrapper<TSrc,TWrpr>>
        where TSrc : class
        where TWrpr:HierarchyWrapper<TSrc,TWrpr> {

        /// <summary>The wrapped node</summary>
        protected TSrc Source { get; }

        /// <summary>Initializes a new instance</summary>
        /// <param name="source">The node to be wrapped</param>
        protected HierarchyWrapper(TSrc source) { 
            Source = source;
        }
        
        TWrpr? _parent;
        /// <summary><inheritdoc/></summary>
        public TWrpr? Parent { 
            get { return _parent; }
            //private protected set {
            //    //SetProperty(ref _parent, value);
            //    if (_parent == value) return;
            //    _parent = value;
            //}
        }
        private protected virtual bool SetParent(TWrpr? parent){
            if (object.ReferenceEquals(_parent,parent)) return false;
            _parent = parent;
            return true;
        }
        /// <summary>Specifies a reference to the source's child node collection. Implementing <see cref="INotifyCollectionChanged"/> is not required if synchronization is not intended.</summary>
        protected abstract IEnumerable<TSrc>? SourceChildren { get; }

        private ImitableCollection<TWrpr>? _innerChildren;
        private protected ImitableCollection<TWrpr> InnerChildren => 
            _innerChildren ??= ImitableCollection.Create(this.SourceChildren ?? new ObservableCollection<TSrc>(), GenerateAndSetupChild, _HandleRemovedChild,IsImitating);

        private IEnumerable<TWrpr>? _children;
        /// <inheritdoc/>
        public IEnumerable<TWrpr> Children => _children ??= SetupPublicChildCollection(InnerChildren);
        /// <summary>Sets the collection to be exposed externally.</summary>
        /// <remarks>From the base class, it returns a wrapped <see cref="ImitableCollection{TSrc, TConv}"/> of <see cref="SourceChildren"/>.</remarks>
        protected virtual IEnumerable<TWrpr> SetupPublicChildCollection(ImitableCollection<TWrpr> children) => children;
        /// <summary>Conversion function applied to child nodes, converting from <typeparamref name="TSrc"/> to <typeparamref name="TWrpr"/>.</summary>
        /// <param name="sourceChildNode">Child node to be wrapped</param>
        /// <returns>Wrapped child node</returns>
        protected abstract TWrpr GenerateChild(TSrc sourceChildNode);
        private protected virtual TWrpr GenerateAndSetupChild(TSrc sourceChildNode) {
            //ThrowExceptionIfDisposed();
            TWrpr? cld = null;
            try {
                cld = GenerateChild(sourceChildNode);
            } catch(NullReferenceException e) {
                string msg = $"{nameof(GenerateChild)} method threw a {nameof(NullReferenceException)}.";
                if(sourceChildNode is null) { msg += $"{nameof(sourceChildNode)} is null."; }
                throw new NullReferenceException( msg, e);
            }
            if(cld != null) {
                //cld.Parent = this as TWrpr;
                cld.SetParent(this as TWrpr);
                cld.IsImitating = true;
                cld._innerChildren?.Imitate();
            }
            return cld;
        }
        void _HandleRemovedChild(TWrpr removeNode){
            //removeNode.Parent = null;
            removeNode.SetParent(null);
            //removeNode.PauseImitation();
            HandleRemovedChild(removeNode);
        }
        /// <summary>Handles processing of a decomposed child node.</summary>
        /// <remarks>This method is intended to be overridden in derived classes to add specific processing for an already decomposed child node.</remarks>
        /// <param name="removedNode">Removed child node</param>
        protected virtual void HandleRemovedChild(TWrpr removedNode) { }

        
        bool _isImitating = true;
        /// <summary>Parameter indicating the state of whether the child node collection is in the imitating state when generating an instance.</summary>
        private protected bool IsImitating {
            get { return _isImitating; }
            set { if (_isImitating != value) _isImitating = value; }
        }


        /// <inheritdoc/>
        public bool Equals(HierarchyWrapper<TSrc, TWrpr>? other) {
            if (other is null || other.Source is null) return false;
            return object.ReferenceEquals(this.Source, other.Source);
		}
		/// <inheritdoc/>
		public override bool Equals(object? obj) {
            return this.Equals(obj as HierarchyWrapper<TSrc, TWrpr>);
		}
		/// <inheritdoc/>
		public override int GetHashCode() {
            return this.Source?.GetHashCode() ?? base.GetHashCode();
		}
		public static bool operator==(HierarchyWrapper<TSrc,TWrpr> obj1,HierarchyWrapper<TSrc, TWrpr> obj2) {
            if (obj1 is null && obj2 is null) return true;
            if(obj1 is null ||  obj2 is null) return false;
            return obj1.Equals(obj2);
        }
        public static bool operator!=(HierarchyWrapper<TSrc,TWrpr> obj1,HierarchyWrapper<TSrc, TWrpr> obj2) {
            return !(obj1 == obj2);
        }
    }

}
