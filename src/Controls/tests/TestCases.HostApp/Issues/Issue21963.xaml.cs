namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 21963, "Shell.MenuItemTemplate Sometime Does Not Work", PlatformAffected.Android)]
public partial class Issue21963 : Shell
{
	public Issue21963()
	{
		InitializeComponent();
	}
}
public partial class Issue21963InnerPage : ContentPage
{
	public Issue21963InnerPage()
	{
		Content = new VerticalStackLayout
		{
			Children =
			{
				new Button
				{
					Text = "Click Me"
				}
			}
		};
	}
}

public partial class Issue21963MenuPage : ContentPage
{
	public Issue21963MenuPage()
	{
		Content = new VerticalStackLayout
		{
			Children =
			{
				new Button
				{
					Text = "Click Me"
				}
			}
		};
	}
}
public partial class Issue21963FlyoutPage : ContentPage
{
	public Issue21963FlyoutPage()
	{
		Content = new VerticalStackLayout
		{
			Children =
			{
				new Button
				{
					Text = "Click Me"
				}
			}
		};
	}
}

