using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures.Utility {

    ///// <summary>文字列に対する、TryParseパターンによって処理されるメソッドを表す。</summary>
    ///// <typeparam name="TValue">出力値の型</typeparam>
    ///// <param name="input">入力する文字列</param>
    ///// <param name="value">出力値</param>
    ///// <returns>実行の成否</returns>
    //public delegate bool TryParseCallback<TValue>(string input, [MaybeNullWhen(false)] out TValue value);

    /// <summary>TryParseパターンによって処理されるメソッドを表す。</summary>
    /// <typeparam name="TInput">入力値の型</typeparam>
    /// <typeparam name="TValue">出力値の型</typeparam>
    /// <param name="input">入力値</param>
    /// <param name="value">出力値</param>
    /// <returns>実行の成否</returns>
    public delegate bool TryCallback<in TInput, TValue>(TInput input, [MaybeNullWhen(false)] out TValue value);

    /// <summary>TryParseパターンによって処理されるメソッドを表す。</summary>
    /// <typeparam name="TValue">出力値の型</typeparam>
    /// <param name="value">出力値</param>
    /// <returns>実行の成否</returns>
    public delegate bool TryCallback<TValue>([MaybeNullWhen(false)] out TValue value);

    ////// <summary>ResultWithValueに関するstaticメソッドを提供する。</summary>
    //public static class Result {
    ///// <summary>文字列に対する、TryParseメソッドをサポートする。</summary>
    ///// <typeparam name="TValue">出力値の型</typeparam>
    ///// <param name="tryParse">TryParseメソッド</param>
    ///// <param name="input">入力文字列</param>
    ///// <returns>結果とその値</returns>
    //public static ResultWithValue<TValue> Of<TValue>(TryParseCallback<TValue> tryParse, string input) {
    //    //TValue value;
    //    if (tryParse(input, out var value))
    //        return new ResultWithValue<TValue>(value);
    //    else
    //        return new ResultWithValue<TValue>();
    //}
    //    /// <summary>TryParseパターンによる処理をサポートする。</summary>
    //    /// <typeparam name="TInput">入力値の型</typeparam>
    //    /// <typeparam name="TValue">出力値の型</typeparam>
    //    /// <param name="tryMethod">Tryメソッド</param>
    //    /// <param name="input">入力値</param>
    //    /// <returns>結果とその値</returns>
    //    public static ResultWithValue<TValue> Of<TInput, TValue>(TryCallback<TInput, TValue> tryMethod, TInput input) {
    //        //TValue value;
    //        if (tryMethod(input, out var value))
    //            return new ResultWithValue<TValue>(value);
    //        else
    //            return new ResultWithValue<TValue>();
    //    }
    //}

    /// <summary>ResultWithValueに関するstaticメソッドを提供する。</summary>
    /// <typeparam name="TValue"></typeparam>
    public static class Result<TValue> {
        /// <summary>TryParseパターンによる処理をサポートする。</summary>
        /// <typeparam name="TInput">入力値の型</typeparam>
        /// <param name="tryMethod"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static ResultWithValue<TValue> Of<TInput>(TryCallback<TInput, TValue> tryMethod, TInput input) {
            if (tryMethod(input, out var value))
                return new ResultWithValue<TValue>(value);
            else
                return new ResultWithValue<TValue>();
        }
        /// <summary>TryParseパターンによる処理をサポートする。</summary>
        /// <typeparam name="TValue">出力値の型</typeparam>
        /// <param name="try">Tryメソッド</param>
        /// <returns>結果とその値</returns>
        public static ResultWithValue<TValue> Of(TryCallback<TValue> @try) {
            //TValue value;
            if (@try(out var value))
                return new ResultWithValue<TValue>(value);
            else
                return new ResultWithValue<TValue>();
        }
    }
}
