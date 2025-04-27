﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Collections;
using TreeStructures;
using System.Collections.Specialized;

namespace TreeStructures.Linq {

    // ツリー構造の組み立てを行うメソッドを提供する。
    public static partial class TreeNodeExtenstions {

        #region ツリーから別ツリーへ変換
        private static U convert<T, U>(ITreeNode<T> self, Func<T, U> generator, Action<int, U, U> addAction)
        where T : ITreeNode<T> {
            if (generator == null) throw new ArgumentNullException(nameof(generator));
            if (addAction == null) throw new ArgumentNullException(nameof(addAction));

            var scroller = self.LevelOrder().Select(x => Tuple.Create(x, generator(x))).ToListScroller();
            scroller.MoveForEach(ele => {
                scroller
                    .TryPrevious(x => x.Item1.Children.Contains(ele.Item1))
                    .When(r => addAction(ele.Item1.BranchIndex(), r.Current.Item2, ele.Item2));
            });
            return scroller.First().Current.Item2;

            //return self.ToNodeMap().AssembleTree(generator, addAction);
            //var dic = self.ToNodeMap(x=>generator(x));
            //return dic.AssembleTree(addAction);
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
		/// Assembles a tree structure (forest) with multiple roots from a dictionary where paths are used as keys.
		/// Nodes corresponding to paths that do not connect to a root and cannot trace parent-child relationships up to the root will be omitted as isolated nodes.
		/// </summary>
		/// <typeparam name="U">The type of the data to be converted.</typeparam>
		/// <typeparam name="UPath">The type of each element in the path.</typeparam>
		/// <typeparam name="T">The type of the resulting tree nodes.</typeparam>
		/// <param name="dic">A dictionary where keys represent paths and values represent data.</param>
		/// <param name="conv">A function to convert data into tree nodes.</param>
		/// <param name="addAction">Function that establishes the parent-child relationship with the first parameter being the parent object, and the second parameter being the child object.</param>
		/// <returns>An enumeration of the root nodes of the assembled tree structure.</returns>
		public static IEnumerable<T> AssembleForestByPath<U,UPath,T>(this IDictionary<NodePath<UPath>,U> dic,Func<U,T> conv,Action<T,T> addAction){
            var pss = dic.Where(x => 0 <= x.Key.Depth).Select(x => Tuple.Create(x.Key, conv(x.Value))).OrderBy(x => x.Item1.Depth);
            if(!pss.Any())return Enumerable.Empty<T>();
            var scroller = pss.ToListScroller();
            scroller.MoveForEach(ele => {
                scroller.TryPrevious(x => x.Item1.SequenceEqual(ele.Item1.SkipLast(1)))
                    .When(o => addAction(o.Current.Item2, ele.Item2));
            });
            return scroller.Items.TakeWhile(x => x.Item1.Depth == 0).Select(x => x.Item2);
        }
		/// <summary>
		/// Assembles a tree structure (forest) with multiple roots from a dictionary where paths are used as keys.
		/// Nodes corresponding to paths that do not connect to a root and cannot trace parent-child relationships up to the root will be omitted as isolated nodes.
		/// </summary>
		/// <typeparam name="TPath">The type of the elements in the path.</typeparam>
		/// <typeparam name="T">The type of the tree node.</typeparam>
		/// <param name="dic">The dictionary containing paths as keys and tree nodes as values.</param>
		/// <param name="addAction">Function that establishes the parent-child relationship with the first parameter being the parent object, and the second parameter being the child object.</param>
		/// <returns>An enumeration of the root nodes of the assembled tree structure.</returns>
		public static IEnumerable<T> AssembleForestByPath<TPath,T>(this IDictionary<NodePath<TPath>,T> dic,Action<T,T> addAction){
            return AssembleForestByPath(dic, x => x, addAction);
        }
		/// <summary>
		/// Assembles a tree structure (forest) with multiple roots from a dictionary where paths are used as keys.
		/// Nodes corresponding to paths that do not connect to a root and cannot trace parent-child relationships up to the root will be omitted as isolated nodes.
		/// </summary>
		/// <typeparam name="TPath">The type of the elements in the path.</typeparam>
		/// <typeparam name="T">The type of the tree node. Must implement <see cref="IMutableTreeNode{T}"/>.</typeparam>
		/// <param name="dic">The dictionary containing paths as keys and tree nodes as values.</param>
		/// <returns>An enumeration of the root nodes of the assembled tree structure.</returns>
		public static IEnumerable<T> AssembleForestByPath<TPath,T>(this IDictionary<NodePath<TPath>,T> dic) where T : IMutableTreeNode<T>{ 
            return AssembleForestByPath(dic,x=>x,(p,c)=>p.AddChild(c));
        }
		/// <summary>
		/// Assembles a tree structure (forest) with multiple roots from an enumerable of paths by using a conversion function to create tree nodes.
		/// </summary>
		/// <typeparam name="TPath">The type of the elements in the path.</typeparam>
		/// <typeparam name="T">The type of the resulting tree node.</typeparam>
		/// <param name="self">The enumerable of paths representing the hierarchy.</param>
		/// <param name="conv">A function to convert each path into a tree node.</param>
		/// <param name="addAction">Function that establishes the parent-child relationship with the first parameter being the parent object, and the second parameter being the child object.</param>
		/// <returns>An enumeration of the root nodes of the assembled tree structure.</returns>
		public static IEnumerable<T> AssembleForestByPath<TPath, T>(this IEnumerable<NodePath<TPath>> self, Func<NodePath<TPath>, T> conv, Action<T, T> addAction) {
            return AssembleForestByPath(self.SelectMany(x=>x._scan()).Distinct().ToDictionary(x => x, y => conv(y)), addAction);
        }
		/// <summary>
		/// Assembles a tree structure (forest) with multiple roots from an enumerable of paths, assuming the tree node type supports mutable relationships.
		/// </summary>
		/// <typeparam name="TPath">The type of the elements in the path.</typeparam>
		/// <typeparam name="T">The type of the tree node. Must implement <see cref="IMutableTreeNode{T}"/>.</typeparam>
		/// <param name="self">The enumerable of paths representing the hierarchy.</param>
		/// <param name="conv">A function to convert each path into a tree node.</param>
		/// <returns>An enumeration of the root nodes of the assembled tree structure.</returns>
        public static IEnumerable<T> AssembleForestByPath<TPath, T>(this IEnumerable<NodePath<TPath>> self, Func<NodePath<TPath>, T> conv) where T : IMutableTreeNode<T> {
            return AssembleForestByPath(self,conv,(p,c)=>p.AddChild(c));
        }

		/// <summary>
		/// Assembles a tree structure from a dictionary where the keys represent paths in the hierarchy. If any nodes leading to the root are missing, those nodes will be omitted.
		/// </summary>
		/// <typeparam name="U">The type of the data to convert.</typeparam>
		/// <typeparam name="UPath">The type of the elements in the path.</typeparam>
		/// <typeparam name="T">The type of the resulting tree node.</typeparam>
		/// <param name="dic">The dictionary containing paths as keys and data as values.</param>
		/// <param name="conv">A function to convert the data into the tree node type.</param>
		/// <param name="addaction">Function that establishes the parent-child relationship with the first parameter being the parent object, and the second parameter being the child object.</param>
		/// <returns>The root node of the assembled tree.</returns>
		public static T AssembleTreeByPath<U, UPath, T>(this IDictionary<NodePath<UPath>, U> dic,Func<U,T> conv,Action<T,T> addaction) {
            return AssembleForestByPath(dic, conv, addaction).First();
        }
		/// <summary>
		/// Assembles a tree structure from a dictionary where the keys represent paths in the hierarchy.If any nodes leading to the root are missing, those nodes will be omitted.
        /// </summary>
		/// <typeparam name="TPath">The type of the elements in the path.</typeparam>
		/// <typeparam name="T">The type of the tree node.</typeparam>
		/// <param name="dic">The dictionary containing paths as keys and tree nodes as values.</param>
		/// <param name="addAction">Function that establishes the parent-child relationship with the first parameter being the parent object, and the second parameter being the child object.</param>
		/// <returns>The root node of the assembled tree.</returns>
		public static T AssembleTreeByPath<TPath,T>(this IDictionary<NodePath<TPath>,T> dic,Action<T,T> addAction){
            return AssembleForestByPath(dic, addAction).First();
        }
		/// <summary>
		/// Assembles a tree structure from a dictionary where the keys represent paths in the hierarchy.If any nodes leading to the root are missing, those nodes will be omitted.
		/// </summary>
		/// <typeparam name="TPath">The type of the elements in the path.</typeparam>
		/// <typeparam name="T">The type of the tree node. Must implement <see cref="IMutableTreeNode{T}"/>.</typeparam>
		/// <param name="dic">The dictionary containing paths as keys and tree nodes as values.</param>
		/// <returns>The root node of the assembled tree.</returns>
		public static T AssembleTreeByPath<TPath,T>(this IDictionary<NodePath<TPath>,T> dic) where T : IMutableTreeNode<T>{
            return AssembleForestByPath(dic).First();
        }
		/// <summary>
		/// Assembles a tree structure from an enumerable of paths, with a conversion function for creating tree nodes.
		/// </summary>
		/// <typeparam name="TPath">The type of the elements in the path.</typeparam>
		/// <typeparam name="T">The type of the resulting tree node.</typeparam>
		/// <param name="self">The enumerable of paths representing the hierarchy.</param>
		/// <param name="conv">A function to convert each path into a tree node.</param>
		/// <param name="addAction">Function that establishes the parent-child relationship with the first parameter being the parent object, and the second parameter being the child object.</param>
		/// <returns>The root node of the assembled tree.</returns>
		public static T AssembleTreeByPath<TPath,T>(this IEnumerable<NodePath<TPath>> self, Func<NodePath<TPath>,T> conv,Action<T,T> addAction){
            return AssembleForestByPath(self,conv,addAction).First();
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
            return AssembleForestByPath(self, conv).First();
        }

        private static IEnumerable<NodePath<T>> _scan<T>(this NodePath<T> self){
            var enme= Enumerable.Empty<T>();
            foreach(var i in self){
                enme = enme.AddTail(i);
                yield return new NodePath<T>(enme);
            }
        }
		#endregion

		#region NodeIndexから組み立て
		private static T _assemble<T>(IEnumerable<Tuple<NodeIndex, T>> dic, Action<int, T, T> addAction) {
            var scroller = dic.OrderBy(x => x.Item1, NodeIndex.GetPostorderComparer()).ToListScroller();
            scroller.MoveForEach(ele => {
                scroller.TryNext(x => ele.Item1.Depth > x.Item1.Depth)
                    .When(r => addAction(ele.Item1.LastOrDefault(), r.Current.Item2, ele.Item2));
            });
            return scroller.Current.Item2;
        }
        private static U assemble<U,TKey, T>(IDictionary<TKey, T> dictionary, Func<T, U> conv, Action<int, U, U> addAction) where TKey: IEnumerable<int> {
            var seq = dictionary.Select(x => Tuple.Create(new NodeIndex(x.Key), conv(x.Value)));
            return _assemble(seq, addAction);
        }
		/// <summary>
		/// Assembles each node based on the index indicated by the key.
        /// /// </summary>
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
        /// <summary>Creates an N-ary tree starting from the first element.</summary>
        /// <remarks>Each node is added in level order.</remarks>
        /// <typeparam name="T">Type of the elements.</typeparam>
        /// <typeparam name="U">Type of the nodes.</typeparam>
        /// <param name="self">Current object.</param>
        /// <param name="nary">Upper limit on the number of child nodes that a parent node can have.</param>
        /// <param name="conv">Function to convert elements to nodes.</param>
        /// <param name="nests">Function to select nested collection.</param>
        /// <param name="addAction">Function that establishes the parent-child relationship with the first parameter being the parent object, and the second parameter being the child object.</param>
        /// <returns>The node converted from the first element that serves as the root.</returns>
        public static U AssembleAsNAryTree<T, U>(this IEnumerable<T> self, int nary, Func<T, U> conv, Func<U,IEnumerable<U>> nests, Action<U, U> addAction) /*where U : ITreeNode<U> */{
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
                    //if (item != null && tgt.Children.Contains(item)) {
                    if (item != null && (nests(tgt) ?? Enumerable.Empty<U>()).Contains(item)) { //tgt.Children.Contains(item)) {
                        queue.Enqueue(item);
                        cnt++;
                    }
                }
            }
            return root;
        }
        /// <summary>Creates an N-ary tree starting from the first element.</summary>
        /// <remarks>Each node is added in level order.</remarks>
        /// <typeparam name="T">Type of the elements.</typeparam>
        /// <typeparam name="U">Type of the nodes.</typeparam>
        /// <param name="self">Current object.</param>
        /// <param name="nary">Upper limit on the number of child nodes that a parent node can have.</param>
        /// <param name="conv">Function to convert elements to nodes.</param>
        /// <param name="addAction">Function that establishes the parent-child relationship with the first parameter being the parent object, and the second parameter being the child object.</param>
        /// <returns>The node converted from the first element that serves as the root.</returns>
        public static U AssembleAsNAryTree<T, U>(this IEnumerable<T> self, int nary, Func<T, U> conv, Action<U, U> addAction) where U : ITreeNode<U> {
            return AssembleAsNAryTree(self, nary, conv, x => x.Children, addAction);
            //if (self == null) throw new ArgumentNullException(nameof(self));
            //if (!self.Any()) throw new InvalidOperationException(nameof(self));
            //var nds = self.Select(a => conv(a)).SkipWhile(a=>a==null);
            //U? root = nds.FirstOrDefault();
            //if (root == null) throw new InvalidOperationException(nameof(conv));
            //Queue<U> items = new Queue<U>(nds.Skip(1));//ルート以外のノードを格納
            //Queue<U> queue = new Queue<U>();//追加待ちのノード
            //queue.Enqueue(root);
            //while (items.Any() && queue.Any()) {
            //    int cnt = 0;
            //    var tgt = queue.Dequeue();
            //    while (tgt != null && cnt < nary && items.Any()) {
            //        var item = items.Dequeue();
            //        addAction(tgt, item);
            //        if (item != null && tgt.Children.Contains(item)) {
            //            queue.Enqueue(item);
            //            cnt++;
            //        }
            //    }
            //}
            //return root;
        }
        /// <summary>Creates an N-ary tree starting from the first element.</summary>
        /// <remarks>Each node is added in level order.</remarks>
        /// <typeparam name="T">Type of the elements.</typeparam>
        /// <typeparam name="U">Type of the nodes.</typeparam>
        /// <param name="self">Current object.</param>
        /// <param name="nary">Upper limit on the number of child nodes that a parent node can have.</param>
        /// <param name="conv">Function to convert elements to nodes.</param>
        /// <returns>The node converted from the first element that serves as the root.</returns>
        public static U AssembleAsNAryTree<T,U>(this IEnumerable<T> self,int nary,Func<T,U> conv) where U : IMutableTreeNode<U> {
            return self.AssembleAsNAryTree(nary, conv, x => x.Children, (a, b) => a.AddChild(b));
        }

        /// <summary>Creates an N-ary tree starting from the first element.</summary>
        /// <remarks>Each node is added in level order.</remarks>
        /// <typeparam name="T">Type of the nodes.</typeparam>
        /// <param name="self">Current object.</param>
        /// <param name="nary">Upper limit on the number of child nodes that a parent node can have.</param>
        /// <returns>The root node.</returns>
        public static T AssembleAsNAryTree<T>(this IEnumerable<T> self,int nary) where T : IMutableTreeNode<T> {
            return self.AssembleAsNAryTree(nary, a => a, a => a.Children, (a, b) => a.AddChild(b));
        }
        #endregion
    }
}
