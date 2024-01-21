using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

namespace TreeStructures.Xml.Serialization {

    /// <summary>
    /// Provides support for the serialization of <see cref="Dictionary{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of keys.</typeparam>
    /// <typeparam name="TValue">The type of values.</typeparam>
    public class DictionarySerializeManager<TKey, TValue> : IXmlSerializable {
        readonly IDictionary<TKey, TValue> _dictionary;
        /// <summary>
        /// Initializes a new instance with the specified <see cref="Dictionary{TKey, TValue}"/> to support serialization.
        /// </summary>
        /// <param name="dictionary">The dictionary to support.</param>
        public DictionarySerializeManager(IDictionary<TKey, TValue> dictionary) {
            _dictionary = dictionary;
        }
        /// <summary>Gets the schema. This method returns null.</summary>
        public System.Xml.Schema.XmlSchema GetSchema() {
            return null;
        }
        /// <summary>Reads XML.</summary>
        /// <param name="reader">The XmlReader used to read the XML document.</param>
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
        /// <summary>Writes XML.</summary>
        /// <param name="writer">The XmlWriter used to write the XML document.</param>
        public void WriteXml(XmlWriter writer) {
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            var serializer = new XmlSerializer(typeof(KeyValueItem));
            foreach (var item in _dictionary.Keys.Select(key => new KeyValueItem(key, _dictionary[key]))) {
                serializer.Serialize(writer, item, ns);
            }
        }
        /// <summary>
        /// Reads XML for serialized <see cref="Dictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The deserialized <see cref="Dictionary{TKey, TValue}"/>.</returns>
        public static IDictionary<TKey, TValue> Read(XmlReader reader) {
            var mng = new DictionarySerializeManager<TKey, TValue>(new Dictionary<TKey, TValue>());
            mng.ReadXml(reader);
            return mng._dictionary;
        }
        /// <summary>Serializes a <see cref="Dictionary{TKey, TValue}"/> to XML.</summary>
        /// <param name="dictionary">The <see cref="Dictionary{TKey, TValue}"/> to be serialized.</param>
        /// <param name="writer">The writer.</param>
        public static void Write(IDictionary<TKey, TValue> dictionary, XmlWriter writer) {
            var mng = new DictionarySerializeManager<TKey, TValue>(dictionary);
            mng.WriteXml(writer);
        }
        /// <summary>Key-value pair for serialization.</summary>
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
