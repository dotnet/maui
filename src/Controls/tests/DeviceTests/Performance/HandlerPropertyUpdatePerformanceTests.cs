#if ANDROID || WINDOWS
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Handlers;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(RunInNewWindowCollection)]
	[Category(TestCategory.PerformanceHandlerPropertyUpdate)]
	public class HandlerPropertyUpdatePerformanceTests : ControlsHandlerTestBase
	{
		const int WarmupCount = 2;
		const int IterationCount = 10;

		[Fact(DisplayName = "Common handler property-update batch performance")]
		public async Task CommonControlPropertyUpdates()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<Button, ButtonHandler>();
					handlers.AddHandler<Entry, EntryHandler>();
					handlers.AddHandler<Picker, PickerHandler>();
					handlers.AddHandler<DatePicker, DatePickerHandler>();
					handlers.AddHandler<TimePicker, TimePickerHandler>();
				});
			});

			var label = new Label { Text = "Label 0" };
			var button = new Button { Text = "Button 0" };
			var entry = new Entry { Text = "Entry 0", Placeholder = "Placeholder 0" };
			var picker = new Picker { ItemsSource = new[] { "Zero", "One", "Two" }, SelectedIndex = 0 };
			var datePicker = new DatePicker { Date = new DateTime(2025, 1, 1) };
			var timePicker = new TimePicker { Time = TimeSpan.FromHours(1) };
			var layout = new VerticalStackLayout
			{
				WidthRequest = 400,
				HeightRequest = 700,
				Children = { label, button, entry, picker, datePicker, timePicker }
			};
			var counters = new Dictionary<string, double>(StringComparer.Ordinal)
			{
				["completedUpdateBatches"] = 0,
				["nativeValueMismatchCount"] = 0
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async _ =>
			{
				DevicePerformanceResult result = await DevicePerformanceMeasurement.MeasureAsync(
					"handler-property-update-batch",
					WarmupCount,
					IterationCount,
					async iteration =>
					{
						int value = iteration + 1;
						await InvokeOnMainThreadAsync(() =>
						{
							label.Text = $"Label {value}";
							label.FontSize = 12 + value;
							button.Text = $"Button {value}";
							button.IsEnabled = value % 2 == 0;
							entry.Text = $"Entry {value}";
							entry.Placeholder = $"Placeholder {value}";
							picker.SelectedIndex = value % 3;
							datePicker.Date = new DateTime(2025, 1, 1).AddDays(value);
							timePicker.Time = TimeSpan.FromMinutes(value * 5);
							layout.Spacing = value % 8;
							layout.Measure(400, 700);
							layout.Arrange(new Microsoft.Maui.Graphics.Rect(0, 0, 400, 700));
						});
					},
					counters,
					async _ =>
					{
						bool nativeValuesMatch = await InvokeOnMainThreadAsync(() =>
							NativeValuesMatch(label, button, entry, picker, datePicker, timePicker));
						if (!nativeValuesMatch)
							counters["nativeValueMismatchCount"]++;
						counters["completedUpdateBatches"]++;
					});

				DevicePerformanceReporter.Write(result);
			}, MauiContext, TimeSpan.FromMinutes(2));
		}

		static bool NativeValuesMatch(
			Label label,
			Button button,
			Entry entry,
			Picker picker,
			DatePicker datePicker,
			TimePicker timePicker)
		{
#if ANDROID
			return label.Handler?.PlatformView is global::Android.Widget.TextView nativeLabel
				&& button.Handler?.PlatformView is global::Android.Widget.Button nativeButton
				&& entry.Handler?.PlatformView is global::Android.Widget.EditText nativeEntry
				&& picker.Handler?.PlatformView is global::Microsoft.Maui.Platform.MauiPicker nativePicker
				&& datePicker.Handler?.PlatformView is global::Microsoft.Maui.Platform.MauiDatePicker nativeDatePicker
				&& timePicker.Handler?.PlatformView is global::Microsoft.Maui.Platform.MauiTimePicker nativeTimePicker
				&& nativeLabel.Text == label.Text
				&& nativeButton.Enabled == button.IsEnabled
				&& nativeEntry.Text == entry.Text
				&& nativePicker.Text == picker.SelectedItem?.ToString()
				&& !string.IsNullOrWhiteSpace(nativeDatePicker.Text)
				&& !string.IsNullOrWhiteSpace(nativeTimePicker.Text);
#elif WINDOWS
			return label.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.TextBlock nativeLabel
				&& button.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.Button nativeButton
				&& entry.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.TextBox nativeEntry
				&& picker.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.ComboBox nativePicker
				&& datePicker.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.CalendarDatePicker nativeDatePicker
				&& timePicker.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.TimePicker nativeTimePicker
				&& nativeLabel.Text == label.Text
				&& nativeButton.IsEnabled == button.IsEnabled
				&& nativeEntry.Text == entry.Text
				&& nativePicker.SelectedIndex == picker.SelectedIndex
				&& nativeDatePicker.Date.GetValueOrDefault().Date == datePicker.Date.GetValueOrDefault().Date
				&& nativeTimePicker.Time == timePicker.Time;
#else
			return false;
#endif
		}
	}
}
#endif
