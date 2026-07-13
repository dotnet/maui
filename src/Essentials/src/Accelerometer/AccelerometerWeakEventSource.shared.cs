#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Devices.Sensors
{
	// Weakly references each subscriber's target so a subscriber that is never removed with "-="
	// can still be collected, while a still-alive subscriber keeps receiving events. Instance
	// delegates are kept in a ConditionalWeakTable keyed by their target so the source can invoke
	// delegates directly without the delegate keeping the target alive for the source lifetime.
	sealed class WeakEventSource<TEventArgs>
		where TEventArgs : EventArgs
	{
		readonly object gate = new object();
		readonly List<Subscription> subscriptions = new();
		readonly ConditionalWeakTable<object, HandlerStore> instanceHandlers = new();
		int nextId;

		public void Subscribe(EventHandler<TEventArgs>? handler)
		{
			if (handler is null)
			{
				return;
			}

			lock (gate)
			{
				var target = handler.Target;
				if (target is null)
				{
					subscriptions.Add(Subscription.CreateStatic(handler));
					return;
				}

				var id = nextId++;
				instanceHandlers.GetOrCreateValue(target).Add(id, handler);
				subscriptions.Add(Subscription.CreateInstance(target, handler.Method, id));
			}
		}

		public void Unsubscribe(EventHandler<TEventArgs>? handler)
		{
			if (handler is null)
			{
				return;
			}

			lock (gate)
			{
				for (int i = subscriptions.Count - 1; i >= 0; i--)
				{
					var subscription = subscriptions[i];

					if (subscription.IsDead)
					{
						subscriptions.RemoveAt(i);
						continue;
					}

					if (subscription.Matches(handler.Target, handler.Method))
					{
						if (handler.Target is not null &&
							instanceHandlers.TryGetValue(handler.Target, out var store))
						{
							store.Remove(subscription.Id);
						}

						subscriptions.RemoveAt(i);
						break;
					}
				}
			}
		}

		/// <summary>
		/// Gets whether there is at least one subscription. This is a plain count check and does
		/// not prune dead subscriptions on every read (pruning happens lazily in <see cref="Raise"/>,
		/// <see cref="Subscribe"/>, and <see cref="Unsubscribe"/>), so it is cheap to query on every
		/// sensor reading without the cost of walking/mutating the subscription list each time.
		/// </summary>
		public bool HasHandlers
		{
			get
			{
				lock (gate)
				{
					PruneDeadSubscriptions();
					return subscriptions.Count > 0;
				}
			}
		}

		public void Raise(object? sender, TEventArgs args)
		{
			EventHandler<TEventArgs>[] toInvoke;
			int count = 0;

			lock (gate)
			{
				if (subscriptions.Count == 0)
				{
					return;
				}

				toInvoke = new EventHandler<TEventArgs>[subscriptions.Count];
				// Iterate and prune dead entries in a single forward pass, preserving subscription order.
				for (int i = 0; i < subscriptions.Count; i++)
				{
					var subscription = subscriptions[i];

					if (subscription.StaticHandler is not null)
					{
						toInvoke[count++] = subscription.StaticHandler;
						continue;
					}

					if (subscription.TryGetHandler(instanceHandlers, out var handler))
					{
						toInvoke[count++] = handler;
					}
					else
					{
						subscriptions.RemoveAt(i);
						i--;
					}
				}
			}

			// Invoke outside the lock so subscriber code can freely subscribe/unsubscribe/raise.
			for (int i = 0; i < count; i++)
			{
				toInvoke[i](sender, args);
			}
		}

		void PruneDeadSubscriptions()
		{
			for (int i = subscriptions.Count - 1; i >= 0; i--)
			{
				if (subscriptions[i].IsDead)
				{
					subscriptions.RemoveAt(i);
				}
			}
		}

		struct Subscription
		{
			Subscription(WeakReference<object>? target, MethodInfo method, int id, EventHandler<TEventArgs>? staticHandler)
			{
				Target = target;
				Method = method;
				Id = id;
				StaticHandler = staticHandler;
			}

			public readonly WeakReference<object>? Target;
			public readonly MethodInfo Method;
			public readonly int Id;
			public readonly EventHandler<TEventArgs>? StaticHandler;

			public static Subscription CreateInstance(object target, MethodInfo method, int id) =>
				new(new WeakReference<object>(target), method, id, null);

			public static Subscription CreateStatic(EventHandler<TEventArgs> handler) =>
				new(null, handler.Method, -1, handler);

			public bool IsDead => Target != null && !Target.TryGetTarget(out _);

			public bool TryGetHandler(
				ConditionalWeakTable<object, HandlerStore> instanceHandlers,
				out EventHandler<TEventArgs> handler)
			{
				handler = null!;

				if (Target is null || !Target.TryGetTarget(out var target))
				{
					return false;
				}

				return instanceHandlers.TryGetValue(target, out var store) &&
					store.TryGetValue(Id, out handler);
			}

			public bool Matches(object? target, MethodInfo method)
			{
				if (Method != method)
				{
					return false;
				}

				if (target is null)
				{
					return Target is null;
				}

				return Target != null && Target.TryGetTarget(out var current) && ReferenceEquals(current, target);
			}
		}

		sealed class HandlerStore
		{
			readonly List<Entry> handlers = new();

			public void Add(int id, EventHandler<TEventArgs> handler) =>
				handlers.Add(new Entry(id, handler));

			public bool Remove(int id)
			{
				for (int i = handlers.Count - 1; i >= 0; i--)
				{
					if (handlers[i].Id == id)
					{
						handlers.RemoveAt(i);
						return true;
					}
				}

				return false;
			}

			public bool TryGetValue(int id, out EventHandler<TEventArgs> handler)
			{
				for (int i = 0; i < handlers.Count; i++)
				{
					if (handlers[i].Id == id)
					{
						handler = handlers[i].Handler;
						return true;
					}
				}

				handler = null!;
				return false;
			}

			struct Entry
			{
				public Entry(int id, EventHandler<TEventArgs> handler)
				{
					Id = id;
					Handler = handler;
				}

				public readonly int Id;
				public readonly EventHandler<TEventArgs> Handler;
			}
		}
	}

	sealed class WeakEventSource
	{
		readonly object gate = new object();
		readonly List<Subscription> subscriptions = new();
		readonly ConditionalWeakTable<object, HandlerStore> instanceHandlers = new();
		int nextId;

		public void Subscribe(EventHandler? handler)
		{
			if (handler is null)
			{
				return;
			}

			lock (gate)
			{
				var target = handler.Target;
				if (target is null)
				{
					subscriptions.Add(Subscription.CreateStatic(handler));
					return;
				}

				var id = nextId++;
				instanceHandlers.GetOrCreateValue(target).Add(id, handler);
				subscriptions.Add(Subscription.CreateInstance(target, handler.Method, id));
			}
		}

		public void Unsubscribe(EventHandler? handler)
		{
			if (handler is null)
			{
				return;
			}

			lock (gate)
			{
				for (int i = subscriptions.Count - 1; i >= 0; i--)
				{
					var subscription = subscriptions[i];

					if (subscription.IsDead)
					{
						subscriptions.RemoveAt(i);
						continue;
					}

					if (subscription.Matches(handler.Target, handler.Method))
					{
						if (handler.Target is not null &&
							instanceHandlers.TryGetValue(handler.Target, out var store))
						{
							store.Remove(subscription.Id);
						}

						subscriptions.RemoveAt(i);
						break;
					}
				}
			}
		}

		/// <summary>
		/// Gets whether there is at least one subscription. This is a plain count check and does
		/// not prune dead subscriptions on every read (pruning happens lazily in <see cref="Raise"/>,
		/// <see cref="Subscribe"/>, and <see cref="Unsubscribe"/>), so it is cheap to query on every
		/// sensor reading (e.g. to gate <c>ShakeDetected</c> processing) without the cost of
		/// walking/mutating the subscription list each time.
		/// </summary>
		public bool HasHandlers
		{
			get
			{
				lock (gate)
				{
					PruneDeadSubscriptions();
					return subscriptions.Count > 0;
				}
			}
		}

		public void Raise(object? sender, EventArgs args)
		{
			EventHandler[] toInvoke;
			int count = 0;

			lock (gate)
			{
				if (subscriptions.Count == 0)
				{
					return;
				}

				toInvoke = new EventHandler[subscriptions.Count];

				for (int i = 0; i < subscriptions.Count; i++)
				{
					var subscription = subscriptions[i];

					if (subscription.StaticHandler is not null)
					{
						toInvoke[count++] = subscription.StaticHandler;
						continue;
					}

					if (subscription.TryGetHandler(instanceHandlers, out var handler))
					{
						toInvoke[count++] = handler;
					}
					else
					{
						subscriptions.RemoveAt(i);
						i--;
					}
				}
			}

			for (int i = 0; i < count; i++)
			{
				toInvoke[i](sender, args);
			}
		}

		void PruneDeadSubscriptions()
		{
			for (int i = subscriptions.Count - 1; i >= 0; i--)
			{
				if (subscriptions[i].IsDead)
				{
					subscriptions.RemoveAt(i);
				}
			}
		}

		struct Subscription
		{
			Subscription(WeakReference<object>? target, MethodInfo method, int id, EventHandler? staticHandler)
			{
				Target = target;
				Method = method;
				Id = id;
				StaticHandler = staticHandler;
			}

			public readonly WeakReference<object>? Target;
			public readonly MethodInfo Method;
			public readonly int Id;
			public readonly EventHandler? StaticHandler;

			public static Subscription CreateInstance(object target, MethodInfo method, int id) =>
				new(new WeakReference<object>(target), method, id, null);

			public static Subscription CreateStatic(EventHandler handler) =>
				new(null, handler.Method, -1, handler);

			public bool IsDead => Target != null && !Target.TryGetTarget(out _);

			public bool TryGetHandler(
				ConditionalWeakTable<object, HandlerStore> instanceHandlers,
				out EventHandler handler)
			{
				handler = null!;

				if (Target is null || !Target.TryGetTarget(out var target))
				{
					return false;
				}

				return instanceHandlers.TryGetValue(target, out var store) &&
					store.TryGetValue(Id, out handler);
			}

			public bool Matches(object? target, MethodInfo method)
			{
				if (Method != method)
				{
					return false;
				}

				if (target is null)
				{
					return Target is null;
				}

				return Target != null && Target.TryGetTarget(out var current) && ReferenceEquals(current, target);
			}
		}

		sealed class HandlerStore
		{
			readonly List<Entry> handlers = new();

			public void Add(int id, EventHandler handler) =>
				handlers.Add(new Entry(id, handler));

			public bool Remove(int id)
			{
				for (int i = handlers.Count - 1; i >= 0; i--)
				{
					if (handlers[i].Id == id)
					{
						handlers.RemoveAt(i);
						return true;
					}
				}

				return false;
			}

			public bool TryGetValue(int id, out EventHandler handler)
			{
				for (int i = 0; i < handlers.Count; i++)
				{
					if (handlers[i].Id == id)
					{
						handler = handlers[i].Handler;
						return true;
					}
				}

				handler = null!;
				return false;
			}

			struct Entry
			{
				public Entry(int id, EventHandler handler)
				{
					Id = id;
					Handler = handler;
				}

				public readonly int Id;
				public readonly EventHandler Handler;
			}
		}
	}
}
