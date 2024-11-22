using System;
using System.Collections.Generic;
using System.Linq;
//using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TreeStructures.Collections;
using TreeStructures.Utility;
using TreeStructures.Xml.Serialization;

namespace TreeStructures.Linq {

    // IReadOnlyTreeNodeに対する拡張メソッドを定義する。
    public static partial class TreeNodeExtenstions {

        #region 列挙
        private static bool expand<T>(ref ISet<T> history, out T? cur, ref IEnumerable<T?> seeds, Func<T, IEnumerable<T?>> getnewseeds, Func<T, IEnumerable<T?>, IEnumerable<T?>, IEnumerable<T?>> updateseeds){
            if (!seeds.Any()) { cur = default; return false; }
            cur = seeds.First();
            while (cur != null && history.Add(cur)) {
                var newSeeds = getnewseeds(cur);
                seeds = updateseeds(cur, newSeeds, seeds.Skip(1));
                cur = seeds.FirstOrDefault();
            }
            seeds = seeds.Skip(1);
            return true;
        }
        //private static IEnumerable<T> _Evolve<T>(this T self, Func<T, IEnumerable<T?>> getnewseeds, Func<T, IEnumerable<T?>, IEnumerable<T?>, IEnumerable<T?>> updateseeds,IEqualityComparer<T> eqComp) where T : class {
        //    ISet<T> exphistory = new HashSet<T>(new EqualityComparer);//展開した履歴
        //    ISet<T> rtnhistory = new HashSet<T>(eqComp);//列挙した履歴
        //    IEnumerable<T?> seeds = new T[1] { (T)self };
        //    while (expand(ref exphistory, out T? cur, ref seeds, getnewseeds, updateseeds)) {
        //        if (cur != null && rtnhistory.Add(cur)) yield return cur;
        //    }
        //}
        /// <summary>
        /// Expands and enumerates the tree structure starting from the current node.
        /// </summary>
        /// <typeparam name="T">Type of the node.</typeparam>
        /// <param name="self">Current node.</param>
        /// <param name="getnewseeds">Specifies additional nodes to add from the elements.</param>
        /// <param name="updateseeds">
        /// Takes the source elements, added elements, and unprocessed elements as arguments and updates the unprocessed elements.
        /// <para>Unprocessed elements are processed in the order they are expanded and enumerated.</para>
        /// </param>
        /// <returns></returns>
        public static IEnumerable<T> Evolve<T>(this ITreeNode<T> self, Func<T, IEnumerable<T?>> getnewseeds, Func<T, IEnumerable<T?>, IEnumerable<T?>, IEnumerable<T?>> updateseeds) where T : ITreeNode<T> {
            ISet<T> exphistory = new HashSet<T>();//展開した履歴
            ISet<T> rtnhistory = new HashSet<T>();//列挙した履歴
            IEnumerable<T?> seeds = new T[1] { (T)self };
            while (expand(ref exphistory, out T? cur, ref seeds, getnewseeds, updateseeds)) {
                if (cur != null && rtnhistory.Add(cur)) yield return cur;
            }
        }

