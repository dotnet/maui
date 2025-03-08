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
							Command = new Command(() => Shell.Current.GoToAsync("Issue27337_SecondPage"))
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

			Routing.RegisterRoute("Issue27337_SecondPage", typeof(Issue27337_SecondPage));
		}
	}

	public class Issue27337_SecondPage : ContentPage
	{
		public Issue27337_SecondPage()
		{
			Title = "Second Page";
		}
	}
}