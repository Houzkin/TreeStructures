using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures.Linq {
    public static partial class TreeNodeExtenstions {

        #region パラメータの取得
        /// <summary>対象ノードの位置を示すパスコードを取得する。</summary>
        public static NodeIndex NodeIndex<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            var z = self
                .Upstream()
                .Select(b => b.BranchIndex())
                .TakeWhile(c => 0 <= c)
                .Reverse();
            return new NodeIndex(z);
        }
        /// <summary>ルートから現在のノードまでのパスを生成する。</summary>
        /// <param name="self">対象ノード</param>
        /// <param name="conv">各ノードの固有の値、またはそのノードを示す値を指定する。</param>
        public static NodePath<TPath> NodePath<TPath, T>(this ITreeNode<T> self, Converter<T, TPath> conv)
        where T : ITreeNode<T> {
            return NodePath<TPath>.Create((T)self, conv);
        }
        /// <summary>対象ノードにおいて、その子孫ノード最深部からの距離を取得する。</summary>
        public static int Height<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            if (self == null) throw new ArgumentNullException(nameof(self));
            return self.Levelorder().Last().Depth() - self.Depth();
        }
        /// <summary>対象ノードにおいて、Rootからの深さを取得する。。</summary>
        public static int Depth<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            if (self == null) throw new ArgumentNullException(nameof(self));
            return self.Upstream().Count() - 1;
        }
        /// <summary>対象ノードが親ノードによって振り当てられているインデックスを取得する。</summary>
        public static int BranchIndex<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            if (self == null) throw new ArgumentNullException(nameof(self));
            if (self.Parent == null) return -1;
            return self.Parent.Children
                .Select((nd, idx) => new { Node = nd, Index = idx })
                .First(x => object.ReferenceEquals(x.Node, self)).Index;
        }
        /// <summary>所属するツリー構造において対象ノードと同じ深さにあるノードの数を取得する。</summary>
        /// <remarks>同世代の数を返します。</remarks>
        public static int Width<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            if (self == null) throw new ArgumentNullException(nameof(self));
            return self.Generations().Count();
        }
        #endregion


        #region 判定メソッド
        /// <summary>現在のノードが、指定したノードの子孫ノードかどうかを示す値を取得する。</summary>
        /// <param name="self">対象ノード</param>
        /// <param name="node">指定ノード</param>
        public static bool IsDescendantOf<T>(this ITreeNode<T> self, T node) where T : ITreeNode<T> {
            return self.Upstream().Skip(1).Contains(node);
        }
        /// <summary>現在のノードが、指定したノードの祖先ノードかどうかを示す値を取得する。</summary>
        /// <param name="self">対象ノード</param>
        /// <param name="node">指定ノード</param>
        public static bool IsAncestorOf<T>(this ITreeNode<T> self, T node) where T : ITreeNode<T> {
            return node.Upstream().Skip(1).Contains((T)self);
        }
        /// <summary>対象ノードが現在ルートノードかどうかを示す値を取得する。</summary>
        public static bool IsRoot<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return self.Parent == null;
        }
        /// <summary>現在のノードの次に兄弟ノードが存在するかどうかを示す値を取得する。</summary>
        /// <param name="self">現在のノード</param>
        /// <param name="predicate">関数を指定した場合、指定した条件を満たすノードの存在を判定する。</param>
        public static bool HasNextSibling<T>(this ITreeNode<T> self, Predicate<T>? predicate = null) where T : ITreeNode<T> {
            predicate = predicate ?? new Predicate<T>(x => true);
            return self.nextSiblings().Any(x => predicate(x));
        }
        /// <summary>現在のノードの前に兄弟ノードが存在するかどうかを示す値を取得する。</summary>
        /// <param name="self">現在のノード</param>
        /// <param name="predicate">関数を指定した場合、指定した条件を満たすノードの存在を判定する。</param>
        public static bool HasPrevoiusSibling<T>(this ITreeNode<T> self, Predicate<T>? predicate = null) where T : ITreeNode<T> {
            predicate = predicate ?? new Predicate<T>(x => true);
            return self.previousSiblings().Any(x => predicate(x));
        }
        /// <summary>現在のノードが最後の兄弟ノードかどうかを示す値を取得する。</summary>
        public static bool IsLastSibling<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return !self.HasNextSibling();
        }
        /// <summary>現在のノードが最初の兄弟ノードかどうかを示す値を取得する。</summary>
        public static bool IsFirstSibling<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return !self.HasPrevoiusSibling();
        }
        #endregion
    }
}
