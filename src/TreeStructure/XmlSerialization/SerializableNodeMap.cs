using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TreeStructures.Xml.Serialization;

namespace TreeStructures.Xml.Serialization {
    /// <summary>
    /// Represents a pair of node index and its data that can be serialized and deserialized by <see cref="XmlSerializer"/>.
    /// </summary>
    /// <typeparam name="T">Type for serialization and deserialization</typeparam>
    public class SerializableNodeMap<T> : ReadOnlyDictionary<NodeIndex, T>, IXmlSerializable {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public SerializableNodeMap() : base(new Dictionary<NodeIndex, T>()) { }

        /// <summary>
        /// Initializes a new instance wrapping the specified <see cref="IDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="map">Dictionary to wrap</param>
        public SerializableNodeMap(IDictionary<NodeIndex, T> map) : base(map) { }

        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema() {
            return null;
        }

        /// <summary>Reads XML.</summary>
        public void ReadXml(System.Xml.XmlReader reader) {
            var dic = DictionarySerializeManager<NodeIndexCode, T>.Read(reader);
            foreach (var p in dic) {
                this.Dictionary.Add(p.Key.ToNodeIndex(), p.Value);
            }
        }

        /// <summary> Writes XML.</summary>
        public void WriteXml(System.Xml.XmlWriter writer) {
            DictionarySerializeManager<NodeIndexCode, T>
                .Write(this.Dictionary.ToDictionary(x => new NodeIndexCode(x.Key), x => x.Value), writer);
        }

        /// <summary>Represents <see cref="NodeIndex"/> as a string.</summary>
        public class NodeIndexCode {
            string _code = "[]";
            /// <summary>Gets or sets the <see cref="NodeIndex"/> represented by the current instance as a string.</summary>
            public string IndexCode {
                get { return _code; }
                set {
                    _code = new NodeIndex(CodeToArray(value)).ToString();
                }
            }
            /// <summary>Initializes a new instance</summary>
            public NodeIndexCode() { }
            /// <summary>Initializes a new Instance</summary>
            /// <param name="ni">The NodeIndex represented by this instance.</param>
            public NodeIndexCode(NodeIndex ni) { _code = ni.ToString(); }
            /// <summary>Gets or sets the <see cref="NodeIndex"/> represented by the current instance.</summary>
            public NodeIndex ToNodeIndex() {
                return new NodeIndex(CodeToArray(_code));
            }
            static int[] CodeToArray(string code) {
                return code.Trim('[', ']').Split(',')
                    .Where(x => 0 < x.Length)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
            }
            /// <summary>現在のオブジェクトと等しいかどうかを判断する。</summary>
            public override bool Equals(object? obj) {
                var o = obj as NodeIndexCode;
                if (o == null) return false;
                return o.IndexCode == this.IndexCode;
            }
            /// <summary>ハッシュ関数として機能する。</summary>
            public override int GetHashCode() {
                return IndexCode.GetHashCode();
            }
            /// <summary>現在のオブジェクトを表す文字列を返す。</summary>
            public override string ToString() {
                return IndexCode;
            }
        }
    }
}
