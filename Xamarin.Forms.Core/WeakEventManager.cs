using System;
using System.Collections.Generic;
using System.Reflection;

namespace Xamarin.Forms
{
	internal class WeakEventManager
	{
		readonly Dictionary<string, List<Subscription>> _eventHandlers =
			new Dictionary<string, List<Subscription>>();

		public void AddEventHandler<TEventArgs>(string eventName, EventHandler<TEventArgs> handler)
			where TEventArgs : EventArgs
		{
			if (eventName == null)
			{
				throw new ArgumentNullException(nameof(eventName));
			}

			if (handler == null)
			{
				throw new ArgumentNullException(nameof(handler));
			}

			AddEventHandler(eventName, handler.Target, handler.GetMethodInfo());
		}

		public void AddEventHandler(string eventName, EventHandler handler)
		{
			if (eventName == null)
			{
				throw new ArgumentNullException(nameof(eventName));
			}

			if (handler == null)
			{
				throw new ArgumentNullException(nameof(handler));
			}

			AddEventHandler(eventName, handler.Target, handler.GetMethodInfo());
		}

		public void HandleEvent(object sender, object args, string eventName)
		{
			var toRaise = new List<Tuple<object, MethodInfo>>();

			List<Subscription> target;
			if (_eventHandlers.TryGetValue(eventName, out target))
			{
				foreach (Subscription subscription in target)
				{
					object subscriber = subscription.Subscriber.Target;

					if (subscriber == null)
					{
						// The subscriber was collected, so there's no need to keep this subscription around
						target.Remove(subscription);
					}
					else
					{
						toRaise.Add(Tuple.Create(subscriber, subscription.Handler));
					}
				}
			}

			foreach (Tuple<object, MethodInfo> tuple in toRaise)
			{
				tuple.Item2.Invoke(tuple.Item1, new[] { sender, args });
			}
		}

		public void RemoveEventHandler<TEventArgs>(string eventName, EventHandler<TEventArgs> handler)
			where TEventArgs : EventArgs
		{
			if (eventName == null)
			{
				throw new ArgumentNullException(nameof(eventName));
			}

			if (handler == null)
			{
				throw new ArgumentNullException(nameof(handler));
			}

			RemoveEventHandler(eventName, handler.Target, handler.GetMethodInfo());
		}

		public void RemoveEventHandler(string eventName, EventHandler handler)
		{
			if (eventName == null)
			{
				throw new ArgumentNullException(nameof(eventName));
			}

			if (handler == null)
			{
				throw new ArgumentNullException(nameof(handler));
			}

			RemoveEventHandler(eventName, handler.Target, handler.GetMethodInfo());
		}

		void AddEventHandler(string eventName, object handlerTarget, MethodInfo methodInfo)
		{
			List<Subscription> target;
			if (!_eventHandlers.TryGetValue(eventName, out target))
			{
				target = new List<Subscription>();
				_eventHandlers.Add(eventName, target);
			}

			target.Add(new Subscription(new WeakReference(handlerTarget), methodInfo));
		}

		void RemoveEventHandler(string eventName, object handlerTarget, MemberInfo methodInfo)
		{
			List<Subscription> subscriptions;
			if (!_eventHandlers.TryGetValue(eventName, out subscriptions))
			{
				return;
			}

			for (int n = subscriptions.Count; n > 0; n--)
			{
				Subscription current = subscriptions[n - 1];

				if (current.Subscriber != handlerTarget
				    || current.Handler.Name != methodInfo.Name)
				{
					continue;
				}

				subscriptions.Remove(current);
			}
		}

		struct Subscription
		{
			public Subscription(WeakReference subscriber, MethodInfo handler)
			{
				if (subscriber == null)
				{
					throw new ArgumentNullException(nameof(subscriber));
				}

				if (handler == null)
				{
					throw new ArgumentNullException(nameof(handler));
				}

				Subscriber = subscriber;
				Handler = handler;
			}

			public readonly WeakReference Subscriber;
			public readonly MethodInfo Handler;
		}
	}
}