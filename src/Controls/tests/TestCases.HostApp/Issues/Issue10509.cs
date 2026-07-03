namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 10509, "Query parameter is missing after navigation", PlatformAffected.Android)]
public class Issue10509 : Shell
{
	public Issue10509()
	{
		Routing.RegisterRoute(nameof(Issue10509Page1), typeof(Issue10509Page1));
		Routing.RegisterRoute(nameof(Issue10509Page2), typeof(Issue10509Page2));

		Items.Add(new ShellContent
		{
			Title = "First Page",
			Route = nameof(Issue10509Page1),
			ContentTemplate = new DataTemplate(typeof(Issue10509Page1))
		});

		Items.Add(new ShellContent
		{
			Title = "Second Page",
			Route = nameof(Issue10509Page2),
			ContentTemplate = new DataTemplate(typeof(Issue10509Page2))
		});
	}
}

public class Issue10509Page1 : ContentPage
{
	public Issue10509Page1()
	{
		var navigateBtn = new Button
		{
			Text = "Navigate to Page 2",
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "Page1Button"
		};
		navigateBtn.Clicked += NavigateBtn_Clicked;

		Content = new VerticalStackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			Children = { navigateBtn }
		};
	}

	private async void NavigateBtn_Clicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync($"//{nameof(Issue10509Page2)}?{Issue10509Page2.NavigationDataParam}=Passed");
	}
}

[QueryProperty(nameof(NavigationData), NavigationDataParam)]
public class Issue10509Page2 : ContentPage
{
	public const string NavigationDataParam = "NavigationDataParam";

	private Label dataLabel;

	public Issue10509Page2()
	{
		dataLabel = new Label
		{
			AutomationId = "Page2Label",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center
		};

		Content = new VerticalStackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			Children = { dataLabel }
		};
	}

	public string NavigationData { get; set; }

	protected override void OnNavigatedTo(NavigatedToEventArgs args)
	{
		base.OnNavigatedTo(args);
		dataLabel.Text = $"Navigation data: {NavigationData ?? "Missed"}";
	}
}