using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	[ContentProperty("Setters")]
	public sealed class MultiTrigger : TriggerBase
	{
		public MultiTrigger([System.ComponentModel.TypeConverter(typeof(TypeTypeConverter))][Parameter("TargetType")] Type targetType) : base(new MultiCondition(), targetType)
		{
		}

		public IList<Condition> Conditions
		{
			get { return ((MultiCondition)Condition).Conditions; }
		}

		public new IList<Setter> Setters
		{
			get { return base.Setters; }
		}
	}
}