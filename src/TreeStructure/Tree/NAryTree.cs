using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures.Tree {
    public abstract class NAryTree<T> : TreeNodeBase<T> where T : NAryTree<T> {
        protected NAryTree(int nary) :base() {
            for(int i = 0; i < nary; i++) {
                this.AddAction(this.ChildNodes, null);
            }
        }
        /// <inheritdoc/>
        protected override IList<T> ChildNodes { get; } = new List<T>();
        public T? Left {
            get { return ChildNodes.ElementAt(0); }
            set {
                if (base.CanAddChildNode(value)) SetChildProcess(0, value);
            }
        }
        public T? Right {
            get { return ChildNodes.ElementAt(1); }
            set {
                if (base.CanAddChildNode(value)) SetChildProcess(1, value);
            }
        }
        protected virtual void SetChildProcess(int index, T value) {
            base.RemoveChildProcess(ChildNodes.ElementAt(index));
            this.InsertChildProcess(index, value);
        }
        protected override void RemoveChildProcess(T child) {
            var idx = ChildNodes/*.ToList()*/.IndexOf(child);
            if(0<=idx) this.SetChildProcess(idx, null);
        }
        public bool CanAddChild(T child) {
            if (!base.CanAddChildNode(child)) return false;
            if (!ChildNodes.Any(x=>x==null)) return false;
            return true;
        }
        public T AddChild(T child) {
            if (!base.CanAddChildNode(child)) return Self;
            var idx = this.ChildNodes/*.ToList()*/.IndexOf(null);
            if(0<=idx) 
                SetChildProcess(idx, child);
            return Self;
        }
        public T RemoveChild(T child) {
            RemoveChildProcess(child);
            return Self;
        }
    }
    
}
