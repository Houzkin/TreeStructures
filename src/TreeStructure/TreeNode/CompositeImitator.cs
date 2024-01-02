using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Linq;

namespace TreeStructures {
    /// <summary>An object that wraps a Composite pattern with the ability to pause/resume synchronization and dispose the instance.</summary>
    /// <remarks>
    /// Although designed with the intention of being utilized as a ViewModel in the MVVM pattern, this wrapper object is not limited to that specific use case.<br/>
    /// It can be applied for various purposes, serving as a ViewModel being just one example.
    /// </remarks>
    /// <typeparam name="TSrc">Type of the Composite pattern</typeparam>
    /// <typeparam name="TImtr">Type of the wrapper node</typeparam>
    public abstract class CompositeImitator<TSrc, TImtr> : CompositeWrapper<TSrc, TImtr>
        where TSrc : class
        where TImtr : CompositeImitator<TSrc, TImtr> {

        /// <summary>Initializes a new instance of the class.</summary>
        /// <param name="sourceNode">The node to be wrapped.</param>
        protected CompositeImitator(TSrc sourceNode) : base(sourceNode) { }

        /// <summary>Handles the removed child node.</summary>
        /// <remarks>
        /// The base class calls the <see cref="Dispose()"/> method.
        /// If you intend to reuse the instance, please specify the <see cref="PauseImitation"/> method.
        /// </remarks>
        /// <param name="removedNode">The removed child node.</param>
        protected override void ManageRemovedChild(TImtr removedNode) {
            removedNode.Dispose();
        }
        private IReadOnlyList<TImtr> StopImitateProcess() {
            this.Parent = null;
            var lst = this
                .Evolve(a => {
                    a.IsImitating = false;
                    return a.InnerChildren; 
                }, (a, b, c) => b.Prepend(a).Concat(c))
                .Skip(1).Reverse().ToList();
            foreach (var item in lst) { 
                item.StopImitateProcess();
            }
            InnerChildren.PauseImitationAndClear();
            lst.Add((this as TImtr)!);
            return lst;
        }
        /// <summary>
        /// Disassembles descendant nodes and unsubscribes from the <see cref="CompositeWrapper{TSrc, TWrpr}.SourceNodeChildren"/> of each node, including the current node.
        /// </summary>
        /// <returns>The disassembled descendant nodes.</returns>
        public IReadOnlyList<TImtr> PauseImitation() {
            this.IsImitating = false;
            var rmc = InnerChildren.Select(x => x.StopImitateProcess()).SelectMany(x => x).ToArray();
            InnerChildren.PauseImitationAndClear();
            return rmc ?? Array.Empty<TImtr>();

        }
        /// <summary>Resume subscription to <see cref="CompositeWrapper{TSrc, TWrpr}.SourceNodeChildren"/> and imitate descendant nodes.</summary>
        public void ImitateSourceSubTree() {
            ThrowExceptionIfDisposed();
            this.IsImitating = true;
            InnerChildren.Imitate();
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                //var nd = this.Levelorder().Skip(1).Reverse().ToArray();
                var nd = this.PauseImitation();
                InnerChildren.Dispose();
                foreach (var n in nd) n.Dispose();
                base.Dispose(disposing);
            }
        }
        /// <summary>Dispose the instance</summary>
        public void Dispose() { (this as IDisposable)?.Dispose(); }
    }
}
