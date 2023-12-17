using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Collections;
using TreeStructures.Linq;

namespace TreeStructures {

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
        /// <summary>対象ノードを始点とするツリーと同じ構造で、各ノードの型を変換した構造を再構築する。</summary>
        /// <typeparam name="T">変換前の型</typeparam>
        /// <typeparam name="U">変換後の型</typeparam>
        /// <param name="self">対象ノード</param>
        /// <param name="generator">各ノードに適用されるノード変換関数</param>
        public static U Convert<T, U>(this ITreeNode<T> self, Func<T, U> generator)
        where T : ITreeNode<T>
        where U : ITreeNodeCollection<U> {
            return convert(self, generator, (i, p, c) => p.AddChild(c));
        }
        /// <summary>対象ノードを始点とするツリーと同じ構造で、各ノードの型を変換した構造を再構築する。</summary>
        /// <typeparam name="T">変換前の型</typeparam>
        /// <typeparam name="U">変換後の型</typeparam>
        /// <param name="self">対象ノード</param>
        /// <param name="generator">各ノードに適用されるノード変換関数</param>
        /// <param name="addAction">第一引数に要求されるBranchIndex、第二引数に親となるオブジェクト、第三引数に子となるオブジェクトを取り、その関係を成り立たせる関数。</param>
        public static U Convert<T, U>(this ITreeNode<T> self, Func<T, U> generator, Action<int, U, U> addAction)
        where T : ITreeNode<T> {
            return convert(self, generator, addAction);
        }
        /// <summary>対象ノードを始点とするツリーと同じ構造で、各ノードの型を変換した構造を再構築する。</summary>
        /// <typeparam name="T">変換前の型</typeparam>
        /// <typeparam name="U">変換後の型</typeparam>
        /// <param name="self">対象ノード</param>
        /// <param name="generator">各ノードに適用されるノード変換関数</param>
        /// <param name="addAction">第一引数に親となるオブジェクト、第二引数に子となるオブジェクトを取り、その関係を成り立たせる関数。</param>
        public static U Convert<T, U>(this ITreeNode<T> self, Func<T, U> generator, Action<U, U> addAction)
        where T : ITreeNode<T> {
            return convert(self, generator, (i, p, c) => addAction(p, c));
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
        /// <summary>各ノードをキーが示すインデックスをもとに組み立てる。</summary>
        /// <typeparam name="T">ノードの型</typeparam>
        /// <typeparam name="TKey">ノードインデックスを示す、<see cref="int"/>型の要素を持つ<see cref="IEnumerable"/></typeparam>
        public static T AssembleTree<TKey,T>(this IDictionary<TKey, T> self) where T : ITreeNodeCollection<T> where TKey: IEnumerable<int> {
            return assemble(self, x => x, (i, p, c) => p.AddChild(c));
        }
        /// <summary>各データをキーが示すインデックスをもとに組み立てる。</summary>
        /// <typeparam name="TKey">ノードインデックスを示す、<see cref="int"/>型の要素を持つ<see cref="IEnumerable"/></typeparam>
        /// <typeparam name="T">データの型</typeparam>
        /// <param name="self">現在のオブジェクト</param>
        /// <param name="addAction">第一引数に要求されるBranchIndex、第二引数に親となるオブジェクト、第三引数に子となるオブジェクトを取り、その関係を成り立たせる関数。</param>
        public static T AssembleTree<TKey,T>(this IDictionary<TKey, T> self, Action<int,T, T> addAction)where TKey : IEnumerable<int> {
            return assemble(self, x => x, addAction);
        }
        /// <summary>各データをキーが示すインデックスをもとに組み立てる。</summary>
         /// <typeparam name="TKey">ノードインデックスを示す、<see cref="int"/>型の要素を持つ<see cref="IEnumerable"/></typeparam>
         /// <typeparam name="T">データの型</typeparam>
         /// <param name="self">現在のオブジェクト</param>
         /// <param name="addAction">第一引数に親となるオブジェクト、第二引数に子となるオブジェクトを取り、その関係を成り立たせる関数。</param>
        public static T AssembleTree<TKey, T>(this IDictionary<TKey, T> self, Action<T, T> addAction) where TKey : IEnumerable<int> {
            return assemble(self, x => x, (i, p, c) => addAction(p, c));
        }
        /// <summary>階層を示すインデックスをもとに、データからノードを生成しつつ組み立てる。</summary>
        /// <typeparam name="TKey">ノードインデックスを示す、<see cref="int"/>型の要素を持つ<see cref="IEnumerable"/></typeparam>
        /// <typeparam name="T">データの型</typeparam>
        /// <typeparam name="U">ノードの型</typeparam>
        /// <param name="self">現在のオブジェクト</param>
        /// <param name="conv">各データからノードへの変換関数</param>
        public static U AssembleTree<TKey,T, U>(this IDictionary<TKey, T> self, Func<T, U> conv)
        where TKey : IEnumerable<int>
        where U : ITreeNodeCollection<U> {
            return assemble(self, conv, (i, p, c) => p.AddChild(c));
        }
        /// <summary>階層を示すインデックスをもとに、各データの変換と組み立てを行う。</summary>
        /// <typeparam name="TKey">ノードインデックスを示す、<see cref="int"/>型の要素を持つ<see cref="IEnumerable"/></typeparam>
        /// <typeparam name="T">データ</typeparam>
        /// <typeparam name="U">変換先の型</typeparam>
        /// <param name="self">現在のオブジェクト</param>
        /// <param name="conv">変換関数</param>
        /// <param name="addAction">第一引数に要求されるBranchIndex、第二引数に親となるオブジェクト、第三引数に子となるオブジェクトを取り、その関係を成り立たせる関数。</param>
        public static U AssembleTree<TKey,T, U>(this IDictionary<TKey, T> self, Func<T, U> conv, Action<int, U, U> addAction) where TKey:IEnumerable<int> {
            return assemble(self, conv, addAction);
        }
        /// <summary>階層を示すインデックスをもとに、各データの変換と組み立てを行う。</summary>
        /// <typeparam name="TKey">ノードインデックスを示す、<see cref="int"/>型の要素を持つ<see cref="IEnumerable"/></typeparam>
        /// <typeparam name="T">データ</typeparam>
        /// <typeparam name="U">変換先の型</typeparam>
        /// <param name="self">現在のオブジェクト</param>
        /// <param name="conv">変換関数</param>
        /// <param name="addAction">第一引数に親となるオブジェクト、第二引数に子となるオブジェクトを取り、その関係を成り立たせる関数。</param>
        public static U AssembleTree<TKey, T, U>(this IDictionary<TKey, T> self, Func<T, U> conv, Action<U, U> addAction) where TKey : IEnumerable<int> {
            return assemble(self, conv, (i, p, c) => addAction(p, c));
        }
        #endregion

        #region IEnumerableからN分木を生成
        /// <summary>最初の要素を起点としたN分木を作成する。</summary>
        /// <remarks>各ノードはレベル順に追加されていきます。</remarks>
        /// <typeparam name="T">要素の型</typeparam>
        /// <typeparam name="U">ノードの型</typeparam>
        /// <param name="self"></param>
        /// <param name="nary">親ノードが持つ子ノードの数の上限</param>
        /// <param name="conv">要素からノードに変換する</param>
        /// <param name="addAction">第一引数に親となるオブジェクト、第二引数に子となるオブジェクトを取り、その関係を成り立たせる関数。</param>
        /// <returns>rootとなる最初の要素から変換されたノードを返す。</returns>
        public static U CreateAsNAryTree<T, U>(this IEnumerable<T> self, int nary, Func<T, U> conv, Action<U, U> addAction) where U : ITreeNode<U> {
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
        /// <summary>最初の要素を起点としたN分木を作成する。</summary>
        /// <remarks>各ノードはレベル順に追加されていきます。</remarks>
        /// <typeparam name="T">要素の型</typeparam>
        /// <typeparam name="U">ノードの型</typeparam>
        /// <param name="self"></param>
        /// <param name="nary">親ノードが持つ子ノードの数の上限</param>
        /// <param name="conv">要素からノードに変換する</param>
        /// <returns>rootとなる最初の要素から変換されたノードを返す。</returns>
        public static U CreateAsNAryTree<T,U>(this IEnumerable<T> self,int nary,Func<T,U> conv) where U : ITreeNodeCollection<U> {
            return self.CreateAsNAryTree(nary, conv, (a, b) => a.AddChild(b));
        }

        /// <summary>最初の要素を起点としたN分木を作成する。</summary>
        /// <remarks>各ノードはレベル順に追加されていきます。</remarks>
        /// <typeparam name="T">ノードの型</typeparam>
        /// <param name="self"></param>
        /// <param name="nary">親ノードが持つ子ノードの数の上限</param>
        /// <returns>rootとなる最初のノードを返す。</returns>
        public static T CreateAsNAryTree<T>(this IEnumerable<T> self,int nary) where T : ITreeNodeCollection<T> {
            return self.CreateAsNAryTree(nary, a => a)!;
        }
        #endregion
    }
}
