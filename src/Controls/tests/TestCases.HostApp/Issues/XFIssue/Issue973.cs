namespace Maui.Controls.Sample.Issues;

internal class PageNameObject
{
	public string PageName { get; private set; }

	public PageNameObject(string pageName)
	{
		PageName = pageName;
	}
}

[Issue(IssueTracker.Github, 973, "ActionBar doesn't immediately update when nested TabbedPage is changed", PlatformAffected.Android)]
public class Issue973 : TestFlyoutPage
{
	protected override void Init()
	{
		// Set FlyoutBehavior to Popover to ensure consistent behavior across desktop and mobile platforms.
		// Windows and Catalyst default (FlyoutLayoutBehavior.Default) uses Split mode, which differs from mobile platforms.
		FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;

		var cells = new[] {
			new PageNameObject ("Close Flyout"),
			new PageNameObject ("Page 1"),
			new PageNameObject ("Page 2"),
			new PageNameObject ("Page 3"),
			new PageNameObject ("Page 4"),
			new PageNameObject ("Page 5"),
			new PageNameObject ("Page 6"),
			new PageNameObject ("Page 7"),
			new PageNameObject ("Page 8"),
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

			var cellName = ((PageNameObject)e.Item).PageName;

			if (cellName == "Close Flyout")
			{
				IsPresented = false;
			}
			else
			{
				var d = new CustomDetailPage(cellName)
				{
					Title = "Detail"
				};

				d.PresentMaster += (s, args) =>
				{
					IsPresented = true;
				};

				Detail = d;
			}
		};

		var master = new ContentPage
		{
			Padding = new Thickness(0, 20, 0, 0),
			Title = "Flyout",
			Content = listView
		};

		Flyout = master;

		var detail = new CustomDetailPage("Initial Page")
		{
			Title = "Detail"
		};

		detail.PresentMaster += (sender, e) =>
		{
			IsPresented = true;
		};

		Detail = detail;
	}
}

internal class CustomDetailPage : TabbedPage
{
	public event EventHandler PresentMaster;

	public CustomDetailPage(string pageName)
	{
		Title = pageName;

		Children.Add(new ContentPage
		{
			Title = "Tab 1",
			Content = new StackLayout
			{
				new Label {
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Start,
					Text = pageName + " Left aligned"
				}
			}
		});

		Children.Add(new ContentPage
		{
			Title = "Tab 2",
			Content = new StackLayout
			{
				new Label {
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.End,
					Text = pageName + " Right aligned"
				},
				new Button {
					Text = "Present Flyout",
					Command = new Command (() => {
						var handler = PresentMaster;
						if (handler != null)
							handler(this, EventArgs.Empty);
					})
				}
			}
		});
	}
}
