using System.Collections.ObjectModel;
using System.Collections.Specialized;
using TreeStructures.EventManagement;
namespace TreeStructures {

    /// <summary>Provides a tree structure.</summary>
    /// <typeparam name="TNode">The type of each node.</typeparam>
    public interface ITreeNode<TNode> where TNode : ITreeNode<TNode> {
        /// <summary>Gets the parent node.</summary>
        TNode? Parent { get; }
        /// <summary>Gets the child nodes.</summary>
        IEnumerable<TNode> Children { get; }
    }

    /// <summary>Defines a mutable tree structure.</summary>
    /// <typeparam name="TNode">The type of the nodes.</typeparam>
    public interface IMutableTreeNode<TNode> : ITreeNode<TNode>
        where TNode : IMutableTreeNode<TNode> {
        /// <summary>Adds a child node.</summary>
        /// <param name="child">The child node.</param>
        /// <returns>The current node.</returns>
        TNode AddChild(TNode child);
        /// <summary>Adds a child node at the specified index.</summary>
        /// <param name="index">The index.</param>
        /// <param name="child">The child node.</param>
        /// <returns>The current node.</returns>
        TNode InsertChild(int index, TNode child);
        /// <summary>Removes a child node.</summary>
        /// <param name="child">The child node to remove.</param>
        /// <returns>The removed node.</returns>
        TNode RemoveChild(TNode child);
        /// <summary>Removes all child nodes.</summary>
        /// <returns>The removed nodes.</returns>
        IReadOnlyList<TNode> ClearChildren();
    }
    /// <summary>Represents an observable tree structure.</summary>
    /// <typeparam name="TNode">The type of the nodes.</typeparam>
    public interface IObservableTreeNode<TNode> : ITreeNode<TNode>
        where TNode : IObservableTreeNode<TNode> {
        /// <summary>Called by <see cref="StructureChangedEventExecutor{TNode}"/> when the tree structure changes.</summary>
        /// <param name="e"></param>
        void OnStructureChanged(StructureChangedEventArgs<TNode> e);
        /// <summary>
        /// Notification event for when the tree structure changes.
        /// </summary>
        event EventHandler<StructureChangedEventArgs<TNode>>? StructureChanged;
    }

}