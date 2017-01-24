using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Xamarin.Forms
{
	public static class MessagingCenter
	{
		class Sender : Tuple<string, Type, Type>
		{
			public Sender(string message, Type senderType, Type argType) : base(message, senderType, argType)
			{
			}
		}

		delegate bool Filter(object sender);

		class MaybeWeakReference
		{
			WeakReference DelegateWeakReference { get; set; }
			object DelegateStrongReference { get; set; }

			readonly bool _isStrongReference;

			public MaybeWeakReference(object subscriber, object delegateSource)
			{
				if (subscriber.Equals(delegateSource))
				{
					// The target is the subscriber; we can use a weakreference
					DelegateWeakReference = new WeakReference(delegateSource);
					_isStrongReference = false;
				}
				else
				{
					DelegateStrongReference = delegateSource;
					_isStrongReference = true;
				}
			}

			public object Target => _isStrongReference ? DelegateStrongReference : DelegateWeakReference.Target;
			public bool IsAlive => _isStrongReference || DelegateWeakReference.IsAlive;
		}

		class Subscription : Tuple<WeakReference, MaybeWeakReference, MethodInfo, Filter>
		{
			public Subscription(object subscriber, object delegateSource, MethodInfo methodInfo, Filter filter)
				: base(new WeakReference(subscriber), new MaybeWeakReference(subscriber, delegateSource), methodInfo, filter)
			{
			}

			public WeakReference Subscriber => Item1;
			MaybeWeakReference DelegateSource => Item2;
			MethodInfo MethodInfo => Item3;
			Filter Filter => Item4;

			public void InvokeCallback(object sender, object args)
			{
				if (!Filter(sender))
				{
					return;
				}

				if (MethodInfo.IsStatic)
				{
					MethodInfo.Invoke(null, MethodInfo.GetParameters().Length == 1 ? new[] { sender } : new[] { sender, args });
					return;
				}

				var target = DelegateSource.Target;

				if (target == null)
				{
					return; // Collected 
				}

				MethodInfo.Invoke(target, MethodInfo.GetParameters().Length == 1 ? new[] { sender } : new[] { sender, args });
			}

			public bool CanBeRemoved()
			{
				return !Subscriber.IsAlive || !DelegateSource.IsAlive;
			}
		}

		static readonly Dictionary<Sender, List<Subscription>> s_subscriptions =
			new Dictionary<Sender, List<Subscription>>();

		public static void Send<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class
		{
			if (sender == null)
				throw new ArgumentNullException(nameof(sender));
			InnerSend(message, typeof(TSender), typeof(TArgs), sender, args);
		}

		public static void Send<TSender>(TSender sender, string message) where TSender : class
		{
			if (sender == null)
				throw new ArgumentNullException(nameof(sender));
			InnerSend(message, typeof(TSender), null, sender, null);
		}

		public static void Subscribe<TSender, TArgs>(object subscriber, string message, Action<TSender, TArgs> callback, TSender source = null) where TSender : class
		{
			if (subscriber == null)
				throw new ArgumentNullException(nameof(subscriber));
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));

			var target = callback.Target;

			Filter filter = sender =>
			{
				var send = (TSender)sender;
				return (source == null || send == source);
			};

			InnerSubscribe(subscriber, message, typeof(TSender), typeof(TArgs), target, callback.GetMethodInfo(), filter);
		}

		public static void Subscribe<TSender>(object subscriber, string message, Action<TSender> callback, TSender source = null) where TSender : class
		{
			if (subscriber == null)
				throw new ArgumentNullException(nameof(subscriber));
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));

			var target = callback.Target;

			Filter filter = sender =>
			{
				var send = (TSender)sender;
				return (source == null || send == source);
			};

			InnerSubscribe(subscriber, message, typeof(TSender), null, target, callback.GetMethodInfo(), filter);
		}

		public static void Unsubscribe<TSender, TArgs>(object subscriber, string message) where TSender : class
		{
			InnerUnsubscribe(message, typeof(TSender), typeof(TArgs), subscriber);
		}

		public static void Unsubscribe<TSender>(object subscriber, string message) where TSender : class
		{
			InnerUnsubscribe(message, typeof(TSender), null, subscriber);
		}

		internal static void ClearSubscribers()
		{
			s_subscriptions.Clear();
		}

		static void InnerSend(string message, Type senderType, Type argType, object sender, object args)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));
			var key = new Sender(message, senderType, argType);
			if (!s_subscriptions.ContainsKey(key))
				return;
			List<Subscription> subcriptions = s_subscriptions[key];
			if (subcriptions == null || !subcriptions.Any())
				return; // should not be reachable

			// ok so this code looks a bit funky but here is the gist of the problem. It is possible that in the course
			// of executing the callbacks for this message someone will subscribe/unsubscribe from the same message in
			// the callback. This would invalidate the enumerator. To work around this we make a copy. However if you unsubscribe 
			// from a message you can fairly reasonably expect that you will therefor not receive a call. To fix this we then
			// check that the item we are about to send the message to actually exists in the live list.
			List<Subscription> subscriptionsCopy = subcriptions.ToList();
			foreach (Subscription subscription in subscriptionsCopy)
			{
				if (subscription.Subscriber.Target != null && subcriptions.Contains(subscription))
				{
					subscription.InvokeCallback(sender, args);
				}
			}
		}

		static void InnerSubscribe(object subscriber, string message, Type senderType, Type argType, object target, MethodInfo methodInfo, Filter filter)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));
			var key = new Sender(message, senderType, argType);
			var value = new Subscription(subscriber, target, methodInfo, filter);
			if (s_subscriptions.ContainsKey(key))
			{
				s_subscriptions[key].Add(value);
			}
			else
			{
				var list = new List<Subscription> { value };
				s_subscriptions[key] = list;
			}
		}

		static void InnerUnsubscribe(string message, Type senderType, Type argType, object subscriber)
		{
			if (subscriber == null)
				throw new ArgumentNullException(nameof(subscriber));
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			var key = new Sender(message, senderType, argType);
			if (!s_subscriptions.ContainsKey(key))
				return;
			s_subscriptions[key].RemoveAll(sub => sub.CanBeRemoved() || sub.Subscriber.Target == subscriber);
			if (!s_subscriptions[key].Any())
				s_subscriptions.Remove(key);
		}
	}
}