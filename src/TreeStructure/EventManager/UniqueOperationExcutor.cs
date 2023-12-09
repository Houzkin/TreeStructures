using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TreeStructure.Utility;
using TreeStructure.Collections;
using System.Runtime.CompilerServices;
using System.Buffers;
using TreeStructure.Internals;
using System.Text.Json;

namespace TreeStructure.EventManager {
    /// <summary>
    /// プロパティの変更通知を管理します。
    /// </summary>
    public class PropertyChangedEventManager /*: INotifyPropertyChanged*/
        /*where T : INotifyPropertyChanged*/ {
        readonly Dictionary<string, CountValuePair> dic = new();
        readonly object? Self;
        public PropertyChangedEventManager(object self) { Self = self; }

        public event PropertyChangedEventHandler? PropertyChanged;
        void RaisePropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(Self, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>Dispose時に指定した値に変更があれば通知します。重複する通知は発行されません。</summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="property">プロパティの値を取得する関数</param>
        /// <param name="propName">プロパティ名</param>
        public IDisposable LateEvaluateProperty<TProp>(Func<TProp> property,string propName) {

            if (dic.TryGetValue(propName, out var tpl)) {
                tpl.Count++;
            } else {
                dic.Add(propName, new() { Count = 1, Value = property() });
            }
            return new DisposableObject(() => {
                if(dic.TryGetValue(propName, out var tpl)) {
                    tpl.Count--;
                    if (tpl.Count == 0) {
                        if (!Equals(tpl.Value, property())) {
                            RaisePropertyChanged(propName);
                        }
                        dic.Remove(propName);
                    }
                }
            });
        }
        private class CountValuePair {
            public int Count { get; set; }
            public object? Value { get; set; }
        }
    }
    /// <summary>
    /// 再帰的、または重複する処理を無視する
    /// </summary>
    public class UniqueOperationExecutor {
        readonly protected Dictionary<string, CountOperationPair> Operations = new();
        readonly Dictionary<int,CountOperationPair> TempOperations = new();
        public void Register(string key,Action action) {
            Result<CountOperationPair>.Of(Operations.TryGetValue, key).When(
                o => throw new InvalidOperationException("指定されたキーは既に登録されています。"),
                x => Operations[key] = new CountOperationPair() { Count = 0, RaiseEvent = action });
        }
        public IDisposable LateEvalute<TProp>(string key,Func<TProp> getPropertyValue) {
            var ele = Result<CountOperationPair>.Of(Operations.TryGetValue, key).When(
                o => { o.Count++; return o; },
                x => throw new KeyNotFoundException("指定されたキーは登録されていません。"));
            var val = getPropertyValue();
            return new DisposableObject(() => {
                if(ele.Count == 1 && !Equals(val, getPropertyValue())) {
                    ele.RaiseEvent?.Invoke();
                }
                ele.Count--;
            });
        }
        public IDisposable ExecuteUnique(string key) {
            var ele = Result<CountOperationPair>.Of(Operations.TryGetValue, key).When(
                o => { o.Count++; return o; },
                x => throw new KeyNotFoundException("指定されたキーは登録されていません。"));
            
            return new DisposableObject(() => {
                if(ele.Count == 1) {
                    ele.RaiseEvent?.Invoke();
                }
                ele.Count--;
            });
        }
        public IDisposable ExecuteUnique(Action operation) {
            int id = operation.Method.MetadataToken;
            var ele = Result<CountOperationPair>.Of(TempOperations.TryGetValue, id).When(
                o => { o.Count++; return o; },
                x => {
                    TempOperations[id] = new CountOperationPair() { Count = 0, RaiseEvent = operation };
                    return TempOperations[id];
                    });
            return new DisposableObject(() => {
                if (ele.Count == 1) {
                    ele.RaiseEvent?.Invoke();
                    TempOperations.Remove(id);
                }
                ele.Count--;
            });
        }
        protected class CountOperationPair {
            public CountOperationPair() {
            }
            public int Count { get; set; }
            //public string Key { get; set; }
            public Action? RaiseEvent { get; set; }
        }
    }
}
