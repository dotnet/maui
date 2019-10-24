using System;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_TimePickerRenderer))]
	public class TimePicker : View, ITextElement, IElementConfiguration<TimePicker>
	{
		public static readonly BindableProperty FormatProperty = BindableProperty.Create(nameof(Format), typeof(string), typeof(TimePicker), "t");

		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		public static readonly BindableProperty TimeProperty = BindableProperty.Create(nameof(Time), typeof(TimeSpan), typeof(TimePicker), new TimeSpan(0), BindingMode.TwoWay, (bindable, value) =>
		{
			var time = (TimeSpan)value;
			return time.TotalHours < 24 && time.TotalMilliseconds >= 0;
		});

		readonly Lazy<PlatformConfigurationRegistry<TimePicker>> _platformConfigurationRegistry;

		public TimePicker()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<TimePicker>>(() => new PlatformConfigurationRegistry<TimePicker>(this));
		}

		public string Format
		{
			get { return (string)GetValue(FormatProperty); }
			set { SetValue(FormatProperty, value); }
		}

		public Color TextColor
		{
			get { return (Color)GetValue(TextElement.TextColorProperty); }
			set { SetValue(TextElement.TextColorProperty, value); }
		}

		public TimeSpan Time
		{
			get { return (TimeSpan)GetValue(TimeProperty); }
			set { SetValue(TimeProperty, value); }
		}

		public IPlatformElementConfiguration<T, TimePicker> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		void ITextElement.OnTextColorPropertyChanged(Color oldValue, Color newValue)
		{
		}
	}
}