[Issue(IssueTracker.Github, 28811, "Overriding back button functionality with OnBackButtonPressed returning false in a modally pushed page causes stack overflow", PlatformAffected.Android)]
public class Issue28811 : ContentPage
{
	public Issue28811()
	{
		Content = new Button()
		{
			AutomationId = "NavigateToDetailPage",
			HeightRequest = 50,
			Text = "Navigate to Detail Page",
			Command = new Command(() => Navigation.PushModalAsync(new Issue28811DetailPage()))
		};
	}

	class Issue28811DetailPage : ContentPage
	{
		public Issue28811DetailPage()
		{
			Content = new Label()
			{
				AutomationId = "Issue28811DetailPage",
				Text = "Hello, World!",
			};
		}

		protected override bool OnBackButtonPressed()
		{
			Navigation.PopModalAsync();
			return false;
		}
	}
}