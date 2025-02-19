using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Picker)]
	[Category(UITestCategories.DatePicker)]
	[Category(UITestCategories.TimePicker)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5159, "[Android] Calling Focus on all Pickers running an API 28 devices no longer opens Picker", PlatformAffected.Android)]
	public class Issue5159 : TestContentPage
	{
		const string DatePickerButton = "DatePickerButton";
		const string TimePickerButton = "TimePickerButton";
		const string PickerButton = "PickerButton";
		readonly string[] _pickerValues = { "Foo", "Bar", "42", "1337" };

		protected override void Init()
		{
			var stackLayout = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center
			};

			// DatePicker
			var datePickerButton = new Button
			{
				Text = "Show DatePicker",
				AutomationId = DatePickerButton
			};

			var datePicker = new DatePicker
			{
				IsVisible = false
			};

			datePickerButton.Clicked += (s, a) =>
			{
				Device.BeginInvokeOnMainThread(() =>
				{
					if (datePicker.IsFocused)
						datePicker.Unfocus();

					datePicker.Focus();
				});
			};

			// TimePicker
			var timePickerButton = new Button
			{
				Text = "Show TimePicker",
				AutomationId = TimePickerButton
			};

			var timePicker = new TimePicker
			{
				IsVisible = false
			};

			timePickerButton.Clicked += (s, a) =>
			{
				Device.BeginInvokeOnMainThread(() =>
				{
					if (timePicker.IsFocused)
						timePicker.Unfocus();

					timePicker.Focus();
				});
			};

			// Picker
			var pickerButton = new Button
			{
				Text = "Show Picker",
				AutomationId = PickerButton
			};

			var picker = new Picker
			{
				IsVisible = false,
				ItemsSource = _pickerValues
			};

			pickerButton.Clicked += (s, a) =>
			{
				Device.BeginInvokeOnMainThread(() =>
				{
					if (picker.IsFocused)
						picker.Unfocus();

					picker.Focus();
				});
			};

			stackLayout.Children.Add(datePickerButton);
			stackLayout.Children.Add(datePicker);

			stackLayout.Children.Add(timePickerButton);
			stackLayout.Children.Add(timePicker);

			stackLayout.Children.Add(pickerButton);
			stackLayout.Children.Add(picker);

			Content = stackLayout;
		}

#if UITEST && __ANDROID__
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
		[Test]
		[UiTest(typeof(DatePicker))]
		public void InvisibleDatepickerShowsDialogOnFocus()
		{
			RunningApp.WaitForElement(DatePickerButton);
			RunningApp.Screenshot("Issue 5159 page is showing in all it's glory");
			RunningApp.Tap(DatePickerButton);

			RunningApp.WaitForElement(x => x.Class("DatePicker"));

			RunningApp.Screenshot("DatePicker is shown");
			RunningApp.TapCoordinates(5, 100);
		}

[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
		[Test]
		[UiTest(typeof(TimePicker))]
		public void InvisibleTimepickerShowsDialogOnFocus()
		{
			RunningApp.WaitForElement(TimePickerButton);
			RunningApp.Screenshot("Issue 5159 page is showing in all it's glory");
			RunningApp.Tap(TimePickerButton);

			RunningApp.WaitForElement(x => x.Class("timePicker"));

			RunningApp.Screenshot("TimePicker is shown");
			RunningApp.TapCoordinates(5, 100);
		}

[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
		[Test]
		[UiTest(typeof(Picker))]
		public void InvisiblePickerShowsDialogOnFocus()
		{
			RunningApp.WaitForElement(PickerButton);
			RunningApp.Screenshot("Issue 5159 page is showing in all it's glory");
			RunningApp.Tap(PickerButton);

			RunningApp.WaitForElement("Foo");

			RunningApp.Screenshot("Picker is shown");

			RunningApp.Tap("Foo");

			RunningApp.WaitForNoElement("Foo");

		}
#endif
	}
}
