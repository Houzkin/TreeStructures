using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Linq;

namespace TreeStructures {
    /// <summary>連動の停止とインスタンスの破棄が可能な、Compositeパターンをラップするオブジェクト</summary>
    /// <remarks>MVVMパターンでのViewModelとしての使用を想定しています</remarks>
    /// <typeparam name="TSrc">Compositeパターンをなす型</typeparam>
    /// <typeparam name="TImtr">ラップするノードの型</typeparam>
    public abstract class CompositeImitator<TSrc, TImtr> : CompositeWrapper<TSrc, TImtr>
        where TSrc : class
        where TImtr : CompositeImitator<TSrc, TImtr> {

        /// <summary>新規インスタンスを初期化する</summary>
        /// <param name="sourceNode">ラップされるノード</param>
        protected CompositeImitator(TSrc sourceNode) : base(sourceNode) { }
        //private protected override TOur GenerateAndSetupChild(TSrc sourceChildNode) {
        //    var cld = base.GenerateAndSetupChild(sourceChildNode);
        //    if (cld != null) {
        //        cld.Parent = this as TOur;
        //        cld.ImitateSourceSubTree();
        //    }
        //    return cld;
        //}

        /// <summary>削除された子ノードに対する処理</summary>
        /// <remarks>基底クラスでは<see cref="Dispose()"/>メソッドが呼び出される<br/>
        /// インスタンスを再利用する場合は<see cref="StopImitate"/>メソッドを指定してください</remarks>
        /// <param name="removedNode"></param>
        protected override void ManageRemovedChild(TImtr removedNode) {
            removedNode.Dispose();
        }
        private IReadOnlyList<TImtr> StopImitateProcess() {
            this.Parent = null;
            var lst = this.Levelorder().Skip(1).Reverse().ToList();
            foreach (var item in lst) { 
                item.StopImitateProcess();
            }
            _children?.StopImitateAndClear();
            lst.Add((this as TImtr)!);
            return lst;
        }
        /// <summary>子孫ノードの分解と、現在のノードを含む各ノードの<see cref="SourceNode"/> の子ノードコレクションに対する購読解除を実行する</summary>
        /// <returns>分解された子孫ノード</returns>
        public IReadOnlyList<TImtr> StopImitate() {
            var rmc = _children?.Select(x => x.StopImitateProcess()).SelectMany(x => x).ToArray();
            _children?.StopImitateAndClear();
            return rmc ?? Array.Empty<TImtr>();

            //this.Parent = null;
            //var lst = this.Levelorder().Skip(1).Reverse().ToList();
            //foreach (var child in lst)
            //    child.StopImitate();
            //_children?.StopImitateAndClear();
            //lst.Add((this as TOur)!);
            //return lst;
        }
        /// <summary><see cref="SourceNode"/>の子ノードコレクションに対する購読を再開する</summary>
        public void ImitateSourceSubTree() {
            ThrowExceptionIfDisposed();
            _children?.Imitate();
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                //var nd = this.Levelorder().Skip(1).Reverse().ToArray();
                var nd = this.StopImitate();
                _children?.Dispose();
                foreach (var n in nd) n.Dispose();
                base.Dispose(disposing);
            }
        }
        /// <summary>インスタンスを破棄する</summary>
        public void Dispose() { (this as IDisposable)?.Dispose(); }
    }
}
