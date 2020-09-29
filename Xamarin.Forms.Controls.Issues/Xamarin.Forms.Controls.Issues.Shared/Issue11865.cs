using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.SearchBar)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11865,
		"iOS SearchBarRenderer throws a NullReferenceException when the SearchButton redirects to a new page",
		PlatformAffected.iOS)]
	public class Issue11865 : TestContentPage
	{
		public Issue11865()
		{
		}

		protected override void Init()
		{
			Title = "Issue 11865";

			var layout = new StackLayout
			{
				BackgroundColor = Color.LightGreen
			};

			var instructions = new Label
			{
				Padding = 12,
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "Type something in the SearchBar and tap the search button."
			};

			var searchBar = new SearchBar();

			layout.Children.Add(instructions);
			layout.Children.Add(searchBar);

			Content = layout;

			searchBar.SearchButtonPressed += (sender, args) =>
			{
				Application.Current.MainPage = new Issue11865SecondPage();
			};
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue11865SecondPage : ContentPage
	{
		public Issue11865SecondPage()
		{
			var layout = new StackLayout
			{
				BackgroundColor = Color.LightCoral
			};

			var instructions = new Label
			{
				Padding = 12,
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "If you can read this message (after changing the MainPage), the test has passed.",
				Margin = new Thickness(0, 36, 0, 0)
			};

			layout.Children.Add(instructions);

			Content = layout;
		}
	}
}