        /// <summary>Generates a sequence in preorder starting from the current node.</summary>
        public static IEnumerable<T> Preorder<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return self.Evolve(a => a.Children, (a, b, c) => b.Prepend(a).Concat(c));//b a c
        }
        /// <summary>Generates a sequence in postorder starting from the current node.</summary>
        public static IEnumerable<T> Postorder<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return self.Evolve(a => a.Children, (a, b, c) => b.Append(a).Concat(c));//a b c
        }
        /// <summary>Generates a sequence in level order starting from the current node.</summary>
        public static IEnumerable<T> Levelorder<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return self.Evolve(a => a.Children, (a, b, c) => new T?[1] { a }.Concat(c).Concat(b));//a c b
        }
        /// <summary>Generates a sequence in inorder starting from the current node.</summary>
        public static IEnumerable<T> Inorder<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return self.Evolve(a => a.Children, (a, b, c) => {
                var lst = new List<T?>(b);
                if (lst.Any()) lst.Insert(1, a); else lst.Add(a);
                return lst.Concat(c);
            });
        }
        /// <summary>Generates a sequence in the ancestor direction starting from the current node, with the root as the last element.</summary>
        public static IEnumerable<T> Upstream<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return self.Evolve(a => new T?[1] { a.Parent }, (a, b, c) => new T?[1] { a }.Concat(b).Concat(c));//upstream
        }
        /// <summary>Enumerates the ancestors of the current node, with the root as the last element.
        /// <para>If you want to include the current node, consider using <see cref="Upstream{T}(ITreeNode{T})"/>.</para></summary>
        public static IEnumerable<T> Ancestors<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return self.Evolve(a => new T?[1] { a.Parent }, (a, b, c) => new T?[1] { a }.Concat(b).Concat(c)).Skip(1);
        }
        /// <summary>Enumerates the leaf nodes in the descendant direction from the current node.</summary>
        public static IEnumerable<T> Leafs<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return self.Evolve(a => a.Children, (a, b, c) => (b.OfType<T>().Any() ? b : new T?[1] { a }).Concat(c));//leafs
        }
        /// <summary>Gets the sibling nodes, including the current node.</summary>
        public static IEnumerable<T> Siblings<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            IEnumerable<T> arr;
            if (self == null) throw new ArgumentNullException(nameof(self));
            if (self.Parent == null) arr = new T[] { (T)self };
            else arr = self.Parent.Children.OfType<T>();
            return arr;
        }
        /// <summary>Retrieves nodes at the same depth as the current node within the belonging tree structure.</summary>
        /// <typeparam name="T">Type of the node.</typeparam>
        /// <param name="self">Current node.</param>
        /// <returns>Nodes at the same depth as the current node.</returns>
        public static IEnumerable<T> Generations<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            int depth = self.Depth();
            return self.Root().Levelorder().SkipWhile(node => node.Depth() < depth).TakeWhile(node => node.Depth() == depth);
        }

        /// <summary>Retrieves sibling nodes that precede the current node.</summary>
        /// <typeparam name="T">Type of the node.</typeparam>
        /// <param name="self">Current node.</param>
        /// <returns>Previous sibling nodes.</returns>
        static IEnumerable<T> previousSiblings<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return self.Siblings().TakeWhile(x => !object.ReferenceEquals(x, self));
        }

        /// <summary>Retrieves sibling nodes that follow the current node.</summary>
        /// <typeparam name="T">Type of the node.</typeparam>
        /// <param name="self">Current node.</param>
        /// <returns>Next sibling nodes.</returns>
        static IEnumerable<T> nextSiblings<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return self.Siblings().SkipWhile(x => !object.ReferenceEquals(x, self)).Skip(1);
        }

        #endregion

        #region 移動メソッド
        /// <summary>Gets the root node of the tree to which the target node belongs.</summary>
        /// <typeparam name="T">Type of the node.</typeparam>
        /// <param name="self">Target node.</param>
        /// <returns>The root node of the tree.</returns>
        public static T Root<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return self.Upstream().Last();
        }

        /// <summary>Moves to the first node among the sibling nodes.</summary>
        /// <typeparam name="T">Type of the node.</typeparam>
        /// <param name="self">Current node.</param>
        /// <param name="predicate">If specified, gets the first node that satisfies the condition.</param>
        /// <returns>The first sibling node.</returns>
        public static T FirstSibling<T>(this ITreeNode<T> self, Predicate<T>? predicate = null) where T : ITreeNode<T> {
            predicate ??= new Predicate<T>(x => true);
            return self.Siblings().First(x => predicate(x));
        }

        /// <summary>Moves to the first node among the sibling nodes. Returns the current node if not found.</summary>
        /// <typeparam name="T">Type of the node.</typeparam>
        /// <param name="self">Current node.</param>
        /// <param name="predicate">If specified, gets the first node that satisfies the condition.</param>
        /// <returns>The first sibling node or the current node if not found.</returns>
        public static ResultWithValue<T> FirstSiblingOrSelf<T>(this ITreeNode<T> self, Predicate<T>? predicate = null) where T : ITreeNode<T> {
            predicate ??= new Predicate<T>(_ => true);
            var firstSibling = self.Siblings().FirstOrDefault(x => predicate(x));
            return firstSibling != null
                ? new ResultWithValue<T>(firstSibling)
                : new ResultWithValue<T>(false, (T)self);
        }

        /// <summary>Moves to the last node among the sibling nodes.</summary>
        /// <typeparam name="T">Type of the node.</typeparam>
        /// <param name="self">Current node.</param>
        /// <param name="predicate">If specified, gets the last node that satisfies the condition.</param>
        /// <returns>The last sibling node.</returns>
        public static T LastSibling<T>(this ITreeNode<T> self, Predicate<T>? predicate = null) where T : ITreeNode<T> {
            predicate ??= new Predicate<T>(x => true);
            return self.Siblings().Last(x => predicate(x));
        }

        /// <summary>
        /// Moves to the last node among the sibling nodes. Returns the current node if not found.
        /// </summary>
        /// <typeparam name="T">Type of the node.</typeparam>
        /// <param name="self">Current node.</param>
        /// <param name="predicate">If specified, gets the last node that satisfies the condition.</param>
        /// <returns>The last sibling node or the current node if not found.</returns>
        public static ResultWithValue<T> LastSiblingOrSelf<T>(this ITreeNode<T> self, Predicate<T>? predicate = null) where T : ITreeNode<T> {
            predicate ??= new Predicate<T>(_ => true);
            var lastSibling = self.Siblings().LastOrDefault(x => predicate(x));
            return lastSibling != null
                ? new ResultWithValue<T>(lastSibling)
                : new ResultWithValue<T>(false, (T)self);
        }

        /// <summary>
        /// Moves to the next sibling node.
        /// </summary>
        /// <typeparam name="T">Type of the node.</typeparam>
        /// <param name="self">Current node.</param>
        /// <param name="predicate">If specified, gets the next node that satisfies the condition.</param>
        /// <returns>The next sibling node.</returns>
        public static T NextSibling<T>(this ITreeNode<T> self, Predicate<T>? predicate = null) where T : ITreeNode<T> {
            Predicate<T> pred = predicate ?? new Predicate<T>(x => true);
            return self.nextSiblings().First(x => pred(x));
        }

        /// <summary>
        /// Moves to the next sibling node. Returns the current node if not found.
        /// </summary>
        /// <typeparam name="T">Type of the node.</typeparam>
        /// <param name="self">Current node.</param>
        /// <param name="predicate">If specified, gets the next node that satisfies the condition.</param>
        /// <returns>The next sibling node or the current node if not found.</returns>
        public static ResultWithValue<T> NextSiblingOrSelf<T>(this ITreeNode<T> self, Predicate<T>? predicate = null) where T : ITreeNode<T> {
            Predicate<T> pred = predicate ?? new Predicate<T>(_ => true);
            var next = self.nextSiblings().FirstOrDefault(x => pred(x));
            if (next != null)
                return new ResultWithValue<T>(next);
            else
                return new ResultWithValue<T>(false, (T)self);
        }

        /// <summary>
        /// Moves to the previous sibling node.
        /// </summary>
        /// <typeparam name="T">Type of the node.</typeparam>
        /// <param name="self">Current node.</param>
        /// <param name="predicate">If specified, gets the previous node that satisfies the condition.</param>
        /// <returns>The previous sibling node.</returns>
        public static T PreviousSibling<T>(this ITreeNode<T> self, Predicate<T>? predicate = null) where T : ITreeNode<T> {
            Predicate<T> pred = predicate ?? new Predicate<T>(x => true);
            return self.previousSiblings().Last(x => pred(x));
        }

        /// <summary>
        /// Moves to the previous sibling node. Returns the current node if not found.
        /// </summary>
        /// <typeparam name="T">Type of the node.</typeparam>
        /// <param name="self">Current node.</param>
        /// <param name="predicate">If specified, gets the previous node that satisfies the condition.</param>
        /// <returns>The previous sibling node or the current node if not found.</returns>
        public static ResultWithValue<T> PreviousSiblingOrSelf<T>(this ITreeNode<T> self, Predicate<T>? predicate = null) where T : ITreeNode<T> {
            Predicate<T> pred = predicate ?? new Predicate<T>(_ => true);
            var previous = self.previousSiblings().LastOrDefault(x => pred(x));
            if (previous != null)
                return new ResultWithValue<T>(previous);
            else
                return new ResultWithValue<T>(false, (T)self);
        }

        /// <summary>
        /// Moves away from the current node due to the processing of the predicate but returns to the node before the processing.
        /// </summary>
        /// <typeparam name="T">Type of the node.</typeparam>
        /// <param name="self">Current node.</param>
        /// <param name="action">Predicate processing to be performed.</param>
        /// <returns>The node targeted before the predicate processing.</returns>
        public static T Fork<T>(this ITreeNode<T> self, Action<T> action) where T : ITreeNode<T> {
            if (self == null) throw new ArgumentNullException(nameof(self));
            action?.Invoke((T)self);
            return (T)self;
        }

        /// <summary>
        /// Moves away from the current node due to the processing of the predicate but returns to the node before the processing.
        /// </summary>
        /// <typeparam name="T">Type of the node.</typeparam>
        /// <param name="self">Current node.</param>
        /// <param name="sentence">Arbitrary object representing the predicate processing.</param>
        /// <returns>The node targeted before the predicate processing.</returns>
        public static T Fork<T>(this ITreeNode<T> self, object sentence) where T : ITreeNode<T> {
            //action?.Invoke();
            return (T)self;
        }

		#endregion

		#region 探索メソッド
		/// <summary>
		/// Searches from the current node towards descendants and returns the terminal node that continues to meet the condition.
		/// </summary>
		/// <typeparam name="T">Type of the node.</typeparam>
		/// <param name="self">The current node.</param>
		/// <param name="predicate">The condition for the nodes in the descendant direction, including the current node.</param>
		/// <returns></returns>
		public static IEnumerable<T> DescendArrivals<T>(this ITreeNode<T> self,Func<T,bool> predicate)where T:ITreeNode<T>{
            return self.Evolve(cur => {
                if (predicate(cur))
                    return cur.Children.Where(predicate);
                else
                    return Array.Empty<T>();
            }, (cur, clds, seeds) => {
                if (predicate(cur) && clds.Any(predicate))
                    return seeds.Concat(clds);
                else if (predicate(cur))
                    return seeds.Append(cur);
                else
                    return seeds;
            });
        }

        /// <summary>
        /// Searches for nodes in descendant direction from the current node, where each key matches in sequence.
        /// </summary>
        /// <typeparam name="T">Type of the node.</typeparam>
        /// <typeparam name="Trc">Type of the key.</typeparam>
        /// <param name="self">Current node.</param>
        /// <param name="selector">Selects the key from each element.</param>
        /// <param name="trace">Keys to compare sequentially in descendant direction from the current node.</param>
        /// <param name="comparer">Comparer used for key comparison.</param>
        /// <returns>Nodes where all keys match.</returns>
        public static IEnumerable<T> DescendArrivals<T, Trc>(this ITreeNode<T> self, Func<T, Trc> selector, IEnumerable<Trc> trace, IEqualityComparer<Trc>? comparer = null) where T : ITreeNode<T> {
            if(!trace.Any()) return Enumerable.Empty<T>();
            comparer ??= EqualityComparer<Trc>.Default;
            var matchs = new SequenceScroller<Trc>(trace);
            var startdpth = self.Depth();
            return self.Evolve(cur => {
                int curlv = cur.Depth() - startdpth;
                if(matchs.TryMoveTo(curlv)) {
                    var cld = comparer.Equals(matchs.Current,selector(cur)) ? cur.Children : Array.Empty<T>();
                    if(matchs.TryNext()) {
                        return cld.Where(x => comparer.Equals(matchs.Current, selector(x))).ToArray();
                    }
                }
                return Array.Empty<T>();
            }, (cur, clds, seeds) => {
                int curlv = cur.Depth() - startdpth;
                return matchs.TryMoveTo(curlv + 1).When(
                    o => clds.Concat(seeds),
                    x => seeds.Prepend(cur));
            });
        }
		/// <summary>
        /// Searches from the current node towards descendants and returns the path to the terminal node that continues to meet the condition.
		/// </summary>
		/// <typeparam name="T">Type of the node.</typeparam>
		/// <param name="self">The current node.</param>
		/// <param name="predicate">The condition for the nodes in the descendant direction, including the current node.</param>
		/// <returns></returns>
		public static IReadOnlyList<IEnumerable<T>> DescendTraces<T>(this ITreeNode<T> self, Func<T, bool> predicate)where T:ITreeNode<T>{
            var tmls = self.DescendArrivals(predicate);
            var lst = new List<IEnumerable<T>>();
            foreach(var tml in tmls){
                lst.Add(tml.Upstream().TakeWhile(a => !object.ReferenceEquals(a, self)).Append((T)self).Reverse().ToArray());
            }
            return lst.AsReadOnly();
        }
        /// <summary>
        /// Searches for nodes in descendant direction from the current node, where each key matches in sequence, and returns the path to the matching node.
        /// </summary>
        /// <typeparam name="T">Type of the node.</typeparam>
        /// <typeparam name="Trc">Type of the key.</typeparam>
        /// <param name="self">Current node.</param>
        /// <param name="selector">Selects the key from each element.</param>
        /// <param name="trace">Keys to compare sequentially in descendant direction from the current node.</param>
        /// <param name="comparer">Comparer used for key comparison.</param>
        /// <returns>Path to the node where all keys match.</returns>
        public static IReadOnlyList<IEnumerable<T>> DescendTraces<T,Trc>(this ITreeNode<T> self,Func<T,Trc> selector,IEnumerable<Trc> trace,IEqualityComparer<Trc>? comparer = null) where T : ITreeNode<T> {
            comparer ??= EqualityComparer<Trc>.Default;
            var peak = self.DescendArrivals(selector,trace, comparer);
            var lst = new List<IEnumerable<T>>();
            foreach(var pk in peak) {
                lst.Add(pk.Upstream().TakeWhile(a=>!object.ReferenceEquals(a, self)).Append((T)self).Reverse().ToArray());
            }
            return lst.AsReadOnly();
        }
		/// <summary>
		/// Enumerates the first nodes that match the specified condition in a descendant direction.
		/// Descendant nodes of a matching node are excluded from further exploration.
		/// </summary>
		/// <typeparam name="T">
		/// The type of the nodes. <typeparamref name="T"/> must implement <see cref="ITreeNode{T}"/>.
		/// </typeparam>
		/// <param name="self">
		/// The node of the tree structure to start the search from.
		/// </param>
		/// <param name="predicate">
		/// A delegate to determine whether a node matches the specified condition.
		/// </param>
		/// <returns>
		/// An <see cref="IEnumerable{T}"/> of nodes that match the specified condition.
		/// Descendant nodes of a matching node are excluded from further exploration.
		/// </returns>
		/// <remarks>
		/// This method performs a depth-first search of the tree structure and enumerates the first nodes that match the specified condition.
		/// For example, if a node matches the condition, its descendants are not explored further.
		/// </remarks>
		public static IEnumerable<T> DescendFirstMatches<T>(this ITreeNode<T> self, Func<T, bool> predicate) where T : ITreeNode<T> {
            return self.Evolve(
                cur => !predicate(cur) ? cur.Children : Array.Empty<T>(), 
                (cur, clds, seeds) => predicate(cur) ? seeds.Prepend(cur) : seeds.Concat(clds));
        }
        #endregion
    }
}
