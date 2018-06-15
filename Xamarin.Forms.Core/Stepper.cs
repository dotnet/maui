using System;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_StepperRenderer))]
	public class Stepper : View, IElementConfiguration<Stepper>
	{
		public static readonly BindableProperty MaximumProperty = BindableProperty.Create("Maximum", typeof(double), typeof(Stepper), 100.0, validateValue: (bindable, value) =>
		{
			var stepper = (Stepper)bindable;
			return (double)value > stepper.Minimum;
		}, coerceValue: (bindable, value) =>
		{
			var stepper = (Stepper)bindable;
			stepper.Value = stepper.Value.Clamp(stepper.Minimum, (double)value);
			return value;
		});

		public static readonly BindableProperty MinimumProperty = BindableProperty.Create("Minimum", typeof(double), typeof(Stepper), 0.0, validateValue: (bindable, value) =>
		{
			var stepper = (Stepper)bindable;
			return (double)value < stepper.Maximum;
		}, coerceValue: (bindable, value) =>
		{
			var stepper = (Stepper)bindable;
			stepper.Value = stepper.Value.Clamp((double)value, stepper.Maximum);
			return value;
		});

		public static readonly BindableProperty ValueProperty = BindableProperty.Create("Value", typeof(double), typeof(Stepper), 0.0, BindingMode.TwoWay, coerceValue: (bindable, value) =>
		{
			var stepper = (Stepper)bindable;
			return ((double)value).Clamp(stepper.Minimum, stepper.Maximum);
		}, propertyChanged: (bindable, oldValue, newValue) =>
		{
			var stepper = (Stepper)bindable;
			EventHandler<ValueChangedEventArgs> eh = stepper.ValueChanged;
			if (eh != null)
				eh(stepper, new ValueChangedEventArgs((double)oldValue, (double)newValue));
		});

		public static readonly BindableProperty IncrementProperty = BindableProperty.Create("Increment", typeof(double), typeof(Stepper), 1.0);

		readonly Lazy<PlatformConfigurationRegistry<Stepper>> _platformConfigurationRegistry;

		public Stepper()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Stepper>>(() => new PlatformConfigurationRegistry<Stepper>(this));
		}

		public Stepper(double min, double max, double val, double increment) : this()
		{
			if (min >= max)
				throw new ArgumentOutOfRangeException("min");
			if (max > Minimum)
			{
				Maximum = max;
				Minimum = min;
			}
			else
			{
				Minimum = min;
				Maximum = max;
			}

			Value = val.Clamp(min, max);
			Increment = increment;
		}

		public double Increment
		{
			get { return (double)GetValue(IncrementProperty); }
			set { SetValue(IncrementProperty, value); }
		}

		public double Maximum
		{
			get { return (double)GetValue(MaximumProperty); }
			set { SetValue(MaximumProperty, value); }
		}

		public double Minimum
		{
			get { return (double)GetValue(MinimumProperty); }
			set { SetValue(MinimumProperty, value); }
		}

		public double Value
		{
			get { return (double)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		public event EventHandler<ValueChangedEventArgs> ValueChanged;
		
		public IPlatformElementConfiguration<T, Stepper> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}
	}
}