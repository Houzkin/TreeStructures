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
            var v = self.Levelorder().Select(x => Tuple.Create(x, generator(x))).ToSequenceScroller();
            foreach(var tr in v.GetSequence()){
                v.MoveTo(tr)
                    .TryPrevious(x => x.Item1.Children.Contains(tr.Item1))
                    .When(r => addAction(tr.Item1.BranchIndex(), r.Current.Item2, tr.Item2));
            }
            return v.First().Current.Item2;
            //var vst = self.Postorder()
            //    .Select(x => Tuple.Create(x, generator(x)))
            //    .ToSequenceScroller();
            //foreach (var tr in vst.GetSequence()) {
            //    vst.MoveTo(tr)
            //        .TryNext(x => x.Item1.Children.Contains(tr.Item1))
            //        .When(r => addAction(tr.Item1.BranchIndex(), r.Current.Item2, tr.Item2));
            //}
            //return vst.Current.Item2;
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

		#endregion

		#region NodePathから組立

		/// <summary>
		/// Assembles a tree structure from a dictionary where the keys represent paths in the hierarchy.
		/// </summary>
		/// <typeparam name="U">The type of the data to convert.</typeparam>
		/// <typeparam name="UPath">The type of the elements in the path.</typeparam>
		/// <typeparam name="T">The type of the resulting tree node.</typeparam>
		/// <param name="dic">The dictionary containing paths as keys and data as values.</param>
		/// <param name="conv">A function to convert the data into the tree node type.</param>
		/// <param name="addaction">An action that defines how to establish a parent-child relationship between tree nodes.</param>
		/// <returns>The root node of the assembled tree.</returns>
		public static T AssembleTreeByPath<U, UPath, T>(this IDictionary<NodePath<UPath>, U> dic,Func<U,T> conv,Action<T,T> addaction) { 
            var ps = dic.Where(x=> 0 <= x.Key.Depth).Select(x => Tuple.Create(x.Key, conv(x.Value))).OrderBy(x => x.Item1.Depth).ToSequenceScroller();
            foreach(var k in ps.GetSequence().Skip(1)){
                ps.MoveTo(k)
                    .TryPrevious(x => x.Item1.SequenceEqual(k.Item1.SkipLast(1)))
                    .When(o => addaction(o.Current.Item2, k.Item2));
            }
            return ps.First().Current.Item2;
        }
		/// <summary>
		/// Assembles a tree structure from a dictionary where the keys represent paths in the hierarchy.
		/// </summary>
		/// <typeparam name="TPath">The type of the elements in the path.</typeparam>
		/// <typeparam name="T">The type of the tree node.</typeparam>
		/// <param name="dic">The dictionary containing paths as keys and tree nodes as values.</param>
		/// <param name="addaction">An action that defines how to establish a parent-child relationship between tree nodes.</param>
		/// <returns>The root node of the assembled tree.</returns>
		public static T AssembleTreeByPath<TPath,T>(this IDictionary<NodePath<TPath>,T> dic,Action<T,T> addaction){
            return AssembleTreeByPath(dic, x => x, addaction);
        }
		/// <summary>
		/// Assembles a tree structure from a dictionary where the keys represent paths in the hierarchy.
		/// </summary>
		/// <typeparam name="TPath">The type of the elements in the path.</typeparam>
		/// <typeparam name="T">The type of the tree node. Must implement <see cref="IMutableTreeNode{T}"/>.</typeparam>
		/// <param name="dic">The dictionary containing paths as keys and tree nodes as values.</param>
		/// <returns>The root node of the assembled tree.</returns>
		public static T AssembleTreeByPath<TPath,T>(this IDictionary<NodePath<TPath>,T> dic) where T : IMutableTreeNode<T>{
            return AssembleTreeByPath(dic,x=>x,(p,c)=>p.AddChild(c));
        }
		/// <summary>
		/// Assembles a tree structure from an enumerable of paths, with a conversion function for creating tree nodes.
		/// </summary>
		/// <typeparam name="TPath">The type of the elements in the path.</typeparam>
		/// <typeparam name="T">The type of the resulting tree node.</typeparam>
		/// <param name="self">The enumerable of paths representing the hierarchy.</param>
		/// <param name="conv">A function to convert each path into a tree node.</param>
		/// <param name="addaction">An action that defines how to establish a parent-child relationship between tree nodes.</param>
		/// <returns>The root node of the assembled tree.</returns>
		public static T AssembleTreeByPath<TPath,T>(this IEnumerable<NodePath<TPath>> self, Func<NodePath<TPath>,T> conv,Action<T,T> addaction){
            return AssembleTreeByPath(self.ToDictionary(x => x, y => conv(y)),x=>x,addaction);
        }
		/// <summary>
		/// Assembles a tree structure from an enumerable of paths, assuming the tree node type supports mutable relationships.
		/// </summary>
		/// <typeparam name="TPath">The type of the elements in the path.</typeparam>
		/// <typeparam name="T">The type of the tree node. Must implement <see cref="IMutableTreeNode{T}"/>.</typeparam>
		/// <param name="self">The enumerable of paths representing the hierarchy.</param>
		/// <param name="conv">A function to convert each path into a tree node.</param>
		/// <returns>The root node of the assembled tree.</returns>
		public static T AssembleTreeByPath<TPath,T>(this IEnumerable<NodePath<TPath>> self,Func<NodePath<TPath>,T> conv) where T : IMutableTreeNode<T>{
            return AssembleTreeByPath(self.ToDictionary(x => x, y => conv(y)), x => x, (p, c) => p.AddChild(c));
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
