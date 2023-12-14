using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Utility;
using TreeStructures.Collections;
using System.Runtime.CompilerServices;
using System.Buffers;
using TreeStructures.Internals;
using System.Text.Json;

namespace TreeStructures.EventManagement {

    /// <summary>
    /// 再帰的、または重複する処理を無視する
    /// </summary>
    public class UniqueOperationExecutor {
        /// <summary>キーとそれに対応する処理を管理する</summary>
        readonly protected Dictionary<string, CountOperationPair> Operations = new();
        readonly Dictionary<int,CountOperationPair> TempOperations = new();
        /// <summary>キーとそれに対応する処理を登録する</summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Register(string key,Action action) {
            Result<CountOperationPair>.Of(Operations.TryGetValue, key).When(
                o => throw new InvalidOperationException("指定されたキーは既に登録されています。"),
                x => Operations[key] = new CountOperationPair() { Count = 0, Operation = action });
        }
        /// <summary>初回メソッド呼び出し時と、戻り値の最後のDispose時で、<paramref name="getPropertyValue"/>の値が変化した場合、<paramref name="key"/>によって指定された処理を実行する</summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="key">登録されている処理を示すkey</param>
        /// <param name="getPropertyValue">評価する値を指定する</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"><paramref name="key"/>に対応する処理が登録されていない</exception>
        public IDisposable LateEvalute<TProp>(string key,Func<TProp> getPropertyValue) {
            var ele = Result<CountOperationPair>.Of(Operations.TryGetValue, key).When(
                o => { o.Count++; return o; },
                x => throw new KeyNotFoundException("指定されたキーは登録されていません。"));
            var val = getPropertyValue();
            return new DisposableObject(() => {
                if(ele.Count == 1 && !Equals(val, getPropertyValue())) {
                    ele.Operation?.Invoke();
                }
                ele.Count--;
            });
        }
        /// <summary>戻り値の最後のDispose時に<paramref name="key"/>によって指定された処理を実行する</summary>
        /// <param name="key">登録されている処理を指定するキー</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public IDisposable ExecuteUnique(string key) {
            var ele = Result<CountOperationPair>.Of(Operations.TryGetValue, key).When(
                o => { o.Count++; return o; },
                x => throw new KeyNotFoundException("指定されたキーは登録されていません。"));
            
            return new DisposableObject(() => {
                if(ele.Count == 1) {
                    ele.Operation?.Invoke();
                }
                ele.Count--;
            });
        }
        /// <summary>戻り値の最後のDispose時に<paramref name="operation"/>を実行する</summary>
        /// <remarks>識別は<paramref name="operation"/>のメタデータトークンの値を使用します。</remarks>
        /// <param name="operation">重複を防止して実行する処理</param>
        /// <returns></returns>
        public IDisposable ExecuteUnique(Action operation) {
            int id = operation.Method.MetadataToken;
            var ele = Result<CountOperationPair>.Of(TempOperations.TryGetValue, id).When(
                o => { o.Count++; return o; },
                x => {
                    TempOperations[id] = new CountOperationPair() { Count = 0, Operation = operation };
                    return TempOperations[id];
                    });
            return new DisposableObject(() => {
                if (ele.Count == 1) {
                    ele.Operation?.Invoke();
                    TempOperations.Remove(id);
                }
                ele.Count--;
            });
        }
        /// <summary>カウンター</summary>
        protected class CountOperationPair {
            /// <summary>
            /// コンストラクタ
            /// </summary>
            public CountOperationPair() {
            }
            /// <summary>カウンター</summary>
            public int Count { get; set; }
            /// <summary>処理</summary>
            public Action? Operation { get; init; }
        }
    }
}
