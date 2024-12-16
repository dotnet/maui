namespace Maui.Controls.Sample.Issues;

public class NavPageNameObject
{
	public string PageName { get; private set; }

	public NavPageNameObject(string pageName)
	{
		PageName = pageName;
	}
}

[Issue(IssueTracker.Github, 892, "NavigationPages as details in FlyoutPage don't work as expected", PlatformAffected.Android)]
public class Issue892 : TestFlyoutPage
{
	protected override void Init()
	{
		// Set FlyoutBehavior to Popover to ensure consistent behavior across desktop and mobile platforms.
		// Windows and Catalyst default (FlyoutLayoutBehavior.Default) uses Split mode, which differs from mobile platforms.
		FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
		var cells = new[] {
			new NavPageNameObject ("Close Flyout"),
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
			if (cellName == "Close Flyout")
			{
				IsPresented = false;
			}
			else
			{
				Detail = new CustomNavDetailPage(cellName);
			}
		};

		var master = new ContentPage
		{
			Padding = new Thickness(0, 20, 0, 0),
			Title = "Flyout",
			Content = listView
		};

		Flyout = master;
		Detail = new CustomNavDetailPage("Initial Page");

#pragma warning disable CS0618 // Type or member is obsolete
		MessagingCenter.Subscribe<NestedNavPageRootView>(this, "PresentMaster", (sender) =>
		{
			IsPresented = true;
		});
#pragma warning restore CS0618 // Type or member is obsolete
	}
}

public class CustomNavDetailPage : NavigationPage
{
	public CustomNavDetailPage(string pageName)
	{
		PushAsync(new NestedNavPageRootView(pageName));
	}
}

public class NestedNavPageRootView : ContentPage
{
	public NestedNavPageRootView(string pageTitle)
	{
		Title = pageTitle;
		BackgroundColor = Color.FromArgb("#666");

		var label = new Label
		{
			Text = "Not Tapped"
		};

		Content = new StackLayout
		{
			label,
			new Button {
				Text = "Check back three",
				Command = new Command (() => { label.Text = "At root"; })
			},
			new Button {
				Text = "Push next page",
				Command = new Command (() => Navigation.PushAsync (new NestedNavPageOneLevel ()))
			},
			new Button {
				Text = "Present Flyout",
				Command = new Command (() => {
#pragma warning disable CS0618 // Type or member is obsolete
					MessagingCenter.Send<NestedNavPageRootView> (this, "PresentMaster");
#pragma warning restore CS0618 // Type or member is obsolete
				})
			}
		};
	}
}

public class NestedNavPageOneLevel : ContentPage
{
	public NestedNavPageOneLevel()
	{
		Title = "One pushed";
		BackgroundColor = Color.FromArgb("#999");

		var label = new Label
		{
			Text = "Not Tapped"
		};

		Content = new StackLayout
		{
			label,
			new Button {
				Text = "Check back two",
				Command = new Command (() => { label.Text = "Pop two"; })
			},
			new Button {
				Text = "Push next next page",
				Command = new Command (() => Navigation.PushAsync (new NestedNavPageTwoLevels ()))
			}
		};
	}
}

public class NestedNavPageTwoLevels : ContentPage
{
	public NestedNavPageTwoLevels()
	{
		Title = "Two pushed";
		BackgroundColor = Color.FromArgb("#BBB");

		var label = new Label
		{
			Text = "Not Tapped",
			TextColor = Colors.Red
		};

		var label2 = new Label
		{
			Text = "You are at the end of the line",
			TextColor = Colors.Red
		};

		Content = new StackLayout
		{
			label,
			label2,
			new Button {
				Text = "Check back one",
				Command = new Command (() => { label.Text = "Pop one"; })
			},
		};
	}
}
