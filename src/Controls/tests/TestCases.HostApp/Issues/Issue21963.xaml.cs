namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 21963, "Shell.MenuItemTemplate Sometime Does Not Work", PlatformAffected.Android)]
public partial class Issue21963 : TestShell
{

	protected override void Init()
	{
		this.FlyoutIsPresented = true;
		this.ItemTemplate = new DataTemplate(() =>
	{
		var content = new ContentView();
		var label = new Label();
		label.SetBinding(Label.TextProperty, "Title");
		content.Content = label;

		return content;
	});

		// MenuItemTemplate
		this.MenuItemTemplate = new DataTemplate(() =>
		{
			var content = new ContentView();
			var label = new Label();
			label.SetBinding(Label.TextProperty, "Title");
			content.Content = label;

			return content;
		});

		// ShellContent 1
		var shellContent1 = new ShellContent
		{
			Title = "ShellContent 1",
			AutomationId = "Issue21963ShellContent1",
			ContentTemplate = new DataTemplate(typeof(Issues.Issue21963InnerPage))
		};

		// ShellContent 2
		var shellContent2 = new ShellContent
		{
			Title = "ShellContent 2",
			AutomationId = "Issue21963ShellContent2",
			ContentTemplate = new DataTemplate(typeof(Issues.Issue21963MenuPage))
		};

		// ShellContent 3
		var shellContent3 = new ShellContent
		{
			Title = "ShellContent 3",
			AutomationId = "Issue21963ShellContent3",
			ContentTemplate = new DataTemplate(typeof(Issues.Issue21963FlyoutPage))
		};

		// MenuItems
		var menuItem1 = new MenuItem
		{
			Text = "MenuItem 1",
			AutomationId = "Issue21963MenuItem1"
		};

		var menuItem2 = new MenuItem
		{
			Text = "MenuItem 2",
			AutomationId = "Issue21963MenuItem2"
		};

		// Add items to Shell
		Items.Add(shellContent1);
		Items.Add(shellContent2);
		Items.Add(shellContent3);

		Items.Add(menuItem1);
		Items.Add(menuItem2);
	}
}

public class Issue21963InnerPage : ContentPage
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

public class Issue21963MenuPage : ContentPage
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
public class Issue21963FlyoutPage : ContentPage
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

