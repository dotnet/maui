using System;
using System.ComponentModel;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_StepperRenderer))]
	public class Stepper : View, IElementConfiguration<Stepper>
	{
		public static readonly BindableProperty MaximumProperty = BindableProperty.Create(nameof(Maximum), typeof(double), typeof(Stepper), 100.0,
			validateValue: (bindable, value) => (double)value > ((Stepper)bindable).Minimum,
			coerceValue: (bindable, value) =>
			{
				var stepper = (Stepper)bindable;
				stepper.Value = stepper.Value.Clamp(stepper.Minimum, (double)value);
				return value;
			});

		public static readonly BindableProperty MinimumProperty = BindableProperty.Create(nameof(Minimum), typeof(double), typeof(Stepper), 0.0,
			validateValue: (bindable, value) => (double)value < ((Stepper)bindable).Maximum,
			coerceValue: (bindable, value) =>
			{
				var stepper = (Stepper)bindable;
				stepper.Value = stepper.Value.Clamp((double)value, stepper.Maximum);
				return value;
			});

		public static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(double), typeof(Stepper), 0.0, BindingMode.TwoWay,
			coerceValue: (bindable, value) =>
			{
				var stepper = (Stepper)bindable;
				return Math.Round(((double)value), stepper.digits).Clamp(stepper.Minimum, stepper.Maximum);
			},
			propertyChanged: (bindable, oldValue, newValue) =>
			{
				var stepper = (Stepper)bindable;
				stepper.ValueChanged?.Invoke(stepper, new ValueChangedEventArgs((double)oldValue, (double)newValue));
			});

		int digits = 4;
		//'-log10(increment) + 4' as rounding digits gives us 4 significant decimal digits after the most significant one.
		//If your increment uses more than 4 significant digits, you're holding it wrong.
		public static readonly BindableProperty IncrementProperty = BindableProperty.Create(nameof(Increment), typeof(double), typeof(Stepper), 1.0,
			propertyChanged: (b, o, n) => { ((Stepper)b).digits = (int)(-Math.Log10((double)n) + 4).Clamp(1, 15); });

		readonly Lazy<PlatformConfigurationRegistry<Stepper>> _platformConfigurationRegistry;

		public Stepper() => _platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Stepper>>(() => new PlatformConfigurationRegistry<Stepper>(this));

		public Stepper(double min, double max, double val, double increment) : this()
		{
			if (min >= max)
				throw new ArgumentOutOfRangeException(nameof(min));
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

			Increment = increment;
			Value = val.Clamp(min, max);
		}

		public double Increment
		{
			get => (double)GetValue(IncrementProperty);
			set => SetValue(IncrementProperty, value);
		}

		public double Maximum
		{
			get => (double)GetValue(MaximumProperty);
			set => SetValue(MaximumProperty, value);
		}

		public double Minimum
		{
			get => (double)GetValue(MinimumProperty);
			set => SetValue(MinimumProperty, value);
		}

		public double Value
		{
			get => (double)GetValue(ValueProperty);
			set => SetValue(ValueProperty, value);
		}

		public event EventHandler<ValueChangedEventArgs> ValueChanged;

		public IPlatformElementConfiguration<T, Stepper> On<T>() where T : IConfigPlatform => _platformConfigurationRegistry.Value.On<T>();

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("deprecated without replacement in 4.8.0")]
		public int StepperPosition { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("deprecated without replacement in 4.8.0")]
		public static readonly BindableProperty StepperPositionProperty = BindableProperty.Create(nameof(StepperPosition), typeof(int), typeof(Stepper), 0);
	}
}