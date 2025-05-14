#nullable disable
using System;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Represents a horizontal bar that a user can slide to select a value from a continuous range.
	/// </summary>
	/// <remarks>
	/// The <see cref="Slider"/> control allows users to select a numeric value by moving a thumb along a track.
	/// Use the <see cref="Minimum"/> and <see cref="Maximum"/> properties to define the range,
	/// and the <see cref="Value"/> property to get or set the current selection.
	/// </remarks>
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	[ElementHandler<SliderHandler>]
	public partial class Slider : View, ISliderController, IElementConfiguration<Slider>, ISlider
	{
		// Stores the value that was requested by the user, before clamping
		double _requestedValue = 0d;
		// Tracks if the user explicitly set Value (vs it being set by recoercion)
		bool _userSetValue = false;
		bool _isRecoercing = false;

		/// <summary>Bindable property for <see cref="Minimum"/>. This is a bindable property.</summary>
		public static readonly BindableProperty MinimumProperty = BindableProperty.Create(
			nameof(Minimum), typeof(double), typeof(Slider), 0d,
			propertyChanged: (bindable, oldValue, newValue) =>
			{
				var slider = (Slider)bindable;
				slider.RecoerceValue();
			});

		/// <summary>Bindable property for <see cref="Maximum"/>. This is a bindable property.</summary>
		public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
			nameof(Maximum), typeof(double), typeof(Slider), 1d,
			propertyChanged: (bindable, oldValue, newValue) =>
			{
				var slider = (Slider)bindable;
				slider.RecoerceValue();
			});

		/// <summary>Bindable property for <see cref="Value"/>. This is a bindable property.</summary>
		public static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(double), typeof(Slider), 0d, BindingMode.TwoWay, coerceValue: (bindable, value) =>
		{
			var slider = (Slider)bindable;
			// Only store the requested value if the user is setting it (not during recoercion)
			if (!slider._isRecoercing)
			{
				slider._requestedValue = (double)value;
				slider._userSetValue = true;
			}
			return ((double)value).Clamp(slider.Minimum, slider.Maximum);
		}, propertyChanged: (bindable, oldValue, newValue) =>
		{
			var slider = (Slider)bindable;
			slider.ValueChanged?.Invoke(slider, new ValueChangedEventArgs((double)oldValue, (double)newValue));
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

		/// <summary>Bindable property for <see cref="MinimumTrackColor"/>. This is a bindable property.</summary>
		public static readonly BindableProperty MinimumTrackColorProperty = BindableProperty.Create(nameof(MinimumTrackColor), typeof(Color), typeof(Slider), null);

		/// <summary>Bindable property for <see cref="MaximumTrackColor"/>. This is a bindable property.</summary>
		public static readonly BindableProperty MaximumTrackColorProperty = BindableProperty.Create(nameof(MaximumTrackColor), typeof(Color), typeof(Slider), null);

		/// <summary>Bindable property for <see cref="ThumbColor"/>. This is a bindable property.</summary>
		public static readonly BindableProperty ThumbColorProperty = BindableProperty.Create(nameof(ThumbColor), typeof(Color), typeof(Slider), null);

		/// <summary>Bindable property for <see cref="ThumbImageSource"/>. This is a bindable property.</summary>
		public static readonly BindableProperty ThumbImageSourceProperty = BindableProperty.Create(nameof(ThumbImageSource), typeof(ImageSource), typeof(Slider), default(ImageSource));

		/// <summary>Bindable property for <see cref="DragStartedCommand"/>. This is a bindable property.</summary>
		public static readonly BindableProperty DragStartedCommandProperty = BindableProperty.Create(nameof(DragStartedCommand), typeof(ICommand), typeof(Slider), default(ICommand));

		/// <summary>Bindable property for <see cref="DragCompletedCommand"/>. This is a bindable property.</summary>
		public static readonly BindableProperty DragCompletedCommandProperty = BindableProperty.Create(nameof(DragCompletedCommand), typeof(ICommand), typeof(Slider), default(ICommand));

		readonly Lazy<PlatformConfigurationRegistry<Slider>> _platformConfigurationRegistry;

		/// <summary>
		/// Initializes a new instance of the <see cref="Slider"/> class with default minimum (0) and maximum (1) values.
		/// </summary>
		public Slider()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Slider>>(() => new PlatformConfigurationRegistry<Slider>(this));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Slider"/> class with specified minimum, maximum, and initial values.
		/// </summary>
		/// <param name="min">The minimum value of the slider.</param>
		/// <param name="max">The maximum value of the slider.</param>
		/// <param name="val">The initial value of the slider, clamped between <paramref name="min"/> and <paramref name="max"/>.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="min"/> is greater than or equal to <paramref name="max"/>.</exception>
		public Slider(double min, double max, double val) : this()
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
			Value = val.Clamp(min, max);
		}

		/// <summary>
		/// Gets or sets the color of the filled portion of the slider track (from minimum to current value).
		/// This is a bindable property.
		/// </summary>
		/// <value>The <see cref="Color"/> of the minimum track. The default is <see langword="null"/>, which uses the platform default.</value>
		public Color MinimumTrackColor
		{
			get { return (Color)GetValue(MinimumTrackColorProperty); }
			set { SetValue(MinimumTrackColorProperty, value); }
		}

		/// <summary>
		/// Gets or sets the color of the unfilled portion of the slider track (from current value to maximum).
		/// This is a bindable property.
		/// </summary>
		/// <value>The <see cref="Color"/> of the maximum track. The default is <see langword="null"/>, which uses the platform default.</value>
		public Color MaximumTrackColor
		{
			get { return (Color)GetValue(MaximumTrackColorProperty); }
			set { SetValue(MaximumTrackColorProperty, value); }
		}

		/// <summary>
		/// Gets or sets the color of the slider thumb (the draggable element).
		/// This is a bindable property.
		/// </summary>
		/// <value>The <see cref="Color"/> of the thumb. The default is <see langword="null"/>, which uses the platform default.</value>
		public Color ThumbColor
		{
			get { return (Color)GetValue(ThumbColorProperty); }
			set { SetValue(ThumbColorProperty, value); }
		}

		/// <summary>
		/// Gets or sets an <see cref="ImageSource"/> to use as the slider thumb instead of the platform default.
		/// This is a bindable property.
		/// </summary>
		/// <value>The <see cref="ImageSource"/> for the thumb. The default is <see langword="null"/>.</value>
		public ImageSource ThumbImageSource
		{
			get { return (ImageSource)GetValue(ThumbImageSourceProperty); }
			set { SetValue(ThumbImageSourceProperty, value); }
		}

		/// <summary>
		/// Gets or sets the command to execute when the user starts dragging the slider thumb.
		/// This is a bindable property.
		/// </summary>
		/// <value>The <see cref="ICommand"/> to execute. The default is <see langword="null"/>.</value>
		public ICommand DragStartedCommand
		{
			get { return (ICommand)GetValue(DragStartedCommandProperty); }
			set { SetValue(DragStartedCommandProperty, value); }
		}

		/// <summary>
		/// Gets or sets the command to execute when the user completes dragging the slider thumb.
		/// This is a bindable property.
		/// </summary>
		/// <value>The <see cref="ICommand"/> to execute. The default is <see langword="null"/>.</value>
		public ICommand DragCompletedCommand
		{
			get { return (ICommand)GetValue(DragCompletedCommandProperty); }
			set { SetValue(DragCompletedCommandProperty, value); }
		}

		/// <summary>
		/// Gets or sets the maximum value of the slider.
		/// This is a bindable property.
		/// </summary>
		/// <value>The maximum value. The default is 1.</value>
		/// <remarks>Changing this value will automatically clamp the <see cref="Value"/> to be within the new range.</remarks>
		public double Maximum
		{
			get { return (double)GetValue(MaximumProperty); }
			set { SetValue(MaximumProperty, value); }
		}

		/// <summary>
		/// Gets or sets the minimum value of the slider.
		/// This is a bindable property.
		/// </summary>
		/// <value>The minimum value. The default is 0.</value>
		/// <remarks>Changing this value will automatically clamp the <see cref="Value"/> to be within the new range.</remarks>
		public double Minimum
		{
			get { return (double)GetValue(MinimumProperty); }
			set { SetValue(MinimumProperty, value); }
		}

		/// <summary>
		/// Gets or sets the current value of the slider.
		/// This is a bindable property.
		/// </summary>
		/// <value>The current value, clamped between <see cref="Minimum"/> and <see cref="Maximum"/>. The default is 0.</value>
		public double Value
		{
			get { return (double)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		/// <summary>
		/// Occurs when the <see cref="Value"/> property changes.
		/// </summary>
		public event EventHandler<ValueChangedEventArgs> ValueChanged;
		
		/// <summary>
		/// Occurs when the user starts dragging the slider thumb.
		/// </summary>
		public event EventHandler DragStarted;
		
		/// <summary>
		/// Occurs when the user completes dragging the slider thumb.
		/// </summary>
		public event EventHandler DragCompleted;

		void ISliderController.SendDragStarted()
		{
			if (IsEnabled)
			{
				DragStartedCommand?.Execute(null);
				DragStarted?.Invoke(this, null);
			}
		}

		void ISliderController.SendDragCompleted()
		{
			if (IsEnabled)
			{
				DragCompletedCommand?.Execute(null);
				DragCompleted?.Invoke(this, null);
			}
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, Slider> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		IImageSource ISlider.ThumbImageSource => ThumbImageSource;

		void ISlider.DragCompleted()
		{
			(this as ISliderController).SendDragCompleted();
		}

		void ISlider.DragStarted()
		{
			(this as ISliderController).SendDragStarted();
		}

		private protected override string GetDebuggerDisplay()
		{
			return $"{base.GetDebuggerDisplay()}, Value = {Value}, Min-Max = {Minimum} - {Maximum}";
		}
	}
}