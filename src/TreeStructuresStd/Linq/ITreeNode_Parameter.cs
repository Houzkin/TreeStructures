using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures.Linq {
    public static partial class TreeNodeExtenstions {

        #region パラメータの取得
        /// <summary>Gets the path code indicating the position of the current node.</summary>
        public static NodeIndex TreeIndex<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            var z = self
                .Upstream()
                .Select(b => b.BranchIndex())
                .TakeWhile(c => 0 <= c)
                .Reverse();
            return new NodeIndex(z);
        }
        /// <summary>Generates the path from the root to the current node.</summary>
        /// <param name="self">Current node</param>
        /// <param name="conv">Specifies the unique value of each node or the value that represents the node.</param>
        public static NodePath<TPath> NodePath<TPath, T>(this ITreeNode<T> self, Converter<T, TPath> conv)
        where T : ITreeNode<T> {
            return NodePath<TPath>.Create((T)self, conv);
        }
        /// <summary>Gets the distance from the current node to the deepest descendant node.</summary>
        public static int Height<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            if (self == null) throw new ArgumentNullException(nameof(self));
            return self.Levelorder().Last().Depth() - self.Depth();
        }
        /// <summary>Gets the depth of the current node from the root.</summary>
        public static int Depth<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            if (self == null) throw new ArgumentNullException(nameof(self));
            return self.Upstream().Count() - 1;
        }
        /// <summary>Gets the index assigned to the current node by its parent node.</summary>
        public static int BranchIndex<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            if (self == null) throw new ArgumentNullException(nameof(self));
            if (self.Parent == null) return -1;
            return self.Parent.Children
                .Select((nd, idx) => new { Node = nd, Index = idx })
                .First(x => object.ReferenceEquals(x.Node, self)).Index;
        }
        /// <summary>Gets the number of nodes at the same depth as the current node in the associated tree structure.</summary>
        /// <remarks>Returns the count of nodes at the same generation.</remarks>
        public static int Width<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            if (self == null) throw new ArgumentNullException(nameof(self));
            return self.Generations().Count();
        }
        #endregion


        #region 判定メソッド
        /// <summary>Gets a value indicating whether the current node is a descendant node of the specified node.</summary>
        /// <param name="self">The current node.</param>
        /// <param name="node">The specified node.</param>
        public static bool IsDescendantOf<T>(this ITreeNode<T> self, T node) where T : ITreeNode<T> {
            return self.Upstream().Skip(1).Contains(node);
        }
        /// <summary>Gets a value indicating whether the current node is an ancestor node of the specified node.</summary>
        /// <param name="self">The current node.</param>
        /// <param name="node">The specified node.</param>
        public static bool IsAncestorOf<T>(this ITreeNode<T> self, T node) where T : ITreeNode<T> {
            return node.Upstream().Skip(1).Contains((T)self);
        }
        /// <summary>Gets a value indicating whether the current node is the current root node.</summary>
        public static bool IsRoot<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return self.Parent == null;
        }
        /// <summary>Gets a value indicating whether there is a next sibling node for the current node.</summary>
        /// <param name="self">The current node.</param>
        /// <param name="predicate">If specified, determines whether a node satisfying the specified condition exists.</param>
        public static bool HasNextSibling<T>(this ITreeNode<T> self, Predicate<T>? predicate = null) where T : ITreeNode<T> {
            predicate = predicate ?? new Predicate<T>(x => true);
            return self.nextSiblings().Any(x => predicate(x));
        }
        /// <summary>Gets a value indicating whether there is a previous sibling node for the current node.</summary>
        /// <param name="self">The current node.</param>
        /// <param name="predicate">If specified, determines whether a node satisfying the specified condition exists.</param>
        public static bool HasPrevoiusSibling<T>(this ITreeNode<T> self, Predicate<T>? predicate = null) where T : ITreeNode<T> {
            predicate = predicate ?? new Predicate<T>(x => true);
            return self.previousSiblings().Any(x => predicate(x));
        }
        /// <summary>Gets a value indicating whether the current node is the last sibling node.</summary>
        public static bool IsLastSibling<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return !self.HasNextSibling();
        }
        /// <summary>Gets a value indicating whether the current node is the first sibling node.</summary>
        public static bool IsFirstSibling<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return !self.HasPrevoiusSibling();
        }
        #endregion
    }
}
