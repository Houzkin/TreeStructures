using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Collections;
using System.Collections.Specialized;

namespace TreeStructures.Linq {

    // ツリー構造の組み立てを行うメソッドを提供する。
    public static partial class TreeNodeExtenstions {

        #region ツリーから別ツリーへ変換
        private static U convert<T, U>(ITreeNode<T> self, Func<T, U> generator, Action<int, U, U> addAction)
        where T : ITreeNode<T> {
            if (generator == null) throw new ArgumentNullException(nameof(generator));
            if (addAction == null) throw new ArgumentNullException(nameof(addAction));
            var vst = self.Postorder()
                .Select(x => Tuple.Create(x, generator(x)))
                .ToSequenceScroller();//new ElementScroller<Tuple<T, U>>(t);
            foreach (var tr in vst.GetSequence()) {
                vst.MoveTo(tr)
                    .TryNext(x => x.Item1.Children.Contains(tr.Item1))
                    .When(r => addAction(tr.Item1.BranchIndex(), r.Current.Item2, tr.Item2));
            }
            return vst.Current.Item2;
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
            return Convert(self, generator, (i, p, c) => p.AddChild(c));
        }

        /// <summary>Reassembles a structure with the same hierarchy as the tree starting from the current node, converting the type of each node.</summary>
        /// <typeparam name="T">Type before conversion.</typeparam>
        /// <typeparam name="U">Type after conversion.</typeparam>
        /// <param name="self">Current node.</param>
        /// <param name="generator">Node transformation function applied to each node.</param>
        /// <param name="addAction">Function that establishes the parent-child relationship with the first parameter being the BranchIndex, the second parameter being the parent object, and the third parameter being the child object.</param>
        /// <returns>The reconstructed structure with the converted node types.</returns>
        public static U Convert<T, U>(this ITreeNode<T> self, Func<T, U> generator, Action<int, U, U> addAction)
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
            return convert(self, generator, (i, p, c) => addAction(p, c));
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
        public static ReadOnlyValuedTreeNode<TNode,TVal> AsValuedTreeNode<TNode,TVal>(this ITreeNode<TNode> self,Func<TNode,TVal> toValue)where TNode :class, ITreeNode<TNode> {
            return ReadOnlyValuedTreeNode<TNode,TVal>.Create((self as TNode)!, toValue);
        }
        
        #endregion

        #region NodeIndexから組み立て
        private static T _assemble<T>(IEnumerable<Tuple<NodeIndex, T>> dic, Action<int, T, T> addAction) {
            //var seq = dic.OrderBy(x => x.Item1, TreeStructure.NodeIndex.GetPostorderComparer());
            var vst = dic.OrderBy(x => x.Item1, TreeStructures.NodeIndex.GetPostorderComparer())
                .ToSequenceScroller();// new ElementScroller<Tuple<NodeIndex, T>>(seq);
            foreach (var tr in vst.GetSequence()) {
                vst.MoveTo(tr)
                    .TryNext(x => tr.Item1.Depth > x.Item1.Depth)
                    .When(r => addAction(tr.Item1.LastOrDefault(), r.Current.Item2, tr.Item2));
            }
            return vst.Current.Item2;
        }
        private static U assemble<U,TKey, T>(IDictionary<TKey, T> dictionary, Func<T, U> conv, Action<int, U, U> addAction) where TKey: IEnumerable<int> {
            var seq = dictionary.Select(x => Tuple.Create(new NodeIndex(x.Key), conv(x.Value)));
            return _assemble(seq, addAction);
        }
        /// <summary>
        /// Assembles each node based on the index indicated by the key.
        /// </summary>
        /// <typeparam name="T">Type of the node.</typeparam>
        /// <typeparam name="TKey">Type of IEnumerable with elements of type int representing node indices.</typeparam>
        /// <param name="self">Dictionary where keys indicate node indices.</param>
        /// <returns>The assembled tree structure.</returns>
        public static T AssembleTree<TKey,T>(this IDictionary<TKey, T> self) where T : IMutableTreeNode<T> where TKey: IEnumerable<int> {
            return assemble(self, x => x, (i, p, c) => p.AddChild(c));
        }
        /// <summary>
        /// Assembles each data based on the index indicated by the key.
        /// </summary>
        /// <typeparam name="TKey">Type of IEnumerable with elements of type int representing node indices.</typeparam>
        /// <typeparam name="T">Type of the data.</typeparam>
        /// <param name="self">Current object.</param>
        /// <param name="addAction">Function that establishes the parent-child relationship with the first parameter being the BranchIndex, the second parameter being the parent object, and the third parameter being the child object.</param>
        /// <returns>The assembled tree structure.</returns>
        public static T AssembleTree<TKey,T>(this IDictionary<TKey, T> self, Action<int,T, T> addAction)where TKey : IEnumerable<int> {
            return assemble(self, x => x, addAction);
        }
        /// <summary>
        /// Assembles each data based on the index indicated by the key.
        /// </summary>
        /// <typeparam name="TKey">Type of IEnumerable with elements of type int representing node indices.</typeparam>
        /// <typeparam name="T">Type of the data.</typeparam>
        /// <param name="self">Current object.</param>
        /// <param name="addAction">Function that establishes the parent-child relationship with the first parameter being the parent object, and the second parameter being the child object.</param>
        /// <returns>The assembled tree structure.</returns>
        public static T AssembleTree<TKey, T>(this IDictionary<TKey, T> self, Action<T, T> addAction) where TKey : IEnumerable<int> {
            return assemble(self, x => x, (i, p, c) => addAction(p, c));
        }
        /// <summary>
        /// Generates and assembles nodes from data based on indices indicating the hierarchy.
        /// </summary>
        /// <typeparam name="TKey">Type of IEnumerable with elements of type int representing node indices.</typeparam>
        /// <typeparam name="T">Type of the data.</typeparam>
        /// <typeparam name="U">Type of the node.</typeparam>
        /// <param name="self">Current object.</param>
        /// <param name="conv">Node conversion function applied to each data.</param>
        /// <returns>The assembled tree structure.</returns>
        public static U AssembleTree<TKey,T, U>(this IDictionary<TKey, T> self, Func<T, U> conv)
        where TKey : IEnumerable<int>
        where U : IMutableTreeNode<U> {
            return assemble(self, conv, (i, p, c) => p.AddChild(c));
        }
        /// <summary>
        /// Generates and assembles nodes from data based on indices indicating the hierarchy, performing conversion and assembly for each data.
        /// </summary>
        /// <typeparam name="TKey">Type of IEnumerable with elements of type int representing node indices.</typeparam>
        /// <typeparam name="T">Type of the data.</typeparam>
        /// <typeparam name="U">Type to convert to.</typeparam>
        /// <param name="self">Current object.</param>
        /// <param name="conv">Conversion function.</param>
        /// <param name="addAction">Function that establishes the parent-child relationship with the first parameter being the BranchIndex, the second parameter being the parent object, and the third parameter being the child object.</param>
        /// <returns>The assembled tree structure.</returns>
        public static U AssembleTree<TKey,T, U>(this IDictionary<TKey, T> self, Func<T, U> conv, Action<int, U, U> addAction) where TKey:IEnumerable<int> {
            return assemble(self, conv, addAction);
        }
        /// <summary>
        /// Generates and assembles nodes from data based on indices indicating the hierarchy, performing conversion and assembly for each data.
        /// </summary>
        /// <typeparam name="TKey">Type of IEnumerable with elements of type int representing node indices.</typeparam>
        /// <typeparam name="T">Type of the data.</typeparam>
        /// <typeparam name="U">Type to convert to.</typeparam>
        /// <param name="self">Current object.</param>
        /// <param name="conv">Conversion function.</param>
        /// <param name="addAction">Function that establishes the parent-child relationship with the first parameter being the parent object, and the second parameter being the child object.</param>
        /// <returns>The assembled tree structure.</returns>
        public static U AssembleTree<TKey, T, U>(this IDictionary<TKey, T> self, Func<T, U> conv, Action<U, U> addAction) where TKey : IEnumerable<int> {
            return assemble(self, conv, (i, p, c) => addAction(p, c));
        }
        #endregion

