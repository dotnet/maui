using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using Xunit;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Microsoft.Maui.DeviceTests;

public partial class DatePickerTests
{
	[Fact]
	[Description("The DatePicker Text and Icon Color should work properly on PointerOver")]
	public async Task DatePickerTextAndIconColorShouldWorkProperlyOnPointerOver()
	{
		SetupBuilder();

		var datePicker = new Controls.DatePicker
		{
			TextColor = Colors.Red
		};
		var expectedValue = datePicker.TextColor;

		var handler = await CreateHandlerAsync<DatePickerHandler>(datePicker);
		var platformView = GetPlatformControl(handler);

		await InvokeOnMainThreadAsync(() =>
		{
			var foregroundPointerOverBrush = platformView.Resources["CalendarDatePickerTextForegroundPointerOver"] as WSolidColorBrush;
			var foregroundPointerOverColor = foregroundPointerOverBrush.Color.ToColor();
			Assert.Equal(expectedValue, foregroundPointerOverColor);

			var glyphForegroundPointerOverBrush = platformView.Resources["CalendarDatePickerCalendarGlyphForegroundPointerOver"] as WSolidColorBrush;
			var glyphForegroundPointerOverColor = glyphForegroundPointerOverBrush.Color.ToColor();
			Assert.Equal(expectedValue, glyphForegroundPointerOverColor);
		});
	}

	static CalendarDatePicker GetPlatformControl(DatePickerHandler handler) =>
		handler.PlatformView;
}