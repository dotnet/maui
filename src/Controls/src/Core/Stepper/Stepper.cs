#nullable disable
using System;
using System.Diagnostics;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Represents a control that allows a user to incrementally adjust a numeric value by tapping plus or minus buttons.
	/// </summary>
	/// <remarks>
	/// The <see cref="Stepper"/> provides buttons to increase or decrease a numeric value by a fixed <see cref="Increment"/>.
	/// The value is constrained between <see cref="Minimum"/> and <see cref="Maximum"/>.
	/// </remarks>
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	[ElementHandler(typeof(StepperHandler))]
	public partial class Stepper : View, IElementConfiguration<Stepper>, IStepper
	{
		// Stores the value that was requested by the user, before clamping
		double _requestedValue = 0d;
		// Tracks if the user explicitly set Value (vs it being set by recoercion)
		bool _userSetValue = false;
		bool _isRecoercing = false;

		/// <summary>Bindable property for <see cref="Maximum"/>. This is a bindable property.</summary>
		public static readonly BindableProperty MaximumProperty = BindableProperty.Create(nameof(Maximum), typeof(double), typeof(Stepper), 100.0,
			validateValue: (bindable, value) => (double)value >= ((Stepper)bindable).Minimum,
			propertyChanged: (bindable, oldValue, newValue) =>
			{
				var stepper = (Stepper)bindable;
				stepper.RecoerceValue();
			});

		/// <summary>Bindable property for <see cref="Minimum"/>. This is a bindable property.</summary>
		public static readonly BindableProperty MinimumProperty = BindableProperty.Create(nameof(Minimum), typeof(double), typeof(Stepper), 0.0,
			validateValue: (bindable, value) => (double)value <= ((Stepper)bindable).Maximum,
			propertyChanged: (bindable, oldValue, newValue) =>
			{
				var stepper = (Stepper)bindable;
				stepper.RecoerceValue();
			});

		/// <summary>Bindable property for <see cref="Value"/>. This is a bindable property.</summary>
		public static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(double), typeof(Stepper), 0.0, BindingMode.TwoWay,
			coerceValue: (bindable, value) =>
			{
				var stepper = (Stepper)bindable;
				// Only store the requested value if the user is setting it (not during recoercion)
				if (!stepper._isRecoercing)
				{
					stepper._requestedValue = (double)value;
					stepper._userSetValue = true;
				}
				return Math.Round(((double)value), stepper.digits).Clamp(stepper.Minimum, stepper.Maximum);
			},
			propertyChanged: (bindable, oldValue, newValue) =>
			{
				var stepper = (Stepper)bindable;
				stepper.ValueChanged?.Invoke(stepper, new ValueChangedEventArgs((double)oldValue, (double)newValue));
			});

		void RecoerceValue()
		{
			_isRecoercing = true;
			try
			{
				// If the user explicitly set Value, try to restore the requested value within the new range
				if (_userSetValue)
					Value = _requestedValue;
				else
					Value = Value.Clamp(Minimum, Maximum);
			}
			finally
			{
				_isRecoercing = false;
			}
		}

		int digits = 4;
		//'-log10(increment) + 4' as rounding digits gives us 4 significant decimal digits after the most significant one.
		//If your increment uses more than 4 significant digits, you're holding it wrong.
		/// <summary>Bindable property for <see cref="Increment"/>. This is a bindable property.</summary>
		public static readonly BindableProperty IncrementProperty = BindableProperty.Create(nameof(Increment), typeof(double), typeof(Stepper), 1.0,
			propertyChanged: (b, o, n) => { ((Stepper)b).digits = (int)(-Math.Log10((double)n) + 4).Clamp(1, 15); });

		readonly Lazy<PlatformConfigurationRegistry<Stepper>> _platformConfigurationRegistry;

		/// <summary>
		/// Initializes a new instance of the <see cref="Stepper"/> class with default minimum (0), maximum (100), and increment (1) values.
		/// </summary>
		public Stepper() => _platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Stepper>>(() => new PlatformConfigurationRegistry<Stepper>(this));

		/// <summary>
		/// Initializes a new instance of the <see cref="Stepper"/> class with specified minimum, maximum, value, and increment.
		/// </summary>
		/// <param name="min">The minimum value.</param>
		/// <param name="max">The maximum value.</param>
		/// <param name="val">The initial value, clamped between <paramref name="min"/> and <paramref name="max"/>.</param>
		/// <param name="increment">The amount to increment or decrement the value by with each button press.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="min"/> is greater than or equal to <paramref name="max"/>.</exception>
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

		/// <summary>
		/// Gets or sets the amount by which the stepper value changes with each button press.
		/// This is a bindable property.
		/// </summary>
		/// <value>The increment value. The default is 1.</value>
		public double Increment
		{
			get => (double)GetValue(IncrementProperty);
			set => SetValue(IncrementProperty, value);
		}

		/// <summary>
		/// Gets or sets the maximum value of the stepper.
		/// This is a bindable property.
		/// </summary>
		/// <value>The maximum value. The default is 100.</value>
		/// <remarks>Changing this value will automatically clamp the <see cref="Value"/> to be within the new range.</remarks>
		public double Maximum
		{
			get => (double)GetValue(MaximumProperty);
			set => SetValue(MaximumProperty, value);
		}

		/// <summary>
		/// Gets or sets the minimum value of the stepper.
		/// This is a bindable property.
		/// </summary>
		/// <value>The minimum value. The default is 0.</value>
		/// <remarks>Changing this value will automatically clamp the <see cref="Value"/> to be within the new range.</remarks>
		public double Minimum
		{
			get => (double)GetValue(MinimumProperty);
			set => SetValue(MinimumProperty, value);
		}

		/// <summary>
		/// Gets or sets the current value of the stepper.
		/// This is a bindable property.
		/// </summary>
		/// <value>The current value, clamped between <see cref="Minimum"/> and <see cref="Maximum"/>. The default is 0.</value>
		public double Value
		{
			get => (double)GetValue(ValueProperty);
			set => SetValue(ValueProperty, value);
		}

		/// <summary>
		/// Occurs when the <see cref="Value"/> property changes.
		/// </summary>
		public event EventHandler<ValueChangedEventArgs> ValueChanged;

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, Stepper> On<T>() where T : IConfigPlatform => _platformConfigurationRegistry.Value.On<T>();

		double IStepper.Interval => Increment;

		private protected override string GetDebuggerDisplay()
		{
			return $"{base.GetDebuggerDisplay()}, Value = {Value}";
		}
	}
}