using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructure.EventManager {

    public class NotificationObject : INotifyPropertyChanged {
        public NotificationObject() {
            PropChangeProxy = new PropertyChangeProxy(this);
        }
        readonly PropertyChangeProxy PropChangeProxy;

        public event PropertyChangedEventHandler? PropertyChanged {
            add { this.PropChangeProxy.Changed += value; }
            remove { this.PropChangeProxy.Changed -= value; }
        }
        protected virtual bool SetProperty<T>(ref T strage, T value, [CallerMemberName] string? propertyName = null) =>
            PropChangeProxy.SetWithNotify(ref strage, value, propertyName);

        protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropChangeProxy.Notify(propertyName);
    }
    public class PropertyChangeProxy :INotifyPropertyChanged {
        object? sender;
        public PropertyChangeProxy(object? sender) {
            this.sender = sender;
        }
        public bool SetWithNotify<T>(ref T strage, T value, [CallerMemberName] string? propertyName = null) {
            if (Equals(strage, value)) return false;
            strage = value;
            Notify(propertyName);
            return true;
        }
        public void Notify([CallerMemberName] string? propertyName = null) {
            if (string.IsNullOrEmpty(propertyName)) return;
            Changed?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
        }
        public void ClearHandler() { 
            Changed = null;
        }
        event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged {
            add { Changed += value; }
            remove { Changed -= value; }
        }
        public event PropertyChangedEventHandler? Changed;
    }
}
