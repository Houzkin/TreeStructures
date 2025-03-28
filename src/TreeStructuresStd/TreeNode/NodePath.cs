using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Linq;

namespace TreeStructures {
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
                else return -1;
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

}
