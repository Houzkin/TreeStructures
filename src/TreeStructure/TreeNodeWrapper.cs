using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TreeStructure.Collections;

namespace TreeStructure {
    
    public abstract class TreeNodeWrapper<TSrc,TOur> : ITreeNode<TOur>
        where TSrc :class, ITreeNode<TSrc>
        where TOur:TreeNodeWrapper<TSrc,TOur> {
        
        protected TSrc SourceNode { get; }

        protected TreeNodeWrapper(TSrc sourceNode) { SourceNode = sourceNode; }
        TOur? _parent;
        public TOur? Parent { get { return _parent; } }
        bool SetParent(TOur? parent) {
            if (_parent == parent) return false;
            _parent = parent;
            return true;
        }
        ConvertedCollection<TSrc,TOur>? _childNodes;
        /// <summary>内部操作用のコレクション</summary>
        /// <remarks><see cref="SourceNode"/>の子ノードコレクションをラップした<see cref="ConvertedCollection{TSrc, TConv}"/>を返す。</remarks>
        protected ConvertedCollection<TSrc,TOur> ChildNodes => 
            _childNodes ??= ConvertedCollection.Create(SourceNode?.Children ?? new ObservableCollection<TSrc>(), _Generate, ManageRemoveChild);
        
        /// <summary><inheritdoc/>外部に公開するコレクション</summary>
        public virtual IEnumerable<TOur> Children => ChildNodes;

        protected abstract TOur GenerateChild(TSrc srcNode);
        TOur _Generate(TSrc srcNode) {
            var cld = GenerateChild(srcNode);
            if (cld != null) { 
                cld.SetParent(this as TOur);
                cld.SetReference();
            }
            return cld;
        }
        
        /// <summary>基底クラスでは<see cref="ReleaseReference"/>メソッドが呼び出される</summary>
        /// <param name="removedNode"></param>
        protected virtual void ManageRemoveChild(TOur removedNode) {
            removedNode.ReleaseReference();
        }

        /// <summary>子孫ノードの分解と、<see cref="SourceNode"/>の<see cref="ITreeNode{TNode}.Children"/>に対する購読を解除する</summary>
        public void ReleaseReference() { 
            this.SetParent(null);
            foreach (var child in this.Levelorder().Skip(1).Reverse().ToArray())
                child.ReleaseReference();
            if(_childNodes is ConvertedCollection<TOur> convs) {
                convs.StopListeningWithClear();
            }
        }
        /// <summary><see cref="SourceNode"/>の<see cref="ITreeNode{TNode}.Children"/>に対する購読を再開する</summary>
        public void SetReference() {
            if(_childNodes is ConvertedCollection<TOur> collection) {
                collection.StartListening();
            }
        }
    }
}
