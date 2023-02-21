#nullable disable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/MultiTrigger.xml" path="Type[@FullName='Microsoft.Maui.Controls.MultiTrigger']/Docs/*" />
	[ContentProperty("Setters")]
	public sealed class MultiTrigger : TriggerBase
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/MultiTrigger.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public MultiTrigger([System.ComponentModel.TypeConverter(typeof(TypeTypeConverter))][Parameter("TargetType")] Type targetType) : base(new MultiCondition(), targetType)
		{
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/MultiTrigger.xml" path="//Member[@MemberName='Conditions']/Docs/*" />
		public IList<Condition> Conditions
		{
			get { return ((MultiCondition)Condition).Conditions; }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/MultiTrigger.xml" path="//Member[@MemberName='Setters']/Docs/*" />
		public new IList<Setter> Setters
		{
			get { return base.Setters; }
		}
	}
}