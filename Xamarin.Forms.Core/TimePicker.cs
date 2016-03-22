using System;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_TimePickerRenderer))]
	public class TimePicker : View
	{
		public static readonly BindableProperty FormatProperty = BindableProperty.Create("Format", typeof(string), typeof(TimePicker), "t");

		public static readonly BindableProperty TimeProperty = BindableProperty.Create("Time", typeof(TimeSpan), typeof(TimePicker), new TimeSpan(0), BindingMode.TwoWay, (bindable, value) =>
		{
			var time = (TimeSpan)value;
			return time.TotalHours < 24 && time.TotalMilliseconds >= 0;
		});

		public string Format
		{
			get { return (string)GetValue(FormatProperty); }
			set { SetValue(FormatProperty, value); }
		}

		public TimeSpan Time
		{
			get { return (TimeSpan)GetValue(TimeProperty); }
			set { SetValue(TimeProperty, value); }
		}
	}
}