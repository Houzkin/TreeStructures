using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Results;
using TreeStructures.Utilities;

namespace TreeStructures.Events {

	/// <summary>
	/// A proxy class that supports the implementation of <see cref="INotifyPropertyChanged"/>.
	/// It helps manage property changes and notifications efficiently.
	///</summary>
	public class PropertyChangeProxy {
        private static readonly ConcurrentDictionary<string,PropertyChangedEventArgs> _eventArgsCash = new();
        Action<string> onAction;
		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyChangeProxy"/> class
		/// that delegates notifications to the specified event handler.
		/// </summary>
		/// <param name="sender">The object that owns this proxy.</param>
		/// <param name="handlerGetter">A function that retrieves the event handler for notifications.</param>
		public PropertyChangeProxy(object? sender, Func<PropertyChangedEventHandler?> handlerGetter) {
            this.onAction = new(name => {
                var hdlr = handlerGetter();
                hdlr?.Invoke(sender,toArgs(name));
            });
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyChangeProxy"/> class
		/// that delegates notifications using a specified action.
		/// </summary>
		/// <param name="raiseAction">An action to invoke property change notifications.</param>
		public PropertyChangeProxy(Action<PropertyChangedEventArgs> raiseAction) {
            this.onAction = new(name => { raiseAction(toArgs(name)); });
        }
        private static PropertyChangedEventArgs toArgs(string name) 
            => _eventArgsCash.GetOrAdd(name, n => new PropertyChangedEventArgs(n));

		/// <summary>
		/// Sets the given storage value and sends a property change notification if the value has changed.
		/// </summary>
		/// <typeparam name="T">The type of the property.</typeparam>
		/// <param name="storage">A reference to the backing field of the property.</param>
		/// <param name="value">The new value to set.</param>
		/// <param name="propertyName">The name of the property (automatically determined).</param>
		/// <returns>A result indicating whether the value was changed.</returns>
		public ResultWithValue<PropertyChangeProxy> TrySetAndNotify<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return new ResultWithValue<PropertyChangeProxy>(false, this);
			storage = value;
			Notify(propertyName);
            return new ResultWithValue<PropertyChangeProxy>(true, this);
		}
		/// <summary>
		/// Sends a property change notification for the specified property name.
		/// </summary>
		/// <param name="propertyName">The name of the property (automatically determined).</param>
		/// <returns>The current instance of <see cref="PropertyChangeProxy"/>.</returns>
		public PropertyChangeProxy Notify([CallerMemberName] string? propertyName = null) {
			if (string.IsNullOrEmpty(propertyName)) return this;
            this.onAction(propertyName);
            return this;
		}
	}


	///// <summary>Supports the implementation of <see cref="INotifyPropertyChanged"/>.</summary>
	//public class PropertyChangeProxy /*:INotifyPropertyChanged */{
 //       object? sender;
 //       /// <summary>Initializes a new instance.</summary>
 //       /// <param name="sender">The instance to be specified as the sender when the event is raised.</param>
 //       public PropertyChangeProxy(object? sender) {
 //           this.sender = sender;
 //       }
 //       /// <summary>Performs value change and issues change notifications.</summary>
 //       /// <typeparam name="T">The type of the value.</typeparam>
 //       /// <param name="storage">The storage for the value.</param>
 //       /// <param name="value">The new value.</param>
 //       /// <param name="propertyName">The name of the property (automatically provided by the compiler).</param>
 //       /// <returns>True if there was a change in the value, false otherwise.</returns>
 //       public bool TrySetAndNotify<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null) {
 //           if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
 //           storage = value;
 //           Notify(propertyName);
 //           return true;
 //       }
 //       /// <summary>Issues property change notification.</summary>
 //       /// <param name="propertyName">The name of the property.</param>
 //       public virtual void Notify([CallerMemberName] string? propertyName = null) {
 //           if (string.IsNullOrEmpty(propertyName)) return;
 //           var handler = Changed;
 //           handler?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
 //       }
 //       /// <summary>Removes all registered handlers.</summary>
 //       public void ClearHandler() { 
 //           Changed = null;
 //       }
 //       //internal int HandlerCount =>
 //       //        PropertyChanged.GetLength();
 //       //event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged {
 //       //    add { PropertyChanged += value; }
 //       //    remove { PropertyChanged -= value; }
 //       //}
 //       /// <summary>Occurs when a property is changed.</summary>
 //       public event PropertyChangedEventHandler? Changed;
 //   }
}
