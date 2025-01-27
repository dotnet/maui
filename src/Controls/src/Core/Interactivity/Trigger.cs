#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/Trigger.xml" path="Type[@FullName='Microsoft.Maui.Controls.Trigger']/Docs/*" />
	[ContentProperty("Setters")]
	public sealed class Trigger : TriggerBase
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/Trigger.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public Trigger([System.ComponentModel.TypeConverter(typeof(TypeTypeConverter))][Parameter("TargetType")] Type targetType) : base(new PropertyCondition(), targetType)
		{
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Trigger.xml" path="//Member[@MemberName='Property']/Docs/*" />
		public BindableProperty Property
		{
			get { return ((PropertyCondition)Condition).Property; }
			set
			{
				if (((PropertyCondition)Condition).Property == value)
					return;
				if (IsSealed)
					throw new InvalidOperationException("Cannot change Property once the Trigger has been applied.");
				OnPropertyChanging();
				((PropertyCondition)Condition).Property = value;
				OnPropertyChanged();
			}
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Trigger.xml" path="//Member[@MemberName='Setters']/Docs/*" />
		public new IList<Setter> Setters
		{
			get { return base.Setters; }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Trigger.xml" path="//Member[@MemberName='Value']/Docs/*" />
		public object Value
		{
			get { return ((PropertyCondition)Condition).Value; }
			set
			{
				if (((PropertyCondition)Condition).Value == value)
					return;
				if (IsSealed)
					throw new InvalidOperationException("Cannot change Value once the Trigger has been applied.");
				OnPropertyChanging();
				((PropertyCondition)Condition).Value = value;
				OnPropertyChanged();
			}
		}
	}
}