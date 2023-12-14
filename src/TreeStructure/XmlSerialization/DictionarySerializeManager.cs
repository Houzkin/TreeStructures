using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

namespace TreeStructures.Xml.Serialization {

    /// <summary><see cref="Dictionary{TKey, TValue}"/>のシリアライズをサポートする。</summary>
    /// <typeparam name="TKey">キーの型</typeparam>
    /// <typeparam name="TValue">値の型</typeparam>
    public class DictionarySerializeManager<TKey, TValue> : IXmlSerializable {
        readonly IDictionary<TKey, TValue> _dictionary;
        /// <summary>シリアライズをサポートする<see cref="Dictionary{TKey, TValue}"/>を指定して、新規インスタンスを初期化する。</summary>
        /// <param name="dictionary">サポートするDictionary</param>
        public DictionarySerializeManager(IDictionary<TKey, TValue> dictionary) {
            _dictionary = dictionary;
        }
        /// <summary>スキーマを取得する。このメソッドはnullを返す。</summary>
        public System.Xml.Schema.XmlSchema GetSchema() {
            return null;
        }
        /// <summary>XMLを読み込む。</summary>
        public void ReadXml(XmlReader reader) {

            var serializer = new XmlSerializer(typeof(KeyValueItem));

            reader.Read();
            if (reader.IsEmptyElement)
                return;

            try {
                while (reader.NodeType != XmlNodeType.EndElement) {
                    if (!serializer.CanDeserialize(reader))
                        return;
                    else {
                        var item = serializer.Deserialize(reader) as KeyValueItem;
                        _dictionary.Add(item.Key, item.Value);
                    }
                }
            } finally {
                reader.Read();
            }
        }
        /// <summary>XMLを書き込む。</summary>
        public void WriteXml(XmlWriter writer) {
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            var serializer = new XmlSerializer(typeof(KeyValueItem));
            foreach (var item in _dictionary.Keys.Select(key => new KeyValueItem(key, _dictionary[key]))) {
                serializer.Serialize(writer, item, ns);
            }
        }
        /// <summary>XMLにシリアライズされた<see cref="Dictionary{TKey, TValue}"/>の読み込みを行う。</summary>
        /// <param name="reader">リーダー</param>
        /// <returns>読み込んだ<see cref="Dictionary{TKey, TValue}"/></returns>
        public static IDictionary<TKey, TValue> Read(XmlReader reader) {
            var mng = new DictionarySerializeManager<TKey, TValue>(new Dictionary<TKey, TValue>());
            mng.ReadXml(reader);
            return mng._dictionary;
        }
        /// <summary><see cref="Dictionary{TKey, TValue}"/>をXMLにシリアライズする。</summary>
        /// <param name="dictionary">出力する<see cref="Dictionary{TKey, TValue}"/></param>
        /// <param name="writer">ライター</param>
        public static void Write(IDictionary<TKey, TValue> dictionary, XmlWriter writer) {
            var mng = new DictionarySerializeManager<TKey, TValue>(dictionary);
            mng.WriteXml(writer);
        }
        /// <summary>シリアライズ用のキーと値のベア。</summary>
        public class KeyValueItem {
            /// <summary>キー</summary>
            public TKey Key { get; set; }
            /// <summary>値</summary>
            public TValue Value { get; set; }
            /// <summary>キーと値を指定して、新規インスタンスを食器化する。</summary>
            /// <param name="key">キー</param>
            /// <param name="value">値</param>
            public KeyValueItem(TKey key, TValue value) {
                Key = key;
                Value = value;
            }
            /// <summary>新規インスタンスを初期化する。</summary>
            public KeyValueItem() { }
        }
    }
}
