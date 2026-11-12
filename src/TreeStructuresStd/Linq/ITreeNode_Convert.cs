using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using TreeStructures.Collections;

namespace TreeStructures.Linq {
	public static partial class TreeNodeExtenstions {

        #region Convert(other as read-only valued TreeNode)
        /// <summary>Wraps the specified object as a read-only valued tree node.</summary>
        /// <typeparam name="TSrc">The type forming the compoiste pattern object to be wrapped.</typeparam>
        /// <typeparam name="TVal">Type of the value associated with each tree node.</typeparam>
        /// <param name="self">The object to be wrapped as the root node of the tree</param>
        /// <param name="toChildren">Function to retrieve objects to be wrapped as child nodes. Requires implementing <see cref="INotifyCollectionChanged"/> for child nodes to be synchronized.</param>
        /// <param name="toValue">Function to convert source to value.</param>
        /// <returns>The root of wrapping tree</returns>
        public static ReadOnlyValuedTreeNode<TSrc, TVal> AsValuedTreeNode<TSrc, TVal>(this TSrc self, Func<TSrc, IEnumerable<TSrc>> toChildren, Func<TSrc, TVal> toValue)
            where TSrc : class {
            if (self == null) throw new ArgumentNullException(nameof(self));
            if (toChildren == null) throw new ArgumentNullException(nameof(toChildren));
            if (toValue == null) throw new ArgumentNullException(nameof(toValue));
            return new ReadOnlyValuedTreeNode<TSrc, TVal>(self, toChildren, toValue);
        }
        /// <summary>
        /// Wraps an existing tree node and its descendants into a read-only valued tree node.
        /// </summary>
        /// <remarks>For proper synchronization of descendant nodes, the child node collection must implement <see cref="INotifyCollectionChanged"/>.</remarks>
        /// <typeparam name="TNode">Type of the original tree node.</typeparam>
        /// <typeparam name="TVal">Type of the value associated with each tree node.</typeparam>
        /// <param name="self">The original tree node to convert.</param>
        /// <param name="toValue">Function to retrieve the value associated with each tree node.</param>
        /// <returns>A read-only valued tree node representing the original tree and its hierarchy.</returns>
        public static ReadOnlyValuedTreeNode<TNode, TVal> AsValuedTreeNode<TNode, TVal>(this ITreeNode<TNode> self, Func<TNode, TVal> toValue) where TNode : class, ITreeNode<TNode> {
            return ReadOnlyValuedTreeNode<TNode, TVal>.Create((self as TNode)!, toValue);
        }
        #endregion

        #region Convert(TreeNode as other TreeNode or Hierarchy)
        private static U convert<T, U>(ITreeNode<T> self, Func<T, U> generator, Action<U, U, int> addAction)
        where T : ITreeNode<T> {
            if (generator == null) throw new ArgumentNullException(nameof(generator));
            if (addAction == null) throw new ArgumentNullException(nameof(addAction));

            var scroller = self.LevelOrder().Select(x => Tuple.Create(x, generator(x))).ToListScroller();
            scroller.MoveForEach(ele => {
                scroller
                    .TryPrevious(x => x.Item1.Children.Contains(ele.Item1))
                    .When(r => addAction(r.Current.Item2, ele.Item2,ele.Item1.BranchIndex()));
            });
            return scroller.First().Current.Item2;
        }
        /// <summary>Reassembles a structure with the same hierarchy as the tree starting from the current node, converting the type of each node.</summary>
        /// <typeparam name="T">Type before conversion.</typeparam>
        /// <typeparam name="U">Type after conversion.</typeparam>
        /// <param name="self">Current node.</param>
        /// <param name="generator">Node transformation function applied to each node.</param>
        /// <returns>The reconstructed structure with the converted node types.</returns>
        public static U Convert<T, U>(this ITreeNode<T> self, Func<T, U> generator)
            where T : ITreeNode<T>
            where U : IMutableTreeNode<U> {
            return Convert(self, generator, (p, c, i) => p.AddChild(c));
        }

        /// <summary>Reassembles a structure with the same hierarchy as the tree starting from the current node, converting the type of each node.</summary>
        /// <typeparam name="T">Type before conversion.</typeparam>
        /// <typeparam name="U">Type after conversion.</typeparam>
        /// <param name="self">Current node.</param>
        /// <param name="generator">Node transformation function applied to each node.</param>
        /// <param name="addAction">Function that establishes the parent-child relationship with the first parameter being the parent object, the second parameter being the child object, and the third parameter being the BranchIndex.</param>
        /// <returns>The reconstructed structure with the converted node types.</returns>
        public static U Convert<T, U>(this ITreeNode<T> self, Func<T, U> generator, Action<U, U, int> addAction)
            where T : ITreeNode<T> {
            return convert(self, generator, addAction);
        }

        /// <summary>
        /// Reassembles a structure with the same hierarchy as the tree starting from the current node, converting the type of each node.
        /// </summary>
        /// <typeparam name="T">Type before conversion.</typeparam>
        /// <typeparam name="U">Type after conversion.</typeparam>
        /// <param name="self">Current node.</param>
        /// <param name="generator">Node transformation function applied to each node.</param>
        /// <param name="addAction">Function that establishes the parent-child relationship with the first parameter being the parent object and the second parameter being the child object.</param>
        /// <returns>The reconstructed structure with the converted node types.</returns>
        public static U Convert<T, U>(this ITreeNode<T> self, Func<T, U> generator, Action<U, U> addAction)
            where T : ITreeNode<T> {
            return convert(self, generator, (p, c, i) => addAction(p, c));
        }

		#endregion
	}
}
