using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public virtual IEnumerable<TNode> Children => _readonlycollection ??= ChildNodes.AsReadOnlyEnumerable();
        /// <summary>子ノードを取得する内部処理用のプロパティ</summary>
        protected abstract IEnumerable<TNode> ChildNodes { get; }

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
        #region edit actions
        /// <summary>子ノードの追加処理。<para><see cref="ChildNodes"/>と追加する子ノートを引数にとる</para></summary>
        /// <remarks>基底クラスでは<see cref="ICollection{TNode}"/>にキャストして処理します</remarks>
        protected virtual Action<IEnumerable<TNode>,TNode> AddAction => (collection, node) => ((ICollection<TNode>)collection).Add(node);

        /// <summary>子ノードの挿入処理。<para><see cref="ChildNodes"/>と挿入する子ノードを引数にとる</para></summary>
        /// <remarks>基底クラスでは<see cref="IList{TNode}"/>にキャストして処理します</remarks>
        protected virtual Action<IEnumerable<TNode>,int,TNode> InsertAction => (collection, index, node) => ((IList<TNode>)collection).Insert(index, node);

        /// <summary>子ノードの削除処理。<para><see cref="ChildNodes"/>と削除するノードを引数にとる</para></summary>
        /// <remarks>基底クラスでは<see cref="ICollection{TNode}"/>にキャストして処理します</remarks>
        protected virtual Action<IEnumerable<TNode>, TNode> RemoveAction => (collection, node) => ((ICollection<TNode>)collection).Remove(node);

        /// <summary>子ノードのクリア処理。<see cref="ChildNodes"/>を引数にとる</summary>
        /// <remarks>基底クラスでは<see cref="ICollection{TNode}"/>にキャストして処理します</remarks>
        protected virtual Action<IEnumerable<TNode>> ClearAction => collection => ((ICollection<TNode>)collection).Clear();

        /// <summary>コレクション内で子ノードの移動処理。<para><see cref="ChildNodes"/>,移動する要素のindex、移動先indexを引数にとる</para></summary>
        /// <remarks>基底クラスでは<see cref="IList{TNode}"/>にキャストして処理します</remarks>
        protected virtual Action<IEnumerable<TNode>,int, int> MoveAction
            => (collection,oldIndex, newIndex) => {
                var nodes = (IList<TNode>)collection;
                var tgt = nodes[oldIndex];
                nodes.RemoveAt(oldIndex);
                nodes.Insert(newIndex, tgt);
            };
        #endregion

        #region edit processes
        /// <summary>子ノードの追加プロセスを実行する。
        /// <para>デフォルトでは<see cref="ChildNodes"/>が<see cref ="ICollection{TNode}"/>へキャストされます。</para>
        /// <para><see cref ="ICollection{TNode}"/>を実装していない場合は<see cref="AddAction"/>をオーバーライドして追加処理に準ずる処理を記述してください。</para>
        /// </summary>
        /// <param name="child">追加する子ノード</param>
        protected virtual void AddChildProcess(TNode child) {
            var bfr = this.childCash();
            try {
                var p = child?.Parent;
                child?.SetParent(Self);
                p?.RemoveChildProcess(child);
                AddAction(ChildNodes,child);
            } catch(InvalidCastException ex) {
                throw new NotSupportedException($"追加処理において指定された操作ができません。プロパティ:{nameof(AddAction)}を指定・確認してください。", ex);
            } catch (Exception) {
                ErrAdj(bfr);
                throw;
            }
        }
        /// <summary>子ノードの挿入処理を実行するプロセス
        /// <para>デフォルトでは<see cref="ChildNodes"/>が<see cref ="IList{TNode}"/>へのキャストされます。</para>
        /// <para><see cref ="IList{TNode}"/>を実装していない場合は<see cref="InsertAction"/>をオーバーライドして挿入処理に準ずる処理を記述してください。</para>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="child">挿入する子ノード</param>
        protected virtual void InsertChildProcess(int index, TNode child) {
            var bfr = this.childCash();
            try {
                var p = child?.Parent;
                child?.SetParent(Self);
                p?.RemoveChildProcess(child);
                InsertAction(ChildNodes,index, child);
            }catch(InvalidCastException ex) {
                throw new NotSupportedException($"挿入処理において指定された操作ができません。プロパティ:{nameof(InsertAction)}を指定・確認してください。", ex);
            } catch (Exception) {
                ErrAdj(bfr);
                throw;
            }
        }
        /// <summary>子ノードの削除処理を実行するプロセス。
        /// <para>デフォルトでは<see cref="ChildNodes"/>が<see cref ="ICollection{TNode}"/>へキャストされます。</para>
        /// <para><see cref ="ICollection{TNode}"/>を実装していない場合は<see cref="RemoveAction"/>をオーバーライドして削除処理に準ずる処理を記述してください。</para></summary>
        /// <param name="child">削除する子ノード</param>
        protected virtual void RemoveChildProcess(TNode child) {
            var bfr = this.childCash();
            try {
                if (child?.Parent == Self) child.SetParent(null);
                RemoveAction(ChildNodes,child);
            }catch(InvalidCastException ex) {
                throw new NotSupportedException($"削除処理において指定された操作ができません。プロパティ:{nameof(RemoveAction)}を指定・確認してください。", ex);
            } catch (Exception) {
                ErrAdj(bfr);
                throw;
            }
        }
        /// <summary>クリア処理を実行するプロセス
        /// <para>デフォルトでは<see cref="ChildNodes"/>が<see cref ="ICollection{TNode}"/>へキャストされます。</para>
        /// <para><see cref ="ICollection{TNode}"/>を実装していない場合は<see cref="ClearAction"/>をオーバーライドしてクリア処理に準ずる処理を記述してください。</para></summary>
        protected virtual void ClearChildProcess() {
            var bfr = this.childCash();
            try {
                foreach (var item in this.ChildNodes.OfType<TNode>()) item.SetParent(null);
                ClearAction(this.ChildNodes);
            } catch (InvalidCastException ex) {
                throw new NotSupportedException($"クリア処理において指定された操作ができません。プロパティ:{nameof(ClearAction)}を指定・確認してください。", ex);
            } catch (Exception) {
                ErrAdj(bfr);
                throw;
            }
        }
        /// <summary>コレクション内の要素の移動を実行するプロセス
        /// <para>デフォルトでは<see cref="ChildNodes"/>が<see cref ="IList{TNode}"/>へのキャストされます。</para>
        /// <para><see cref ="IList{TNode}"/>を実装していない場合は<see cref="InsertAction"/>をオーバーライドして挿入処理に準ずる処理を記述してください。</para></summary>
        /// <param name="oldIndex">移動対象となる要素のインデックス</param>
        /// <param name="newIndex">移動先のインデックス</param>
        protected virtual void MoveChildProcess(int oldIndex,int newIndex) {
            var bfr = this.childCash();
            try {
                MoveAction(this.ChildNodes, oldIndex, newIndex);
            } catch (InvalidCastException ex) {
                throw new NotSupportedException($"移動処理において指定された操作ができません。プロパティ:{nameof(MoveAction)}を指定・確認してください。", ex);
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

        /// <summary>Disposeを実行するプロセス<para>基底クラスで<see cref="RemoveChildProcess(TNode)"/>が実行されます</para></summary>
        /// <remarks>基底クラスでは、現在のノードを親ノードから切り離し、<see cref="IDisposable"/>を実装している子孫ノードの末柄から順にDisposeメソッドを呼び出す</remarks>
        protected virtual void DisposeProcess() {
            try {
                this.Parent?.RemoveChildProcess(Self);
            } catch { }
            foreach (var cld in this.Levelorder().Skip(1).Reverse().OfType<IDisposable>().ToArray()) {
                cld.Dispose();
            }
        }
        #endregion
    }
}
