using Microsoft.VisualBasic;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TreeStructures.Linq;

namespace TreeStructures {
    /// <summary>ツリー構造をなすノードを表す。</summary>
    /// <typeparam name="TNode">各ノードの共通基本クラスとなる型</typeparam>
    [Serializable]
    public abstract class TreeNodeBase<TNode> : ITreeNode<TNode>
        where TNode:TreeNodeBase<TNode>,ITreeNode<TNode>{
        /// <summary>インスタンスを初期化する。</summary>
        protected TreeNodeBase() { }

        /// <summary><inheritdoc/></summary>
        public TNode? Parent { get; private set; }

        IEnumerable<TNode>? _readonlycollection;
        /// <summary><inheritdoc/>ReadOnlyの公開用プロパティ</summary>
        public IEnumerable<TNode> Children => _readonlycollection ??= SetupPublicChildCollection(ChildNodes);
        IEnumerable<TNode>? _childNodes;
        /// <summary>子ノードを管理する、内部処理用のプロパティ</summary>
        protected IEnumerable<TNode> ChildNodes => _childNodes ??= SetupInnerChildCollection();
        /// <summary>子ノードを扱う内部処理用コレクションを指定する</summary>
        protected abstract IEnumerable<TNode> SetupInnerChildCollection();
        /// <summary>外部公開用の子ノードコレクションを指定する</summary>
        /// <param name="innerCollection">内部処理用の子ノードコレクション</param>
        protected virtual IEnumerable<TNode> SetupPublicChildCollection(IEnumerable<TNode> innerCollection) {
            return innerCollection.AsReadOnly();
        }

        internal TNode Self => (this as TNode)!;

        /// <summary>追加、削除などのプロセス中に親ノードから呼び出される</summary>
        /// <param name="newParent"></param>
        bool SetParent(TNode? newParent) {
            if (this.Parent == newParent) return false;
            this.Parent = newParent;
            return true;
        }
        /// <summary>ツリー構造をなす上で、指定されたノードが子ノードとして追加可能かどうかを示す。</summary>
        /// <remarks>基底クラスではnullの追加を許容します。</remarks>
        /// <param name="child">追加しようとする子ノード</param>
        /// <returns>Treeの循環、兄弟ノードとの重複をチェックします。</returns>
        protected virtual bool CanAddChildNode([AllowNull] TNode child) {
            if (child == null) return true;
            if (this.Upstream().Any(x => object.ReferenceEquals(x, child))) return false;

            if (this.ChildNodes.Any(x => object.ReferenceEquals(x, child)) && object.ReferenceEquals(child?.Parent, Self)) {
                return false;
            } else if (this.ChildNodes.Any(x => object.ReferenceEquals(x, child)) ^ object.ReferenceEquals(child?.Parent, Self)) {
                if (object.ReferenceEquals(child?.Parent, Self)) {
                    Debug.Assert(false,
                        "再帰構造が正常に機能していない可能性があります。",
                        $"現在のノード{Self}の{nameof(Children)}に指定されたノード{child}は含まれていませんが、指定されたノードは既に現在のノードを親ノードとして設定済みです。");
                } else {
                    Debug.Assert(false,
                        "再帰構造が正常に機能していない可能性があります。",
                        $"現在のノード{Self}の{nameof(Children)}に指定されたノード{child}は既に含まれていますが、指定されたノードは既に現在のノードを親ノードとして設定されていません。");
                }
            }
            return true;
        }

        #region edit processes
        /// <summary>子ノードの差替えを実行するプロセス。</summary>
        /// <remarks><paramref name="action"/> = null で、<see cref="IList{T}"/>へキャストし、インデクサーを使用して要素を差し替える</remarks>
        /// <param name="index">削除されるノードのインデックス</param>
        /// <param name="child">差替えるノード</param>
        /// <param name="action">コレクションの操作を指示。以下、デフォルトの処理<br/>
        /// <code>(collection, index, node)=>((IList&lt;<typeparamref name="TNode"/>&gt;)collection)[index] = node;</code></param>
        /// <exception cref="NotSupportedException"></exception>
        protected virtual void SetChildProcess(int index, TNode child, Action<IEnumerable<TNode>, int, TNode>? action = null) {
            if (!CanAddChildNode(child)) return;
            var bfr = this.childCash();
            action ??= (collection, index, node) => ((IList<TNode>)collection)[index] = node;
            try {
                var p = child?.Parent;
                child?.SetParent(Self);
                p?.RemoveChildProcess(child);
                var rmv = ChildNodes.ElementAt(index);
                if (rmv?.Parent == Self) rmv.SetParent(null);
                action(ChildNodes, index, child);
            }catch(InvalidCastException ex) {
                throw new NotSupportedException($"{nameof(ChildNodes)}において、指定された差替え操作ができません。プロパティ:{nameof(action)}を指定・確認してください。", ex);
            }catch (Exception) {
                ErrAdj(bfr);
                throw;
            }
        }

        /// <summary>インデックスを指定して、子ノードの追加処理を実行するプロセス</summary>
        /// <remarks><paramref name="action"/> = null で、<see cref="IList{T}"/>へキャストして挿入処理を行う。</remarks>
        /// <param name="index">追加先のインデックス</param>
        /// <param name="child">追加する子ノード</param>
        /// <param name="action">コレクションの操作を指示。以下、デフォルトの処理<br/>
        /// <code>(collection, idx, node) => ((IList&lt;<typeparamref name="TNode"/>&gt;)collection).Insert(idx, node);</code></param>
        /// <exception cref="NotSupportedException"></exception>
        protected virtual void InsertChildProcess(int index, TNode child,Action<IEnumerable<TNode>,int,TNode>? action = null) {
            if (!CanAddChildNode(child)) return;
            action ??= (collection, idx, node) => ((IList<TNode>)collection).Insert(idx, node);
            
            var bfr = this.childCash();
            try {
                var p = child?.Parent;
                child?.SetParent(Self);
                p?.RemoveChildProcess(child);
                action(ChildNodes,index, child);
            }catch(InvalidCastException ex) {
                throw new NotSupportedException($"{nameof(ChildNodes)}において、指定された追加操作ができません。プロパティ:{nameof(action)}を指定・確認してください。", ex);
            } catch (Exception) {
                ErrAdj(bfr);
                throw;
            }
        }

        /// <summary>子ノードの削除処理を実行するプロセス。</summary>
        /// <remarks><paramref name="action"/> = null で、<see cref="ICollection{T}"/>へキャストして削除処理を行う。</remarks>
        /// <param name="child">削除する子ノード</param>
        /// <param name="action">コレクションの操作を指示。以下、デフォルトの処理<br/>
        /// <code>(collection, node)=>((ICollection&lt;<typeparamref name="TNode"/>&gt;)collection).Remove(node);</code></param>
        protected virtual void RemoveChildProcess(TNode child,Action<IEnumerable<TNode>,TNode>? action = null) {
            action ??= (collection, node) => ((ICollection<TNode>)collection).Remove(node);
            var bfr = this.childCash();
            try {
                if (child?.Parent == Self) child.SetParent(null);
                action(ChildNodes,child);
            }catch(InvalidCastException ex) {
                throw new NotSupportedException($"{nameof(ChildNodes)}において、指定された削除操作ができません。プロパティ:{nameof(action)}を指定・確認してください。", ex);
            } catch (Exception) {
                ErrAdj(bfr);
                throw;
            }
        }

        /// <summary>クリア処理を実行するプロセス</summary>
        /// <remarks><paramref name="action"/> = null で、<see cref="ICollection{T}"/>へキャストしてクリア処理を行う。</remarks>
        /// <param name="action">コレクションの操作を指示。以下、デフォルトの処理<br/>
        /// <code>(collection)=>((ICollection&lt;<typeparamref name="TNode"/>&gt;)collection).Clear();</code></param>
        /// <exception cref="NotSupportedException"></exception>
        protected virtual void ClearChildProcess(Action<IEnumerable<TNode>>? action = null) {
            action ??= (collection) => ((ICollection<TNode>)collection).Clear();
            var bfr = this.childCash();
            try {
                foreach (var item in this.ChildNodes.OfType<TNode>()) item.SetParent(null);
                action(this.ChildNodes);
            } catch (InvalidCastException ex) {
                throw new NotSupportedException($"{nameof(ChildNodes)}において、指定されたクリア操作ができません。プロパティ:{nameof(action)}を指定・確認してください。", ex);
            } catch (Exception) {
                ErrAdj(bfr);
                throw;
            }
        }

        /// <summary>コレクション内の要素の移動または入替を実行するプロセス</summary>
        /// <remarks><paramref name="action"/> = null で、<see cref="IList{T}"/>へキャストして入替処理を行う。</remarks>
        /// <param name="oldIndex">移動対象となる要素のインデックス</param>
        /// <param name="newIndex">移動先のインデックス</param>
        /// <param name="action">コレクションの操作を指示。以下、デフォルトの処理<br/>
        /// <code>(collection, newidx, oldidx) => {
        ///     var nodes = (IList&lt;<typeparamref name="TNode"/>&gt;)collection;
        ///     var tgt = nodes[oldidx];
        ///     nodes.RemoveAt(oldidx);
        ///     nodes.Insert(newidx, tgt);
        /// };</code></param>
        /// <exception cref="NotSupportedException"></exception>
        protected virtual void ShiftChildProcess(int oldIndex,int newIndex,Action<IEnumerable<TNode>,int,int>? action = null) {
            action ??= (collection, newidx, oldidx) => {
                var nodes = (IList<TNode>)collection;
                var tgt = nodes[oldidx];
                nodes.RemoveAt(oldidx);
                nodes.Insert(newidx, tgt);
            };
            var bfr = this.childCash();
            try {
                action(this.ChildNodes, oldIndex, newIndex);
            } catch (InvalidCastException ex) {
                throw new NotSupportedException($"{nameof(ChildNodes)}において、指定された移動操作ができません。プロパティ:{nameof(action)}を指定・確認してください。", ex);
            }catch (Exception) {
                ErrAdj(bfr);
                throw;
            }
        }
        private TNode[] childCash() {
            return this.ChildNodes.OfType<TNode>().ToArray();
        }
        private void ErrAdj(TNode[] bfr) {
            var aft = childCash();
            var rmdchilds = bfr.Except(aft);
            var addchilds = aft.Except(bfr);
            var immtchilds = aft.Intersect(bfr);
            try {//削除されたノード過去のキャッシュ
                foreach (var rmd in rmdchilds) {
                    //現在の親とSelfが同じであればnullを設定
                    if (rmd.Parent == Self) rmd.SetParent(null);
                }
                foreach (var ads in addchilds) {//追加されたノード
                    if (ads.Parent != null && ads.Parent != Self && ads.Parent.Children.Contains(ads)) {
                        RemoveChildProcess(ads);
                    } else {
                        ads.SetParent(Self);
                    }
                }
                foreach (var ads in immtchilds) {
                    //子ノードの親への参照が変わっていたときの処理
                    if (ads.Parent != null && ads.Parent != Self && ads.Parent.Children.Contains(ads)) {
                        ads.Parent.RemoveChildProcess(ads);
                    } else {
                        ads.SetParent(Self);
                    }
                }
            }catch (Exception ignore) { }
        }

        /// <summary>Disposeを実行するプロセス<para>基底クラスで<see cref="RemoveChildProcess(TNode, Action{IEnumerable{TNode}, TNode}?)"/>が実行されます</para></summary>
        /// <remarks>基底クラスでは、現在のノードを親ノードから切り離し、<see cref="IDisposable"/>を実装している子孫ノードの末柄から順にDisposeメソッドを呼び出す</remarks>
        protected virtual void DisposeProcess() {
            try {
                this.Parent?.RemoveChildProcess(Self);
            } catch { }
            foreach (var cld in this.Levelorder().Skip(1).Reverse().OfType<IDisposable>()) {
                cld.Dispose();
            }
        }
        #endregion
    }
}
