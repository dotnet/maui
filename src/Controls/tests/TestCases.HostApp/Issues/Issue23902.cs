using Maui.Controls.Sample.Issues;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 23902, "NavigationPage and FlyoutPage both call OnNavigatedTo, so it is called twice", PlatformAffected.Android)]
public class Issue23902FlyoutPage : FlyoutPage
{
	public Page DetailPage { get; set; } 
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
		DetailPage = navigationPage;

		Flyout = flyoutPage;
		Detail = DetailPage;

	}
}

public class Issue23902SecondPage : ContentPage
{
	Button _button;
	Label _label;
	Page thirdPage;
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
				if(thirdPage == null)
					thirdPage = new NavigationPage(new Issue23902ThirdPage());
				flyoutPage.Detail = thirdPage;
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
	Button _button;
	Button _button2;
	public int NavigatedToCount { get; set; } = 0;
	public Issue23902ThirdPage()
	{
		stacklayout = new StackLayout
		{
			Padding = 16,
			Spacing = 16,
		};
		_button = new Button
		{
			HeightRequest = 100,
			Text = "Go to Second Page",
			AutomationId = "SecondPageButton"
		};
		_button2 = new Button
		{
			HeightRequest = 100,
			Text = "Go to 4th Page",
			AutomationId = "FourthPageButton"
		};
		_button.Clicked += OnButtonClicked;
		_button2.Clicked += OnButtonClicked2;
		_label = new Label();
		_label.AutomationId = "HeaderLabel";
		stacklayout.Children.Add(_button);
		stacklayout.Children.Add(_label);
		stacklayout.Children.Add(_button2);
		Content = stacklayout;
	}

	private void OnButtonClicked2(object sender, EventArgs e)
	{
		Navigation.PushAsync(new Issue23902FourthdPage());
	}

	private void OnButtonClicked(object sender, EventArgs e)
	{
		Element current = this;
		while (current?.Parent != null)
		{
			if (current.Parent is Issue23902FlyoutPage flyoutPage)
			{
				flyoutPage.Detail = flyoutPage.DetailPage;
				break;
			}
			current = current.Parent;
		}
	}

	protected override void OnNavigatedTo(NavigatedToEventArgs args)
	{
		base.OnNavigatedTo(args);
		NavigatedToCount++;
		_label.Text = $"NavigatedTo called {NavigatedToCount} times";
	}
}

public class Issue23902FourthdPage : ContentPage
{
	Button _button;
	StackLayout stack;
	public Issue23902FourthdPage()
	{
		_button = new Button();
		_button.Text = "Go Back to 3rd Page";
		_button.AutomationId = "ThirdPageButton";
		_button.Clicked += OnButtonClicked;
		_button.HeightRequest = 70;
		stack = new StackLayout();
		stack.Children.Add(_button);
		Content = stack;
	}

	private void OnButtonClicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}
}