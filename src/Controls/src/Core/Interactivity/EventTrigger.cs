#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/EventTrigger.xml" path="Type[@FullName='Microsoft.Maui.Controls.EventTrigger']/Docs/*" />
	[ContentProperty("Actions")]
	public sealed class EventTrigger : TriggerBase
	{
		static readonly MethodInfo s_handlerinfo = typeof(EventTrigger).GetRuntimeMethods().Single(mi => mi.Name == "OnEventTriggered" && mi.IsPublic == false);
		readonly List<WeakReference<BindableObject>> _associatedObjects = new List<WeakReference<BindableObject>>();

		EventInfo _eventinfo;

		string _eventname;
		Delegate _handlerdelegate;

		/// <include file="../../../docs/Microsoft.Maui.Controls/EventTrigger.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public EventTrigger() : base(typeof(BindableObject))
		{
			Actions = new SealedList<TriggerAction>();
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/EventTrigger.xml" path="//Member[@MemberName='Actions']/Docs/*" />
		public IList<TriggerAction> Actions { get; }

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
			if (!string.IsNullOrEmpty(Event))
				AttachHandlerTo(bindable);
			_associatedObjects.Add(new WeakReference<BindableObject>(bindable));
		}

		internal override void OnDetachingFrom(BindableObject bindable)
		{
			_associatedObjects.RemoveAll(wr =>
			{
				if (wr.TryGetTarget(out var target) && target == bindable)
				{
					DetachHandlerFrom(bindable);
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

		void AttachHandlerTo(BindableObject bindable)
		{
			try
			{
				_eventinfo = bindable.GetType().GetRuntimeEvent(Event);
				_handlerdelegate = s_handlerinfo.CreateDelegate(_eventinfo.EventHandlerType, this);
			}
			catch (Exception)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<EventTrigger>()?.LogWarning("Cannot attach EventTrigger to {Type}.{Event}. Check if the handler exists and if the signature is right.", bindable.GetType(), Event);
			}
			if (_eventinfo != null && _handlerdelegate != null)
				_eventinfo.AddEventHandler(bindable, _handlerdelegate);
		}

		void DetachHandlerFrom(BindableObject bindable)
		{
			if (_eventinfo != null && _handlerdelegate != null)
				_eventinfo.RemoveEventHandler(bindable, _handlerdelegate);
		}

		[Preserve]
		void OnEventTriggered(object sender, EventArgs e)
		{
			var bindable = (BindableObject)sender;
			foreach (TriggerAction action in Actions)
				action.DoInvoke(bindable);
		}
	}
}