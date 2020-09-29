using System;
using System.Collections.Generic;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms
{
	[ContentProperty("Setters")]
	[ProvideCompiled("Xamarin.Forms.Core.XamlC.PassthroughValueProvider")]
	[AcceptEmptyServiceProvider]
	public sealed class Trigger : TriggerBase, IValueProvider
	{
		public Trigger([TypeConverter(typeof(TypeTypeConverter))][Parameter("TargetType")] Type targetType) : base(new PropertyCondition(), targetType)
		{
		}

		public BindableProperty Property
		{
			get { return ((PropertyCondition)Condition).Property; }
			set
			{
				if (((PropertyCondition)Condition).Property == value)
					return;
				if (IsSealed)
					throw new InvalidOperationException("Can not change Property once the Trigger has been applied.");
				OnPropertyChanging();
				((PropertyCondition)Condition).Property = value;
				OnPropertyChanged();
			}
		}

		public new IList<Setter> Setters
		{
			get { return base.Setters; }
		}

		public object Value
		{
			get { return ((PropertyCondition)Condition).Value; }
			set
			{
				if (((PropertyCondition)Condition).Value == value)
					return;
				if (IsSealed)
					throw new InvalidOperationException("Can not change Value once the Trigger has been applied.");
				OnPropertyChanging();
				((PropertyCondition)Condition).Value = value;
				OnPropertyChanged();
			}
		}

		object IValueProvider.ProvideValue(IServiceProvider serviceProvider)
		{
			//This is no longer required
			return this;
		}
	}
}