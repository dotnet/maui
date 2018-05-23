using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

#if UITEST
using Xamarin.UITest;
using Xamarin.Forms.Core.UITests;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
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

			_updateButton = new Button { Text = "Update" };
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

			if (Device.RuntimePlatform == Device.iOS)
			{
				var red = new Button { Text = "Red", TextColor = Color.Red };
				red.Clicked += (sender, e) => _entry.On<PlatformConfiguration.iOS>().SetCursorColor(Color.Red);

				var blue = new Button { Text = "Blue", TextColor = Color.Blue };
				blue.Clicked += (sender, e) => _entry.On<PlatformConfiguration.iOS>().SetCursorColor(Color.Blue);

				var defaultColor = new Button { Text = "Default" };
				defaultColor.Clicked += (sender, e) => _entry.On<PlatformConfiguration.iOS>().SetCursorColor(Color.Default);

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

#if UITEST
		[Test]
		[NUnit.Framework.Category(UITestCategories.ManualReview)]
		public void TestCursorPositionAndSelection()
		{
			RunningApp.WaitForElement("CursorTextEntry");

			RunningApp.ClearText( "CursorStart");
			RunningApp.EnterText("CursorStart", "2");
			RunningApp.ClearText("SelectionLength");
			RunningApp.EnterText("SelectionLength", "3");
			RunningApp.DismissKeyboard();
			RunningApp.Tap("Update");
			RunningApp.Screenshot("Text selection from char 2 length 3.");

			RunningApp.Tap("CursorTextEntry");
			Assert.AreEqual("0", RunningApp.WaitForElement("SelectionLength")[0].Text);
		}

#if __IOS__
		[Test]
		[NUnit.Framework.Category(UITestCategories.ManualReview)]
		public void TestCursorColorOniOS()
		{
			RunningApp.WaitForElement("CursorTextEntry");
			RunningApp.Tap("Red");
			RunningApp.Tap("CursorTextEntry");
			RunningApp.Screenshot("Cursor is red.");

			RunningApp.Tap("Blue");
			RunningApp.Tap("CursorTextEntry");
			RunningApp.Screenshot("Cursor is blue.");

			RunningApp.Tap("Default");
			RunningApp.Tap("CursorTextEntry");
			RunningApp.Screenshot("Cursor is default color.");

		}
#endif
#endif
	}
}
