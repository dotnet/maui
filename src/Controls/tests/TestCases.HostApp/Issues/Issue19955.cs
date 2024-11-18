namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 19955, "Navigating Back to FlyoutPage Renders Blank Page")]
	public class Issue19955 : NavigationPage
	{
		public Issue19955() : base(new MainFlyout())
		{

		}

		public class MainFlyout : FlyoutPage
		{
			public MainFlyout()
			{
				Flyout = new MainFlyoutMenu();
				Detail = new MarkerNavigationPage();
			}

			class MarkerNavigationPage : NavigationPage
			{
				public MarkerNavigationPage() : base(new FirstPage())
				{
				}
			}

			class MarkerNavigationPage2 : NavigationPage
			{
				public MarkerNavigationPage2() : base(new FirstPage())
				{
				}
			}
		}

		public class MainFlyoutMenu : ContentPage
		{
			public MainFlyoutMenu()
			{
				Title = "MainFlyoutMenu";
				Content = new VerticalStackLayout(){
					new Label()
					{
						Text = "I'm the Flyout Page"
					}
				};
			}
		}

		public class FirstPage : ContentPage
		{
			public FirstPage()
			{
				Content = new VerticalStackLayout(){
					new Button(){
						Text = "Navigate to Second page",
						Command = new Command(Button_Clicked),
						AutomationId = "NavigateToSecondPageButton"
					},
				};
			}

			async void Button_Clicked()
			{
				await this.Window.Page.Navigation.PushAsync(new SecondPage());
			}
		}

		public class SecondPage : ContentPage
		{
			public SecondPage()
			{
				Content = new VerticalStackLayout()
				{
					new Button(){
						Text = "Navigate back to first page",
						Command = new Command(Button_Clicked),
						AutomationId = "NavigateBackToFirstPageButton"
					}
				};
			}

			async void Button_Clicked()
			{
				await this.Window.Page.Navigation.PopAsync();
			}
		}
	}
}