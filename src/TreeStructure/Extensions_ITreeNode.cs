using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TreeStructures.Collections;
using TreeStructures.Utility;
using TreeStructures.Xml.Serialization;

namespace TreeStructures {

    // IReadOnlyTreeNodeに対する拡張メソッドを定義する。
    public static partial class TreeNodeExtenstions {

        #region 列挙
        private static bool expand<T>(ref ISet<T> history, out T? cur, ref IEnumerable<T?> seeds, Func<T, IEnumerable<T?>> getnewseeds, Func<T, IEnumerable<T?>, IEnumerable<T?>, IEnumerable<T?>> updateseeds) where T : ITreeNode<T> {
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
        /// <summary>現在のノードを起点として、ツリー構造を展開・列挙する</summary>
        /// <typeparam name="T">ノードの型</typeparam>
        /// <param name="self">現在のノード</param>
        /// <param name="getnewseeds">要素から、追加するノードを指定する</param>
        /// <param name="updateseeds">展開元の要素、追加された要素、未処理要素を引数にり、未処理要素を更新する。<para>未処理要素は先頭から展開・列挙処理される。</para></param>
        /// <returns></returns>
        public static IEnumerable<T> Evolve<T>(this ITreeNode<T> self, Func<T, IEnumerable<T?>> getnewseeds, Func<T, IEnumerable<T?>, IEnumerable<T?>, IEnumerable<T?>> updateseeds) where T : ITreeNode<T> {
            ISet<T> exphistory = new HashSet<T>();//展開した履歴
            ISet<T> rtnhistory = new HashSet<T>();//列挙した履歴
            IEnumerable<T?> seeds = new T[1] { (T)self };
            while (expand(ref exphistory, out T? cur, ref seeds, getnewseeds, updateseeds)) {
                if (cur != null && rtnhistory.Add(cur)) yield return cur;
            }
        }

        /// <summary>対象ノードを始点とし、先行順でシーケンスを生成する。</summary>
        public static IEnumerable<T> Preorder<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return self.Evolve(a => a.Children, (a, b, c) => b.Prepend(a).Concat(c));
        }
        /// <summary>対象ノードを始点とし、後行順でシーケンスを生成する。</summary>
        public static IEnumerable<T> Postorder<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return self.Evolve(a => a.Children, (a, b, c) => b.Append(a).Concat(c));
        }
        /// <summary>対象ノードを始点とし、レベル順でシーケンスを生成する。</summary>
        public static IEnumerable<T> Levelorder<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return self.Evolve(a => a.Children, (a, b, c) => new T?[1] { a }.Concat(c).Concat(b));
        }
        /// <summary>対象ノードを始点とし、中間順でシーケンスを生成する。</summary>
        public static IEnumerable<T> Inorder<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return self.Evolve(a => a.Children, (a, b, c) => {
                var lst = new List<T?>(b);
                if (lst.Any()) lst.Insert(1, a); else lst.Add(a);
                return lst.Concat(c);
            });
        }
        /// <summary>対象ノードから祖先方向へシーケンスを生成する。最後尾がRootとなる。</summary>
        public static IEnumerable<T> Upstream<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return self.Evolve(a => new T?[1] { a.Parent }, (a, b, c) => new T?[1] { a }.Concat(b).Concat(c));//upstream
        }
        /// <summary>対象ノードの祖先を列挙する。最後尾がRootとなる。
        /// <para>対象ノードを含める場合は<see cref="Upstream{T}(ITreeNode{T})"/>の使用を検討してください。</para></summary>
        public static IEnumerable<T> Ancestors<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return self.Evolve(a => new T?[1] { a.Parent }, (a, b, c) => new T?[1] { a }.Concat(b).Concat(c)).Skip(1);
        }
        /// <summary>対象ノードから子孫方向に、末端のノードを列挙する。</summary>
        public static IEnumerable<T> Leafs<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return self.Evolve(a => a.Children, (a, b, c) => (b.OfType<T>().Any() ? b : new T?[1] { a }).Concat(c));//leafs
        }
        /// <summary>現在のノードを含めた兄弟ノードを取得する。</summary>
        public static IEnumerable<T> Siblings<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            IEnumerable<T> arr;
            if (self == null) throw new ArgumentNullException(nameof(self));
            if (self.Parent == null) arr = new T[] { (T)self };
            else arr = self.Parent.Children.OfType<T>();
            return arr;
        }
        /// <summary>所属するツリー構造において、対象ノードと同じ深さにあるノードを取得する。</summary>
        public static IEnumerable<T> Generations<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            int dpt = self.Depth();
            return self.Root().Levelorder().SkipWhile(a => a.Depth() < dpt).TakeWhile(a => a.Depth() == dpt);
        }
        /// <summary>現在より前の兄弟ノードを取得する。</summary>
        static IEnumerable<T> previousSiblings<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return self.Siblings().TakeWhile(x => !object.ReferenceEquals(x, self));
        }
        /// <summary>現在より後の兄弟ノードを取得する。</summary>
        static IEnumerable<T> nextSiblings<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return self.Siblings().SkipWhile(x => !object.ReferenceEquals(x, self)).Skip(1);
        }
        #endregion

        #region 移動メソッド
        /// <summary>対象ノードが所属するツリーのルートノードを取得する。</summary>
        public static T Root<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
            return self.Upstream().Last();
        }
        /// <summary>兄弟ノードの最初のノードへ移動する。</summary>
        /// <param name="self">現在のノード</param>
        /// <param name="predicate">条件を指定した場合、条件を満たす最初のノードを取得する。</param>
        public static T FirstSibling<T>(this ITreeNode<T> self, Predicate<T>? predicate = null) where T : ITreeNode<T> {
            predicate = predicate ?? new Predicate<T>(x => true);
            return self.Siblings().First(x => predicate(x));
        }
        /// <summary>兄弟ノードの最初のノードへ移動する。見つからなかった場合は現在のノードを返す。</summary>
        /// <param name="self">現在のノード</param>
        /// <param name="predicate">条件を指定した場合、条件を満たす最初のノードを取得する。</param>
        public static ResultWithValue<T> FirstSiblingOrSelf<T>(this ITreeNode<T> self, Predicate<T>? predicate = null) where T : ITreeNode<T> {
            Predicate<T> pred = predicate ?? new Predicate<T>(_ => true);
            var fst = self.Siblings().FirstOrDefault(x => pred(x));
            if (fst != null) return new ResultWithValue<T>(fst);
            else return new ResultWithValue<T>(false, (T)self);
        }
        /// <summary>兄弟ノードの最後へ移動する。</summary>
        /// <param name="self">現在のノード</param>
        /// <param name="predicate">条件を指定した場合、条件を満たす最後のノードを取得する。</param>
        public static T LastSibling<T>(this ITreeNode<T> self, Predicate<T>? predicate = null) where T : ITreeNode<T> {
            Predicate<T> pred = predicate ?? new Predicate<T>(x => true);
            return self.Siblings().Last(x => pred(x));
        }
        /// <summary>兄弟ノードの最後へ移動する。見つからなかった場合は現在のノードを返す。</summary>
        /// <param name="self">現在のノード</param>
        /// <param name="predicate">条件を指定した場合、条件を満たす最後のノードを取得する。</param>
        public static ResultWithValue<T> LastSiblingOrSelf<T>(this ITreeNode<T> self, Predicate<T>? predicate = null) where T : ITreeNode<T> {
            Predicate<T> pred = predicate ?? new Predicate<T>(_ => true);
            var lst = self.Siblings().LastOrDefault(x => pred(x));
            if (lst != null) return new ResultWithValue<T>(lst);
            else return new ResultWithValue<T>(false, (T)self);
        }
        /// <summary>次の兄弟ノードへ移動する。</summary>
        /// <param name="self">現在のノード</param>
        /// <param name="predicate">条件を指定した場合、条件を満たす次のノードを取得する。</param>
        public static T NextSibling<T>(this ITreeNode<T> self, Predicate<T>? predicate = null) where T : ITreeNode<T> {
            Predicate<T> pred = predicate ?? new Predicate<T>(x => true);
            return self.nextSiblings().First(x => pred(x));
        }
        /// <summary>次の兄弟ノードへ移動する。見つからなかった場合は現在のノードを返す。</summary>
        /// <param name="self">現在のノード</param>
        /// <param name="predicate">条件を指定した場合、条件を満たす次のノードを取得する。</param>
        public static ResultWithValue<T> NextSiblingOrSelf<T>(this ITreeNode<T> self, Predicate<T>? predicate = null) where T : ITreeNode<T> {
            Predicate<T> pred = predicate ?? new Predicate<T>(_ => true);
            var nxt = self.nextSiblings().FirstOrDefault(x => pred(x));
            if (nxt != null) return new ResultWithValue<T>(nxt);
            else return new ResultWithValue<T>(false, (T)self);
        }
        /// <summary>前の兄弟ノードへ移動する。</summary>
        /// <param name="self">現在のノード</param>
        /// <param name="predicate">条件を指定した場合、条件を満たす前のノードを取得する。</param>
        public static T PreviousSibling<T>(this ITreeNode<T> self, Predicate<T>? predicate = null) where T : ITreeNode<T> {
            Predicate<T> pred = predicate ?? new Predicate<T>(x => true);
            return self.previousSiblings().Last(x => pred(x));
        }
        /// <summary>前の兄弟ノードへ移動する。見つからなかった場合は現在のノードを返す。</summary>
        /// <param name="self">現在のノード</param>
        /// <param name="predicate">条件を指定した場合、条件を満たす前のノードを取得する。</param>
        public static ResultWithValue<T> PreviousSiblingOrSelf<T>(this ITreeNode<T> self, Predicate<T>? predicate = null) where T : ITreeNode<T> {
            Predicate<T> pred = predicate ?? new Predicate<T>(_ => true);
            var prv = self.previousSiblings().LastOrDefault(x => pred(x));
            if (prv != null) return new ResultWithValue<T>(prv);
            else return new ResultWithValue<T>(false, (T)self);
        }
        /// <summary>述語の処理により現在のノードから移動しても、処理前のノードへ戻ってくる。</summary>
        /// <returns>述語の処理を行う前に対象としていたノード</returns>
        public static T Fork<T>(this ITreeNode<T> self, Action<T> action)//Fork or Pretreat
        where T : ITreeNode<T> {
            if (self == null) throw new ArgumentNullException(nameof(self));
            action?.Invoke((T)self);
            return (T)self;
        }
        /// <summary>述語の処理により現在のノードから移動しても、処理前のノードへ戻ってくる。</summary>
        /// <returns>述語の処理を行う前に対象としていたノード</returns>
        public static T Fork<T>(this ITreeNode<T> self, object sentence) where T : ITreeNode<T> {
            //action?.Invoke();
            return (T)self;
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

        #region 変換
        /// <summary>各ノードから生成したデータとそのインデックスをペアとするコレクションを取得する。</summary>
        /// <typeparam name="U">生成するデータの型</typeparam>
        /// <typeparam name="T">各ノードの型</typeparam>
        /// <param name="self">始点となるノード</param>
        /// <param name="conv">変換関数</param>
        public static Dictionary<NodeIndex, U> ToNodeMap<T, U>(this ITreeNode<T> self, Func<T, U> conv)
        where T : ITreeNode<T> {
            return self.Levelorder().ToDictionary(x => x.NodeIndex(), x => conv(x));
        }
        /// <summary>各ノードとそのインデックスをペアとするコレクションを取得する。</summary>
        /// <typeparam name="T">各ノードの型</typeparam>
        /// <param name="self">始点となるノード</param>
        public static Dictionary<NodeIndex, T> ToNodeMap<T>(this ITreeNode<T> self)
        where T : ITreeNode<T> {
            return self.ToNodeMap(x => x);
        }
        /// <summary>各ノードから生成したシリアライズ用のインスタンスとそのインデックスを、<see cref="XmlSerializer"/>によってシリアライズ可能なコレクションとして取得する。</summary>
        /// <typeparam name="U">シリアライズするオブジェクトの型</typeparam>
        /// <typeparam name="T">各ノードの型</typeparam>
        /// <param name="self">始点となるノード</param>
        /// <param name="conv">各ノードからシリアライズ用のオブジェクトを取得する関数</param>
        public static SerializableNodeMap<U> ToSerializableNodeMap<U, T>(this ITreeNode<T> self, Func<T, U> conv)
        where T : ITreeNode<T> {
            return new SerializableNodeMap<U>(self.ToNodeMap(conv));
        }
        /// <summary>各ノードとそのインデックスを、<see cref="XmlSerializer"/>によってシリアライズ可能なコレクションとして取得する。</summary>
        /// <typeparam name="T">シリアライズするノードの型</typeparam>
        public static SerializableNodeMap<T> ToSerializableNodeMap<T>(this ITreeNode<T> self)
        where T : ITreeNode<T> {
            return self.ToSerializableNodeMap(x => x);
        }
        /// <summary>文字列で樹形図を生成する。</summary>
        /// <typeparam name="T">ノードの型</typeparam>
        /// <param name="self">対象ノード</param>
        /// <param name="tostring">ノードを表す文字列へ変換</param>
        /// <returns>樹形図</returns>
        public static string ToTreeDiagram<T>(this ITreeNode<T> self, Func<T, string> tostring) where T : ITreeNode<T> {
            string branch = "├ ";
            string lastBranch = "└ ";
            string through = "│ ";
            string blank = "  ";

            string createLine(IEnumerable<T> nodes, NodeIndex idx) {
                var line = string.Concat(nodes.Zip(idx).Select(pair => pair.First.IsRoot() ? "" : pair.First.HasNextSibling() ? through : blank));
                return line;
            }
            var strlit = new List<string>();
            foreach (var n in self.Preorder()) {
                var line = createLine(n.Upstream().Reverse(), n.NodeIndex());
                var head = n.IsRoot() ? "" : n.HasNextSibling() ? branch : lastBranch;
                line = line + head + tostring(n) + Environment.NewLine;
                strlit.Add(line);
            }
            return string.Concat(strlit);
        }
        #endregion

        #region 探索メソッド
       
        /// <summary>現在のノードから子孫方向へ順に、各キーが全て一致するノードを探索</summary>
        /// <typeparam name="T">ノードの型</typeparam>
        /// <typeparam name="Trc">キー</typeparam>
        /// <param name="self"></param>
        /// <param name="selector">各要素からキーを選択する</param>
        /// <param name="trace">現在のノードから子孫方向へ順に比較するキー</param>
        /// <param name="comparer"></param>
        /// <returns>全てのキーが一致したノード</returns>
        public static IEnumerable<T> DescendArrivals<T, Trc>(this ITreeNode<T> self, Func<T, Trc> selector, IEnumerable<Trc> trace, IEqualityComparer<Trc>? comparer = null) where T : ITreeNode<T> {
            comparer ??= EqualityComparer<Trc>.Default;
            var matchs = new SequenceScroller<Trc>(trace);
            var startdpth = self.Depth();
            return self.Evolve(cur => {
                int curlv = cur.Depth() - startdpth;
                if(matchs.TryMoveTo(curlv)) {
                    var cld = comparer.Equals(matchs.Current,selector(cur)) ? cur.Children : Array.Empty<T>();
                    if(matchs.TryNext()) {
                        return cld.Where(x => comparer.Equals(matchs.Current, selector(x)));
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
        /// <summary>現在のノードから子孫方向へ順に、各キーが全て一致するノードを探索し、その経路を返す</summary>
        /// <typeparam name="T">ノードの型</typeparam>
        /// <typeparam name="Trc">キー</typeparam>
        /// <param name="self"></param>
        /// <param name="selector">各要素からキーを選択する</param>
        /// <param name="trace">現在のノードから子孫方向へ順に比較するキー</param>
        /// <param name="comparer"></param>
        /// <returns>全てのキーが一致したノードへの経路</returns>
        public static IReadOnlyList<IEnumerable<T>> DescendTraces<T,Trc>(this ITreeNode<T> self,Func<T,Trc> selector,IEnumerable<Trc> trace,IEqualityComparer<Trc>? comparer = null) where T : ITreeNode<T> {
            comparer ??= EqualityComparer<Trc>.Default;
            var peak = self.DescendArrivals(selector,trace, comparer);
            var lst = new List<IEnumerable<T>>();
            foreach(var pk in peak) {
                lst.Add(pk.Upstream().TakeWhile(a=>!object.ReferenceEquals(a, self)).Append((T)self).Reverse().ToArray());
            }
            return lst.AsReadOnly();
        }
        #endregion
    }
}
