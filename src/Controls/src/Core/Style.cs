#nullable disable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Groups property setters that can be shared between multiple visual elements.
	/// </summary>
	[ContentProperty(nameof(Setters))]
	public sealed class Style : IStyle
	{
		internal const string StyleClassPrefix = "Microsoft.Maui.Controls.StyleClass.";

		readonly BindableProperty _basedOnResourceProperty = BindableProperty.CreateAttached("BasedOnResource", typeof(Style), typeof(Style), default(Style),
			propertyChanged: OnBasedOnResourceChanged);

		readonly ConditionalWeakTable<BindableObject, object> _targets = new();

		Style _basedOnStyle;

		string _baseResourceKey;

		IList<Behavior> _behaviors;

		IList<TriggerBase> _triggers;

		/// <summary>
		/// Initializes a new instance of the <see cref="Style"/> class.
		/// </summary>
		/// <param name="targetType">The type to which the style applies.</param>
		public Style([System.ComponentModel.TypeConverter(typeof(TypeTypeConverter))][Parameter("TargetType")] Type targetType)
		{
			TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
			Setters = new List<Setter>();
		}

		/// <summary>
		/// Gets or sets whether the style can be applied to types derived from <see cref="TargetType"/>.
		/// </summary>
		public bool ApplyToDerivedTypes { get; set; }

		/// <summary>
		/// Gets or sets the style from which this style inherits.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the resource key for the base style.
		/// </summary>
		public string BaseResourceKey
		{
			get { return _baseResourceKey; }
			set
			{
				if (_baseResourceKey == value)
					return;
				_baseResourceKey = value;
				//update all DynamicResources
				foreach (var target in (IEnumerable<KeyValuePair<BindableObject, object>>)(object)_targets)
				{
					target.Key.RemoveDynamicResource(_basedOnResourceProperty);
					if (value != null)
						target.Key.SetDynamicResource(_basedOnResourceProperty, value);
				}
				if (value != null)
					BasedOn = null;
			}
		}

		/// <summary>
		/// Gets the collection of <see cref="Behavior"/> objects attached to this style.
		/// </summary>
		public IList<Behavior> Behaviors => _behaviors ??= new AttachedCollection<Behavior>();

		/// <summary>
		/// Gets or sets whether this style can cascade to nested elements.
		/// </summary>
		public bool CanCascade { get; set; }

		/// <summary>
		/// Gets or sets the class name for the style.
		/// </summary>
		public string Class { get; set; }

		/// <summary>
		/// Gets the collection of <see cref="Setter"/> objects that define the property values for this style.
		/// </summary>
		public IList<Setter> Setters { get; }

		/// <summary>
		/// Gets the collection of <see cref="TriggerBase"/> objects attached to this style.
		/// </summary>
		public IList<TriggerBase> Triggers => _triggers ??= new AttachedCollection<TriggerBase>();

		void IStyle.Apply(BindableObject bindable, SetterSpecificity specificity)
		{
			lock (_targets)
			{
#if NETSTANDARD2_0
				_targets.Remove(bindable);
				_targets.Add(bindable, specificity);
#else
				_targets.AddOrUpdate(bindable, specificity);
#endif
			}

			if (BaseResourceKey != null)
				bindable.SetDynamicResource(_basedOnResourceProperty, BaseResourceKey);
			ApplyCore(bindable, BasedOn ?? GetBasedOnResource(bindable), specificity);
		}

		/// <summary>
		/// Gets the type to which this style applies.
		/// </summary>
		public Type TargetType { get; }

		void IStyle.UnApply(BindableObject bindable)
		{
			UnApplyCore(bindable, BasedOn ?? GetBasedOnResource(bindable));
			bindable.RemoveDynamicResource(_basedOnResourceProperty);
			lock (_targets)
			{
				_targets.Remove(bindable);
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
				targetType = targetType.BaseType;
				if (TargetType == targetType)
					return true;
			} while (targetType != typeof(Element));
			return false;
		}

		void BasedOnChanged(Style oldValue, Style newValue)
		{
			foreach (var target in (IEnumerable<KeyValuePair<BindableObject, object>>)(object)_targets)
			{
				UnApplyCore(target.Key, oldValue);
				ApplyCore(target.Key, newValue, (SetterSpecificity)target.Value);
			}
		}

		Style GetBasedOnResource(BindableObject bindable) => (Style)bindable.GetValue(_basedOnResourceProperty);

		static void OnBasedOnResourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			Style style = (bindable as IStyleElement).Style;
			if (style == null)
				return;
			if (!style._targets.TryGetValue(bindable, out var objectspecificity))
				return;

			style.UnApplyCore(bindable, (Style)oldValue);
			style.ApplyCore(bindable, (Style)newValue, (SetterSpecificity)objectspecificity);
		}

		ConditionalWeakTable<BindableObject, object> specificities = new();

		void ApplyCore(BindableObject bindable, Style basedOn, SetterSpecificity specificity)
		{
			if (basedOn != null)
				((IStyle)basedOn).Apply(bindable, specificity.AsBaseStyle());

#if NETSTANDARD2_0
			specificities.Remove(bindable);
			specificities.Add(bindable, specificity);
#else
			specificities.AddOrUpdate(bindable, specificity);
#endif

			foreach (Setter setter in Setters)
				setter.Apply(bindable, specificity);

			((AttachedCollection<Behavior>)Behaviors).AttachTo(bindable);
			((AttachedCollection<TriggerBase>)Triggers).AttachTo(bindable);
		}

		void UnApplyCore(BindableObject bindable, Style basedOn)
		{
			((AttachedCollection<TriggerBase>)Triggers).DetachFrom(bindable);
			((AttachedCollection<Behavior>)Behaviors).DetachFrom(bindable);

			if (!specificities.TryGetValue(bindable, out var specificity))
				return;

			foreach (Setter setter in Setters)
				setter.UnApply(bindable, (SetterSpecificity)specificity);

			if (basedOn != null)
				((IStyle)basedOn).UnApply(bindable);
		}

		bool ValidateBasedOn(Style value)
			=> value is null || value.TargetType.IsAssignableFrom(TargetType);
	}
}