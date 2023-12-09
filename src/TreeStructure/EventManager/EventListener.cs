using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructure.EventManager {
    
    public class EventListener<THandler> : IDisposable where THandler : class {
        bool _disposed;
        THandler? _handler;
        Action<THandler>? _remove;
        public EventListener([NotNull] Action<THandler> add,[NotNull] Action<THandler> remove,[NotNull] THandler handler) {
            if (add == null) throw new ArgumentNullException(nameof(add));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _remove = remove ?? throw new ArgumentNullException(nameof(remove));
            add(handler);
        }
        protected void Dispose(bool disposing) {
            if(_disposed) return;
            if (disposing) {
                if(_handler != null)_remove?.Invoke(_handler);
                _remove = null;
                _handler = null;
            }
            _disposed = true;
        }
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
    public class EventListener<THandler,TArgs> : EventListener<THandler> where THandler:class {
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
