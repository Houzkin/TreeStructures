using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Collections;
using System.Runtime.CompilerServices;
//using System.Buffers;
using TreeStructures.Internals;
using TreeStructures.Results;
//using System.Text.Json;

namespace TreeStructures.Utilities {

    /// <summary>Manages operations, ignoring recursive or duplicate executions.</summary>
    public class UniqueOperationExecutor {
        /// <summary>Manages keys and their corresponding operations.</summary>
        readonly protected Dictionary<string, CountOperationPair> Operations = new();
        readonly Dictionary<int,CountOperationPair> TempOperations = new();
        /// <summary>Registers a key and its corresponding operation.</summary>
        /// <exception cref="InvalidOperationException">Thrown when an invalid operation is detected.</exception>
        public void Register(string key,Action action) {
            ResultWith<CountOperationPair>.Of(Operations.TryGetValue, key).When(
                o => throw new InvalidOperationException("The specified key is already registered."),
                x => Operations[key] = new CountOperationPair() { Count = 0, Operation = action });
        }
        /// <summary>
        /// Executes the operation specified by <paramref name="key"/> when the value returned by <paramref name="getPropertyValue"/> changes, either during the first method call or at the end of the returned value's last Dispose.
        /// </summary>
        /// <typeparam name="TProp">The type of the property.</typeparam>
        /// <param name="key">The key indicating the registered operation.</param>
        /// <param name="getPropertyValue">Specifies the value to evaluate.</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">Thrown when no operation is registered for <paramref name="key"/>.</exception>
        public IDisposable LateEvaluate<TProp>(string key,Func<TProp> getPropertyValue) {
            var ele = ResultWith<CountOperationPair>.Of(Operations.TryGetValue, key).When(
                o => { o.Count++; return o; },
                x => throw new KeyNotFoundException("The specified key is not registered."));
            var val = getPropertyValue();
            return new DisposableObject(() => {
                if(ele.Count == 1 && !Equals(val, getPropertyValue())) {
                    ele.Operation?.Invoke();
                }
                ele.Count--;
            });
        }
        /// <summary>Executes the operation specified by <paramref name="key"/> at the end of the returned value's last Dispose.</summary>
        /// <param name="key">The key indicating the registered operation.</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">Thrown when no operation is registered for <paramref name="key"/>.</exception>
        public IDisposable ExecuteUnique(string key) {
            var ele = ResultWith<CountOperationPair>.Of(Operations.TryGetValue, key).When(
                o => { o.Count++; return o; },
                x => throw new KeyNotFoundException("The specified key is not registered."));
            
            return new DisposableObject(() => {
                if(ele.Count == 1) {
                    ele.Operation?.Invoke();
                }
                ele.Count--;
            });
        }
        /// <summary>Executes the <paramref name="operation"/> at the end of the returned value's last Dispose.</summary>
        /// <remarks>Identification is based on the value of the metadata token of <paramref name="operation"/>.</remarks>
        /// <param name="operation">The operation to be executed, preventing duplication.</param>
        /// <returns></returns>
        public IDisposable ExecuteUnique(Action operation) {
            int id = operation.Method.MetadataToken;
            var ele = ResultWith<CountOperationPair>.Of(TempOperations.TryGetValue, id).When(
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
        /// <summary>A pair of count and the corresponding operation to be executed.</summary>
        protected class CountOperationPair {
            /// <summary>ctr</summary>
            public CountOperationPair() {
            }
            /// <summary>Count</summary>
            public int Count { get; set; }
            /// <summary>Operation</summary>
            public Action? Operation { get; internal set; }
        }
    }
}
