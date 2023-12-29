using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures.EventManagement {
    /// <summary>An event listener that enables unsubscribing in the Dispose method.</summary>
    /// <typeparam name="THandler">The type of the event handler.</typeparam>
    public class EventListener<THandler> : IDisposable where THandler : class {
        bool _disposed;
        THandler? _handler;
        Action<THandler>? _remove;
        /// <summary>Initializes the listener.</summary>
        /// <param name="add"><code>h => obj.Event += h</code></param>
        /// <param name="remove"><code>h => obj.Event -= h</code></param>
        /// <param name="handler">The action to be executed by the event.</param>
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
        /// <summary>Unsubscribes from the event.</summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
    /// <summary>
    /// An event listener that enables unsubscribing in the Dispose method.
    /// </summary>
    /// <typeparam name="THandler">The type of the event handler.</typeparam>
    /// <typeparam name="TArgs">The type of the event arguments for the subscribed event.</typeparam>
    public class EventListener<THandler,TArgs> : EventListener<THandler> where THandler:class {
        /// <summary>Initializes the listener.</summary>
        /// <param name="conversion">Converts from <typeparamref name="THandler"/> to <see cref="EventHandler{TArgs}"/>. Example: <code>h => (s, e) => h(s, new OtherEventArgs())</code></param>
        /// <param name="add">Registers the <see cref="EventHandler{TArgs}"/>. Example: <code>h => obj.Event += h</code></param>
        /// <param name="remove">Unsubscribes from the <see cref="EventHandler{TArgs}"/>. Example: <code>h => obj.Event -= h</code></param>
        /// <param name="handler">The action to be executed by the event.</param>
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
