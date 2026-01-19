#nullable disable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls;

/// <summary>
/// A trimming and AOT-safe version of <see cref="EventTrigger"/> that uses delegates instead of reflection.
/// This class is intended to be instantiated by XAML source generators.
/// </summary>
/// <typeparam name="TBindable">The type of the bindable object that owns the event.</typeparam>
/// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
[ContentProperty("Actions")]
internal sealed class EventTrigger<TBindable, TEventArgs> : TriggerBase
	where TBindable : BindableObject
	where TEventArgs : EventArgs
{
	readonly List<WeakReference<BindableObject>> _associatedObjects = new List<WeakReference<BindableObject>>();
	readonly Action<TBindable, EventHandler<TEventArgs>> _addEventHandler;
	readonly Action<TBindable, EventHandler<TEventArgs>> _removeEventHandler;

	/// <summary>
	/// Creates a new instance of <see cref="EventTrigger{TBindable, TEventArgs}"/>.
	/// </summary>
	/// <param name="addEventHandler">A delegate that subscribes to the event on the target object.</param>
	/// <param name="removeEventHandler">A delegate that unsubscribes from the event on the target object.</param>
	/// <example>
	/// <code>
	/// // For a Button.Clicked event:
	/// new EventTrigger&lt;Button, EventArgs&gt;(
	///     (button, handler) => button.Clicked += handler,
	///     (button, handler) => button.Clicked -= handler);
	/// </code>
	/// </example>
	public EventTrigger(
		Action<TBindable, EventHandler<TEventArgs>> addEventHandler,
		Action<TBindable, EventHandler<TEventArgs>> removeEventHandler)
		: base(typeof(TBindable))
	{
		_addEventHandler = addEventHandler ?? throw new ArgumentNullException(nameof(addEventHandler));
		_removeEventHandler = removeEventHandler ?? throw new ArgumentNullException(nameof(removeEventHandler));
		Actions = new SealedList<TriggerAction>();
	}

	/// <summary>
	/// Gets the collection of <see cref="TriggerAction"/> objects to invoke when the event is raised.
	/// </summary>
	public IList<TriggerAction> Actions { get; }

	internal override void OnAttachedTo(BindableObject bindable)
	{
		base.OnAttachedTo(bindable);
		if (bindable is TBindable target)
		{
			_addEventHandler(target, OnEventTriggered);
		}
		_associatedObjects.Add(new WeakReference<BindableObject>(bindable));
	}

	internal override void OnDetachingFrom(BindableObject bindable)
	{
		_associatedObjects.RemoveAll(wr =>
		{
			if (wr.TryGetTarget(out var target) && target == bindable)
			{
				if (bindable is TBindable typedTarget)
				{
					_removeEventHandler(typedTarget, OnEventTriggered);
				}
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

	void OnEventTriggered(object sender, TEventArgs e)
	{
		var bindable = (BindableObject)sender;
		foreach (TriggerAction action in Actions)
		{
			action.DoInvoke(bindable);
		}
	}
}

/// <summary>
/// A trimming and AOT-safe version of <see cref="EventTrigger"/> for events with <see cref="EventHandler"/> signature.
/// This class is intended to be instantiated by XAML source generators.
/// </summary>
/// <typeparam name="TBindable">The type of the bindable object that owns the event.</typeparam>
[ContentProperty("Actions")]
internal sealed class EventTrigger<TBindable> : TriggerBase
	where TBindable : BindableObject
{
	readonly List<WeakReference<BindableObject>> _associatedObjects = new List<WeakReference<BindableObject>>();
	readonly Action<TBindable, EventHandler> _addEventHandler;
	readonly Action<TBindable, EventHandler> _removeEventHandler;

	/// <summary>
	/// Creates a new instance of <see cref="EventTrigger{TBindable}"/>.
	/// </summary>
	/// <param name="addEventHandler">A delegate that subscribes to the event on the target object.</param>
	/// <param name="removeEventHandler">A delegate that unsubscribes from the event on the target object.</param>
	/// <example>
	/// <code>
	/// // For a Button.Clicked event:
	/// new EventTrigger&lt;Button&gt;(
	///     (button, handler) => button.Clicked += handler,
	///     (button, handler) => button.Clicked -= handler);
	/// </code>
	/// </example>
	public EventTrigger(
		Action<TBindable, EventHandler> addEventHandler,
		Action<TBindable, EventHandler> removeEventHandler)
		: base(typeof(TBindable))
	{
		_addEventHandler = addEventHandler ?? throw new ArgumentNullException(nameof(addEventHandler));
		_removeEventHandler = removeEventHandler ?? throw new ArgumentNullException(nameof(removeEventHandler));
		Actions = new SealedList<TriggerAction>();
	}

	/// <summary>
	/// Gets the collection of <see cref="TriggerAction"/> objects to invoke when the event is raised.
	/// </summary>
	public IList<TriggerAction> Actions { get; }

	internal override void OnAttachedTo(BindableObject bindable)
	{
		base.OnAttachedTo(bindable);
		if (bindable is TBindable target)
		{
			_addEventHandler(target, OnEventTriggered);
		}
		_associatedObjects.Add(new WeakReference<BindableObject>(bindable));
	}

	internal override void OnDetachingFrom(BindableObject bindable)
	{
		_associatedObjects.RemoveAll(wr =>
		{
			if (wr.TryGetTarget(out var target) && target == bindable)
			{
				if (bindable is TBindable typedTarget)
				{
					_removeEventHandler(typedTarget, OnEventTriggered);
				}
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

	void OnEventTriggered(object sender, EventArgs e)
	{
		var bindable = (BindableObject)sender;
		foreach (TriggerAction action in Actions)
		{
			action.DoInvoke(bindable);
		}
	}
}
