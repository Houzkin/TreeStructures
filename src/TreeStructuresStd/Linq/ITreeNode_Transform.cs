using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Xml.Serialization;
using System.Xml.Serialization;
using System.Collections.Specialized;
using System.ComponentModel;

namespace TreeStructures.Linq {
    public static partial class TreeNodeExtenstions {

		#region Representing(TreeNode to other)
		/// <summary>Gets a collection of pairs consisting of data generated from each node and its index.</summary>
		/// <typeparam name="U">Type of the data to generate</typeparam>
		/// <typeparam name="T">Type of each node</typeparam>
		/// <param name="self">Starting node</param>
		/// <param name="conv">Conversion function</param>
		/// <returns>A dictionary containing pairs of node index and the generated data</returns>
		public static Dictionary<NodeIndex, U> ToNodeMap<T, U>(this ITreeNode<T> self, Func<T, U> conv)
        where T : ITreeNode<T> {
            return self.LevelOrder().ToDictionary(x => x.GetNodeIndex(), x => conv(x));
        }
        /// <summary>Gets a collection of pairs consisting of each node and its index.</summary>
        /// <typeparam name="T">Type of each node</typeparam>
        /// <param name="self">Starting node</param>
        /// <returns>A dictionary containing pairs of node index and the node</returns>
        public static Dictionary<NodeIndex, T> ToNodeMap<T>(this ITreeNode<T> self)
        where T : ITreeNode<T> {
            return self.ToNodeMap(x => x);
        }
        /// <summary>Gets a collection of pairs consisting of each node's serialized instance and its index, as a collection that can be serialized by <see cref="XmlSerializer"/>.</summary>
        /// <typeparam name="U">Type of the object to be serialized</typeparam>
        /// <typeparam name="T">Type of each node</typeparam>
        /// <param name="self">Starting node</param>
        /// <param name="conv">Function to obtain a serialized object from each node</param>
        /// <returns>A SerializableNodeMap collection containing pairs of node index and the serialized object</returns>
        public static SerializableNodeMap<U> ToSerializableNodeMap<U, T>(this ITreeNode<T> self, Func<T, U> conv)
        where T : ITreeNode<T> {
            return new SerializableNodeMap<U>(self.ToNodeMap(conv));
        }
        /// <summary>Gets a collection of pairs consisting of each node and its index, as a collection that can be serialized by <see cref="XmlSerializer"/>.</summary>
        /// <typeparam name="T">Type of the node to be serialized</typeparam>
        /// <param name="self">Starting node</param>
        /// <returns>A SerializableNodeMap collection containing pairs of node index and the node itself</returns>
        public static SerializableNodeMap<T> ToSerializableNodeMap<T>(this ITreeNode<T> self)
        where T : ITreeNode<T> {
            return self.ToSerializableNodeMap(x => x);
        }
		/// <summary>Generates a tree diagram as a string.</summary>
		/// <typeparam name="T">Type of the node</typeparam>
		/// <param name="self">Current node</param>
		/// <param name="toString">Function to convert a node to its string representation</param>
		/// <param name="nullText">The string to display for null nodes. If null, null nodes are ignored.</param>
		/// <returns>Tree diagram</returns>
		//public static string ToTreeDiagram<T>(this ITreeNode<T> self, Func<T, string> tostring) where T : ITreeNode<T> {
		//    string branch = "├ ";
		//    string lastBranch = "└ ";
		//    string through = "│ ";
		//    string indent = "  ";

		//    string createLine(IEnumerable<T> nodes, NodeIndex idx) {
		//        //var line = string.Concat(nodes.Zip(idx).Select(pair => pair.First.IsRoot() ? "" : pair.First.HasNextSibling() ? through : blank));
		//        var line = string.Concat(nodes.Zip(idx, (fst, sec) => new { First = fst, Second = sec }).Select(pair => pair.First.IsRoot() ? "" : pair.First.HasNextSibling() ? through : indent));
		//        return line;
		//    }
		//    var strlit = new List<string>();
		//    foreach (var n in self.PreOrder()) {
		//        var line = createLine(n.Upstream().Reverse(), n.GetNodeIndex());
		//        var head = n.IsRoot() ? "" : n.HasNextSibling() ? branch : lastBranch;
		//        line = line + head + tostring(n) + Environment.NewLine;
		//        strlit.Add(line);
		//    }
		//    return string.Concat(strlit);
		//}
		public static string ToTreeDiagram<T>(this ITreeNode<T> self, Func<T, string> toString, string? nullText = null) where T : class, ITreeNode<T>{
            string branch = "├ ";
            string lastBranch = "└ ";
            string through = "│ ";
            string indent = "  ";

            var strlit = new List<string>();
            var wp = ((T)self).AsValuedTreeNode(x => x.Children, x => x);
            var wprst = nullText is null ? wp.PreOrder().Where(x => x.Value != null) : wp.PreOrder();
            bool hasNextSibling(ReadOnlyValuedTreeNode<T,T> node) {
                if(nullText is null) {
                    return node.HasNextSibling(x => x.Value != null);
                } else {
                    return node.HasNextSibling();
                }
            }

            foreach (var n in wprst) {
                var line = string.Concat(n.Ancestors().Reverse().Select(nd => nd.IsRoot() ? "" : hasNextSibling(nd) ? through : indent));
                var head = n.IsRoot() ? "" : hasNextSibling(n) ? branch : lastBranch;
                var ndstring = n.Value is null ? nullText : toString(n.Value);
                line = line + head + ndstring + Environment.NewLine;
                strlit.Add(line);
            }
            return string.Concat(strlit);
        }
        internal static string ToPhylogeneticDiagram<T>(this ITreeNode<T> self, Func<T, string> tostring) where T : ITreeNode<T> {
            string firstnode = "┬";
            string singlenode = "─";
            string lastBranch = "└";
            string innernode = "├";
            string through = "│";
            string indent = "  ";
            string blank = "  ";
            string leafInnerBr = "┼";

            var strlst = new List<string>();

            var order = self.PostOrder();
            foreach(var nd in order) {
                //var line = string.Concat(nd.Ancestors().Reverse().Select(x=>x.IsFirstSibling() && !x.HasNextSibling() ? singlenode :))
                string line = "";
                foreach(var n in nd.Upstream().Reverse()) {
                    if (n.IsRoot()) {

                    }else if (!n.Children.Any()) {

                    }
                }
                strlst.Add(line);
            }

            throw new NotImplementedException();
        }
        #endregion
    }
}
