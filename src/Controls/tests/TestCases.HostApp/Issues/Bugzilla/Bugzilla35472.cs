namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 35472, "PopAsync during ScrollToAsync throws NullReferenceException")]
	public class Bugzilla35472 : NavigationPage
	{
		public Bugzilla35472() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				// Set up the scroll viewer page
				var scrollToButton = new Button() { AutomationId = "NowPushButton", Text = "Now push this button" };

				var stackLayout = new StackLayout();

				stackLayout.Children.Add(scrollToButton);

				for (int n = 0; n < 100; n++)
				{
					stackLayout.Children.Add(new Label() { Text = n.ToString() });
				}

				var scrollView = new ScrollView()
				{
					Content = stackLayout
				};

				var pageWithScrollView = new ContentPage()
				{
					Content = scrollView
				};

				// Set up the start page
				var goButton = new Button()
				{
					AutomationId = "PushButton",
					Text = "Push this button"
				};

				var successLabel = new Label() { AutomationId = "ScrollResult", IsVisible = false };

				Content = new StackLayout
				{
					VerticalOptions = LayoutOptions.Center,
					Children =
					{
						goButton,
						successLabel
					}
				};

				goButton.Clicked += async (sender, args) => await Navigation.PushAsync(pageWithScrollView);

				scrollToButton.Clicked += async (sender, args) =>
				{
					try
					{
						// Deliberately not awaited so we can simulate a user navigating back before the scroll is finished
						var scrolling = scrollView.ScrollToAsync(0, 1500, true);
						// A disconnected handler need not complete the scroll task, but faults must still fail the app.
						_ = scrolling.ContinueWith(task =>
							Dispatcher.Dispatch(() => System.Runtime.ExceptionServices.ExceptionDispatchInfo
								.Capture(task.Exception.GetBaseException()).Throw()),
							CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
						await Navigation.PopAsync();
						successLabel.Text = "The test has passed";
					}
					catch (Exception ex)
					{
						successLabel.Text = ex.ToString();
					}
					finally
					{
						successLabel.IsVisible = true;
					}
				};
			}
		}
	}
}