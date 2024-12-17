#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/DataTrigger.xml" path="Type[@FullName='Microsoft.Maui.Controls.DataTrigger']/Docs/*" />
	[ContentProperty("Setters")]
	public sealed class DataTrigger : TriggerBase
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/DataTrigger.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public DataTrigger([System.ComponentModel.TypeConverter(typeof(TypeTypeConverter))][Parameter("TargetType")] Type targetType) : base(new BindingCondition(), targetType)
		{
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/DataTrigger.xml" path="//Member[@MemberName='Binding']/Docs/*" />
		public BindingBase Binding
		{
			get { return ((BindingCondition)Condition).Binding; }
			set
			{
				if (((BindingCondition)Condition).Binding == value)
					return;
				if (IsSealed)
					throw new InvalidOperationException("Cannot change Binding once the Trigger has been applied.");
				OnPropertyChanging();
				((BindingCondition)Condition).Binding = value;
				OnPropertyChanged();
			}
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/DataTrigger.xml" path="//Member[@MemberName='Setters']/Docs/*" />
		public new IList<Setter> Setters
		{
			get { return base.Setters; }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/DataTrigger.xml" path="//Member[@MemberName='Value']/Docs/*" />
		public object Value
		{
			get { return ((BindingCondition)Condition).Value; }
			set
			{
				if (((BindingCondition)Condition).Value == value)
					return;
				if (IsSealed)
					throw new InvalidOperationException("Cannot change Value once the Trigger has been applied.");
				OnPropertyChanging();
				((BindingCondition)Condition).Value = value;
				OnPropertyChanged();
			}
		}
	}
}