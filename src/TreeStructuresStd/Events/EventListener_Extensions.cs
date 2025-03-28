using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Events;
using TreeStructures.Utilities;

namespace TreeStructures.Events {
//	public static class EventListenerExtensions {
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
//	}

}
