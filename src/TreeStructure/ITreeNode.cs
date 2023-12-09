using System.Collections.ObjectModel;
using System.Collections.Specialized;
using TreeStructure.EventManager;
namespace TreeStructure {

    /// <summary>
    /// ツリー構造を提供する。
    /// </summary>
    /// <typeparam name="TNode">各ノードの型</typeparam>
    public interface ITreeNode<TNode> where TNode : ITreeNode<TNode> {
        /// <summary>親ノードを取得する。</summary>
        TNode? Parent { get; }
        /// <summary>子ノードを取得する。</summary>
        IEnumerable<TNode> Children { get; }
    }

    /// <summary>子ノードをコレクションとして管理するツリー構造を定義する。</summary>
    /// <typeparam name="TNode">ノードの型</typeparam>
    public interface ITreeNodeCollection<TNode> : ITreeNode<TNode> where TNode : ITreeNodeCollection<TNode> {
        /// <summary>子ノードを追加する。</summary>
        /// <param name="child">子ノード</param>
        /// <returns>現在のノード</returns>
        TNode AddChild(TNode child);
        /// <summary>指定されたインデックスの位置に子ノードを追加する。</summary>
        /// <param name="index">インデックス</param>
        /// <param name="child">子ノード</param>
        /// <returns>現在のノード</returns>
        TNode InsertChild(int index, TNode child);
        /// <summary>子ノードを削除する。</summary>
        /// <param name="child">削除する子ノード</param>
        /// <returns>削除されたノード</returns>
        TNode RemoveChild(TNode child);
        /// <summary>子ノードを全て削除する。</summary>
        /// <returns>削除されたノード</returns>
        IReadOnlyList<TNode> ClearChildren();
    }
    /// <summary>
    /// 観測可能なツリー構造を表す。
    /// </summary>
    /// <typeparam name="TNode">ノードの型</typeparam>
    public interface IObservableTreeNode<TNode>: ITreeNode<TNode> 
        where TNode : IObservableTreeNode<TNode>{
        /// <summary>ツリー構造変更時、<see cref="StructureChangedEventManager{TNode}"/>によって呼び出されます。</summary>
        /// <param name="e"></param>
        void OnStructureChanged(StructureChangedEventArgs<TNode> e);
        /// <summary>
        /// ツリー構造が変化したときの通知イベント
        /// </summary>
        event EventHandler<StructureChangedEventArgs<TNode>>? StructureChanged;
    }
}