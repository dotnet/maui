using System.ComponentModel;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Entry = Microsoft.Maui.Controls.Entry;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 1667, "Entry: Position and color of caret", PlatformAffected.All)]
	public class Issue1667 : TestContentPage
	{
		readonly string CursorTextEntryText = "Enter cursor position and selection length";
		Entry _entry;
		Entry _cursorStartPosition;
		Entry _selectionLength;
		Button _updateButton;

		protected override void Init()
		{
			_entry = new Entry { Text = CursorTextEntryText, AutomationId = "CursorTextEntry" };
			_entry.PropertyChanged += ReadCursor;

			_cursorStartPosition = new Entry { AutomationId = "CursorStart" };
			_selectionLength = new Entry { AutomationId = "SelectionLength" };

			_updateButton = new Button { AutomationId = "Update", Text = "Update" };
			_updateButton.Clicked += UpdateCursor;

			var layout = new StackLayout
			{
				AutomationId = "MainContent",
				Margin = new Thickness(10, 40),
				Children =
				{
					_entry,
					new Label {Text = "Start:"},
					_cursorStartPosition,
					new Label {Text = "Selection Length:"},
					_selectionLength,
					_updateButton
				}
			};

			if (DeviceInfo.Platform == DevicePlatform.iOS)
			{
				var red = new Button { AutomationId = "Red", Text = "Red", TextColor = Colors.Red };
				red.Clicked += (sender, e) => _entry.On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>().SetCursorColor(Colors.Red);

				var blue = new Button { AutomationId = "Blue", Text = "Blue", TextColor = Colors.Blue };
				blue.Clicked += (sender, e) => _entry.On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>().SetCursorColor(Colors.Blue);

				var defaultColor = new Button { AutomationId = "Default", Text = "Default" };
				defaultColor.Clicked += (sender, e) => _entry.On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>().SetCursorColor(null);

				layout.Children.Add(red);
				layout.Children.Add(blue);
				layout.Children.Add(defaultColor);
			}

			Content = layout;

		}

		void UpdateCursor(object sender, EventArgs args)
		{
			var start = 0;
			var length = 0;
			if (int.TryParse(_cursorStartPosition.Text, out start))
			{
				_entry.CursorPosition = start;
			}
			if (int.TryParse(_selectionLength.Text, out length))
			{
				_entry.SelectionLength = length;
			}
		}

		void ReadCursor(object sender, PropertyChangedEventArgs args)
		{
			if (args.PropertyName == Entry.CursorPositionProperty.PropertyName)
				_cursorStartPosition.Text = _entry.CursorPosition.ToString();
			else if (args.PropertyName == Entry.SelectionLengthProperty.PropertyName)
				_selectionLength.Text = _entry.SelectionLength.ToString();
		}
	}
}
