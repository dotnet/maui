using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 33268, "Picker is broken on Windows Phone 8.1", PlatformAffected.WinRT)]
	public class Bugzilla33268 : TestTabbedPage
	{
		protected override void Init ()
		{
			Children.Add (new Bugzilla33268ListView ());
			Children.Add (new Simple ());
			Children.Add (new Bugzilla33268NoListView ());
		}
	}

	[Preserve (AllMembers = true)]
	public class Simple : TestContentPage
	{
		protected override void Init ()
		{
			Title = "Simple";

			var fiveItemPicker = new Picker { Title = "Picker With 4 Items" };
			for (var i = 1; i <= 4; i++) {
				fiveItemPicker.Items.Add ("Sample Option " + i);
			}

			Content = new StackLayout {
				BackgroundColor = Color.Green,
				VerticalOptions = LayoutOptions.Start,
				Children = {
					fiveItemPicker
				}
			};
		}
	}

	[Preserve (AllMembers = true)]
	public class Bugzilla33268NoListView : TestContentPage
	{
		protected override void Init ()
		{
			Title = "No ListView";

			var fiveItemLabel = new Label {
				Text =
					"The picker below should display four items when opened. If you open it and all four items are not visible, this test has failed."
			};

			var fiveItemPicker = new Picker { Title = "Picker With 4 Items" };
			for (var i = 1; i <= 4; i++) {
				fiveItemPicker.Items.Add ("Sample Option " + i);
			}

			var sixItemLabel = new Label {
				Text =
					"The picker below should display full screen when opened. If you open it and it's not full screen, this test has failed."
			};

			var sixItemPicker = new Picker { Title = "Picker With 6 Items" };
			for (var i = 1; i <= 6; i++) {
				sixItemPicker.Items.Add ("Sample Option " + i);
			}

			Content = new StackLayout {
				Children = {
					fiveItemLabel,
					fiveItemPicker,
					sixItemLabel,
					sixItemPicker
				}
			};
		}
	}

	[Preserve (AllMembers = true)]
	public class Bugzilla33268ListView : TestContentPage
	{
		protected override void Init ()
		{
			Title = "ListView";

			var listItems = new List<string> { "One" };

			var listView = new ListView {
				Header = "Pickers in a ListView",
				ItemTemplate = new DataTemplate (typeof(PickerCell)),
				ItemsSource = listItems
			};

			Content = new StackLayout {
				Children = {
					listView
				}
			};
		}

		[Preserve (AllMembers = true)]
		internal class PickerCell : ViewCell
		{
			public PickerCell ()
			{
				var cellWrapper = new StackLayout ();
				var stack = new StackLayout ();

				var fiveItemLabel = new Label { 
					Text =
						"The picker below should display five items when opened. If you open it and all five items are not visible, this test has failed."
				};

				var fiveItemPicker = new Picker { Title = "Picker With 5 Items" };
				for (var i = 1; i <= 5; i++) {
					fiveItemPicker.Items.Add ("Sample Option " + i);
				}

				var sixItemLabel = new Label {
					Text =
						"The picker below should display full screen when opened. If you open it and it's not full screen, this test has failed."
				};

				var sixItemPicker = new Picker { Title = "Picker With 6 Items" };
				for (var i = 1; i <= 6; i++) {
					sixItemPicker.Items.Add ("Sample Option " + i);
				}

				stack.Orientation = StackOrientation.Vertical;

				stack.Children.Add (fiveItemLabel);
				stack.Children.Add (fiveItemPicker);
				stack.Children.Add (sixItemLabel);
				stack.Children.Add (sixItemPicker);

				cellWrapper.VerticalOptions = LayoutOptions.StartAndExpand;

				cellWrapper.Children.Add (stack);

				View = cellWrapper;
			}
		}
	}
}