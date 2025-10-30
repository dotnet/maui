namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 44476, "[Android] Unwanted margin at top of details page when nested in a NavigationPage")]
	public class Bugzilla44476 : NavigationPage
	{
		public Bugzilla44476() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				BackgroundColor = Colors.Maroon;
				SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
#pragma warning disable CS0618 // Type or member is obsolete
				Navigation.PushAsync(new FlyoutPage
				{
					Title = "Bugzilla Issue 44476",
					Flyout = new ContentPage
					{
						Title = "Flyout",
						Content = new StackLayout
						{
							Children =
						{
							new Label { Text = "Flyout" }
						}
						}
					},
					Detail = new ContentPage
					{
						Title = "Detail",
						Content = new StackLayout
						{
							VerticalOptions = LayoutOptions.FillAndExpand,
							Children =
						{
							new Label { Text = "Detail Page" },
							new StackLayout
							{
								VerticalOptions = LayoutOptions.EndAndExpand,
								Children =
								{
									new Label { Text = "This should be visible." }
								}
							}
						}
						}
					},
				});
#pragma warning restore CS0618 // Type or member is obsolete
			}
		}
	}
}