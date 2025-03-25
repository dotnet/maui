namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 23674, "Page.IsBusy activity indicators gets stuck/causes multiple to be displayed", PlatformAffected.Android)]
	public class Issue23674 : NavigationPage
	{
		public Issue23674() : base(new Issue23674Page1()) { }

		class Issue23674Page1 : ContentPage
		{
			public Issue23674Page1()
			{
				Content = new Button()
				{
					AutomationId = "button1",
					VerticalOptions = LayoutOptions.Start,
					Text = "Navigate to page 1",
					HeightRequest = 100,
					Command = new Command(async () =>
					{
						await Navigation.PushAsync(new Issue23674Page2());
					})
				};
			}
		}

		class Issue23674Page2 : ContentPage
		{
			public Issue23674Page2()
			{
				IsBusy = true;
				Content = new Button()
				{
					AutomationId = "button2",
					VerticalOptions = LayoutOptions.Start,
					Text = "Navigate to page 2",
					HeightRequest = 100,
					Command = new Command(() =>
					{
						Navigation.PopAsync();
					})
				};
				IsBusy = true;
			}
		}
	}
}