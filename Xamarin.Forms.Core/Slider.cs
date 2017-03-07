using System;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_SliderRenderer))]
	public class Slider : View, IElementConfiguration<Slider>
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
			EventHandler<ValueChangedEventArgs> eh = slider.ValueChanged;
			if (eh != null)
				eh(slider, new ValueChangedEventArgs((double)oldValue, (double)newValue));
		});

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

		public IPlatformElementConfiguration<T, Slider> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}
	}
}