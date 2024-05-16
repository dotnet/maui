using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 16499, "Crash when using NavigationPage.TitleView and Restarting App", PlatformAffected.Android)]
	public class Issue16499 : NavigationPage
	{
		public Issue16499() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage() : base()
			{
				var contentPage = new ContentPage();
				var navPage = new NavigationPage(contentPage);

				NavigationPage.SetTitleView(contentPage, new Label());

				NavigatedTo += Issue16499_NavigatedTo;

				async void Issue16499_NavigatedTo(object sender, NavigatedToEventArgs e)
				{
					NavigatedTo -= Issue16499_NavigatedTo;
					await Navigation.PushModalAsync(navPage);
					await Navigation.PopModalAsync();
					await Navigation.PushModalAsync(navPage);
					await Navigation.PopModalAsync();
					Content = new VerticalStackLayout()
					{
						new Label()
						{
							Text = "If the app didn't crash this test was a success",
							AutomationId = "SuccessLabel"
						}
					};
				}
			}
		}
	}
}
