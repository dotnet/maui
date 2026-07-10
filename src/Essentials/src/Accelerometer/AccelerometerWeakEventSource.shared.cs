#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Maui.Devices.Sensors
{
	// Weakly references each subscriber's target (not the delegate itself) so a subscriber
	// that is never removed with "-=" can still be collected, while a still-alive subscriber
	// keeps receiving events. Mirrors the shape of Microsoft.Maui.WeakEventManager (Core),
	// including its use of MethodInfo.Invoke to raise events -- Essentials cannot reference
	// Core's WeakEventManager directly here (Core already references Essentials, so the
	// reverse reference would be circular), hence this local equivalent. MethodInfo.Invoke is
	// used deliberately rather than a dynamically-created delegate (e.g. via
	// MethodInfo.MakeGenericMethod/Delegate.CreateDelegate for an unknown target type): that
	// would require RequiresDynamicCode-annotated APIs that are unsafe under NativeAOT, whereas
	// MethodInfo.Invoke is the same pattern WeakEventManager already ships with
	// IsAotCompatible=true in Core, so it is proven safe for this codebase's AOT/trimming story.
	sealed class WeakEventSource<TEventArgs>
		where TEventArgs : EventArgs
	{
		readonly object gate = new object();
		readonly List<Subscription> subscriptions = new();

		public void Subscribe(EventHandler<TEventArgs>? handler)
		{
			if (handler is null)
			{
				return;
			}

			lock (gate)
			{
				subscriptions.Add(new Subscription(handler.Target, handler.Method));
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
					return subscriptions.Count > 0;
				}
			}
		}

		public void Raise(object? sender, TEventArgs args)
		{
			(object? target, MethodInfo method)[] toInvoke;
			int count = 0;

			lock (gate)
			{
				if (subscriptions.Count == 0)
				{
					return;
				}

				toInvoke = new (object?, MethodInfo)[subscriptions.Count];

				// Iterate and prune dead entries in a single forward pass, preserving subscription order.
				for (int i = 0; i < subscriptions.Count; i++)
				{
					var subscription = subscriptions[i];

					if (subscription.Target == null)
					{
						// Static method target.
						toInvoke[count++] = (null, subscription.Method);
						continue;
					}

					if (subscription.Target.TryGetTarget(out var target))
					{
						toInvoke[count++] = (target, subscription.Method);
					}
					else
					{
						subscriptions.RemoveAt(i);
						i--;
					}
				}
			}

			// Invoke outside the lock so subscriber code can freely subscribe/unsubscribe/raise.
			var invokeArgs = new object?[] { sender, args };

			for (int i = 0; i < count; i++)
			{
				toInvoke[i].method.Invoke(toInvoke[i].target, invokeArgs);
			}
		}

		readonly struct Subscription
		{
			public Subscription(object? target, MethodInfo method)
			{
				Target = target is null ? null : new WeakReference<object>(target);
				Method = method;
			}

			public readonly WeakReference<object>? Target;
			public readonly MethodInfo Method;

			public bool IsDead => Target != null && !Target.TryGetTarget(out _);

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
	}

	sealed class WeakEventSource
	{
		readonly object gate = new object();
		readonly List<Subscription> subscriptions = new();

		public void Subscribe(EventHandler? handler)
		{
			if (handler is null)
			{
				return;
			}

			lock (gate)
			{
				subscriptions.Add(new Subscription(handler.Target, handler.Method));
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
					return subscriptions.Count > 0;
				}
			}
		}

		public void Raise(object? sender, EventArgs args)
		{
			(object? target, MethodInfo method)[] toInvoke;
			int count = 0;

			lock (gate)
			{
				if (subscriptions.Count == 0)
				{
					return;
				}

				toInvoke = new (object?, MethodInfo)[subscriptions.Count];

				for (int i = 0; i < subscriptions.Count; i++)
				{
					var subscription = subscriptions[i];

					if (subscription.Target == null)
					{
						toInvoke[count++] = (null, subscription.Method);
						continue;
					}

					if (subscription.Target.TryGetTarget(out var target))
					{
						toInvoke[count++] = (target, subscription.Method);
					}
					else
					{
						subscriptions.RemoveAt(i);
						i--;
					}
				}
			}

			var invokeArgs = new object?[] { sender, args };

			for (int i = 0; i < count; i++)
			{
				toInvoke[i].method.Invoke(toInvoke[i].target, invokeArgs);
			}
		}

		readonly struct Subscription
		{
			public Subscription(object? target, MethodInfo method)
			{
				Target = target is null ? null : new WeakReference<object>(target);
				Method = method;
			}

			public readonly WeakReference<object>? Target;
			public readonly MethodInfo Method;

			public bool IsDead => Target != null && !Target.TryGetTarget(out _);

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
	}
}
