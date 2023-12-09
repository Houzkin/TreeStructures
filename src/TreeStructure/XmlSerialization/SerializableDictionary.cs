using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

namespace TreeStructure.Xml.Serialization {

    /// <summary>シリアライズ可能な<see cref="Dictionary{TKey, TValue}"/>を提供する。</summary>
    /// <typeparam name="TKey">キーの型</typeparam>
    /// <typeparam name="TValue">値の型</typeparam>
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable where TKey : notnull {
        /// <summary>新規インスタンスを初期化する。</summary>
        public SerializableDictionary() : base() { }

        /// <summary>新規インスタンスを初期化する。</summary>
        /// <param name="capacity">初期量</param>
        public SerializableDictionary(int capacity) : base(capacity) { }

        /// <summary>新規インスタンスを初期化する。</summary>
        /// <param name="comparer">比較演算子</param>
        public SerializableDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }

        /// <summary>新規インスタンスを初期化する。</summary>
        /// <param name="dictionary">コピーするDictionary</param>
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }

        /// <summary>新規インスタンスを初期化する。</summary>
        /// <param name="capacity">初期量</param>
        /// <param name="comparer">比較演算子</param>
        public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }

        /// <summary>新規インスタンスを初期化する。</summary>
        /// <param name="dictionary">コピーするDictionary</param>
        /// <param name="comparer">比較演算子</param>
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
        /// <summary>XMLを読み込む。</summary>
        public void ReadXml(XmlReader reader) {
            Manager.ReadXml(reader);
        }
        /// <summary>XMLを書き込む。</summary>
        /// <param name="writer"></param>
        public void WriteXml(XmlWriter writer) {
            Manager.WriteXml(writer);
        }
    }
}
