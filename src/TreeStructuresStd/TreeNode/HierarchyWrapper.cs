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

        //private ImitableCollection<TWrpr>? _innerChildren;
        //private protected ImitableCollection<TWrpr> InnerChildren => 
        //    _innerChildren ??= ImitableCollection.Create(this.SourceChildren ?? new ObservableCollection<TSrc>(), GenerateAndSetupChild, _HandleRemovedChild,IsImitating);



        private CombinableChildWrapperCollection<TWrpr>? _wrappers;
        private protected CombinableChildWrapperCollection<TWrpr> InnerChildNodes{
            get{
                _wrappers ??= new CombinableChildWrapperCollection<TWrpr>(
                    new ImitableCollection<TSrc, TWrpr>(SourceChildren ?? new ObservableCollection<TSrc>(), GenerateChild, null, IsImitating),
                    Insert, Replace, Remove, Move, Clear);
                return _wrappers;
            }
        }
        void Insert(ObservableCollection<TWrpr> wrprs, int index, TWrpr wrpr) {
            SetupChild(wrpr);
            wrprs.Insert(index, wrpr);
        }
        void Replace(ObservableCollection<TWrpr> wrprs, int index, TWrpr wrpr) {
            _HandleRemovedChild(wrprs[index]);
            SetupChild(wrpr);
            wrprs[index] = wrpr;
        }
        void Remove(ObservableCollection<TWrpr> wrprs, int idx) {
            _HandleRemovedChild(wrprs[idx]);
            wrprs.RemoveAt(idx);
        }
        void Move(ObservableCollection<TWrpr> wrprs, int tgtIdx, int toIdx) { 
            try{
                wrprs.Move(tgtIdx, toIdx);
            }catch(Exception e){
            }
        }
        void Clear(ObservableCollection<TWrpr> wrprs) {
            var lst = wrprs.ToArray();
            wrprs.Clear();
            foreach (var wrpr in lst) { _HandleRemovedChild(wrpr); }
        }

        private protected virtual void SetupChild(TWrpr child) {
            if(child != null){
                child.SetParent(this as TWrpr);
                child.IsImitating = true;
                child._wrappers?.Imitate();
            }
        }

        private IEnumerable<TWrpr>? _children;
        /// <inheritdoc/>
        public IEnumerable<TWrpr> Children => _children ??= SetupPublicChildCollection(InnerChildNodes);
		/// <summary>Sets the collection to be exposed externally.</summary>
		/// <param name="children">A combinable collection wrapping each node of <see cref="SourceChildren"/>.</param>
		protected virtual IEnumerable<TWrpr> SetupPublicChildCollection(CombinableChildWrapperCollection<TWrpr> children)
            => children.AsReadOnlyObservableCollection();

        /// <summary>Conversion function applied to child nodes, converting from <typeparamref name="TSrc"/> to <typeparamref name="TWrpr"/>.</summary>
        /// <param name="sourceChildNode">Child node to be wrapped</param>
        /// <returns>Wrapped child node</returns>
        protected abstract TWrpr GenerateChild(TSrc sourceChildNode);
        private protected virtual void _HandleRemovedChild(TWrpr removeNode){
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
		/// <typeparam name="T"></typeparam>
		public sealed class CombinableChildWrapperCollection<T> : CombinableObservableCollection<T> where T : class {
		    ImitableCollection<T> _childNodes;
		    internal CombinableChildWrapperCollection(ImitableCollection<T> childNodes,
			    Action<ObservableCollection<T>,int,T> insertAction,
			    Action<ObservableCollection<T>,int,T> replaceAction,
			    Action<ObservableCollection<T>,int> removeAction,
			    Action<ObservableCollection<T>,int,int> moveAction,
			    Action<ObservableCollection<T>> clearAction) : base(){

			    _childNodes = childNodes;
                this.ListAligner = new ListAligner<T, ObservableCollection<T>>(
                    editList:(this.Items as ObservableCollection<T>)!,insert: insertAction,replace: replaceAction,remove: removeAction,move: moveAction,clear: clearAction,comparer: Equality<T>.ReferenceComparer);
			    this.AppendCollection(childNodes);
		    }
		    internal void Imitate(){ _childNodes.Imitate(); }
            /// <inheritdoc/>
		    protected override ListAligner<T, ObservableCollection<T>> ListAligner { get; }
	    }




    }

}
