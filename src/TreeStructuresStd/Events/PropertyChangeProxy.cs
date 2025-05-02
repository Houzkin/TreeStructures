using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Utilities;

namespace TreeStructures.Events {

	//public class PropChangeProxy {
	//    public PropChangeProxy(Action<PropertyChangedEventArgs> notifyAction) {
	//        NotifyAction = notifyAction;
	//    }
	//    Action<PropertyChangedEventArgs> NotifyAction;
	//    public bool SetWithNotify<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null) {
	//        if (Equals(storage, value)) return false;
	//        storage = value;
	//        Notify(propertyName);
	//        return true;
	//    }
	//    public void Notify([CallerMemberName] string? propertyName = null) {
	//        if (string.IsNullOrEmpty(propertyName)) return;
	//        NotifyAction(new PropertyChangedEventArgs(propertyName));
	//        //PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
	//    }
	//}
	public class PropChangeProxy {
        Action<string> onAction;
        Func<string, PropertyChangedEventArgs> toArgs = name => new PropertyChangedEventArgs(name);
		public PropChangeProxy(object? sender, Func<PropertyChangedEventHandler?> handlerGetter) {
            this.onAction = new(name => {
                var hdlr = handlerGetter();
                hdlr?.Invoke(sender,toArgs(name));
            });
		}
        public PropChangeProxy(Action<PropertyChangedEventArgs> raiseAction) {
            this.onAction = new(name => { raiseAction(toArgs(name)); });
        }
        public PropChangeProxy(Action<string> onAction) {
            this.onAction=onAction; 
        }

		public bool SetWithNotify<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null) {
			if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
			storage = value;
			Notify(propertyName);
			return true;
		}

		public void Notify([CallerMemberName] string? propertyName = null) {
			if (string.IsNullOrEmpty(propertyName)) return;
            this.onAction(propertyName);
            //this.raiseAction(new PropertyChangedEventArgs(propertyName));
			//var handler = handlerGetter();
			//handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}


	/// <summary>Supports the implementation of <see cref="INotifyPropertyChanged"/>.</summary>
	public class PropertyChangeProxy /*:INotifyPropertyChanged */{
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
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
            storage = value;
            Notify(propertyName);
            return true;
        }
        /// <summary>Issues property change notification.</summary>
        /// <param name="propertyName">The name of the property.</param>
        public virtual void Notify([CallerMemberName] string? propertyName = null) {
            if (string.IsNullOrEmpty(propertyName)) return;
            var handler = Changed;
            handler?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>Removes all registered handlers.</summary>
        public void ClearHandler() { 
            Changed = null;
        }
        //internal int HandlerCount =>
        //        PropertyChanged.GetLength();
        //event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged {
        //    add { PropertyChanged += value; }
        //    remove { PropertyChanged -= value; }
        //}
        /// <summary>Occurs when a property is changed.</summary>
        public event PropertyChangedEventHandler? Changed;
    }
}
