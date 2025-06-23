namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Bugzilla, 27731, "[Android] Action Bar can not be controlled reliably on FlyoutPage", PlatformAffected.Android)]
public class Bugzilla27731 : NavigationPage
{
	public Bugzilla27731()
	{
		Navigation.PushAsync(new MainPage());
	}

	public class MainPage : TestFlyoutPage
	{
		string _pageTitle = "PageTitle";

		protected override void Init()
		{
			// Initialize ui here instead of ctor
			Flyout = new ContentPage { Content = new Label { Text = "Menu Item" }, Title = "Menu" };
			Detail = new NavigationPage(new Page2(_pageTitle)
			{
				AutomationId = _pageTitle
			});
		}

		class Page2 : ContentPage
		{
			static int count;
			public Page2(string title)
			{
				count++;
				Title = $"{title}{count}";
				NavigationPage.SetHasNavigationBar(this, false);
				Content = new StackLayout
				{
					Children =
					{
						new Label { Text = $"This is page {count}.", AutomationId = "PageLabel" },
						new Button { Text = "Click", AutomationId = "Click", Command = new Command(() => Navigation.PushAsync(new Page2(title))) }
					}
				};
			}
		}
	}
}