namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 23902, "NavigationPage and FlyoutPage both call OnNavigatedTo, so it is called twice", PlatformAffected.Android)]
public partial class Issue23902FlyoutPage : FlyoutPage
{
	public Issue23902FlyoutPage()
	{
		var flyoutPage = new ContentPage
		{
			Title = "Flyout",
			Content = new StackLayout
			{
				Children =
				{
					new Label
					{
						Text = "This is Flyout",
						AutomationId = "Issue31390_FlyoutLabel"
					}
				}
			}
		};

		var detailPage = new Issue23902SecondPage();

		var navigationPage = new NavigationPage(detailPage)
		{
			Title = "Detail"
		};

		Flyout = flyoutPage;
		Detail = navigationPage;

	}
}

public class Issue23902SecondPage : ContentPage
{
	Button _button;
	Label _label;
	public Issue23902SecondPage()
	{
		_button = new Button
		{
			Text = "Go to Third Page",
			AutomationId = "ThirdPageButton"
		};
		_label = new Label { Text = "This is the second page" };
		Content = new StackLayout
		{
			Padding = 16,
			Spacing = 16,
			Children =
				{
					_button,
					_label
				}
		};
		_button.Clicked += OnButtonClicked;
	}
	
	private void OnButtonClicked(object sender, EventArgs e)
	{
		Element current = this;
		while (current?.Parent != null)
		{
			if (current.Parent is FlyoutPage flyoutPage)
			{
				flyoutPage.Detail = new NavigationPage(new Issue23902ThirdPage());
				break;
			}
			current = current.Parent;
		}
	}
}

public class Issue23902ThirdPage : ContentPage
{
	Label _label;
	StackLayout stacklayout;

	public int NavigatedToCount { get; set; } = 0;
	public Issue23902ThirdPage()
	{
		stacklayout = new StackLayout
		{
			Padding = 16,
			Spacing = 16,
		};
		_label = new Label();
		_label.AutomationId = "HeaderLabel";
		stacklayout.Children.Add(_label);
		Content = stacklayout;
	}

	protected override void OnNavigatedTo(NavigatedToEventArgs args)
	{
		base.OnNavigatedTo(args);
		NavigatedToCount++;
		_label.Text = $"NavigatedTo called {NavigatedToCount} times";
	}
}