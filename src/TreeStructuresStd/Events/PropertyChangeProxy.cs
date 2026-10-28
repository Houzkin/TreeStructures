using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Collections;
using TreeStructures.Results;
using TreeStructures.Utilities;

namespace TreeStructures.Events {
	/// <summary>Provides helper methods for implementing <see cref="INotifyPropertyChanged"/> and managing property values.</summary>
	/// <remarks>
	/// This class supports two approaches to property change notification: retrieving the event handler from the owner object, or invoking a specified action to raise the notification.
	/// Property values can also be stored and retrieved by property name, allowing properties to be implemented without explicit backing fields.
	/// </remarks>
	public class PropertyAccessor {
		private static readonly ConcurrentDictionary<string, PropertyChangedEventArgs> _eventArgsCash = new();
		Action<string> onAction;
		object? _owner;
		/// <summary>Initializes a new instance of the <see cref="PropertyAccessor"/> class using a function that retrieves the property change event handler.</summary>
		/// <param name="owner">
		/// The object that owns the properties being observed.
		/// This object is also passed as the sender when the event is raised.
		/// </param>
		/// <param name="handlerGetter">
		/// A function that retrieves the current property change event handler.
		/// If the function returns <see langword="null"/>, no notification is raised.
		/// </param>
		public PropertyAccessor(object owner, Func<PropertyChangedEventHandler?> handlerGetter) {
			_owner = owner;
			this.onAction = new(name => {
				var hdlr = handlerGetter();
				hdlr?.Invoke(owner, GetOrAddCachedEventArgs(name));
			});
		}
		/// <summary>Initializes a new instance of the <see cref="PropertyAccessor"/> class using an action that raises a property change notification.</summary>
		/// <param name="owner">The object that owns the properties being observed.</param>
		/// <param name="raiseAction">An action that raises a property change notification using the specified <see cref="PropertyChangedEventArgs"/>.</param>
		public PropertyAccessor(object owner, Action<PropertyChangedEventArgs> raiseAction) {
			_owner = owner;
			this.onAction = new(name => { raiseAction(GetOrAddCachedEventArgs(name)); });
		}
		/// <summary>Gets a cached <see cref="PropertyChangedEventArgs"/> instance for the specified property name, creating one if necessary.</summary>
		/// <param name="name">The name of the property that changed.</param>
		/// <returns>The cached <see cref="PropertyChangedEventArgs"/> instance associated with the specified property name.</returns>
		/// <remarks>This method is thread-safe. Thread safety is not guaranteed for other members of the <see cref="PropertyAccessor"/> class.</remarks>
		public static PropertyChangedEventArgs GetOrAddCachedEventArgs(string name)
			=> _eventArgsCash.GetOrAdd(name, n => new PropertyChangedEventArgs(n));
		/// <summary>Sets the specified storage field to a new value and raises a property change notification if the value has changed.</summary>
		/// <typeparam name="T">The type of the property value.</typeparam>
		/// <param name="storage">A reference to the field storing the property value.</param>
		/// <param name="value">The new property value.</param>
		/// <param name="propertyName">
		/// The name of the property associated with the storage field.
		/// Automatically supplied by the compiler when omitted.
		/// </param>
		/// <returns>A <see cref="ResultWithValue{T}"/> containing this instance and a Boolean indicating whether the value was changed.</returns>
		/// <remarks>
		/// Value equality is determined by <see cref="EqualityComparer{T}.Default"/>.
		/// If the values are equal, the storage field is left unchanged and no notification is raised.
		/// </remarks>
		public ResultWithValue<PropertyAccessor> TrySetField<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null) {
			if (EqualityComparer<T>.Default.Equals(storage, value)) return new ResultWithValue<PropertyAccessor>(false, this);
			storage = value;
			Notify(propertyName);
			return new ResultWithValue<PropertyAccessor>(true, this);
		}
		/// <summary>Raises a property change notification for the specified property.</summary>
		/// <param name="propertyName">
		/// The name of the property that changed.
		/// Automatically supplied by the compiler when omitted.
		/// </param>
		/// <returns>This instance.</returns>
		/// <remarks>
		/// Automatically supplied by the compiler when omitted.
		/// If the property name is <see langword="null"/> or empty, no notification is raised.
		/// </remarks>
		public PropertyAccessor Notify([CallerMemberName] string? propertyName = null) {
			if (string.IsNullOrEmpty(propertyName)) return this;
			this.onAction(propertyName);
			return this;
		}

