using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6368, "[CustomRenderer]Crash when navigating back from page with custom renderer control", PlatformAffected.iOS)]
	public class Issue6368 : NavigationPage
	{
		public Issue6368() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				var rootPage = new ContentPage();
				var button = new Button()
				{
					AutomationId = "btnGo",
					Text = "Click me to go to the next page",
					Command = new Command(() => Navigation.PushAsync(new ContentPage()
					{
						Content = GetContent()
					}))
				};
				var content = GetContent();
				content.Children.Add(button);
				rootPage.Content = content;
				Navigation.PushAsync(rootPage);
			}

			public class CustomView : View
			{
			}

			public class RoundedLabel : Label
			{
			}

			static StackLayout GetContent()
			{
				var content2 = new StackLayout();
				content2.Children.Add(new RoundedLabel { AutomationId = "GoToNextPage", Text = "Go to next Page" });
				content2.Children.Add(new RoundedLabel { Text = "then navigate back" });
				content2.Children.Add(new RoundedLabel { Text = "If test doesn't crash it passed" });
				content2.Children.Add(new CustomView());
				return content2;
			}
		}
	}
}