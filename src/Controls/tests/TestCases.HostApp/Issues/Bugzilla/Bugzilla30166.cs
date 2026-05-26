namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Bugzilla, 30166, "NavigationBar.BarBackgroundColor resets on Lollipop after popping modal page", PlatformAffected.Android)]
	public class Bugzilla30166 : NavigationPage
	{
		public Bugzilla30166() : base(new MainPage())
		{
			BarBackgroundColor = Colors.Red;
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				Navigation.PushAsync(new ContentPage
				{
					SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container),
					Content = new Button
					{
						AutomationId = "PushModal",
						Text = "Push Modal",
						Command = new Command(async () => await Navigation.PushModalAsync(new ContentPage
						{
							Content = new Button
							{
								AutomationId = "GoBack",
								Text = "GoBack",
								Command = new Command(async () => await Navigation.PopModalAsync()),
							},
						})),
					},
				});
			}
		}
	}
}