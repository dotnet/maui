using System;
using System.Collections.Generic;

namespace Xamarin.Forms
{
	[ContentProperty("Setters")]
	public sealed class MultiTrigger : TriggerBase
	{
		public MultiTrigger([TypeConverter(typeof(TypeTypeConverter))][Parameter("TargetType")] Type targetType) : base(new MultiCondition(), targetType)
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