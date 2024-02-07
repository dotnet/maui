using System;
using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// An internal, performance-focused version of WeakEventManager
	/// By restricting to one event type, it can be strongly typed.
	/// </summary>
	class WeakEventHandler<TEventArgs> where TEventArgs : EventArgs
	{
		readonly List<WeakReference<EventHandler<TEventArgs>>> _subscriptions = new();

		public void AddEventHandler(EventHandler<TEventArgs> handler)
		{
			if (handler is null)
			{
				throw new ArgumentNullException(nameof(handler));
			}
			_subscriptions.Add(new(handler));
		}

		public void RemoveEventHandler(EventHandler<TEventArgs> handler)
		{
			if (handler is null)
			{
				throw new ArgumentNullException(nameof(handler));
			}

			// Reversed, so we can RemoveAt() as we go
			for (int i = _subscriptions.Count - 1; i >= 0; i--)
			{
				var subscription = _subscriptions[i];
				if (subscription.TryGetTarget(out var existing))
				{
					if (handler == existing)
					{
						_subscriptions.RemoveAt(i);
						break;
					}
				}
				else
				{
					_subscriptions.RemoveAt(i);
				}
			}
		}

		public void HandleEvent(object? sender, TEventArgs args)
		{
			// Reversed, so we can RemoveAt() as we go
			for (int i = _subscriptions.Count - 1; i >= 0; i--)
			{
				var subscription = _subscriptions[i];
				if (subscription.TryGetTarget(out var handler))
				{
					handler.Invoke(sender, args);
				}
				else
				{
					_subscriptions.RemoveAt(i);
				}
			}
		}
	}
}
