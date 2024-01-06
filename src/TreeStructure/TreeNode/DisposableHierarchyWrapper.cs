using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Linq;

namespace TreeStructures {
	/// <summary>An object that encapsulates a hierarchy, providing the ability to dispose of the instance.</summary>
	/// <remarks>If resource disposal is necessary, please inherit and use this class.</remarks>
	/// <typeparam name="TSrc">Type of the object forming the hierarchical structure</typeparam>
	/// <typeparam name="TWrpr">Type of the wrapper node</typeparam>
	public abstract class DisposableHierarchyWrapper<TSrc, TWrpr> : HierarchyWrapper<TSrc, TWrpr>,IDisposable
        where TSrc : class
        where TWrpr : DisposableHierarchyWrapper<TSrc, TWrpr> {

        /// <summary>Initializes a new instance of the class.</summary>
        /// <param name="source">The node to be wrapped.</param>
        protected DisposableHierarchyWrapper(TSrc source) : base(source) { }

        /// <summary>Handles the removed child node.</summary>
        /// <remarks>
        /// The base class calls the <see cref="Dispose()"/> method.
        /// </remarks>
        /// <param name="removedNode">The removed child node.</param>
        protected override void HandleRemovedChild(TWrpr removedNode) {
            removedNode.Dispose();
        }
		private protected override TWrpr GenerateAndSetupChild(TSrc sourceChildNode) {
            this.ThrowExceptionIfDisposed();
			return base.GenerateAndSetupChild(sourceChildNode);
		}
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
                    .Evolve(a => {
                        a.IsImitating = false;
                        return a.InnerChildren;
                    }, (a, b, c) => b.Prepend(a).Concat(c))
                    .Skip(1).Reverse().OfType<IDisposable>().ToArray();
                foreach (var n in nd) n.Dispose();
                InnerChildren.Dispose();
                this.Parent = null;
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
