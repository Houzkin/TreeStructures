using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TreeStructure.Collections;
using TreeStructure.EventManagement;

namespace TreeStructure {
    /// <summary>Compositeパターンをツリー構造としてラップする</summary>
    /// <remarks>参照は子孫方向へのみ広がります。</remarks>
    /// <typeparam name="TSrc">Compositeパターンをなす型</typeparam>
    /// <typeparam name="TOur">ラップするノードの型</typeparam>
    public abstract class CompositeImitator<TSrc,TOur> : ITreeNode<TOur> ,INotifyPropertyChanged,IDisposable
        where TSrc : class
        where TOur:CompositeImitator<TSrc,TOur> {
        /// <summary>ラップされたノード</summary>
        protected TSrc SourceNode { get; }
        /// <summary>新規インスタンスを初期化する</summary>
        /// <param name="sourceNode">ラップされるノード</param>
        protected CompositeImitator(TSrc sourceNode) { 
            SourceNode = sourceNode;
        }
        #region NotifyPropertyChanged
        PropertyChangeProxy? _propChangeProxy;
        PropertyChangeProxy PropChangeProxy => _propChangeProxy ??= new PropertyChangeProxy(this);
        /// <summary><inheritdoc/></summary>
        public event PropertyChangedEventHandler? PropertyChanged {
            add { this.PropChangeProxy.Changed += value; }
            remove { this.PropChangeProxy.Changed -= value; }
        }
        /// <summary>
        /// 値の変更と変更通知の発行を行う
        /// </summary>
        protected virtual bool SetProperty<T>(ref T strage, T value, [CallerMemberName] string? propertyName = null) =>
            PropChangeProxy.SetWithNotify(ref strage, value, propertyName);
        /// <summary>
        /// プロパティ変更通知を発行する
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropChangeProxy.Notify(propertyName);
        #endregion

        TOur? _parent;
        /// <summary><inheritdoc/></summary>
        public TOur? Parent { 
            get { return _parent; }
            private set { SetProperty(ref _parent, value); }
        }
        /// <summary><see cref="INotifyCollectionChanged"/>を実装した子ノードコレクションの参照を指定する</summary>
        protected abstract IEnumerable<TSrc>? SourceNodeChildren { get; }

        ImitableCollection<TOur>? _children;

        /// <summary><inheritdoc/>外部に公開するコレクション</summary>
        /// <remarks>基底クラスからは<see cref="SourceNodeChildren"/>をラップした<see cref="ImitableCollection{TSrc, TConv}"/>を返す。</remarks>
        public virtual IEnumerable<TOur> Children => 
            _children ??= ImitableCollection.Create(this.SourceNodeChildren ?? new ObservableCollection<TSrc>(), _Generate, ManageRemovedChild);
        /// <summary>子ノードに適用される、<typeparamref name="TSrc"/>から<typeparamref name="TOur"/>への変換関数</summary>
        /// <param name="sourceChildNode">ラップされる子ノード</param>
        /// <returns>ラップした子ノード</returns>
        protected abstract TOur GenerateChild(TSrc sourceChildNode);
        TOur _Generate(TSrc srcNode) {
            var cld = GenerateChild(srcNode);
            if (cld != null) {
                cld.Parent = this as TOur;
                cld.ImitateSourceSubTree();
            }
            return cld;
        }
        /// <summary>基底クラスでは<see cref="Dispose()"/>メソッドが呼び出される</summary>
        /// <remarks>インスタンスを再利用する場合は<see cref="StopImitateWithDescendant"/>メソッドを指定してください</remarks>
        /// <param name="removedNode"></param>
        protected virtual void ManageRemovedChild(TOur removedNode) {
            removedNode.Dispose();
        }

        /// <summary>現在のノードと子孫ノードの分解、各ノードの<see cref="SourceNode"/>の<see cref="ITreeNode{TNode}.Children"/>に対する購読解除を実行する</summary>
        public void StopImitateWithDescendant() {
            this.Parent = null;
            foreach (var child in this.Levelorder().Skip(1).Reverse().ToArray())
                child.StopImitateWithDescendant();
            _children?.StopImitateAndClear();
        }
        /// <summary><see cref="SourceNode"/>の<see cref="ITreeNode{TNode}.Children"/>に対する購読を再開する</summary>
        public void ImitateSourceSubTree() {
            ThrowExceptionIfDisposed();
            _children?.Imitate();
        }

        private bool isDisposed;
        /// <summary></summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                var nd = this.Levelorder().Skip(1).Reverse().ToArray();
                this.StopImitateWithDescendant();
                _children?.Dispose();
                foreach (var n in nd) n.Dispose();
            }
            isDisposed = true;
        }
        /// <summary>インスタンスを破棄する</summary>
        public void Dispose() {
            if(isDisposed) return;
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        /// <summary>既に破棄されたインスタンスの操作を禁止する。</summary>
        protected void ThrowExceptionIfDisposed() {
            if (isDisposed) throw new ObjectDisposedException(this.ToString(), "既に破棄されたインスタンスが操作されました。");
        }
    }
}
