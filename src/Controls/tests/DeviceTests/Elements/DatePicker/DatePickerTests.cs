using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
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

	[Fact(DisplayName = "DateSelected Event Fires When Platform View Date Changes")]
	public async Task DateSelectedEventFiresWhenPlatformViewDateChanges()
	{
		SetupBuilder();

		var originalDate = new DateTime(2023, 5, 15);
		var newDate = new DateTime(2023, 8, 20);

		var datePicker = new DatePicker
		{
			Date = originalDate
		};

		bool eventFired = false;

		datePicker.DateSelected += (sender, e) =>
		{
			eventFired = true;
		};

		await CreateHandlerAndAddToWindow<DatePickerHandler>(datePicker, async (handler) =>
		{
			await InvokeOnMainThreadAsync(() =>
			{
				datePicker.Date = newDate;

#if ANDROID
				handler.DatePickerDialog?.UpdateDate(newDate.Year, newDate.Month - 1, newDate.Day);
#elif IOS
                if (handler.DatePickerDialog != null)
                {
                    handler.DatePickerDialog.SetDate(newDate.ToNSDate(), false);
                }
#elif WINDOWS
                if (handler.PlatformView != null)
                {
                    handler.PlatformView.Date = newDate;
                }
#elif MACCATALYST
	 handler.PlatformView.Date = newDate.ToNSDate();
#endif
			});

			await Task.Delay(20);

			Assert.True(eventFired, "DateSelected event should fire when platform view date changes");
		});
	}
}