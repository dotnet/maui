#if NET6_0_OR_GREATER

namespace Microsoft.Maui.Controls.Core.UnitTests;

using System;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;

public class TimeOnlyTypeConverterTests : BaseTestFixture
{
	[Fact]
	public void TimeOnlyToTimeSpanConversion()
	{
		var converter = new TimeSpanTypeConverter();

		var timeOnlyValue = new TimeOnly(8, 30, 0);

		var actualTimeSpan = converter.ConvertFrom(null, CultureInfo.InvariantCulture, timeOnlyValue);
		var expectedTimeSpan = new TimeSpan(8, 30, 0);

		Assert.Equal(expectedTimeSpan, actualTimeSpan);
	}

	[Fact]
	public void TimeSpanToTimeOnlyConversion()
	{
		var converter = new TimeSpanTypeConverter();

		var timeSpanValue = new TimeSpan(8, 30, 0);

		var actualTimeOnly = converter.ConvertTo(null, CultureInfo.InvariantCulture, timeSpanValue, typeof(TimeOnly));
		var expectedTimeOnly = new TimeOnly(8, 30, 0);

		Assert.Equal(expectedTimeOnly, actualTimeOnly);
	}

	[Fact]
	public void TimeOnlyToTimePickerBinding()
	{
		var timePicker = new TimePicker();
		var source = new Issue20438TimePickerViewModel
		{
			SelectedTime = new TimeOnly(14, 30, 0)
		};
		timePicker.BindingContext = source;
		timePicker.SetBinding(TimePicker.TimeProperty, "SelectedTime");
		var expectedTimeSpan = new TimeSpan(14, 30, 0);
		Assert.Equal(expectedTimeSpan, timePicker.Time);
	}

	public class Issue20438TimePickerViewModel
	{
		public TimeOnly SelectedTime { get; set; }
	}
}
#endif