namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 13243, "Flyout Not Displayed on Android When FlyoutWidth Is Set Only for Desktop via OnIdiom", PlatformAffected.All)]
public partial class Issue13243 : Shell
{
	public Issue13243()
	{
		InitializeComponent();
	}
}

public class Issue13243Page1 : ContentPage
{
	public Issue13243Page1()
	{
		Title = "Page 1";
		var label = new Label
		{
			Text = "Hello, World!",
			AutomationId = "Label",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};
		Content = new StackLayout
		{
			Children = { label },
		};
	}
}

public class Issue13243Page2 : ContentPage
{
	public Issue13243Page2()
	{
		Title = "Page 2";
		var label = new Label
		{
			Text = "Hello, World!",
		};
		Content = new StackLayout
		{
			Children = { label },
		};
	}
}
