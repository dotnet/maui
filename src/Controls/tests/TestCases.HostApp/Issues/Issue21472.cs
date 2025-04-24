namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 21472, "Shell FlyoutBackgroundImage doesn't shown", PlatformAffected.UWP)]
public class Issue21472 : Shell
{
	public Issue21472()
	{
		FlyoutBackgroundImage = "dotnet_bot.png";
		FlyoutBehavior = FlyoutBehavior.Flyout;
		FlyoutBackgroundImageAspect = Aspect.AspectFill;
		var flyoutItem1 = new FlyoutItem { Title = "Item1" };
		var tab1 = new Tab();
		tab1.Items.Add(new ShellContent { ContentTemplate = new DataTemplate(typeof(Issue21472ContentPage)) });
		flyoutItem1.Items.Add(tab1);
		Items.Add(flyoutItem1);
	}
}

public class Issue21472ContentPage : ContentPage
{
	public Issue21472ContentPage()
	{
		var button = new Button
		{
			Text = "RemoveFlyoutBackground",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			AutomationId = "button"
		};

		button.Clicked += (sender, e) =>
		{
			Shell.Current.FlyoutBackgroundImage = null;
		};

		Content = button;
	}
}