namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 27337, "Flyout with FlyoutBehavior='Locked' unlocks (reverts to Flyout) after navigation", PlatformAffected.UWP)]
	public class Issue27337 : TestShell
	{
		protected override void Init()
		{
			ContentPage MainPage = new ContentPage
			{
				Title = "Home",
				Content = new VerticalStackLayout
				{
					Children =
					{
						new Button
						{
							AutomationId = "SecondPageButton",
							Text = "Navigate to Second Page",
							HorizontalOptions = LayoutOptions.Fill,
							Command = new Command(() => Shell.Current.GoToAsync("secondPage"))
						}
					}
				}
			};

			FlyoutBehavior = FlyoutBehavior.Locked;

			Items.Add(new FlyoutItem
			{
				Title = "Home",
				Items =
				{
					new ShellContent
					{
						Content = MainPage
					}
				}
			});

			Routing.RegisterRoute("secondPage", typeof(SecondPage));
		}
	}

	public class SecondPage : ContentPage
	{
		public SecondPage()
		{
			Title = "Second Page";
			Content = new VerticalStackLayout
			{
				Children =
				{
					new Button
					{
						AutomationId = "GoBackButton",
						Text = "Go Back",
						HorizontalOptions = LayoutOptions.Fill,
						Command = new Command(()=> Shell.Current.GoToAsync(".."))
					}
				}
			};
		}
	}
}