        #region IEnumerableからN分木を生成
        /// <summary>
        /// Creates an N-ary tree starting from the first element.
        /// </summary>
        /// <remarks>
        /// Each node is added in level order.
        /// </remarks>
        /// <typeparam name="T">Type of the elements.</typeparam>
        /// <typeparam name="U">Type of the nodes.</typeparam>
        /// <param name="self">Current object.</param>
        /// <param name="nary">Upper limit on the number of child nodes that a parent node can have.</param>
        /// <param name="conv">Function to convert elements to nodes.</param>
        /// <param name="addAction">Function that establishes the parent-child relationship with the first parameter being the parent object, and the second parameter being the child object.</param>
        /// <returns>The node converted from the first element that serves as the root.</returns>
        public static U AssembleAsNAryTree<T, U>(this IEnumerable<T> self, int nary, Func<T, U> conv, Action<U, U> addAction) where U : ITreeNode<U> {
            if (self == null) throw new ArgumentNullException(nameof(self));
            if (!self.Any()) throw new InvalidOperationException(nameof(self));
            var nds = self.Select(a => conv(a)).SkipWhile(a=>a==null);
            U? root = nds.FirstOrDefault();
            if (root == null) throw new InvalidOperationException(nameof(conv));
            Queue<U> items = new Queue<U>(nds.Skip(1));//ルート以外のノードを格納
            Queue<U> queue = new Queue<U>();//追加待ちのノード
            queue.Enqueue(root);
            while (items.Any() && queue.Any()) {
                int cnt = 0;
                var tgt = queue.Dequeue();
                while (tgt != null && cnt < nary && items.Any()) {
                    var item = items.Dequeue();
                    addAction(tgt, item);
                    if (item != null && tgt.Children.Contains(item)) {
                        queue.Enqueue(item);
                        cnt++;
                    }
                }
            }
            return root;
        }
        /// <summary>
        /// Creates an N-ary tree starting from the first element.
        /// </summary>
        /// <remarks>
        /// Each node is added in level order.
        /// </remarks>
        /// <typeparam name="T">Type of the elements.</typeparam>
        /// <typeparam name="U">Type of the nodes.</typeparam>
        /// <param name="self">Current object.</param>
        /// <param name="nary">Upper limit on the number of child nodes that a parent node can have.</param>
        /// <param name="conv">Function to convert elements to nodes.</param>
        /// <returns>The node converted from the first element that serves as the root.</returns>
        public static U AssembleAsNAryTree<T,U>(this IEnumerable<T> self,int nary,Func<T,U> conv) where U : IMutableTreeNode<U> {
            return self.AssembleAsNAryTree(nary, conv, (a, b) => a.AddChild(b));
        }

        /// <summary>
        /// Creates an N-ary tree starting from the first element.
        /// </summary>
        /// <remarks>
        /// Each node is added in level order.
        /// </remarks>
        /// <typeparam name="T">Type of the nodes.</typeparam>
        /// <param name="self">Current object.</param>
        /// <param name="nary">Upper limit on the number of child nodes that a parent node can have.</param>
        /// <returns>The root node.</returns>
        public static T AssembleAsNAryTree<T>(this IEnumerable<T> self,int nary) where T : IMutableTreeNode<T> {
            return self.AssembleAsNAryTree(nary, a => a)!;
        }
        #endregion
    }
}
