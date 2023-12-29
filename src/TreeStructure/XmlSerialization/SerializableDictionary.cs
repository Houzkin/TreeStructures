using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

namespace TreeStructures.Xml.Serialization {

    /// <summary>
    /// Provides a serializable <see cref="Dictionary{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">Type of keys</typeparam>
    /// <typeparam name="TValue">Type of values</typeparam>
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable where TKey : notnull {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public SerializableDictionary() : base() { }

        /// <summary>
        /// Initializes a new instance with the specified capacity.
        /// </summary>
        /// <param name="capacity">Initial capacity</param>
        public SerializableDictionary(int capacity) : base(capacity) { }

        /// <summary>
        /// Initializes a new instance with the specified comparer.
        /// </summary>
        /// <param name="comparer">Comparer</param>
        public SerializableDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }

        /// <summary>
        /// Initializes a new instance by copying the specified dictionary.
        /// </summary>
        /// <param name="dictionary">Dictionary to copy</param>
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }

        /// <summary>
        /// Initializes a new instance with the specified capacity and comparer.
        /// </summary>
        /// <param name="capacity">Initial capacity</param>
        /// <param name="comparer">Comparer</param>
        public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }

        /// <summary>
        /// Initializes a new instance by copying the specified dictionary with the specified comparer.
        /// </summary>
        /// <param name="dictionary">Dictionary to copy</param>
        /// <param name="comparer">Comparer</param>
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }

        DictionarySerializeManager<TKey, TValue>? _manager;
        DictionarySerializeManager<TKey, TValue> Manager {
            get {
                _manager ??= new DictionarySerializeManager<TKey, TValue>(this);
                return _manager;
            }
        }

        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema() {
            return Manager.GetSchema();
        }

        /// <summary>
        /// Reads XML.
        /// </summary>
        public void ReadXml(XmlReader reader) {
            Manager.ReadXml(reader);
        }

        /// <summary>
        /// Writes XML.
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(XmlWriter writer) {
            Manager.WriteXml(writer);
        }
    }
}
