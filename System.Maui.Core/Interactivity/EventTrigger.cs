using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	[ContentProperty("Actions")]
	public sealed class EventTrigger : TriggerBase
	{
		static readonly MethodInfo s_handlerinfo = typeof(EventTrigger).GetRuntimeMethods().Single(mi => mi.Name == "OnEventTriggered" && mi.IsPublic == false);
		readonly List<BindableObject> _associatedObjects = new List<BindableObject>();

		EventInfo _eventinfo;

		string _eventname;
		Delegate _handlerdelegate;

		public EventTrigger() : base(typeof(BindableObject))
		{
			Actions = new SealedList<TriggerAction>();
		}

		public IList<TriggerAction> Actions { get; }

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
			_associatedObjects.Add(bindable);
		}

		internal override void OnDetachingFrom(BindableObject bindable)
		{
			_associatedObjects.Remove(bindable);
			DetachHandlerFrom(bindable);
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
				Log.Warning("EventTrigger", "Can not attach EventTrigger to {0}.{1}. Check if the handler exists and if the signature is right.", bindable.GetType(), Event);
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