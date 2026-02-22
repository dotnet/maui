namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32419, "[iOS, macOS] Shell menu and flyout items do not update correctly in RTL mode", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue32419 : TestShell
{
	protected override void Init()
	{

		Title = "Issue 32419";
		FlowDirection = FlowDirection.RightToLeft;
		FlyoutBehavior = FlyoutBehavior.Flyout;

		var homeContent = new ShellContent
		{
			Title = "Home",
			ContentTemplate = new DataTemplate(typeof(Issue32419HomePage)),

			Route = "HomePage"
		};

		var logInContent = new ShellContent
		{
			Title = "Login",
			Route = "LoginPage",
			ContentTemplate = new DataTemplate(typeof(Issue32419LoginPage)),
		};

		var flyoutItems = new FlyoutItem
		{
			Title = "Home Page",
		};

		flyoutItems.Items.Add(homeContent);
		flyoutItems.Items.Add(logInContent);

		Items.Add(flyoutItems);

		var menuItem1 = new MenuItem
		{
			Text = "MenuItem 1",
		};

		var menuItem2 = new MenuItem
		{
			Text = "MenuItem 2",
		};

		var menuItem3 = new MenuItem
		{
			Text = "MenuItem 3",
		};

		Items.Add(menuItem1);
		Items.Add(menuItem2);
		Items.Add(menuItem3);
	}

	class Issue32419HomePage : ContentPage
	{
		public Issue32419HomePage()
		{
			Content = new VerticalStackLayout
			{
				Children =
				{
					new Label
					{
						Text = "Welcome to .NET MAUI Home Page.",
						AutomationId = "homePageLabel",
					},
					new Button
					{
						Text = "Flyout Behavior locked",
						Command = new Command(() => Shell.SetFlyoutBehavior(this, FlyoutBehavior.Locked)),
						AutomationId = "FlyoutLockedButton",
					}
				}
			};
		}
	}

	class Issue32419LoginPage : ContentPage
	{
		public Issue32419LoginPage()
		{
			Content = new VerticalStackLayout
			{
				Children =
				{
					new Label
					{
						AutomationId = "LoginPageLabel",
						Text = "Welcome to Login Page",
					},
				}
			};
		}
	}
}