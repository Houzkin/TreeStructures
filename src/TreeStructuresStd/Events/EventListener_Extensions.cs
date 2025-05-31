using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Collections;
using TreeStructures.Events;
using TreeStructures.Utilities;

namespace TreeStructures.Events {
	/// <summary>
	/// Provides an interface for observing additions and removals in a collection that implements <see cref="INotifyCollectionChanged"/>.
	/// </summary>
	/// <typeparam name="T">The type of elements in the collection.</typeparam>
	public interface ICollectionAddRemoveObserver<T> : IDisposable {
		/// <summary>
		/// Registers an action to be invoked when an item is added to the collection.
		/// </summary>
		/// <param name="added">The action to execute when an item is added.</param>
		/// <returns>The current instance of <see cref="ICollectionAddRemoveObserver{T}"/>.</returns>
		ICollectionAddRemoveObserver<T> Added(Action<T> added);
		/// <summary>
		/// Registers an action to be invoked when an item is removed from the collection.
		/// </summary>
		/// <param name="removed">The action to execute when an item is removed.</param>
		/// <returns>The current instance of <see cref="ICollectionAddRemoveObserver{T}"/>.</returns>
		ICollectionAddRemoveObserver<T> Removed(Action<T> removed);
	}

	public static class EventListenerExtensions {
		internal class CollectionObserver<T> : ICollectionAddRemoveObserver<T> {
			internal CollectionObserver(IEnumerable<T> list) {
				_collection = new ReadOnlyObservableProxyCollection<T>(list, null, OnRemoved);
				var pxy = (INotifyCollectionChanged)_collection;
				pxy.CollectionChanged += Pxy_CollectionChanged;
			}
			private ReadOnlyObservableProxyCollection<T> _collection;

			private void Pxy_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
				switch (e.Action) {
				case NotifyCollectionChangedAction.Add:
				case NotifyCollectionChangedAction.Replace:
					var lst = e.NewItems?.Cast<T?>() ?? Array.Empty<T>();
					foreach(var item in lst) this.OnAdded(item);
					break;
				}
			}

			event Action<T>? _added;
			event Action<T>? _removed;
			void OnAdded(T item) { _added?.Invoke(item); }
			void OnRemoved(T item) { _removed?.Invoke(item); }

			public ICollectionAddRemoveObserver<T> Added(Action<T> added) {
				_added += added;
				return this;
			}
			public ICollectionAddRemoveObserver<T> Removed(Action<T> removed) {
				_removed += removed;
				return this;
			}
			public void Dispose() {
				((INotifyCollectionChanged)_collection).CollectionChanged -= Pxy_CollectionChanged;
				_collection.Dispose();
			}
		}

		/// <summary>
		/// Observes additions and removals in the specified collection that implements <see cref="INotifyCollectionChanged"/>.
		/// </summary>
		/// <typeparam name="T">The type of elements in the collection.</typeparam>
		/// <param name="self">The collection to observe.</param>
		/// <returns>An instance of <see cref="ICollectionAddRemoveObserver{T}"/> to monitor changes.</returns>
		/// <exception cref="InvalidCastException">
		/// Thrown if the specified collection does not implement <see cref="INotifyCollectionChanged"/>.
		/// </exception>
		public static ICollectionAddRemoveObserver<T> Observe<T>(this IEnumerable<T> self)  {
			if (self is not INotifyCollectionChanged) throw new InvalidCastException("The specified collection does not implement INotifyCollectionChanged.");
			return new CollectionObserver<T>(self);
		}

		//		/// <summary>
		//		/// Subscribes to an event and returns an IDisposable that allows unsubscribing automatically.
		//		/// </summary>
		//		public static IDisposable Subscribe<T, THandler>(
		//			this T obj,
		//			Action<THandler> add,
		//			Action<THandler> remove,
		//			THandler handler)
		//			where T : class
		//#if NETSTANDARD2_0
		//			where THandler : class
		//#else
		//			where THandler : Delegate 
		//#endif
		//			{ 
		//			if (obj == null) throw new ArgumentNullException(nameof(obj));
		//			if (handler == null) throw new ArgumentNullException(nameof(handler));

		//			return new EventListener<THandler>(add, remove, handler);
		//		}

		//		/// <summary>
		//		/// Subscribes to an event with event arguments and returns an IDisposable that allows unsubscribing automatically.
		//		/// </summary>
		//		public static IDisposable Subscribe<T, THandler, TArgs>(
		//			this T obj,
		//			Expression<Func<T, THandler>> eventSelector,
		//			Func<EventHandler<TArgs>, THandler> converter,
		//			EventHandler<TArgs> handler)
		//			where T : class
		//#if NETSTANDARD2_0
		//			where THandler : class
		//#else
		//			where THandler : Delegate 
		//#endif
		//			{ 
		//			//where THandler : class {
		//			if (obj == null) throw new ArgumentNullException(nameof(obj));
		//			if (eventSelector == null) throw new ArgumentNullException(nameof(eventSelector));
		//			if (converter == null) throw new ArgumentNullException(nameof(converter));
		//			if (handler == null) throw new ArgumentNullException(nameof(handler));

		//			var (add, remove) = GetEventAccessors(obj, eventSelector);
		//			return new EventListener<THandler, TArgs>(add, remove, converter, handler);
		//		}

		//		/// <summary>
		//		/// Extracts add and remove accessors for an event from an object.
		//		/// </summary>
		//		private static (Action<THandler> add, Action<THandler> remove) GetEventAccessors<T, THandler>(
		//			T obj,
		//			Expression<Func<T, THandler>> eventSelector)
		//			where T : class
		//			where THandler : class {
		//			if (eventSelector.Body is MemberExpression memberExpr && memberExpr.Member is EventInfo eventInfo) {
		//				var addMethod = eventInfo.GetAddMethod(true);
		//				var removeMethod = eventInfo.GetRemoveMethod(true);
		//				if (addMethod == null || removeMethod == null)
		//					throw new InvalidOperationException("The specified event does not have accessible add/remove methods.");

		//				return (
		//					h => addMethod.Invoke(obj, new object[] { h }),
		//					h => removeMethod.Invoke(obj, new object[] { h })
		//				);
		//			}
		//			throw new ArgumentException("The event selector must be a lambda expression selecting an event.", nameof(eventSelector));
		//		}
	}

}
