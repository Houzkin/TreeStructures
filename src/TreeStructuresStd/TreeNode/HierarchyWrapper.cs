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
using TreeStructures.Events;
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
        private protected virtual IEnumerable<TSrc>? getSourceChildren {
            get {
                var src = SourceChildren;
                if (src is INotifyCollectionChanged) return src.AsReadOnly();
                return src;
            }
        }

        private CombinableChildrenProxyCollection<TWrpr>? _wrappers;
        private protected CombinableChildrenProxyCollection<TWrpr> InnerChildNodes {
            get {
                //_wrappers ??= new CombinableChildrenProxyCollection<TWrpr>(
                //    (getSourceChildren ?? new ObservableCollection<TSrc>()).ToImitable(GenerateChild, null, IsImitating),
                //    SetupChildNode,
                //    HandleRemovedChildNode);
                if(_wrappers is null) {
                    var srccn = getSourceChildren;
                    _wrappers = new CombinableChildrenProxyCollection<TWrpr>(
                        (srccn ?? new ObservableCollection<TSrc>()).ToImitable(GenerateChild, null, IsImitating),
                        srccn is INotifyCollectionChanged,
                        SetupChildNode,
                        HandleRemovedChildNode);
                }
                return _wrappers;
            }
        }
        private protected virtual void SetupChildNode(TWrpr child) {
            if(child != null){
                child.SetParent(this as TWrpr);
                child.IsImitating = true;
                child._wrappers?.Imitate();
            }
        }

        private IEnumerable<TWrpr>? _children;
        /// <inheritdoc/>
        public IEnumerable<TWrpr> Children {
            get {
                _children ??= SetupPublicChildCollection(InnerChildNodes);
				if (this.IsImitating && !InnerChildNodes.SourceChildrenIsObservable) {
					//InnerChildNodes.Pause();
					InnerChildNodes.Imitate();
				}
                return _children;
            }
        }// => _Children;//_children ??= SetupPublicChildCollection(InnerChildNodes);
        /// <summary>Sets the collection to be exposed externally.</summary>
        /// <param name="children">A combinable collection wrapping each node of <see cref="SourceChildren"/>.</param>
        protected virtual IEnumerable<TWrpr> SetupPublicChildCollection(CombinableChildrenProxyCollection<TWrpr> children)
            => children;
        //=> children.AsReadOnlyObservableCollection();
        //private protected virtual IEnumerable<TWrpr> getSetupPublicChildCollection(CombinableChildrenProxyCollection<TWrpr> children) {
        //    var spcc = SetupPublicChildCollection(children);
        //    return (spcc is INotifyCollectionChanged) ? spcc.AsReadOnly() : spcc;
        //}

        /// <summary>Conversion function applied to child nodes, converting from <typeparamref name="TSrc"/> to <typeparamref name="TWrpr"/>.</summary>
        /// <param name="sourceChildNode">Child node to be wrapped</param>
        /// <returns>Wrapped child node</returns>
        protected abstract TWrpr GenerateChild(TSrc sourceChildNode);
        private protected virtual void HandleRemovedChildNode(TWrpr removeNode){
            if (removeNode != null) {
                removeNode.SetParent(null);
                HandleRemovedChild(removeNode);
            }
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
            if (other is null) return false;
            if (other.Source is null && this.Source is null) return object.ReferenceEquals(this, other);
            //if (other is null || other.Source is null) return false;
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
		public static bool operator ==(HierarchyWrapper<TSrc, TWrpr> obj1, HierarchyWrapper<TSrc, TWrpr> obj2) {
            if (obj1 is null && obj2 is null) return true;
            if(obj1 is null ||  obj2 is null) return false;
            return obj1.Equals(obj2);
        }
		public static bool operator !=(HierarchyWrapper<TSrc, TWrpr> obj1, HierarchyWrapper<TSrc, TWrpr> obj2) {
            return !(obj1 == obj2);
        }

		/// <summary>
		/// Provides a collection of wrappers for each child node in a state combinable with other collections.
		/// </summary>
        /// <typeparam name="Twrpr">Type of the wrapper node</typeparam>
        public sealed class CombinableChildrenProxyCollection<Twrpr> : ObservableCombinableCollection<Twrpr> where Twrpr : class {
            ImitableCollection<Twrpr> _childNodes;
            bool _isObserving;
            internal CombinableChildrenProxyCollection(ImitableCollection<Twrpr> childNodes,bool isObservable ,Action<Twrpr> addOption,Action<Twrpr> removedOption):base(addOption,removedOption) {
                _childNodes = childNodes;
                _isObserving = isObservable;
                this.AppendCollection(childNodes);
            }
            internal bool SourceChildrenIsObservable => _isObserving;
            internal void Imitate() { _childNodes.Imitate(); }
            internal void Pause() {  _childNodes.Pause(); }
            /// <inheritdoc/>
			protected override void Dispose(bool disposing) {
				base.Dispose(disposing);
                _childNodes.Dispose();
			}
		}
    }
}
