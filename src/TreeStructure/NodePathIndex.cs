using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Linq;

namespace TreeStructures {

    /// <summary>ノードをたどる道筋を定義する。</summary>
    /// <typeparam name="T">各ノードが示す型</typeparam>
    public interface INodePath<T> : IEnumerable<T> {
        /// <summary>指定したレベルでのパスを取得する。</summary>
        /// <param name="level">レベル</param>
        T this[int level] { get; }
        /// <summary>
        /// 現在のインスタンスのRootからの深さを取得する。
        /// </summary>
        int Depth { get; } //Level に変更した方がよい？
    }
    /// <summary>ノードをだとる道筋を示す。</summary>
    /// <typeparam name="T">各ノードを示すデータ型</typeparam>
    public class NodePath<T> : INodePath<T> {
        readonly IReadOnlyList<T> _path;
        /// <summary>新規インスタンスを初期化する。</summary>
        public NodePath(params T[] path) : this(path ?? Array.Empty<T>().AsEnumerable()) { }
        /// <summary>新規インスタンスを初期化する。</summary>
        public NodePath(IEnumerable<T> path) { _path = path.ToArray(); }
        /// <summary>
        /// ルートノードから指定したノードまでのパスを生成する。
        /// </summary>
        /// <param name="node">ノード</param>
        /// <param name="conv">各ノードから取得するパス</param>
        public static NodePath<T> Create<TNode>(TNode node, Converter<TNode, T> conv) where TNode : ITreeNode<TNode> {
            return new NodePath<T>(node.Upstream().Select(x => conv(x)).Reverse());
        }
        /// <summary>指定したレベルでのパスを取得する。</summary>
        /// <param name="level">レベル</param>
        public T this[int level] {
            get { return this._path[level]; }
        }
        /// <summary>現在のインスタンスのRootからの深さを取得する。</summary>
        public int Depth {
            get {
                if (this._path.Any()) return this._path.Count() - 1;
                else return 0;
            }
        }
        /// <summary>文字列として表す。</summary>
        /// <param name="sepalater">各ノードのパスの区切りを指定する。</param>
        public string ToString(string sepalater) {
            var s = _path.FirstOrDefault()?.ToString() ?? "";
            sepalater = sepalater ?? "";
            foreach (var str in _path.Skip(1)) {
                s += sepalater;
                s += str?.ToString() ?? "";
            }
            return s;
        }
        /// <summary>文字列として表す。</summary>
        public override string ToString() {
            return this.ToString("/");
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return _path.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return _path.GetEnumerator();
        }
    }

    /// <summary>
    /// 階層構造においてノードの位置を示すインデックスを表す。
    /// </summary>
    public struct NodeIndex : INodePath<int> {
        readonly IReadOnlyList<int> _nodePath;
        /// <summary>新規インスタンスを初期化する。</summary>
        /// <param name="nodePath">ルートを除く、各階層でのインデックス</param>
        public NodeIndex(params int[] nodePath) {
            if (nodePath == null) {
                this._nodePath = new List<int>();
            } else {
                this._nodePath = new List<int>(nodePath);
            }
        }
        /// <summary>新規インスタンスを初期化する。</summary>
        /// <param name="nodePath">ルートを除く、各階層でのインデックス</param>
        public NodeIndex(IEnumerable<int> nodePath)
            : this(nodePath == null ? null : nodePath.ToArray()) { }

        /// <summary>指定された階層におけるコレクションのインデックスを取得する。
        /// <para>ルートを示す0階層指定では定数0、指定された階層が存在しなかった場合は-1を返す。</para></summary>
        /// <param name="level">階層を指定する。</param>
        public int this[int level] {
            get {
                var lv = level - 1;
                if (lv < -1 || _nodePath.Count <= lv) {
                    return -1;
                } else if (lv == -1) {
                    return 0;
                }
                return _nodePath[lv];
            }
        }
        /// <summary>このパスコードが示すノードのRootからの深さを取得する。</summary>
        public int Depth {
            get {
                if (_nodePath == null) return 0;
                return _nodePath.Count;
            }
        }
        /// <summary>パスコードを文字列として表す。</summary>
        public override string ToString() {
            string str = "[";
            for (int i = 0; i < _nodePath.Count; i++) {
                if (0 < i) str += ",";
                str += _nodePath[i].ToString();
            }
            str += "]";
            return str;
        }
        /// <summary>現在のオブジェクトと等しいかどうかを判断する。</summary>
        public override bool Equals(object? obj) {
            if (obj is NodeIndex) {
                var ob = (NodeIndex)obj;
                if (this.SequenceEqual(ob)) return true;
            }
            return false;
        }
        /// <summary>現在のオブジェクトを表す文字列を返す。</summary>
        public override int GetHashCode() {
            return this.ToArray().GetHashCode();
        }
        IEnumerator<int> IEnumerable<int>.GetEnumerator() {
            if (_nodePath == null) return Array.Empty<int>().GetEnumerator() as IEnumerator<int>;
            return this._nodePath.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            if (_nodePath == null) return Array.Empty<int>().GetEnumerator();
            return this._nodePath.GetEnumerator();
        }
        /// <summary>
        /// 先行順で並び替えを行うための比較方法を実装したオブジェクトを返す。
        /// </summary>
        public static IComparer<NodeIndex> GetPreorderComparer() {
            var cpn = new Comparison<NodeIndex>((x, y) => {
                var lv = x.ToArray().Zip(y.ToArray(), (xi, yi) => xi - yi).FirstOrDefault(s => s != 0);
                if (lv != 0) return lv;
                return x.Depth - y.Depth;
            });
            return Comparer<NodeIndex>.Create(cpn);
        }
        /// <summary>
        /// 後行順で並び替えを行うための比較方法を実装したオブジェクトを返す。
        /// </summary>
        public static IComparer<NodeIndex> GetPostorderComparer() {
            var cpn = new Comparison<NodeIndex>((x, y) => {
                var lv = x.ToArray().Zip(y.ToArray(), (xi, yi) => xi - yi).FirstOrDefault(s => s != 0);
                if (lv != 0) return lv;
                return y.Depth - x.Depth;
            });
            return Comparer<NodeIndex>.Create(cpn);
        }
        /// <summary>
        /// 中間順で並び替えを行うための比較方法を実装したオブジェクトを返す。
        /// </summary>
        public static IComparer<NodeIndex> GetLevelorderComparer() {
            var cpn = new Comparison<NodeIndex>((x, y) => {
                var lv = x.Depth - y.Depth;
                if (lv != 0) return lv;
                return x.ToArray().Zip(y.ToArray(), (xl, yl) => xl - yl)
                    .FirstOrDefault(s => s != 0);
            });
            return Comparer<NodeIndex>.Create(cpn);
        }
    }
}
