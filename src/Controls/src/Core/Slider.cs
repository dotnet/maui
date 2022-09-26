using System;
using System.Windows.Input;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Slider.xml" path="Type[@FullName='Microsoft.Maui.Controls.Slider']/Docs/*" />
	public partial class Slider : View, ISliderController, IElementConfiguration<Slider>
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/Slider.xml" path="//Member[@MemberName='MinimumProperty']/Docs/*" />
		public static readonly BindableProperty MinimumProperty = BindableProperty.Create(nameof(Minimum), typeof(double), typeof(Slider), 0d, coerceValue: (bindable, value) =>
		{
			var slider = (Slider)bindable;
			slider.Value = slider.Value.Clamp((double)value, slider.Maximum);
			return value;
		});

		/// <include file="../../docs/Microsoft.Maui.Controls/Slider.xml" path="//Member[@MemberName='MaximumProperty']/Docs/*" />
		public static readonly BindableProperty MaximumProperty = BindableProperty.Create(nameof(Maximum), typeof(double), typeof(Slider), 1d, coerceValue: (bindable, value) =>
		{
			var slider = (Slider)bindable;
			slider.Value = slider.Value.Clamp(slider.Minimum, (double)value);
			return value;
		});

		/// <include file="../../docs/Microsoft.Maui.Controls/Slider.xml" path="//Member[@MemberName='ValueProperty']/Docs/*" />
		public static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(double), typeof(Slider), 0d, BindingMode.TwoWay, coerceValue: (bindable, value) =>
		{
			var slider = (Slider)bindable;
			return ((double)value).Clamp(slider.Minimum, slider.Maximum);
		}, propertyChanged: (bindable, oldValue, newValue) =>
		{
			var slider = (Slider)bindable;
			slider.ValueChanged?.Invoke(slider, new ValueChangedEventArgs((double)oldValue, (double)newValue));
		});

		/// <include file="../../docs/Microsoft.Maui.Controls/Slider.xml" path="//Member[@MemberName='MinimumTrackColorProperty']/Docs/*" />
		public static readonly BindableProperty MinimumTrackColorProperty = BindableProperty.Create(nameof(MinimumTrackColor), typeof(Color), typeof(Slider), null);

		/// <include file="../../docs/Microsoft.Maui.Controls/Slider.xml" path="//Member[@MemberName='MaximumTrackColorProperty']/Docs/*" />
		public static readonly BindableProperty MaximumTrackColorProperty = BindableProperty.Create(nameof(MaximumTrackColor), typeof(Color), typeof(Slider), null);

		/// <include file="../../docs/Microsoft.Maui.Controls/Slider.xml" path="//Member[@MemberName='ThumbColorProperty']/Docs/*" />
		public static readonly BindableProperty ThumbColorProperty = BindableProperty.Create(nameof(ThumbColor), typeof(Color), typeof(Slider), null);

		/// <include file="../../docs/Microsoft.Maui.Controls/Slider.xml" path="//Member[@MemberName='ThumbImageSourceProperty']/Docs/*" />
		public static readonly BindableProperty ThumbImageSourceProperty = BindableProperty.Create(nameof(ThumbImageSource), typeof(ImageSource), typeof(Slider), default(ImageSource));

		/// <include file="../../docs/Microsoft.Maui.Controls/Slider.xml" path="//Member[@MemberName='DragStartedCommandProperty']/Docs/*" />
		public static readonly BindableProperty DragStartedCommandProperty = BindableProperty.Create(nameof(DragStartedCommand), typeof(ICommand), typeof(Slider), default(ICommand));

		/// <include file="../../docs/Microsoft.Maui.Controls/Slider.xml" path="//Member[@MemberName='DragCompletedCommandProperty']/Docs/*" />
		public static readonly BindableProperty DragCompletedCommandProperty = BindableProperty.Create(nameof(DragCompletedCommand), typeof(ICommand), typeof(Slider), default(ICommand));

		readonly Lazy<PlatformConfigurationRegistry<Slider>> _platformConfigurationRegistry;

		/// <include file="../../docs/Microsoft.Maui.Controls/Slider.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public Slider()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Slider>>(() => new PlatformConfigurationRegistry<Slider>(this));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Slider.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Slider.xml" path="//Member[@MemberName='MinimumTrackColor']/Docs/*" />
		public Color MinimumTrackColor
		{
			get { return (Color)GetValue(MinimumTrackColorProperty); }
			set { SetValue(MinimumTrackColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Slider.xml" path="//Member[@MemberName='MaximumTrackColor']/Docs/*" />
		public Color MaximumTrackColor
		{
			get { return (Color)GetValue(MaximumTrackColorProperty); }
			set { SetValue(MaximumTrackColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Slider.xml" path="//Member[@MemberName='ThumbColor']/Docs/*" />
		public Color ThumbColor
		{
			get { return (Color)GetValue(ThumbColorProperty); }
			set { SetValue(ThumbColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Slider.xml" path="//Member[@MemberName='ThumbImageSource']/Docs/*" />
		public ImageSource ThumbImageSource
		{
			get { return (ImageSource)GetValue(ThumbImageSourceProperty); }
			set { SetValue(ThumbImageSourceProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Slider.xml" path="//Member[@MemberName='DragStartedCommand']/Docs/*" />
		public ICommand DragStartedCommand
		{
			get { return (ICommand)GetValue(DragStartedCommandProperty); }
			set { SetValue(DragStartedCommandProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Slider.xml" path="//Member[@MemberName='DragCompletedCommand']/Docs/*" />
		public ICommand DragCompletedCommand
		{
			get { return (ICommand)GetValue(DragCompletedCommandProperty); }
			set { SetValue(DragCompletedCommandProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Slider.xml" path="//Member[@MemberName='Maximum']/Docs/*" />
		public double Maximum
		{
			get { return (double)GetValue(MaximumProperty); }
			set { SetValue(MaximumProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Slider.xml" path="//Member[@MemberName='Minimum']/Docs/*" />
		public double Minimum
		{
			get { return (double)GetValue(MinimumProperty); }
			set { SetValue(MinimumProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Slider.xml" path="//Member[@MemberName='Value']/Docs/*" />
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

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, Slider> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}
	}
}