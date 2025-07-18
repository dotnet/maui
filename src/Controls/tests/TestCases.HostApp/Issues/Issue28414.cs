namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28414, "NavigationStack updated when OnAppearing triggered", PlatformAffected.iOS)]

public class Issue28414 : NavigationPage
{
	public Issue28414() : base(new Issue28414FirstPage())
	{

	}
}

public class Issue28414FirstPage : ContentPage
{
	public Issue28414FirstPage()
	{
		Title = "FirstPage";

		var navigateButton = new Button
		{
			Text = "Go to second page",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "FirstPageButton"
		};

		navigateButton.Clicked += async (sender, args) =>
		{
			await Navigation.PushAsync(new Issue28414SecondPage());
		};

		Content = new VerticalStackLayout
		{
			Children = { navigateButton }
		};
	}
}

public class Issue28414SecondPage : ContentPage
{
	private Label label;

	public Issue28414SecondPage()
	{
		Title = "SecondPage";

		label = new Label
		{
			Text = "Initial state",
			AutomationId = "OnAppearingLabel"
		};

		var navigateButton = new Button
		{
			Text = "Go to third page",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "SecondPageButton"
		};

		navigateButton.Clicked += async (sender, args) =>
		{
			label.Text = string.Empty;
			await Navigation.PushAsync(new Issue28414ThirdPage());
		};

		Content = new VerticalStackLayout
		{
			Children = { label, navigateButton }
		};
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		if (Navigation.NavigationStack.Count == 2)
		{
			label.Text = $"Stack has {Navigation.NavigationStack.Count} pages";
		}
		else
		{
			label.Text = "Page not popped yet";
		}
	}
}

public class Issue28414ThirdPage : ContentPage
{
	public Issue28414ThirdPage()
	{
		Title = "ThirdPage";

		var button = new Button
		{
			Text = "Go Back",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			AutomationId = "ThirdPageButton"
		};

		button.Clicked += async (sender, args) =>
		{
			await Navigation.PopAsync();
		};

		Content = new VerticalStackLayout
		{
			Children = { button }
		};
	}
}