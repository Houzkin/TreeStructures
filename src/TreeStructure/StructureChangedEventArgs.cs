using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures {


    /// <summary>祖先ノードが追加または削除されたときのイベントにデータを提供する。</summary>
    /// <typeparam name="TNode">ノードの型</typeparam>
    [Serializable]
    public sealed class ChangedAncestorInfo<TNode> {
        /// <summary>変更前の祖先ノード。</summary>
        public TNode? PreviousParentOfTarget { get; private set; }
        /// <summary>移動した祖先ノード。</summary>
        public TNode MovedTarget { get; private set; }
        /// <summary>移動前に振り当てられていたインデックス。<para>親ノードが存在しなかった場合は -1。</para></summary>
        public int OldIndex { get; private set; }
        /// <summary>ルートが変更したかどうかを示す値を取得する。</summary>
        public bool RootWasChanged { get; private set; }
        /// <summary>新しいインスタンスを初期化する。</summary>
        /// <param name="movedTarget">移動したノード</param>
        /// <param name="previous">移動元ノード</param>
        /// <param name="oldIndex">移動前に振り当てられていたインデックス。</param>
        /// <param name="rootChanged">ルートが変化したかどうかを示す値を設定する。</param>
        public ChangedAncestorInfo(TNode movedTarget, TNode? previous, int oldIndex, bool rootChanged) {
            this.PreviousParentOfTarget = previous;
            this.MovedTarget = movedTarget;
            this.OldIndex = oldIndex;
            this.RootWasChanged = rootChanged;
        }
    }

    /// <summary>子孫ノードが移動したときのイベントにデータを提供する。</summary>
    /// <typeparam name="TNode">ノードの型</typeparam>
    [Serializable]
    public sealed class ChangedDescendantInfo<TNode> {
        /// <summary>新しいインスタンスを初期化する。</summary>
        /// <param name="action">イベントの原因となったアクション。</param>
        /// <param name="target">アクションの対象となったノード。</param>
        /// <param name="previousParent">対象の移動前の親ノード。</param>
        /// <param name="oldIndex">移動前に振り当てられていたインデックス。</param>
        public ChangedDescendantInfo(TreeNodeChangedAction action, TNode target, TNode? previousParent, int oldIndex) {
            this.NodeAction = action;
            this.Target = target;
            this.PreviousParentOfTarget = previousParent;
            this.OldIndex = oldIndex;
        }
        /// <summary>各ノードにおける、イベントの原因となったアクションを取得する。</summary>
        public TreeNodeChangedAction NodeAction { get; private set; }
        /// <summary>アクションの対象となったノードを取得する。</summary>
        public TNode Target { get; private set; }
        /// <summary>対象の、変更前の親ノードを取得する。</summary>
        public TNode? PreviousParentOfTarget { get; private set; }
        /// <summary>対象が移動前に振り当てられていたインデックス。<para>親ノードが存在しなかった場合は -1。</para></summary>
        public int OldIndex { get; private set; }
    }
    /// <summary>再帰構造が変更されたときのイベントにデータを提供する。</summary>
    /// <typeparam name="TNode">ノードの型</typeparam>
    [Serializable]
    public sealed class StructureChangedEventArgs<TNode> : EventArgs {
        /// <summary>新規インスタンスを初期化する。</summary>
        /// <param name="action">イベントの原因となったアクション。</param>
        /// <param name="target">アクションの対象となったノード。</param>
        /// <param name="previousParent">対象の移動前の親ノード。</param>
        /// <param name="oldIndex">対象が移動前に振り当てられていたインデックス。</param>
        public StructureChangedEventArgs(TreeNodeChangedAction action, TNode target, TNode? previousParent, int oldIndex) {
            TreeAction = action;
            Target = target;
            PreviousParentOfTarget = previousParent;
            OldIndex = oldIndex;
        }
        /// <summary>現在の再帰構造において、イベントの原因となったアクション。</summary>
        public TreeNodeChangedAction TreeAction { get; private set; }
        /// <summary>アクションの対象となったノード。</summary>
        public TNode Target { get; private set; }
        /// <summary>対象ノードの変更前の親ノード。</summary>
        public TNode? PreviousParentOfTarget { get; private set; }
        /// <summary>対象が移動前に振り当てられていたインデックス。<para>親ノードが存在しなかった場合は -1。</para></summary>
        public int OldIndex { get; private set; }
        /// <summary>祖先方向に変更があったかどうかを示す値を取得する。</summary>
        public bool AncestorWasChanged {
            get { return AncestorInfo != null; }
        }
        /// <summary>子孫方向に変更があったかどうかを示す値を取得する。</summary>
        public bool DescendantWasChanged {
            get { return DescendantInfo != null; }
        }
        /// <summary>各ノードにおいて、子孫方向に変更があった場合、その情報を示す参照が適宜設定される。</summary>
        public ChangedDescendantInfo<TNode>? DescendantInfo { get; internal set; }
        /// <summary>各ノードにおいて、子孫方向に変更があった場合、その情報を示す参照が適宜設定される。</summary>
        public ChangedAncestorInfo<TNode>? AncestorInfo { get; internal set; }
    }
    /// <summary>特定のノードから広がるツリー構造がどのように変更されたかを示す。</summary>
    public enum TreeNodeChangedAction {
        /// <summary>追加された。</summary>
        Join,
        /// <summary>特定のノードから広がるツリー構造内を移動した。</summary>
        Move,
        /// <summary>特定のノードから広がるツリー構造から外れた。</summary>
        Deviate,
    }
}
