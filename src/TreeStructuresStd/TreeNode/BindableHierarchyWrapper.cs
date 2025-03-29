using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Events;
using TreeStructures.Linq;

namespace TreeStructures {
	/// <summary>Represents an object that wraps a hierarchal structure and is bindable to data.</summary>
	/// <typeparam name="TSrc">Type of the object forming the hierarchical structure</typeparam>
	/// <typeparam name="TWrpr">Type of the wrapper node</typeparam>
	public abstract class BindableHierarchyWrapper<TSrc, TWrpr> : HierarchyWrapper<TSrc, TWrpr>,INotifyPropertyChanged,IDisposable
        where TSrc : class
        where TWrpr : BindableHierarchyWrapper<TSrc, TWrpr> {

        /// <summary>Initializes a new instance of the class.</summary>
        /// <param name="source">The node to be wrapped.</param>
        protected BindableHierarchyWrapper(TSrc source) : base(source) { }

        /// <summary>Handles the removed child node.</summary>
        /// <remarks>
        /// The base class calls the <see cref="Dispose()"/> method.
        /// </remarks>
        /// <param name="removedNode">The removed child node.</param>
        protected override void HandleRemovedChild(TWrpr removedNode) {
            removedNode.Dispose();
        }
		//private protected override TWrpr GenerateAndSetupChild(TSrc sourceChildNode) {
		//          this.ThrowExceptionIfDisposed();
		//	return base.GenerateAndSetupChild(sourceChildNode);
		//}
		private protected override void _HandleRemovedChild(TWrpr removeNode) {
			if(removeNode != null && !removeNode.isDisposed){
				base._HandleRemovedChild(removeNode);
			}
		}
		private protected override void SetupChild(TWrpr child) {
			this.ThrowExceptionIfDisposed();
			base.SetupChild(child);
		}
		private protected override bool SetParent(TWrpr? parent) {
			if(base.SetParent(parent)){
				this.RaisePropertyChanged(nameof(Parent));
				return true;
			}
			return false;
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

		//private IReadOnlyList<TImtr> StopImitateProcess() {
		//    this.Parent = null;
		//    var lst = this
		//        .Evolve(a => {
		//            a.IsImitating = false;
		//            return a.InnerChildren; 
		//        }, (a, b, c) => b.Prepend(a).Concat(c))
		//        .Skip(1).Reverse().ToList();
		//    foreach (var item in lst) { 
		//        item.StopImitateProcess();
		//    }
		//    InnerChildren.PauseImitationAndClear();
		//    lst.Add((this as TImtr)!);
		//    return lst;
		//}
		///// <summary>
		///// Disassembles descendant nodes and unsubscribes from the <see cref="CompositeWrapper{TSrc, TWrpr}.SourceChildren"/> of each node, including the current node.
		///// </summary>
		///// <returns>The disassembled descendant nodes.</returns>
		//public IReadOnlyList<TImtr> PauseImitation() {
		//    this.IsImitating = false;
		//    var rmc = InnerChildren.Select(x => x.StopImitateProcess()).SelectMany(x => x).ToArray();
		//    InnerChildren.PauseImitationAndClear();
		//    return rmc ?? Array.Empty<TImtr>();

        //public new void PauseImitation(){ base.PauseImitation(); }

		//}
		///// <summary>Resume subscription to <see cref="CompositeWrapper{TSrc, TWrpr}.SourceChildren"/> and imitate descendant nodes.</summary>
		//public void ImitateSourceSubTree() {
		//    ThrowExceptionIfDisposed();
		//    this.IsImitating = true;
		//    InnerChildren.Imitate();
		//}

        //public new void ImitateSourceSubTree(){
        //    this.ThrowExceptionIfDisposed();
        //    base.ImitateSourceSubTree(); 
        //}
		//public override void RefreshHierarchy() {
  //          this.ThrowExceptionIfDisposed();
		//	base.RefreshHierarchy();
		//}

		private bool isDisposed;
		/// <summary>Indicates whether the instance has been disposed.</summary>
		public bool IsDisposed => isDisposed;
        /// <summary></summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
				var nd = this
					.Traverse(a => {
						a.IsImitating = false;
						//return a.InnerChildren;
						return a.InnerChildNodes;
					}, (a, b, c) => c.Prepend(a).Concat(b))// b.Prepend(a).Concat(c))
                    .Skip(1).Reverse().OfType<IDisposable>();
                foreach (var n in nd) n.Dispose();
                //InnerChildren.Dispose();
				InnerChildNodes.ClearCollection();
				
            }
            isDisposed = true;
        }
        /// <summary>Prohibits operations on an instance that has already been disposed.</summary>
        protected void ThrowExceptionIfDisposed() {
            if (isDisposed) throw new ObjectDisposedException(this.ToString(), "The instance has already been disposed and cannot be operated on.");
        }
		//void IDisposable.Dispose() {
		//    if (isDisposed) return;
		//    Dispose(disposing: true);
		//    GC.SuppressFinalize(this);
		//}


		///// <inheritdoc/>
		//protected override void Dispose(bool disposing) {
		//    if (disposing) {
		//        var nd = this.PauseImitation();
		//        InnerChildren.Dispose();
		//        foreach (var n in nd) n.Dispose();
		//        base.Dispose(disposing);
		//    }
		//}

		/// <summary>Disposes of the current instance and all descendant nodes.</summary>
		public void Dispose() {
            if (isDisposed) return;
            this.Dispose(true);
            GC.SuppressFinalize(this);
        
        }
    }
}
