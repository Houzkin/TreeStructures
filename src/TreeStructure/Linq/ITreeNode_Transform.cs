using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Xml.Serialization;

namespace TreeStructures.Linq {
    public static partial class TreeNodeExtenstions {

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
        internal static string ToPhylogeneticTree<T>(this ITreeNode<T> self, Func<T, string> tostring) where T : ITreeNode<T> {
            string node = "┬";
            string lastBranch = "└";
            string through = "─";
            string blank = "  ";
            string leafInnerBr = "┼";

            int len(string comment) => Encoding.GetEncoding("Shift_JIS").GetByteCount(comment);

            throw new NotImplementedException();
        }
        #endregion
    }
}
