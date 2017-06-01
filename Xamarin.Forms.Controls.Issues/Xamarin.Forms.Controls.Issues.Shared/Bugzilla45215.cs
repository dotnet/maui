using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 45215, "AdjustResize Causes Content to Be Sized Incorrectly After Toggling Keyboard",
		PlatformAffected.Android)]
	public class Bugzilla45215 : TestNavigationPage
	{
		WindowSoftInputModeAdjust _beforeTest;

		protected override void Init()
		{
			_beforeTest = Application.Current.On<Android>().GetWindowSoftInputModeAdjust();

			Application.Current.On<Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
			PushAsync(Root());
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			Application.Current.On<Android>().UseWindowSoftInputModeAdjust(_beforeTest);
		}

		ContentPage Root()
		{
			var buttonTest1 = new Button { Text = "AdjustResize in ScrollView" };
			buttonTest1.Clicked += (sender, args) => PushAsync(ScrollViewTests());

			var buttonTest2 = new Button { Text = "AdjustResize with Navigation" };
			buttonTest2.Clicked += (sender, args) => PushAsync(BackNavigationTest());

			var layout = new StackLayout { Children = { buttonTest1, buttonTest2 } };

			return new ContentPage { Title = "Root", Content = layout };
		}

		#region ScrollViewTests

		ContentPage EditorScrollViewTest()
		{
			return ScrollViewTestPage(() => new Editor
			{
				Text = "Editor",
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Start
			});
		}

		ContentPage EntryScrollViewTest()
		{
			return ScrollViewTestPage(() => new Entry
			{
				Placeholder = "Entry",
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Start
			});
		}

		ContentPage ScrollViewTestPage(Func<View> view)
		{
			var instructions = new Label
			{
				Text =
					"Tap on the control to bring up the software keyboard, then use the hardware 'Back' button to dismiss the keyboard. If the part of the page where the keyboard was is not green, the test has failed."
			};

			var layout = new StackLayout
			{
				Children = { instructions, view() },
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill,
				BackgroundColor = Color.YellowGreen
			};

			var sv = new ScrollView { Content = layout };

			return new ContentPage { Title = "Page with ScrollView", Content = sv };
		}

		ContentPage ScrollViewTests()
		{
			var instructions = new Label { Text = "Select a test" };

			var buttonEntry = new Button { Text = "Entry Version" };
			var buttonEditor = new Button { Text = "Editor Version" };
			var buttonSearchBar = new Button { Text = "SearchBar" };

			buttonEntry.Clicked += (sender, args) => PushAsync(EntryScrollViewTest());
			buttonEditor.Clicked += (sender, args) => PushAsync(EditorScrollViewTest());
			buttonSearchBar.Clicked += (sender, args) => PushAsync(SearchBarScrollViewTest());

			var layout = new StackLayout
			{
				Children = { instructions, buttonEntry, buttonEditor, buttonSearchBar },
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				BackgroundColor = Color.YellowGreen
			};

			return new ContentPage { Title = "ScrollView Tests", Content = layout };
		}

		ContentPage SearchBarScrollViewTest()
		{
			return ScrollViewTestPage(() => new SearchBar
			{
				Placeholder = "SearchBar",
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Start
			});
		}

		#endregion

		#region BackNavTests

		ContentPage BackNavigationTest()
		{
			var instructions = new Label { Text = "Select a test" };

			var buttonEntry = new Button { Text = "Entry Version" };
			var buttonEditor = new Button { Text = "Editor Version" };
			var buttonSearchBar = new Button { Text = "SearchBar" };

			buttonEntry.Clicked += (sender, args) => PushAsync(EntryBackNav());
			buttonEditor.Clicked += (sender, args) => PushAsync(EditorBackNav());
			buttonSearchBar.Clicked += (sender, args) => PushAsync(SearchBarBackNav());

			var layout = new StackLayout
			{
				Children = { instructions, buttonEntry, buttonEditor, buttonSearchBar },
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				BackgroundColor = Color.YellowGreen
			};

			return new ContentPage { Title = "Back Nav Tests", Content = layout };
		}

		const string TapHere = "TapHere";

		ContentPage BackNavTestPage(string title, Func<View> element)
		{
			var instructions = new Label
			{
				Text =
					"Tap the control to bring up the keyboard. Then hit the 'Back' arrow on the Navigation Bar. If the screen background not green where the keyboard was, this test has failed."
			};

			var layout = new StackLayout { Children = { instructions, element() } };

			return new ContentPage { Title = title, Content = layout };
		}

		ContentPage SearchBarBackNav()
		{
			return BackNavTestPage("Entry", () => new SearchBar { Placeholder = TapHere });
		}

		ContentPage EntryBackNav()
		{
			return BackNavTestPage("Entry", () => new Entry { Placeholder = TapHere });
		}

		ContentPage EditorBackNav()
		{
			return BackNavTestPage("Entry", () => new Editor { Text = TapHere });
		}

		#endregion
	}
}