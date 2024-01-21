using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Linq;

namespace TreeStructures {

    /// <summary>Represents a path traversing nodes.</summary>
    /// <typeparam name="T">The type represented by each node.</typeparam>
    public interface INodePath<T> : IEnumerable<T>{
        /// <summary>Gets the path at the specified level.</summary>
        /// <param name="level">The level.</param>
        T this[int level] { get; }
        /// <summary>Gets the depth from the root.</summary>
        int Depth { get; }
    }
    /// <summary>Represents a path traversing nodes.</summary>
    /// <typeparam name="T">The data type representing each node.</typeparam>
    public class NodePath<T> : INodePath<T> ,IEquatable<NodePath<T>> {
        readonly IReadOnlyList<T> _path;
        /// <summary>Initializes a new instance of the class.</summary>
        public NodePath(params T[] path) : this(path ?? Array.Empty<T>().AsEnumerable()) { }
        /// <summary>Initializes a new instance of the class.</summary>
        public NodePath(IEnumerable<T> path) { _path = path.ToArray(); }
        /// <summary>
        /// Generates a path from the root node to the specified node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="conv">A converter to obtain the path from each node.</param>
        public static NodePath<T> Create<TNode>(TNode node, Converter<TNode, T> conv) where TNode : ITreeNode<TNode> {
            return new NodePath<T>(node.Upstream().Select(x => conv(x)).Reverse());
        }
        /// <summary>Gets the path at the specified level.</summary>
        /// <param name="level">The level.</param>
        public T this[int level] {
            get { return this._path[level]; }
        }
        /// <summary>Gets the depth from the root of the current instance.</summary>
        public int Depth {
            get {
                if (this._path.Any()) return this._path.Count() - 1;
                else return 0;
            }
        }
        /// <summary>Returns a string representation.</summary>
        /// <param name="separator">Specifies the delimiter for each node's path.</param>
        public string ToString(string separator) {
            var s = _path.FirstOrDefault()?.ToString() ?? "";
            separator = separator ?? "";
            foreach (var str in _path.Skip(1)) {
                s += separator;
                s += str?.ToString() ?? "";
            }
            return s;
        }
        /// <summary>Returns a string representation.</summary>
        public override string ToString() {
            return this.ToString("/");
        }
        /// <summary>Determines whether the current object is equal to another object.</summary>
        public override bool Equals(object? obj) {
            if (obj is INodePath<T> np) {
                if (this.SequenceEqual(np)) return true;
            }
            return false;
        }
        /// <summary>Returns the hash code representing the current object.</summary>
        public override int GetHashCode() {
            return this.Aggregate(0,(total,next)=>HashCode.Combine(total,next));
            //return this.ToArray().GetHashCode();
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return _path.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return _path.GetEnumerator();
        }
        ///<inheritdoc/>
		public bool Equals(NodePath<T>? other) {
            if (other is null) return false;
            return this.SequenceEqual(other);
		}
        /// <inheritdoc/>
		public static bool operator ==(NodePath<T> left, NodePath<T> right) {
            if (left is null && right is null) return true;
            if(left is not null) return left.Equals(right);
            else return right.Equals(left);
        }
        /// <inheritdoc/>
        public static bool operator !=(NodePath<T> left , NodePath<T> right) {
            return !(left == right);
        }
	}

    /// <summary>Represents an index indicating the position of a node in a tree structure.</summary>
    public readonly struct NodeIndex : INodePath<int>,IEquatable<NodeIndex> {
        readonly IReadOnlyList<int> _nodePath;
        /// <summary>Initializes a new instance.</summary>
        /// <param name="nodePath">Branch indices at each level, excluding the root.</param>
        public NodeIndex(params int[] nodePath) {
            if (nodePath == null) {
                this._nodePath = new List<int>();
            } else {
                this._nodePath = new List<int>(nodePath);
            }
        }
        /// <summary>Initializes a new instance.</summary>
        /// <param name="nodePath">Branch indices at each level, excluding the root.</param>
        public NodeIndex(IEnumerable<int> nodePath)
            : this(nodePath == null ? null : nodePath.ToArray()) { }

        /// <summary>Gets the index of the collection at the specified level.
        /// <para>Returns a constant 0 for 0-level specifying the root, and -1 if the specified level does not exist.</para></summary>
        /// <param name="level">Specifies the level.</param>
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
        /// <summary>Gets the depth of the node indicated by this NodeIndex from the root.</summary>
        public int Depth {
            get {
                if (_nodePath == null) return 0;
                return _nodePath.Count;
            }
        }
        /// <summary>Represents as a string.</summary>
        public override string ToString() {
            string str = "[";
            for (int i = 0; i < _nodePath.Count; i++) {
                if (0 < i) str += ",";
                str += _nodePath[i].ToString();
            }
            str += "]";
            return str;
        }
        /// <summary>Determines whether the current object is equal to another object.</summary>
        public override bool Equals(object? obj) {
            if (obj is NodeIndex ob) {
                //var ob = (NodeIndex)obj;
                if (this.SequenceEqual(ob)) return true;
            }
            return false;
        }
		public bool Equals(NodeIndex other) {
            //if(other is null) return false;
            return this.SequenceEqual(other);
		}
        /// <summary>Returns the hash code representing the current object.</summary>
        public override int GetHashCode() {
            return this.Aggregate(0, (total, next) => HashCode.Combine(total, next));
            //return this.ToArray().GetHashCode();
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
        /// Returns an object implementing a comparison method for performing a pre-order sorting.
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
        /// Returns an object implementing a comparison method for performing a post-order sorting.
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
        /// Returns an object implementing a comparison method for performing an in-order sorting.
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

        /// <inheritdoc/>
		public static bool operator ==(NodeIndex left, NodeIndex right) {
			return left.Equals(right);
		}
        /// <inheritdoc/>
		public static bool operator !=(NodeIndex left, NodeIndex right) {
			return !(left == right);
		}
	}
}
