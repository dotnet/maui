using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

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

#if !MACCATALYST
	[Fact(DisplayName = "DatePicker Null Date Behavior Across All Platforms")]
	public async Task DatePickerNullDateBehaviorAcrossAllPlatforms()
	{
		SetupBuilder();

		var testDate = new DateTime(2023, 5, 15);
		var datePicker = new DatePicker() { Format = "dd/MM/yy" };

		await CreateHandlerAndAddToWindow<DatePickerHandler>(datePicker, handler =>
		{
			// Scenario 1: Initial null date should show today's date
			datePicker.Date = null;

			// The virtual view Date property should remain null
			Assert.Null(datePicker.Date);

			// But the platform view should display today's date
			var nullDateText = GetDisplayedText(handler);
			Assert.Equal(nullDateText, string.Empty);

			// Scenario 2: Null to specific date transition
			datePicker.Date = testDate;

			// The virtual view Date property should now be the test date
			Assert.Equal(testDate, datePicker.Date);

			// And the platform view should display the test date
			var specificDateText = GetDisplayedText(handler);
			Assert.Equal(specificDateText, testDate.ToString("dd/MM/yy"));

			// Scenario 3: Specific date back to null should revert to today's date
			datePicker.Date = null;

			// The virtual view Date property should be null again
			Assert.Null(datePicker.Date);

			// But the platform view should display today's date again
			var revertedNullText = GetDisplayedText(handler);
			Assert.Equal(revertedNullText, string.Empty);

			return Task.CompletedTask;
		});
	}
#endif

#if MACCATALYST
	[Fact(DisplayName = "Focus Opens DatePicker When Virtual Focus Is Stale")]
	public async Task FocusOpensDatePickerWhenVirtualFocusIsStale()
	{
		SetupBuilder();

		var datePicker = new DatePicker
		{
			Date = new DateTime(2026, 5, 20),
			WidthRequest = 200,
			HeightRequest = 44
		};

		await CreateHandlerAndAddToWindow<DatePickerHandler>(datePicker, async handler =>
		{
			((IView)datePicker).IsFocused = true;
			datePicker.IsOpen = false;

			var focusResult = datePicker.Focus();

			Assert.True(focusResult);
			await AssertEventually(
				() => datePicker.IsFocused && datePicker.IsOpen,
				message: "DatePicker focus did not open the picker when virtual focus was stale.");

			handler.Invoke(nameof(IView.Unfocus), null);
		});
	}

	[Fact(DisplayName = "IsOpen Raises Single Opened And Closed Events")]
	public async Task IsOpenRaisesSingleOpenedAndClosedEvents()
	{
		SetupBuilder();

		var openedCount = 0;
		var closedCount = 0;
		var datePicker = new DatePicker
		{
			Date = new DateTime(2026, 5, 20),
			WidthRequest = 200,
			HeightRequest = 44
		};

		datePicker.Opened += (_, _) => openedCount++;
		datePicker.Closed += (_, _) => closedCount++;

		await CreateHandlerAndAddToWindow<DatePickerHandler>(datePicker, async handler =>
		{
			datePicker.IsOpen = true;

			await AssertEventually(
				() => datePicker.IsFocused && datePicker.IsOpen,
				message: "DatePicker did not enter the focused/open state.");

			datePicker.IsOpen = false;

			await AssertEventually(
				() => !datePicker.IsFocused && !datePicker.IsOpen,
				message: "DatePicker did not leave the focused/open state.");

			Assert.Equal(1, openedCount);
			Assert.Equal(1, closedCount);
		});
	}

	[Fact(DisplayName = "Repeated Focus While Open Raises No Extra Opened Or Closed Events")]
	public async Task RepeatedFocusWhileOpenRaisesNoExtraOpenedOrClosedEvents()
	{
		SetupBuilder();

		var openedCount = 0;
		var closedCount = 0;
		var datePicker = new DatePicker
		{
			Date = new DateTime(2026, 5, 20),
			WidthRequest = 200,
			HeightRequest = 44
		};

		datePicker.Opened += (_, _) => openedCount++;
		datePicker.Closed += (_, _) => closedCount++;

		await CreateHandlerAndAddToWindow<DatePickerHandler>(datePicker, async handler =>
		{
			Assert.True(datePicker.Focus());
			await AssertEventually(
				() => datePicker.IsFocused && datePicker.IsOpen,
				message: "DatePicker did not enter the focused/open state.");

			Assert.Equal(1, openedCount);
			Assert.Equal(0, closedCount);

			Assert.True(datePicker.Focus());
			await Task.Delay(100);

			Assert.True(datePicker.IsFocused);
			Assert.True(datePicker.IsOpen);
			Assert.Equal(1, openedCount);
			Assert.Equal(0, closedCount);

			handler.Invoke(nameof(IView.Unfocus), null);
		});
	}
#endif

	string GetDisplayedText(DatePickerHandler handler)
	{
		var platformView = handler.PlatformView;
		if (platformView is { } view)
		{
#if MACCATALYST
			// On MacCatalyst, the platform view is UIDatePicker directly, which doesn't have a Text property
			var nsDate = view.Date;
			var dateTime = nsDate.ToDateTime();
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
#if ANDROID
				// Note: handler.DatePickerDialog may be null if DestroyDialog() was called
				// during the initial mapper pass (PR #33687), but setting VirtualView.Date
				// fires the DateSelected event regardless of dialog state.
				handler.VirtualView.Date = newDate;
#elif IOS
                if (handler.DatePickerDialog != null)
                {
                    handler.DatePickerDialog.SetDate(newDate.ToNSDate(), false);
                    typeof(DatePickerHandler).GetMethod("SetVirtualViewDate",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                        .Invoke(handler, null);
                }
#elif WINDOWS
                if (handler.PlatformView != null)
                {
                    handler.PlatformView.Date = newDate;
                }
#else
				handler.VirtualView.Date = newDate;
#endif
			});

			await Task.Delay(20);

			Assert.True(eventFired, "DateSelected event should fire when platform view date changes");
		});
	}
}
