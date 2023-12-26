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
    /// <summary>Compositeパターンをツリー構造としてラップする<br/>参照は子孫方向へのみ広がります。</summary>
    /// <remarks>プライベートなツリー構造をReadOnlyなデータ構造として公開する用途を想定しています。</remarks>
    /// <typeparam name="TSrc">Compositeパターンをなす型</typeparam>
    /// <typeparam name="TWrpr">ラップするノードの型</typeparam>
    public abstract class CompositeWrapper<TSrc,TWrpr> : ITreeNode<TWrpr> ,INotifyPropertyChanged, IDisposable
        where TSrc : class
        where TWrpr:CompositeWrapper<TSrc,TWrpr> {
        /// <summary>ラップされたノード</summary>
        protected TSrc SourceNode { get; }
        /// <summary>新規インスタンスを初期化する</summary>
        /// <param name="sourceNode">ラップされるノード</param>
        protected CompositeWrapper(TSrc sourceNode) { 
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

        TWrpr? _parent;
        /// <summary><inheritdoc/></summary>
        public TWrpr? Parent { 
            get { return _parent; }
            private protected set { SetProperty(ref _parent, value); }
        }
        /// <summary><see cref="INotifyCollectionChanged"/>を実装した子ノードコレクションの参照を指定する</summary>
        protected abstract IEnumerable<TSrc>? SourceNodeChildren { get; }

        private protected ImitableCollection<TWrpr>? _children;

        /// <summary><inheritdoc/>外部に公開するコレクション</summary>
        /// <remarks>基底クラスからは<see cref="SourceNodeChildren"/>をラップした<see cref="ImitableCollection{TSrc, TConv}"/>を返す。</remarks>
        public virtual IEnumerable<TWrpr> Children => 
            _children ??= ImitableCollection.Create(this.SourceNodeChildren ?? new ObservableCollection<TSrc>(), GenerateAndSetupChild, ManageRemovedChild);
        /// <summary>子ノードに適用される、<typeparamref name="TSrc"/>から<typeparamref name="TWrpr"/>への変換関数</summary>
        /// <param name="sourceChildNode">ラップされる子ノード</param>
        /// <returns>ラップした子ノード</returns>
        protected abstract TWrpr GenerateChild(TSrc sourceChildNode);
        private TWrpr GenerateAndSetupChild(TSrc sourceChildNode) {
            ThrowExceptionIfDisposed();
            TWrpr? cld = null;
            try {
                cld = GenerateChild(sourceChildNode);
            } catch(NullReferenceException e) {
                string msg = $"{nameof(GenerateChild)}メソッドで{nameof(NullReferenceException)}が発生しました。";
                if(sourceChildNode is null) { msg += $"{nameof(sourceChildNode)}は null です。"; }
                throw new NullReferenceException( msg, e);
            }
            if(cld != null) {
                cld.Parent = this as TWrpr;
                cld._children?.Imitate();
            }
            return cld;
        }
        /// <summary>削除された子ノードに対する処理</summary>
        /// <remarks>基底クラスでは<see cref="Dispose()"/>メソッドが呼び出される</remarks>
        /// <param name="removedNode">削除された子ノード</param>
        protected virtual void ManageRemovedChild(TWrpr removedNode) {
            (removedNode as IDisposable)?.Dispose();
        }

        private bool isDisposed;
        /// <summary></summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                this.Parent = null;
                var nd = this.Levelorder().Skip(1).Reverse().OfType<IDisposable>().ToArray();
                _children?.Dispose();
                foreach (var n in nd) n.Dispose();
            }
            isDisposed = true;
        }
        /// <summary>既に破棄されたインスタンスの操作を禁止する。</summary>
        protected void ThrowExceptionIfDisposed() {
            if (isDisposed) throw new ObjectDisposedException(this.ToString(), "既に破棄されたインスタンスが操作されました。");
        }
        void IDisposable.Dispose() {
            if (isDisposed) return;
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}
