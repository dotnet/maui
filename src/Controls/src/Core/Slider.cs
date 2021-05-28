using System;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class Slider : View, ISliderController, IElementConfiguration<Slider>
	{
		public static readonly BindableProperty MinimumProperty = BindableProperty.Create("Minimum", typeof(double), typeof(Slider), 0d, validateValue: (bindable, value) =>
		{
			var slider = (Slider)bindable;
			return (double)value < slider.Maximum;
		}, coerceValue: (bindable, value) =>
		{
			var slider = (Slider)bindable;
			slider.Value = slider.Value.Clamp((double)value, slider.Maximum);
			return value;
		});

		public static readonly BindableProperty MaximumProperty = BindableProperty.Create("Maximum", typeof(double), typeof(Slider), 1d, validateValue: (bindable, value) =>
		{
			var slider = (Slider)bindable;
			return (double)value > slider.Minimum;
		}, coerceValue: (bindable, value) =>
		{
			var slider = (Slider)bindable;
			slider.Value = slider.Value.Clamp(slider.Minimum, (double)value);
			return value;
		});

		public static readonly BindableProperty ValueProperty = BindableProperty.Create("Value", typeof(double), typeof(Slider), 0d, BindingMode.TwoWay, coerceValue: (bindable, value) =>
		{
			var slider = (Slider)bindable;
			return ((double)value).Clamp(slider.Minimum, slider.Maximum);
		}, propertyChanged: (bindable, oldValue, newValue) =>
		{
			var slider = (Slider)bindable;
			slider.ValueChanged?.Invoke(slider, new ValueChangedEventArgs((double)oldValue, (double)newValue));
		});

		public static readonly BindableProperty MinimumTrackColorProperty = BindableProperty.Create(nameof(MinimumTrackColor), typeof(Color), typeof(Slider), null);

		public static readonly BindableProperty MaximumTrackColorProperty = BindableProperty.Create(nameof(MaximumTrackColor), typeof(Color), typeof(Slider), null);

		public static readonly BindableProperty ThumbColorProperty = BindableProperty.Create(nameof(ThumbColor), typeof(Color), typeof(Slider), null);

		public static readonly BindableProperty ThumbImageSourceProperty = BindableProperty.Create(nameof(ThumbImageSource), typeof(ImageSource), typeof(Slider), default(ImageSource));

		public static readonly BindableProperty DragStartedCommandProperty = BindableProperty.Create(nameof(DragStartedCommand), typeof(ICommand), typeof(Slider), default(ICommand));

		public static readonly BindableProperty DragCompletedCommandProperty = BindableProperty.Create(nameof(DragCompletedCommand), typeof(ICommand), typeof(Slider), default(ICommand));

		readonly Lazy<PlatformConfigurationRegistry<Slider>> _platformConfigurationRegistry;

		public Slider()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Slider>>(() => new PlatformConfigurationRegistry<Slider>(this));
		}

		public Slider(double min, double max, double val) : this()
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
		}

		public Color MinimumTrackColor
		{
			get { return (Color)GetValue(MinimumTrackColorProperty); }
			set { SetValue(MinimumTrackColorProperty, value); }
		}

		public Color MaximumTrackColor
		{
			get { return (Color)GetValue(MaximumTrackColorProperty); }
			set { SetValue(MaximumTrackColorProperty, value); }
		}

		public Color ThumbColor
		{
			get { return (Color)GetValue(ThumbColorProperty); }
			set { SetValue(ThumbColorProperty, value); }
		}

		public ImageSource ThumbImageSource
		{
			get { return (ImageSource)GetValue(ThumbImageSourceProperty); }
			set { SetValue(ThumbImageSourceProperty, value); }
		}

		public ICommand DragStartedCommand
		{
			get { return (ICommand)GetValue(DragStartedCommandProperty); }
			set { SetValue(DragStartedCommandProperty, value); }
		}

		public ICommand DragCompletedCommand
		{
			get { return (ICommand)GetValue(DragCompletedCommandProperty); }
			set { SetValue(DragCompletedCommandProperty, value); }
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
		public event EventHandler DragStarted;
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

		public IPlatformElementConfiguration<T, Slider> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}
	}
}