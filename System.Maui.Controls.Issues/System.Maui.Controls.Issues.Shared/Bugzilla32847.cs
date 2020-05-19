using System;
using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 32847,
		"Picker text is cleared after selecting an item, whether Picker, DatePicker, or TimePicker (when in a TableView (or ListView))", PlatformAffected.WinRT)]
	public class Bugzilla32847 : TestContentPage
	{
		protected override void Init ()
		{
			var instructions =
				@"In the picker below, select the option labeled 'Two'. If the selection immediately disappears, the test has failed.
In the TimePicker below, change the time to 5:21 PM. If the selection immediately disappears, the test has failed.
In the DatePicker below, change the date to May 25, 1977. If the selection immediately disappears, the test has failed.";

			var tableInstructions = new Label {
				Text = instructions
			};

			var picker = new Picker ();

			var pickerItems = new List<string> { "One", "Two", "Three" };

			foreach(string item in pickerItems)
			{
				picker.Items.Add(item);
			}

			var datePicker = new DatePicker ();
			var timePicker = new TimePicker ();

			var tableView = new TableView() { BackgroundColor = Color.Green };

			var tableSection = new TableSection();

			var pickerCell = new ViewCell { View = picker };
			var datepickerCell = new ViewCell { View = datePicker };
			var timepickerCell = new ViewCell { View = timePicker };

			tableSection.Add(pickerCell);
			tableSection.Add(timepickerCell);
			tableSection.Add(datepickerCell);

			var tableRoot = new TableRoot() {
				tableSection
			};

			tableView.Root = tableRoot;

			var listItems = new List<string> { "One" };

			var listView = new ListView
			{
				Header = instructions,
				BackgroundColor = Color.Pink,
				ItemTemplate = new DataTemplate(typeof(CustomCell)),
				ItemsSource = listItems
			};

			var nonListDatePicker = new DatePicker();
			var nonListTimePicker = new TimePicker();
			var nonListPicker = new Picker();

			foreach(string item in pickerItems)
			{
				nonListPicker.Items.Add(item);
			}

			Content = new StackLayout {
				VerticalOptions = LayoutOptions.Fill,
				HorizontalOptions = LayoutOptions.Fill,
				Children = {
					new Label { Text = instructions },
					nonListPicker, 
					nonListDatePicker,
					nonListTimePicker,
					tableInstructions,
					tableView,
					listView
				}
			};
		}
	}

	[Preserve (AllMembers = true)]
	public class CustomCell : ViewCell
	{
		public CustomCell()
		{
			StackLayout cellWrapper = new StackLayout();
			StackLayout stack = new StackLayout();

			var picker = new Picker();
			var datePicker = new DatePicker ();
			var timePicker = new TimePicker ();

			var items = new List<string> { "One", "Two", "Three" };

			foreach(string item in items)
			{
				picker.Items.Add(item);
			}

			cellWrapper.BackgroundColor = Color.FromHex("#eee");
			stack.Orientation = StackOrientation.Vertical;

			stack.Children.Add(picker);
			stack.Children.Add(timePicker);
			stack.Children.Add(datePicker);

			cellWrapper.Children.Add(stack);
			View = cellWrapper;
		}

	}
}
