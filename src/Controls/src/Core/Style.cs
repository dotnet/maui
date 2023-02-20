#nullable disable
using System;
using System.Collections.Generic;
using System.Reflection;
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

		readonly WeakList<BindableObject> _targets = new();

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
				foreach (var target in _targets)
				{
					target.RemoveDynamicResource(_basedOnResourceProperty);
					if (value != null)
						target.SetDynamicResource(_basedOnResourceProperty, value);
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

		void IStyle.Apply(BindableObject bindable)
		{
			lock (_targets)
			{
				_targets.Add(bindable);
			}

			if (BaseResourceKey != null)
				bindable.SetDynamicResource(_basedOnResourceProperty, BaseResourceKey);
			ApplyCore(bindable, BasedOn ?? GetBasedOnResource(bindable));
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
			foreach (var bindable in _targets)
			{
				UnApplyCore(bindable, oldValue);
				ApplyCore(bindable, newValue);
			}
		}

		Style GetBasedOnResource(BindableObject bindable) => (Style)bindable.GetValue(_basedOnResourceProperty);

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
			=> value is null || value.TargetType.IsAssignableFrom(TargetType);
	}
}