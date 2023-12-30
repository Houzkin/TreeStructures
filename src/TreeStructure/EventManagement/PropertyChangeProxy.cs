using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures.EventManagement {

    //public class NotificationObject : INotifyPropertyChanged {
    //    public NotificationObject() {
    //        PropChangeProxy = new PropertyChangeProxy(this);
    //    }
    //    readonly PropertyChangeProxy PropChangeProxy;

    //    public event PropertyChangedEventHandler? PropertyChanged {
    //        add { this.PropChangeProxy.Changed += value; }
    //        remove { this.PropChangeProxy.Changed -= value; }
    //    }
    //    protected virtual bool SetProperty<T>(ref T strage, T value, [CallerMemberName] string? propertyName = null) =>
    //        PropChangeProxy.SetWithNotify(ref strage, value, propertyName);

    //    protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null) =>
    //        PropChangeProxy.Notify(propertyName);
    //}

    /// <summary>Supports the implementation of <see cref="INotifyPropertyChanged"/>.</summary>
    public class PropertyChangeProxy :INotifyPropertyChanged {
        object? sender;
        /// <summary>Initializes a new instance.</summary>
        /// <param name="sender">The instance to be specified as the sender when the event is raised.</param>
        public PropertyChangeProxy(object? sender) {
            this.sender = sender;
        }
        /// <summary>Performs value change and issues change notifications.</summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="storage">The storage for the value.</param>
        /// <param name="value">The new value.</param>
        /// <param name="propertyName">The name of the property (automatically provided by the compiler).</param>
        /// <returns>True if there was a change in the value, false otherwise.</returns>
        public bool SetWithNotify<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null) {
            if (Equals(storage, value)) return false;
            storage = value;
            Notify(propertyName);
            return true;
        }
        /// <summary>Issues property change notification.</summary>
        /// <param name="propertyName">The name of the property.</param>
        public void Notify([CallerMemberName] string? propertyName = null) {
            if (string.IsNullOrEmpty(propertyName)) return;
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>Removes all registered handlers.</summary>
        public void ClearHandler() { 
            PropertyChanged = null;
        }
        event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged {
            add { PropertyChanged += value; }
            remove { PropertyChanged -= value; }
        }
        /// <summary>Occurs when a property is changed.</summary>
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
