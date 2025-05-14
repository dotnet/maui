#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

using static System.String;
namespace Microsoft.Maui
{
	/// <summary>
	/// Manages weak event subscriptions, preventing memory leaks by maintaining weak references to handlers.
	/// </summary>
	public class WeakEventManager
	{
		readonly Dictionary<string, List<Subscription>> _eventHandlers = new(StringComparer.Ordinal);

		/// <summary>
		/// Adds an event handler for the specified event, storing a weak reference to the handler's target.
		/// </summary>
		/// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
		/// <param name="handler">The event handler to add.</param>
		/// <param name="eventName">The name of the event.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="eventName"/> is null or empty.</exception>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="handler"/> is null.</exception>
		public void AddEventHandler<TEventArgs>(EventHandler<TEventArgs> handler, [CallerMemberName] string eventName = "")
			where TEventArgs : EventArgs
		{
			if (IsNullOrEmpty(eventName))
				throw new ArgumentNullException(nameof(eventName));

			if (handler == null)
				throw new ArgumentNullException(nameof(handler));

			AddEventHandler(eventName, handler.Target, handler.GetMethodInfo());
		}

		/// <summary>
		/// Adds an event handler for the specified event, storing a weak reference to the handler's target.
		/// </summary>
		/// <param name="handler">The event handler to add.</param>
		/// <param name="eventName">The name of the event.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="eventName"/> is null or empty.</exception>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="handler"/> is null.</exception>
		public void AddEventHandler(Delegate? handler, [CallerMemberName] string eventName = "")
		{
			if (IsNullOrEmpty(eventName))
				throw new ArgumentNullException(nameof(eventName));

			if (handler == null)
				throw new ArgumentNullException(nameof(handler));

			AddEventHandler(eventName, handler.Target, handler.GetMethodInfo());
		}

		/// <summary>
		/// Invokes the handlers registered for the specified event. Removes handlers whose targets have been garbage collected.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="args">The event arguments.</param>
		/// <param name="eventName">The name of the event to raise.</param>
		public void HandleEvent(object? sender, object? args, string eventName)
		{
			var toRaise = new List<(object? subscriber, MethodInfo handler)>();
			var toRemove = new List<Subscription>();

			if (_eventHandlers.TryGetValue(eventName, out List<Subscription>? target))
			{
				for (int i = 0; i < target.Count; i++)
				{
					Subscription subscription = target[i];
					bool isStatic = subscription.Subscriber == null;
					if (isStatic)
					{
						// For a static method, we'll just pass null as the first parameter of MethodInfo.Invoke
						toRaise.Add((null, subscription.Handler));
						continue;
					}

					object? subscriber = subscription.Subscriber?.Target;

					if (subscriber == null)
						// The subscriber was collected, so there's no need to keep this subscription around
						toRemove.Add(subscription);
					else
						toRaise.Add((subscriber, subscription.Handler));
				}

				for (int i = 0; i < toRemove.Count; i++)
				{
					Subscription subscription = toRemove[i];
					target.Remove(subscription);
				}
			}

			for (int i = 0; i < toRaise.Count; i++)
			{
				(var subscriber, var handler) = toRaise[i];
				handler.Invoke(subscriber, new[] { sender, args });
			}
		}

		/// <summary>
		/// Removes a previously added event handler for the specified event.
		/// </summary>
		/// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
		/// <param name="handler">The event handler to remove.</param>
		/// <param name="eventName">The name of the event.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="eventName"/> is null or empty.</exception>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="handler"/> is null.</exception>
		public void RemoveEventHandler<TEventArgs>(EventHandler<TEventArgs> handler, [CallerMemberName] string eventName = "")
			where TEventArgs : EventArgs
		{
			if (IsNullOrEmpty(eventName))
				throw new ArgumentNullException(nameof(eventName));

			if (handler == null)
				throw new ArgumentNullException(nameof(handler));

			RemoveEventHandler(eventName, handler.Target, handler.GetMethodInfo());
		}

		/// <summary>
		/// Removes a previously added event handler for the specified event.
		/// </summary>
		/// <param name="handler">The event handler to remove.</param>
		/// <param name="eventName">The name of the event.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="eventName"/> is null or empty.</exception>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="handler"/> is null.</exception>
		public void RemoveEventHandler(Delegate? handler, [CallerMemberName] string eventName = "")
		{
			if (IsNullOrEmpty(eventName))
				throw new ArgumentNullException(nameof(eventName));

			if (handler == null)
				throw new ArgumentNullException(nameof(handler));

			RemoveEventHandler(eventName, handler.Target, handler.GetMethodInfo());
		}

		void AddEventHandler(string eventName, object? handlerTarget, MethodInfo methodInfo)
		{
			if (!_eventHandlers.TryGetValue(eventName, out List<Subscription>? targets))
			{
				targets = new List<Subscription>();
				_eventHandlers.Add(eventName, targets);
			}

			if (handlerTarget == null)
			{
				// This event handler is a static method
				targets.Add(new Subscription(null, methodInfo));
				return;
			}

			targets.Add(new Subscription(new WeakReference(handlerTarget), methodInfo));
		}

		void RemoveEventHandler(string eventName, object? handlerTarget, MemberInfo methodInfo)
		{
			if (!_eventHandlers.TryGetValue(eventName, out List<Subscription>? subscriptions))
				return;

			for (int n = subscriptions.Count - 1; n >= 0; n--)
			{
				Subscription current = subscriptions[n];

				if (current.Subscriber != null && !current.Subscriber.IsAlive)
				{
					// If not alive, remove and continue
					subscriptions.RemoveAt(n);
					continue;
				}

				if (current.Subscriber?.Target == handlerTarget && current.Handler.Name == methodInfo.Name)
				{
					// Found the match, we can break
					subscriptions.RemoveAt(n);
					break;
				}
			}
		}

		readonly struct Subscription : IEquatable<Subscription>
		{
			/// <summary>
			/// Initializes a new <see cref="Subscription"/> with a weak reference to the subscriber and the handler method.
			/// </summary>
			/// <param name="subscriber">A weak reference to the subscriber object.</param>
			/// <param name="handler">The method info of the handler to invoke.</param>
			public Subscription(WeakReference? subscriber, MethodInfo handler)
			{
				Subscriber = subscriber;
				Handler = handler ?? throw new ArgumentNullException(nameof(handler));
			}

			public readonly WeakReference? Subscriber;
			public readonly MethodInfo Handler;

			public bool Equals(Subscription other) => Subscriber == other.Subscriber && Handler == other.Handler;

			public override bool Equals(object? obj) => obj is Subscription other && Equals(other);

			public override int GetHashCode() => Subscriber?.GetHashCode() ?? 0 ^ Handler.GetHashCode();
		}
	}
}
