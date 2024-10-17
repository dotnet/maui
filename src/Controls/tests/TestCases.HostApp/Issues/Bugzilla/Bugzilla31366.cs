namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 31366, "Pushing and then popping a page modally causes ArgumentOutOfRangeException",
		PlatformAffected.All)]
	public class Bugzilla31366 : NavigationPage
	{
		public Bugzilla31366() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				var page1 = new ContentPage() { Title = "Page1" };

				var successLabel = new Label();
				var startPopOnAppearing = new Button() { AutomationId = "StartPopOnAppearingTest", Text = "Start PopOnAppearing Test" };
				var startModalStack = new Button() { AutomationId = "StartModalStackTest", Text = "Start ModalStack Test" };

				page1.Content = new StackLayout()
				{
					Children = { startPopOnAppearing, startModalStack, successLabel }
				};

				var popOnAppearing = new ContentPage()
				{
					Title = "PopOnAppearing",
					Content = new StackLayout()
				};

				popOnAppearing.Appearing += async (sender, args) =>
				{
					await Task.Yield();
					await popOnAppearing.Navigation.PopModalAsync();
				};

				startPopOnAppearing.Clicked += async (sender, args) =>
				{
					successLabel.Text = string.Empty;

					await page1.Navigation.PushModalAsync(popOnAppearing);

					successLabel.Text = "If this is visible, the PopOnAppearing test has passed.";
				};

				startModalStack.Clicked += async (sender, args) =>
				{
					successLabel.Text = string.Empty;

					var intermediatePage = new ContentPage()
					{
						Content = new StackLayout()
						{
							Children = {
							new Label () { Text = "If this is visible, the modal stack test has passed." }
							}
						}
					};

					await intermediatePage.Navigation.PushModalAsync(popOnAppearing);

					await page1.Navigation.PushModalAsync(intermediatePage);
				};

				Navigation.PushAsync(page1);
			}
		}
	}
}