#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/EventTrigger.xml" path="Type[@FullName='Microsoft.Maui.Controls.EventTrigger']/Docs/*" />
	[ContentProperty("Actions")]
	public sealed class EventTrigger : TriggerBase
	{
		readonly List<WeakReference<BindableObject>> _associatedObjects = new List<WeakReference<BindableObject>>();
		readonly IEventSubscriptionStrategy _strategy;
		string _eventname;

		/// <include file="../../../docs/Microsoft.Maui.Controls/EventTrigger.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		[RequiresUnreferencedCode("EventTrigger uses reflection to subscribe to events. Use EventTrigger.Create<T>() factory methods for trimming-safe alternatives.")]
#if !NETSTANDARD
		[RequiresDynamicCode("EventTrigger uses reflection to subscribe to events. Use EventTrigger.Create<T>() factory methods for trimming-safe alternatives.")]
#endif
		public EventTrigger()
			: base(typeof(BindableObject))
		{
			_strategy = new ReflectionStrategy(this);
		}

		private EventTrigger(Func<EventTrigger, IEventSubscriptionStrategy> strategyFactory)
			: base(typeof(BindableObject))
		{
			_strategy = strategyFactory(this);
		}

#nullable enable
		/// <summary>
		/// Creates an AOT-safe EventTrigger for events using <see cref="EventHandler"/>.
		/// </summary>
		/// <typeparam name="TBindable">The type of the bindable object that owns the event.</typeparam>
		/// <param name="eventName">The name of the event to respond to.</param>
		/// <param name="addHandler">A static lambda to subscribe to the event.</param>
		/// <param name="removeHandler">A static lambda to unsubscribe from the event.</param>
		/// <returns>A new EventTrigger instance.</returns>
		/// <example>
		/// <code>
		/// EventTrigger.Create&lt;Button&gt;("Clicked",
		///     static (b, h) =&gt; b.Clicked += h,
		///     static (b, h) =&gt; b.Clicked -= h);
		/// </code>
		/// </example>
		public static EventTrigger Create<TBindable>(
			string eventName,
			Action<TBindable, EventHandler> addHandler,
			Action<TBindable, EventHandler> removeHandler) where TBindable : BindableObject
		{
			return new EventTrigger(trigger => new StaticStrategy<TBindable>(addHandler, removeHandler, trigger))
			{
				Event = eventName
			};
		}

		/// <summary>
		/// Creates an AOT-safe EventTrigger for events using <see cref="EventHandler{TEventArgs}"/>.
		/// </summary>
		/// <typeparam name="TBindable">The type of the bindable object that owns the event.</typeparam>
		/// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
		/// <param name="eventName">The name of the event to respond to.</param>
		/// <param name="addHandler">A static lambda to subscribe to the event.</param>
		/// <param name="removeHandler">A static lambda to unsubscribe from the event.</param>
		/// <returns>A new EventTrigger instance.</returns>
		/// <example>
		/// <code>
		/// EventTrigger.Create&lt;Entry, TextChangedEventArgs&gt;("TextChanged",
		///     static (e, h) =&gt; e.TextChanged += h,
		///     static (e, h) =&gt; e.TextChanged -= h);
		/// </code>
		/// </example>
		public static EventTrigger Create<TBindable, TEventArgs>(
			string eventName,
			Action<TBindable, EventHandler<TEventArgs>> addHandler,
			Action<TBindable, EventHandler<TEventArgs>> removeHandler)
			where TBindable : BindableObject
			where TEventArgs : EventArgs
		{
			return new EventTrigger(trigger => new StaticStrategy<TBindable, TEventArgs>(addHandler, removeHandler, trigger))
			{
				Event = eventName
			};
		}
#nullable disable

		/// <include file="../../../docs/Microsoft.Maui.Controls/EventTrigger.xml" path="//Member[@MemberName='Actions']/Docs/*" />
		public IList<TriggerAction> Actions { get; } = new SealedList<TriggerAction>();

		/// <include file="../../../docs/Microsoft.Maui.Controls/EventTrigger.xml" path="//Member[@MemberName='Event']/Docs/*" />
		public string Event
		{
			get { return _eventname; }
			set
			{
				if (_eventname == value)
					return;
				if (IsSealed)
					throw new InvalidOperationException("Event cannot be changed once the Trigger has been applied");
				OnPropertyChanging();
				_eventname = value;
				OnPropertyChanged();
			}
		}

		internal override void OnAttachedTo(BindableObject bindable)
		{
			base.OnAttachedTo(bindable);
			_strategy.AttachHandlerTo(bindable);
			_associatedObjects.Add(new WeakReference<BindableObject>(bindable));
		}

		internal override void OnDetachingFrom(BindableObject bindable)
		{
			_associatedObjects.RemoveAll(wr =>
			{
				if (wr.TryGetTarget(out var target) && target == bindable)
				{
					_strategy.DetachHandlerFrom(bindable);
					return true;
				}
				return false;
			});
			base.OnDetachingFrom(bindable);
		}

		internal override void OnSeal()
		{
			base.OnSeal();
			((SealedList<TriggerAction>)Actions).IsReadOnly = true;
		}

		private void InvokeActions(object sender)
		{
			if (sender is not BindableObject bindable)
				return;

			foreach (TriggerAction action in Actions)
				action.DoInvoke(bindable);
		}

#nullable enable
		private interface IEventSubscriptionStrategy
		{
			void AttachHandlerTo(BindableObject target);
			void DetachHandlerFrom(BindableObject target);
		}

		[RequiresUnreferencedCode("Uses reflection to subscribe to events.")]
#if !NETSTANDARD
		[RequiresDynamicCode("Uses reflection to subscribe to events.")]
#endif
		private sealed class ReflectionStrategy(EventTrigger trigger) : IEventSubscriptionStrategy
		{
			private EventInfo? _eventInfo;
			private Delegate? _handler;

			public void AttachHandlerTo(BindableObject target)
			{
				if (string.IsNullOrEmpty(trigger.Event))
					return;

				try
				{
					_eventInfo = target.GetType().GetRuntimeEvent(trigger.Event);
					if (_eventInfo == null)
					{
						LogWarning(target);
						return;
					}

					// Create a delegate that matches the event's signature
					var handlerMethod = typeof(ReflectionStrategy).GetMethod(nameof(OnEventTriggered), BindingFlags.NonPublic | BindingFlags.Instance);
					_handler = handlerMethod!.CreateDelegate(_eventInfo.EventHandlerType!, this);
					_eventInfo.AddEventHandler(target, _handler);
				}
				catch (Exception)
				{
					LogWarning(target);
				}
			}

			public void DetachHandlerFrom(BindableObject target)
			{
				if (_eventInfo != null && _handler != null)
					_eventInfo.RemoveEventHandler(target, _handler);
			}

			private void OnEventTriggered(object? sender, EventArgs e)
			{
				trigger.InvokeActions(sender);
			}

			private void LogWarning(BindableObject target)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<EventTrigger>()?
					.LogWarning("Cannot attach EventTrigger to {Type}.{Event}. Check if the event exists and the signature is correct.", 
						target.GetType(), trigger.Event);
			}
		}

		private sealed class StaticStrategy<TBindable>(
			Action<TBindable, EventHandler> addHandler,
			Action<TBindable, EventHandler> removeHandler,
			EventTrigger trigger)
				: IEventSubscriptionStrategy
				where TBindable : BindableObject
		{
			public void AttachHandlerTo(BindableObject target) => addHandler((TBindable)target, InvokeActions);
			public void DetachHandlerFrom(BindableObject target) => removeHandler((TBindable)target, InvokeActions);
			private void InvokeActions(object? sender, EventArgs e) => trigger.InvokeActions(sender);
		}

		private sealed class StaticStrategy<TBindable, TEventArgs>(
			Action<TBindable, EventHandler<TEventArgs>> addHandler,
			Action<TBindable, EventHandler<TEventArgs>> removeHandler,
			EventTrigger trigger)
				: IEventSubscriptionStrategy
				where TBindable : BindableObject
				where TEventArgs : EventArgs
		{
			public void AttachHandlerTo(BindableObject target) => addHandler((TBindable)target, InvokeActions);
			public void DetachHandlerFrom(BindableObject target) => removeHandler((TBindable)target, InvokeActions);
			private void InvokeActions(object? sender, TEventArgs e) => trigger.InvokeActions(sender);
		}
	}
}