using System;
using System.Collections.Generic;
using System.Reflection;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	[ContentProperty("Setters")]
	public sealed class Style : IStyle
	{
		internal const string StyleClassPrefix = "Xamarin.Forms.StyleClass.";

		const int CleanupTrigger = 128;
		int _cleanupThreshold = CleanupTrigger;

		readonly BindableProperty _basedOnResourceProperty = BindableProperty.CreateAttached("BasedOnResource", typeof(Style), typeof(Style), default(Style),
			propertyChanged: OnBasedOnResourceChanged);

		readonly List<WeakReference<BindableObject>> _targets = new List<WeakReference<BindableObject>>(4);

		Style _basedOnStyle;

		string _baseResourceKey;

		IList<Behavior> _behaviors;

		IList<TriggerBase> _triggers;

		public Style([TypeConverter(typeof(TypeTypeConverter))][Parameter("TargetType")] Type targetType)
		{
			TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
			Setters = new List<Setter>();
		}

		public bool ApplyToDerivedTypes { get; set; }

		public Style BasedOn
		{
			get { return _basedOnStyle; }
			set
			{
				if (_basedOnStyle == value)
					return;
				if (!ValidateBasedOn(value))
					throw new ArgumentException("BasedOn.TargetType is not compatible with TargetType");
				Style oldValue = _basedOnStyle;
				_basedOnStyle = value;
				BasedOnChanged(oldValue, value);
				if (value != null)
					BaseResourceKey = null;
			}
		}

		public string BaseResourceKey
		{
			get { return _baseResourceKey; }
			set
			{
				if (_baseResourceKey == value)
					return;
				_baseResourceKey = value;
				//update all DynamicResources
				foreach (WeakReference<BindableObject> bindableWr in _targets)
				{
					BindableObject target;
					if (!bindableWr.TryGetTarget(out target))
						continue;
					target.RemoveDynamicResource(_basedOnResourceProperty);
					if (value != null)
						target.SetDynamicResource(_basedOnResourceProperty, value);
				}
				if (value != null)
					BasedOn = null;
			}
		}

		public IList<Behavior> Behaviors
		{
			get { return _behaviors ?? (_behaviors = new AttachedCollection<Behavior>()); }
		}

		public bool CanCascade { get; set; }

		public string Class { get; set; }

		public IList<Setter> Setters { get; }

		public IList<TriggerBase> Triggers
		{
			get { return _triggers ?? (_triggers = new AttachedCollection<TriggerBase>()); }
		}

		void IStyle.Apply(BindableObject bindable)
		{
			_targets.Add(new WeakReference<BindableObject>(bindable));
			if (BaseResourceKey != null)
				bindable.SetDynamicResource(_basedOnResourceProperty, BaseResourceKey);
			ApplyCore(bindable, BasedOn ?? GetBasedOnResource(bindable));

			CleanUpWeakReferences();
		}

		public Type TargetType { get; }

		void IStyle.UnApply(BindableObject bindable)
		{
			UnApplyCore(bindable, BasedOn ?? GetBasedOnResource(bindable));
			bindable.RemoveDynamicResource(_basedOnResourceProperty);
			lock (_targets)
			{
				_targets.RemoveAll(wr =>
				{
					BindableObject target;
					return wr != null && wr.TryGetTarget(out target) && target == bindable;
				});
			}
		}

		internal bool CanBeAppliedTo(Type targetType)
		{
			if (TargetType == targetType)
				return true;
			if (!ApplyToDerivedTypes)
				return false;
			do
			{
				targetType = targetType.GetTypeInfo().BaseType;
				if (TargetType == targetType)
					return true;
			} while (targetType != typeof(Element));
			return false;
		}

		void ApplyCore(BindableObject bindable, Style basedOn)
		{
			if (basedOn != null)
				((IStyle)basedOn).Apply(bindable);

			foreach (Setter setter in Setters)
				setter.Apply(bindable, true);
			((AttachedCollection<Behavior>)Behaviors).AttachTo(bindable);
			((AttachedCollection<TriggerBase>)Triggers).AttachTo(bindable);
		}

		void BasedOnChanged(Style oldValue, Style newValue)
		{
			foreach (WeakReference<BindableObject> bindableRef in _targets)
			{
				BindableObject bindable;
				if (!bindableRef.TryGetTarget(out bindable))
					continue;

				UnApplyCore(bindable, oldValue);
				ApplyCore(bindable, newValue);
			}
		}

		Style GetBasedOnResource(BindableObject bindable)
		{
			return (Style)bindable.GetValue(_basedOnResourceProperty);
		}

		static void OnBasedOnResourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			Style style = (bindable as IStyleElement).Style;
			if (style == null)
				return;
			style.UnApplyCore(bindable, (Style)oldValue);
			style.ApplyCore(bindable, (Style)newValue);
		}

		void UnApplyCore(BindableObject bindable, Style basedOn)
		{
			((AttachedCollection<TriggerBase>)Triggers).DetachFrom(bindable);
			((AttachedCollection<Behavior>)Behaviors).DetachFrom(bindable);
			foreach (Setter setter in Setters)
				setter.UnApply(bindable, true);

			if (basedOn != null)
				((IStyle)basedOn).UnApply(bindable);
		}

		bool ValidateBasedOn(Style value)
		{
			if (value == null)
				return true;
			return value.TargetType.IsAssignableFrom(TargetType);
		}

		void CleanUpWeakReferences()
		{
			if (_targets.Count < _cleanupThreshold)
			{
				return;
			}

			lock (_targets)
			{
				_targets.RemoveAll(t => t == null || !t.TryGetTarget(out _));
				_cleanupThreshold = _targets.Count + CleanupTrigger;
			}
		}
	}
}