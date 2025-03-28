using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeStructures{

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
		/// <inheritdoc/>
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
