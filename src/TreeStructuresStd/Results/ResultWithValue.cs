using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures.Results {
    //public interface IResultWithValue<TValue> {
    //    bool Result { get; }
    //    TValue Value { get; }

    //}

    /// <summary>Represents a result and the associated value.</summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public struct ResultWithValue<TValue> {
        bool _result;
        TValue _value;
        /// <summary>Gets the result.</summary>
        public bool Result { get { return _result; } }
        /// <summary>Gets the value associated with the result.</summary>
        public TValue Value { get { return _value; } }
        /// <summary>Initializes the result as true.</summary>
        /// <param name="value">The associated value.</param>
        public ResultWithValue(TValue value) : this(true, value) { }
        /// <summary>Initializes with the result and its associated value.</summary>
        /// <param name="result">The result.</param>
        /// <param name="value">The associated value.</param>
        public ResultWithValue(bool result, TValue value) {
            _result = result; _value = value;
        }
        /// <summary>Represents the result and its value.</summary>
        public override string ToString() {
            string s = "[" + Result.ToString() + ", ";
            if (Value != null) s += Value.ToString();
            s += "]";
            return s;
        }
        /// <summary>Reflects only the result.</summary>
        public static implicit operator Boolean(ResultWithValue<TValue> rwv) {
            return rwv.Result;
        }

        /// <summary>Routes the execution based on the result.</summary>
        /// <param name="caseTrue">The action to execute when the result is true.</param>
        /// <param name="caseFalse">The action to execute when the result is false.</param>
        public ResultWithValue<TValue> When(Action<TValue>? caseTrue = null, Action<TValue>? caseFalse = null) {
            if (this) {
                caseTrue?.Invoke(this.Value);
            } else {
                caseFalse?.Invoke(this.Value);
            }
            return this;
        }
        /// <summary>Routes the output function based on the result.</summary>
        /// <typeparam name="TOutput">The type of the return value.</typeparam>
        /// <param name="caseTrue">The function to execute when the result is true.</param>
        /// <param name="caseFalse">The function to execute when the result is false.</param>
        public TOutput When<TOutput>(Func<TValue, TOutput> caseTrue, Func<TValue, TOutput> caseFalse) {
            if (caseTrue == null) throw new ArgumentNullException("caseTrue");
            if (caseFalse == null) throw new ArgumentNullException("caseFalse");
            if (this) return caseTrue(this.Value);
            else return caseFalse(this.Value);
        }
        /// <summary>Specifies an action to execute regardless of the result.</summary>
        /// <param name="resultAction">The action to execute.</param>
        public ResultWithValue<TValue> EitherWay(Action<TValue> resultAction) {
            resultAction(this.Value);
            return this;
        }
        /// <summary>Specifies a function to apply regardless of the result.</summary>
        /// <typeparam name="TOutput">The type of the return value.</typeparam>
        /// <param name="resultFunc">The function to apply.</param>
        public TOutput EitherWay<TOutput>(Func<TValue, TOutput> resultFunc) {
            return resultFunc(this.Value);
        }
    }
}
