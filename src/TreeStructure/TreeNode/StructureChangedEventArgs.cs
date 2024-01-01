using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures {


    /// <summary>Provides data for events when ancestor nodes are added or removed.</summary>
    /// <typeparam name="TNode">The type of nodes.</typeparam>
    [Serializable]
    public sealed class ChangedAncestorInfo<TNode> {
        /// <summary>Gets the ancestor node before the change.</summary>
        public TNode? PreviousParentOfTarget { get; private set; }
        /// <summary>Gets the moved ancestor node.</summary>
        public TNode MovedTarget { get; private set; }
        /// <summary>Gets the index assigned before the move. <para>If there was no parent node, it is -1.</para></summary>
        public int OldIndex { get; private set; }
        /// <summary>Gets a value indicating whether the root has changed.</summary>
        public bool IsRootChanged { get; private set; }
        /// <summary>Initializes a new instance of the <see cref="ChangedAncestorInfo{TNode}"/> class.</summary>
        /// <param name="movedTarget">The moved node.</param>
        /// <param name="previous">The node from which it was moved.</param>
        /// <param name="oldIndex">The index assigned before the move.</param>
        /// <param name="rootChanged">A value indicating whether the root has changed.</param>
        public ChangedAncestorInfo(TNode movedTarget, TNode? previous, int oldIndex, bool rootChanged) {
            this.PreviousParentOfTarget = previous;
            this.MovedTarget = movedTarget;
            this.OldIndex = oldIndex;
            this.IsRootChanged = rootChanged;
        }
    }


    /// <summary>Provides data for events when descendant nodes are moved.</summary>
    /// <typeparam name="TNode">Type of the node.</typeparam>
    [Serializable]
    public sealed class ChangedDescendantInfo<TNode> {
        /// <summary>Initializes a new instance of the class.</summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="target">The node that is the target of the action.</param>
        /// <param name="previousParent">The previous parent of the target before the move.</param>
        /// <param name="oldIndex">The index assigned to the target before the move, or -1 if no parent existed.</param>
        public ChangedDescendantInfo(TreeNodeChangedAction action, TNode target, TNode? previousParent, int oldIndex) {
            this.SubTreeAction = action;
            this.Target = target;
            this.PreviousParentOfTarget = previousParent;
            this.OldIndex = oldIndex;
        }
        /// <summary>Gets the action that caused the event for each node.</summary>
        public TreeNodeChangedAction SubTreeAction { get; private set; }
        /// <summary>Gets the node that is the target of the action.</summary>
        public TNode Target { get; private set; }
        /// <summary>Gets the previous parent of the target node before the move.</summary>
        public TNode? PreviousParentOfTarget { get; private set; }
        /// <summary>Gets the index assigned to the target node before the move. Returns -1 if no parent existed.</summary>
        public int OldIndex { get; private set; }
    }

    /// <summary>Provides data for events when the structure of the tree changes.</summary>
    /// <typeparam name="TNode">Type of the node.</typeparam>
    [Serializable]
    public sealed class StructureChangedEventArgs<TNode> : EventArgs {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="target">The node that is the target of the action.</param>
        /// <param name="previousParent">The previous parent of the target node before the change.</param>
        /// <param name="oldIndex">The index assigned to the target node before the change.</param>
        public StructureChangedEventArgs(TreeNodeChangedAction action, TNode target, TNode? previousParent, int oldIndex) {
            TreeAction = action;
            Target = target;
            PreviousParentOfTarget = previousParent;
            OldIndex = oldIndex;
        }

        /// <summary>Gets the action that caused the event in the current recursive structure.</summary>
        public TreeNodeChangedAction TreeAction { get; private set; }

        /// <summary>Gets the node that is the target of the action.</summary>
        public TNode Target { get; private set; }

        /// <summary>Gets the previous parent of the target node before the change.</summary>
        public TNode? PreviousParentOfTarget { get; private set; }

        /// <summary>Gets the index assigned to the target node before the change. Returns -1 if no parent existed.</summary>
        public int OldIndex { get; private set; }

        /// <summary>Gets a value indicating whether there was a change in the ancestor direction.</summary>
        public bool IsAncestorChanged {
            get { return AncestorInfo != null; }
        }

        /// <summary>Gets a value indicating whether there was a change in the descendant direction.</summary>
        public bool IsDescendantChanged {
            get { return DescendantInfo != null; }
        }

        /// <summary>For each node, if there was a change in the descendant direction, a reference indicating that information is appropriately set.</summary>
        public ChangedDescendantInfo<TNode>? DescendantInfo { get; internal set; }

        /// <summary>For each node, if there was a change in the ancestor direction, a reference indicating that information is appropriately set.</summary>
        public ChangedAncestorInfo<TNode>? AncestorInfo { get; internal set; }
    }

    /// <summary>Indicates how the tree structure, spreading from a specific node, has been modified.</summary>
    public enum TreeNodeChangedAction {
        /// <summary>Added within the tree structure spreading from a specific node.</summary>
        Join,
        /// <summary>Moved within the tree structure spreading from a specific node.</summary>
        Move,
        /// <summary>Removed from the tree structure spreading from a specific node.</summary>
        Deviate,
    }
}
