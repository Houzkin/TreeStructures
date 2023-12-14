using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TreeStructures.Xml.Serialization;

namespace TreeStructures.Xml.Serialization {
    /// <summary><see cref="XmlSerializer"/>によってシリアライズ・デシリアライズ可能な、各ノードのインデックスとそのデータの組を表す。</summary>
    /// <typeparam name="T">シリアライズ・デシリアライズを行う型</typeparam>
    public class SerializableNodeMap<T> : ReadOnlyDictionary<NodeIndex, T>, IXmlSerializable {
        /// <summary>新規インスタンスを初期化する。</summary>
        public SerializableNodeMap() : base(new Dictionary<NodeIndex, T>()) { }
        /// <summary>指定した<see cref="IDictionary{TKey, TValue}"/>をラップする新規スタンスを初期化する。</summary>
        /// <param name="map">ラップする Dictionary</param>
        public SerializableNodeMap(IDictionary<NodeIndex, T> map) : base(map) { }

        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema() {
            return null;
        }
        /// <summary>XMLを読み込む。</summary>
        public void ReadXml(System.Xml.XmlReader reader) {
            var dic = DictionarySerializeManager<NodeIndexCode, T>.Read(reader);
            foreach (var p in dic) this.Dictionary.Add(p.Key.ToNodeIndex(), p.Value);
        }
        /// <summary>XMLとして書き込む。</summary>
        public void WriteXml(System.Xml.XmlWriter writer) {
            DictionarySerializeManager<NodeIndexCode, T>
                .Write(this.Dictionary.ToDictionary(x => new NodeIndexCode(x.Key), x => x.Value), writer);
        }

        /// <summary><see cref="NodeIndex"/>を文字列として表す。</summary>
        public class NodeIndexCode {
            string _code = "[]";
            /// <summary>現在のインスタンスが示す<see cref="NodeIndex"/>を文字列として取得・設定する。</summary>
            public string IndexCode {
                get { return _code; }
                set {
                    _code = new NodeIndex(CodeToArray(value)).ToString();
                }
            }
            /// <summary>新規インスタンスを初期化する。</summary>
            public NodeIndexCode() { }
            /// <summary>新規インスタンスを初期化する。</summary>
            /// <param name="ni">このインスタンスが示す NodeIndex</param>
            public NodeIndexCode(NodeIndex ni) { _code = ni.ToString(); }
            /// <summary>現在のインスタンスが示す<see cref="NodeIndex"/>を取得・設定する。</summary>
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
            public override bool Equals(object obj) {
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
