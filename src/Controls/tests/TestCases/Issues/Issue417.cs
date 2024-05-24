using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 417, "Navigation.PopToRootAsync does nothing", PlatformAffected.Android)]
	public class Issue417 : NavigationPage
	{
		public Issue417() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				Navigation.PushAsync(new FirstPage());
			}

			public class FirstPage : ContentPage
			{
				public FirstPage()
				{
					Title = "First Page";
					BackgroundColor = Colors.Black;

					var nextPageBtn = new Button
					{
						Text = "Next Page"
					};

					nextPageBtn.Clicked += (s, e) => Navigation.PushAsync(new NextPage());

					Content = nextPageBtn;
				}

			}

			public class NextPage : ContentPage
			{
				public NextPage()
				{
					Title = "Second Page";

					var nextPage2Btn = new Button
					{
						Text = "Next Page 2"
					};

					nextPage2Btn.Clicked += (s, e) => Navigation.PushAsync(new NextPage2());
					BackgroundColor = Colors.Black;
					Content = nextPage2Btn;

				}
			}

			public class NextPage2 : ContentPage
			{
				public NextPage2()
				{
					Title = "Third Page";

					var popToRootButton = new Button
					{
						Text = "Pop to root"
					};

					popToRootButton.Clicked += (s, e) => Navigation.PopToRootAsync();
					BackgroundColor = Colors.Black;
					Content = popToRootButton;
				}
			}
		}
	}
}