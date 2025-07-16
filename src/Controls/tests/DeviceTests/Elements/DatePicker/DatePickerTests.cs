using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.DatePicker)]
public partial class DatePickerTests : ControlsHandlerTestBase
{
	void SetupBuilder()
	{
		EnsureHandlerCreated(builder =>
		{
			builder.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<DatePicker, DatePickerHandler>();
			});
		});
	}

	[Fact(DisplayName = "DatePicker Null Date Behavior Across All Platforms")]
	public async Task DatePickerNullDateBehaviorAcrossAllPlatforms()
	{
		SetupBuilder();

		var testDate = new DateTime(2023, 5, 15);
		var datePicker = new DatePicker() { Format = "dd/MM/yy" };

		await CreateHandlerAndAddToWindow<DatePickerHandler>(datePicker, async handler =>
		{
			// Scenario 1: Initial null date should show today's date
			datePicker.Date = null;
			await Task.Delay(100);

			// The virtual view Date property should remain null
			Assert.Null(datePicker.Date);

			// But the platform view should display today's date
			var nullDateText = GetDisplayedText(handler);
			Assert.Equal(nullDateText, DateTime.Today.ToString("dd/MM/yy"));

			// Scenario 2: Null to specific date transition
			datePicker.Date = testDate;
			await Task.Delay(100);

			// The virtual view Date property should now be the test date
			Assert.Equal(testDate, datePicker.Date);

			// And the platform view should display the test date
			var specificDateText = GetDisplayedText(handler);
			Assert.Equal(specificDateText, testDate.ToString("dd/MM/yy"));

			// Scenario 3: Specific date back to null should revert to today's date
			datePicker.Date = null;
			await Task.Delay(100);

			// The virtual view Date property should be null again
			Assert.Null(datePicker.Date);

			// But the platform view should display today's date again
			var revertedNullText = GetDisplayedText(handler);
			Assert.Equal(revertedNullText, DateTime.Today.ToString("dd/MM/yy"));
		});
	}

	string GetDisplayedText(DatePickerHandler handler)
	{
		var platformView = handler.PlatformView;
		if (platformView is { } view)
		{
#if MACCATALYST
				// On MacCatalyst, the platform view is UIDatePicker directly, which doesn't have a Text property
				var nsDate = view.Date;
				var dateTime = ((DateTime)nsDate);
				return dateTime.ToString("dd/MM/yy");
#elif WINDOWS
	        // On Windows, the platform view is CalendarDatePicker with a Date property
	        if (view is Microsoft.UI.Xaml.Controls.CalendarDatePicker calendarPicker && 
	            calendarPicker.Date.HasValue)
	        {
	            return calendarPicker.Date.Value.DateTime.ToString("dd/MM/yy");
	        }

	        return string.Empty;
#else
			var textProperty = view.GetType().GetProperty("Text");
			if (textProperty != null)
			{
				return textProperty.GetValue(view) as string ?? string.Empty;
			}
#endif
		}

		return string.Empty;
	}
}