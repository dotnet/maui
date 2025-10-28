using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31539, "[iOS, macOS] Navigation Page BackButtonTitle Not Updating", PlatformAffected.iOS | PlatformAffected.macOS)]

public class Issue31539 : TestNavigationPage
{
	protected override void Init()
	{
		Navigation.PushAsync(new Issue31539ContentPage());
	}
}

public class Issue31539ContentPage : ContentPage
{
	public Issue31539ContentPage()
	{
		var layout = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 12
		};

		var pushButton = new Button
		{
			Text = "Push Second Page",
			AutomationId = "PushSecondPage"
		};
		pushButton.Clicked += OnPushSecondPage;

		var backTitleButton = new Button
		{
			Text = "Set BackButtonTitle = 'Back Home'",
			AutomationId = "SetBackButtonTitle"
		};
		backTitleButton.Clicked += OnSetBackButtonTitle;

		layout.Children.Add(pushButton);
		layout.Children.Add(backTitleButton);

		Content = layout;
	}

	void OnPushSecondPage(object sender, EventArgs e)
	{
		Navigation.PushAsync(new Issue31539SecondContentPage());
	}

	void OnSetBackButtonTitle(object sender, EventArgs e)
	{
		NavigationPage.SetBackButtonTitle(this, "Back Home");
	}
}

public class Issue31539SecondContentPage : ContentPage
{
	public Issue31539SecondContentPage()
	{
		Content = new StackLayout
		{
			Padding = 20,
			Spacing = 12,
			Children =
			{
				new Label { Text = "This is the second page.", AutomationId = "SecondPageLabel" }
			}
		};
	}
}