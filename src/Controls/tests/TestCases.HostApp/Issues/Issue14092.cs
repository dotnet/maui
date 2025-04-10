namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 14092, "Disappeared was not triggered when popping a page", PlatformAffected.Android)]

public class Issue14092 : NavigationPage
{
	Issue14092FirstPage _firstPage;

	public Issue14092() : base(new Issue14092FirstPage())
	{
		_firstPage = (Issue14092FirstPage)CurrentPage;
		Application.Current.PageDisappearing += Current_PageDisappearing;
	}

	private void Current_PageDisappearing(object sender, Page e)
	{
		if (e is Issue14092SecondPage)
		{
			_firstPage.UpdateStatusLabel("Disappearing triggered when pop");
		}
	}
}

public class Issue14092FirstPage : ContentPage
{
	Label _label;
	public Issue14092FirstPage()
	{
		Title = "Main Page";

		_label = new Label
		{
			Text = "Status: Ready",
			AutomationId = "StatusLabel",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		var counterBtn = new Button
		{
			Text = "Click me",
			AutomationId = "firstPageButton",
			HorizontalOptions = LayoutOptions.Fill
		};

		counterBtn.Clicked += OnCounterClicked;

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(30, 0),
			Spacing = 25,
			Children =
			{
				_label,
				counterBtn
			}
		};
	}

	public void UpdateStatusLabel(string text)
	{
		_label.Text = text;
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
		Navigation.PushAsync(new Issue14092SecondPage());
	}
}

public class Issue14092SecondPage : ContentPage
{
	public Issue14092SecondPage()
	{
		Title = "Second Page";

		var button = new Button
		{
			Text = "Go Back",
			AutomationId = "secondPageButton",
		};
		button.Clicked += OnButtonClicked;

		Content = new StackLayout
		{
			Children = { button }
		};
	}

	private void OnButtonClicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}
}