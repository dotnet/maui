namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 889, "Assigning to FlyoutPage.Detail after construction doesn't work", PlatformAffected.Android | PlatformAffected.iOS)]
public class Issue889 : TestFlyoutPage
{
	protected override void Init()
	{
		var cells = new[] {
			new NavPageNameObject ("Page 1"),
			new NavPageNameObject ("Page 3"),
			new NavPageNameObject ("Page 4"),
			new NavPageNameObject ("Page 5"),
			new NavPageNameObject ("Page 6"),
			new NavPageNameObject ("Page 7"),
			new NavPageNameObject ("Page 8"),
		};

		var template = new DataTemplate(typeof(TextCell));
		template.SetBinding(TextCell.TextProperty, "PageName");

		var listView = new ListView
		{
			ItemTemplate = template,
			ItemsSource = cells
		};

		listView.BindingContext = cells;

		listView.ItemTapped += (sender, e) =>
		{
			var cellName = ((NavPageNameObject)e.Item).PageName;
			Detail = new CustomNavTabDetailPage(cellName);
		};

		var master = new ContentPage
		{
			Title = "Flyout",
			IconImageSource = "bank.png",
			Content = listView
		};

		Flyout = master;
		Detail = new CustomNavTabDetailPage("Initial Page");
	}
}

public class CustomNavTabDetailPage : NavigationPage
{
	public CustomNavTabDetailPage(string pageName)
	{
		PushAsync(new NestedNavTabPageRootView(pageName));
	}
}

public class NestedNavTabPageRootView : TabbedPage
{
	public NestedNavTabPageRootView(string pageTitle)
	{
		Title = pageTitle;

		var tabOne = new ContentPage
		{
			Title = "Tab 1 Title",
			BackgroundColor = Color.FromArgb("#666"),
			Content = new StackLayout
			{
				new Button {
					Text = "Push new page",
					AutomationId = "PushPage",
					Command = new Command (() => Navigation.PushAsync (new NestedNavTabPageOneLevel ()))
				}
			}
		};

		var tabTwo = new ContentPage
		{
			Title = "Tab 2 Title",
			BackgroundColor = Color.FromArgb("#BBB"),
			Content = new StackLayout
			{
				new Button {
					Text = "Push new page",
					AutomationId = "SecondTabPageButton",
					Command = new Command (() => Navigation.PushAsync (new NestedNavTabPageOneLevel ()))
				}
			}
		};

		tabOne.SetValue(IconImageSourceProperty, "bank.png");
		tabTwo.SetValue(IconImageSourceProperty, "coffee.png");
		Children.Add(tabOne);
		Children.Add(tabTwo);
	}
}

public class NestedNavTabPageOneLevel : ContentPage
{
	public NestedNavTabPageOneLevel()
	{
		Title = "One pushed";
		BackgroundColor = Color.FromArgb("#999");

		Content = new StackLayout
		{
			Children = {
				new Label {
					TextColor = Colors.Red,
					Text = "I have been pushed",
					AutomationId = "PushedPageLabel"
				}
			}
		};
	}
}