		private readonly Dictionary<string, object?> propList = new();
		/// <summary>Updates the stored value associated with the specified property name and raises a property change notification if the value has changed.</summary>
		/// <typeparam name="T">The type of the property value.</typeparam>
		/// <param name="value">The new property value.</param>
		/// <param name="propertyName">
		/// The name of the property to update.
		/// Automatically supplied by the compiler when omitted.
		/// </param>
		/// <returns>A <see cref="ResultWithValue{T}"/> containing this instance and a Boolean indicating whether the value was changed.</returns>
		/// <remarks>
		/// If a value has already been stored for the specified property name, it is compared with the new value using <see cref="EqualityComparer{T}.Default"/> or <see cref="object.Equals(object?, object?)"/>.
		///
		/// If no value has been stored yet, the current property value is retrieved from the owner object using reflection and compared with the new value.
		/// The new value is stored and a notification is raised only if the values are not equal.
		/// An exception is thrown if the property name is null or empty.
		/// </remarks>
		public ResultWithValue<PropertyAccessor> TrySetValue<T>(T value, [CallerMemberName] string? propertyName = null) {
			if(string.IsNullOrEmpty(propertyName) || propertyName is null) throw new ArgumentException("Failed to get the property name.", nameof(propertyName));
			if(propList.TryGetValue(propertyName, out var preval)) {//既にDictionaryに存在した場合
				if(preval is T preTval) {
					if(!EqualityComparer<T>.Default.Equals(value, preTval)) {//変更あり
						propList[propertyName] = value;
						Notify(propertyName);
						return new ResultWithValue<PropertyAccessor>(true, this);
					} else {//変更なし
						return new ResultWithValue<PropertyAccessor>(false, this);
					}
				}
				if (!Equals(preval, value)) {//値変更あり
					propList[propertyName] = value;
					Notify(propertyName);
					return new ResultWithValue<PropertyAccessor>(true, this);
				} else {//値変更なし
					return new ResultWithValue<PropertyAccessor>(false, this);
				}
			} else {//Dictionaryに存在しなかった場合
				var defval = _owner?.GetType()?.GetProperty(propertyName)?.GetValue(_owner);
				if (!Equals(defval, value)) {//初期値から変更あり
					propList[propertyName] = value;
					Notify(propertyName);
					return new ResultWithValue<PropertyAccessor>(true, this);
				}
				//初期値から変更なし
				return new ResultWithValue<PropertyAccessor>(false, this);
			}
		}
		/// <summary>Gets the stored value associated with the specified property name, initializing it with the specified default value if it has not yet been stored.</summary>
		/// <typeparam name="T">The type of the property value.</typeparam>
		/// <param name="initialValue">
		/// The initial value to store if no value has been stored for the property.
		/// Defaults to <see langword="default"/> for <typeparamref name="T"/>.
		/// </param>
		/// <param name="propertyName">
		/// The name of the property whose value is retrieved.
		/// Automatically supplied by the compiler when omitted.
		/// </param>
		/// <returns>The stored or newly initialized property value.</returns>
		/// <remarks>
		/// If a value has already been stored for the specified property name, that value is returned instead of <paramref name="initialValue"/>.
		/// An exception is thrown if the property name is null or empty.
		/// </remarks>
		public T GetValue<T>(T initialValue = default, [CallerMemberName] string? propertyName = null) {
			if (string.IsNullOrEmpty(propertyName) || propertyName is null) throw new ArgumentException("Failed to get the property name.", nameof(propertyName));
			if (propList.TryGetValue(propertyName, out var value)) {
				return (T)value;
			} else {
				propList.Add(propertyName, initialValue);
				return initialValue;
			}
		}
	}

}
