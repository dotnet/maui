#nullable disable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Style.xml" path="Type[@FullName='Microsoft.Maui.Controls.Style']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Style.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public Style([System.ComponentModel.TypeConverter(typeof(TypeTypeConverter))][Parameter("TargetType")] Type targetType)
		{
			TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
			Setters = new List<Setter>();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Style.xml" path="//Member[@MemberName='ApplyToDerivedTypes']/Docs/*" />
		public bool ApplyToDerivedTypes { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/Style.xml" path="//Member[@MemberName='BasedOn']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Style.xml" path="//Member[@MemberName='BaseResourceKey']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Style.xml" path="//Member[@MemberName='Behaviors']/Docs/*" />
		public IList<Behavior> Behaviors => _behaviors ??= new AttachedCollection<Behavior>();

		/// <include file="../../docs/Microsoft.Maui.Controls/Style.xml" path="//Member[@MemberName='CanCascade']/Docs/*" />
		public bool CanCascade { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/Style.xml" path="//Member[@MemberName='Class']/Docs/*" />
		public string Class { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/Style.xml" path="//Member[@MemberName='Setters']/Docs/*" />
		public IList<Setter> Setters { get; }

		/// <include file="../../docs/Microsoft.Maui.Controls/Style.xml" path="//Member[@MemberName='Triggers']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Style.xml" path="//Member[@MemberName='TargetType']/Docs/*" />
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
				((IStyle)basedOn).Apply(bindable, new SetterSpecificity(specificity.Style - 1, 0, 0, 0));

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