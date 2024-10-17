namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 6384, "content page in tabbed page not showing inside shell tab", PlatformAffected.iOS | PlatformAffected.Android)]
public class Github6384 : TestShell
{
	protected override void Init()
	{
		var tabOneButton = new Button
		{
			AutomationId = "NavigationButton",
			Text = "Push me!"
		};

		tabOneButton.Clicked += TabOneButton_Clicked;

		var tabOnePage = new ContentPage { Content = tabOneButton };

		var tabTwoPage = new ContentPage { Content = new Label { Text = "Go to TabOne" } };
		var tabOne = new Tab { Title = "TabOne" };
		var tabTwo = new Tab { Title = "TabTwo" };
		tabOne.Items.Add(tabOnePage);
		tabTwo.Items.Add(tabTwoPage);

		Items.Add(
				new TabBar
				{
					Items = { tabOne, tabTwo }
				}
		);
	}

	private void TabOneButton_Clicked(object sender, System.EventArgs e)
	{
		var subTabPageOne = new ContentPage
		{
			Content = new Label
			{
				Text = "SubPage One",
				AutomationId = "SubTabLabel1",
				VerticalTextAlignment = TextAlignment.Center,
			}
		};
		var subTabPageTwo = new ContentPage
		{
			Content = new Label
			{
				Text = "SubPage Two",
				AutomationId = "SubTabLabel2",
				VerticalTextAlignment = TextAlignment.Center,
			}
		};

		var tabbedPage = new TabbedPage { Title = "TabbedPage" };
		tabbedPage.Children.Add(subTabPageOne);
		tabbedPage.Children.Add(subTabPageTwo);
		Shell.SetTabBarIsVisible(tabbedPage, false);
		this.Navigation.PushAsync(tabbedPage);
	}
}
