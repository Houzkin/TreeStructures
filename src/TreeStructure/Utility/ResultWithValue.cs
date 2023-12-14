using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures.Utility {
    //public interface IResultWithValue<TValue> {
    //    bool Result { get; }
    //    TValue Value { get; }
        
    //}

    /// <summary>結果と、結果に付随する値を表す。</summary>
    /// <typeparam name="TValue">値の型</typeparam>
    public struct ResultWithValue<TValue> {
        bool _result;
        TValue _value;
        /// <summary>結果を取得する。</summary>
        public bool Result { get { return _result; } }
        /// <summary>結果に付随している値を取得する。</summary>
        public TValue Value { get { return _value; } }
        /// <summary>結果を true として初期化する。</summary>
        /// <param name="value">値</param>
        public ResultWithValue(TValue value) : this(true, value) { }
        /// <summary>結果と、そのときの値で初期化する。</summary>
        /// <param name="result">結果</param>
        /// <param name="value">値</param>
        public ResultWithValue(bool result, TValue value) {
            _result = result; _value = value;
        }
        /// <summary>結果とその値を表す。</summary>
        public override string ToString() {
            string s = "[" + Result.ToString() + ", ";
            if (Value != null) s += Value.ToString();
            s += "]";
            return s;
        }
        /// <summary>結果のみを反映する。</summary>
        public static implicit operator Boolean(ResultWithValue<TValue> rwv) {
            return rwv.Result;
        }

        /// <summary>結果によって実行する処理を振り分ける。</summary>
        /// <param name="caseTrue">結果が true だった場合の処理</param>
        /// <param name="caseFalse">結果が false だった場合の処理</param>
        public ResultWithValue<TValue> When(Action<TValue>? caseTrue = null, Action<TValue>? caseFalse = null) {
            if (this) {
                caseTrue?.Invoke(this.Value);
            } else {
                caseFalse?.Invoke(this.Value);
            }
            return this;
        }
        /// <summary>結果によって出力関数を振り分ける。</summary>
        /// <typeparam name="TOutput">戻り値の型</typeparam>
        /// <param name="caseTrue">結果が true だった場合の処理</param>
        /// <param name="caseFalse">結果が false だった場合の処理</param>
        public TOutput When<TOutput>(Func<TValue, TOutput> caseTrue, Func<TValue, TOutput> caseFalse) {
            if (caseTrue == null) throw new ArgumentNullException("caseTrue");
            if (caseFalse == null) throw new ArgumentNullException("caseFalse");
            if (this) return caseTrue(this.Value);
            else return caseFalse(this.Value);
        }
        /// <summary>結果に関わらず実行する処理を指定する。</summary>
        /// <param name="resultAction">実行する処理</param>
        public ResultWithValue<TValue> EitherWay(Action<TValue> resultAction) {
            resultAction(this.Value);
            return this;
        }
        /// <summary>結果に関わらず適用する処理を指定する。</summary>
        /// <typeparam name="TOutput">戻り値の型</typeparam>
        /// <param name="resultFunc">適用する関数</param>
        public TOutput EitherWay<TOutput>(Func<TValue, TOutput> resultFunc) {
            return resultFunc(this.Value);
        }
    }
}
