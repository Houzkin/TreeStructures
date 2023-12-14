using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures.EventManagement {
    /// <summary>Disposeで購読解除を可能にするイベントリスナー</summary>
    /// <typeparam name="THandler">イベントハンドラーの型</typeparam>
    public class EventListener<THandler> : IDisposable where THandler : class {
        bool _disposed;
        THandler? _handler;
        Action<THandler>? _remove;
        /// <summary>リスナーを初期化する</summary>
        /// <param name="add"><code>h => obj.Event += h</code></param>
        /// <param name="remove"><code>h => obj.Event -= h</code></param>
        /// <param name="handler">イベントによって実行する処理</param>
        /// <exception cref="ArgumentNullException"></exception>
        public EventListener([NotNull] Action<THandler> add,[NotNull] Action<THandler> remove,[NotNull] THandler handler) {
            if (add == null) throw new ArgumentNullException(nameof(add));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _remove = remove ?? throw new ArgumentNullException(nameof(remove));
            add(handler);
        }
        /// <summary></summary>
        /// <param name="disposing"></param>
        protected void Dispose(bool disposing) {
            if(_disposed) return;
            if (disposing) {
                if(_handler != null)_remove?.Invoke(_handler);
                _remove = null;
                _handler = null;
            }
            _disposed = true;
        }
        /// <summary>イベントの購読を解除する</summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
    /// <summary>
    /// Disposeで購読解除を可能にするイベントリスナー
    /// </summary>
    /// <typeparam name="THandler">登録先のハンドラー</typeparam>
    /// <typeparam name="TArgs">登録元のハンドラーのイベント引数</typeparam>
    public class EventListener<THandler,TArgs> : EventListener<THandler> where THandler:class {
        /// <summary></summary>
        /// <param name="conversion"><typeparamref name="THandler"/>からへ<see cref="EventHandler{TArgs}"/>変換する<code>h => (s, e) => h(s, new OtherEventArgs())</code></param>
        /// <param name="add"><see cref="EventHandler{TArgs}"/>を登録<code>h => obj.Event += h</code></param>
        /// <param name="remove"><see cref="EventHandler{TArgs}"/>を解除<code>h => obj.Event -= h</code></param>
        /// <param name="handler">イベントによって実行する処理</param>
        public EventListener([NotNull] Func<EventHandler<TArgs>,THandler> conversion, [NotNull] Action<THandler> add, [NotNull] Action<THandler> remove, [NotNull] EventHandler<TArgs> handler)
            :base(add,remove,conversion(handler)){

        }
    }
    /*
     * 使用例
            var a = new NotificationObject();
            var x = new EventListener<EventHandler>(
                h => a.Disposed += h,
                h => a.Disposed -= h,
                (s, e) => { });
            var xx = new EventListener<EventHandler<StructureChangedEventArgs<TestCommonNode>>, PropertyChangedEventArgs>(
                h => (s, e) => h(s, new PropertyChangedEventArgs($"{e.Target.Name}")),
                h => a.StructureChanged += h,
                h => a.StructureChanged -= h,
                (s, e) => { });
            INotifyCollectionChanged obsonly = new ReadOnlyObservableCollection<object>(new ObservableCollection<object>());
            var xxx = new EventListener<NotifyCollectionChangedEventHandler>(
                h => obsonly.CollectionChanged += h,
                h => obsonly.CollectionChanged -= h,
                (s, e) => { });
            var xxxx = new EventListener<PropertyChangedEventHandler>(
                h => a.PropertyChanged += h,
                h => a.PropertyChanged -= h,
                (s, e) => { }); */
}
