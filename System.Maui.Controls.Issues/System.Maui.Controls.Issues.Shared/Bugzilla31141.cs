using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 31141, "Change Entry keyboard type while typing", PlatformAffected.iOS)]
	public class Bugzilla31141 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init()
		{
			var stackLayout = new StackLayout
			{
				Orientation = StackOrientation.Vertical,
				Spacing = 10,
				VerticalOptions = LayoutOptions.Start,
				HorizontalOptions = LayoutOptions.Center
			};

			var label = new Label
			{
				Text = "Focus Entry or Editor and type characters. For every 3 characters, the keyboard type will change while keyboard is focused up to 12 characters."
			};
			stackLayout.Children.Add(label);

			var entry = new Entry
			{
				WidthRequest = 250,
				HeightRequest = 50,
				BackgroundColor = Color.DarkGoldenrod
			};
			entry.TextChanged += InputViewOnTextChanged;
			stackLayout.Children.Add(entry);

			var editor = new Editor
			{
				WidthRequest = 250,
				HeightRequest = 50,
				BackgroundColor = Color.AntiqueWhite
			};
			editor.TextChanged += InputViewOnTextChanged;
			stackLayout.Children.Add(editor);

			Content = stackLayout;
		}

		void InputViewOnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
		{
			if (textChangedEventArgs.NewTextValue.Length >= 15)
				return;

			switch (textChangedEventArgs.NewTextValue.Length % 15)
			{
				case 0:
					(sender as InputView).Keyboard = Keyboard.Default;
					break;
				case 3:
					(sender as InputView).Keyboard = Keyboard.Numeric;
					break;
				case 6:
					(sender as InputView).Keyboard = Keyboard.Email;
					break;
				case 9:
					(sender as InputView).Keyboard = Keyboard.Telephone;
					break;
				case 12:
					(sender as InputView).Keyboard = Keyboard.Url;
					break;
			}
		}
	}
}