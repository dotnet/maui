namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 52419, "[A] OnAppearing called for previous pages in a tab's navigation when switching active tabs", PlatformAffected.Android)]
public class Bugzilla52419 : TestTabbedPage
{
	protected override void Init()
	{
		var nav1 = new NavigationPage { Title = "Tab Page 1" };
		nav1.PushAsync(new Bugzilla52419Page1());
		var nav2 = new NavigationPage { Title = "Tab Page 2" };
		nav2.PushAsync(new Bugzilla52419Page2());
		Children.Add(nav1);
		Children.Add(nav2);
	}
}

class Bugzilla52419Page1 : ContentPage
{
	public Label _timesAppeared { get; set; }
	int _count;

	string _guid = Guid.NewGuid().ToString();
	public Bugzilla52419Page1()
	{
		_timesAppeared = new Label
		{
			Text = "Times Appeared: " + _count.ToString(),
			AutomationId = "AppearanceLabel"
		};
		Content = new StackLayout
		{
			Children =
				{
					new Label
					{
						Text = "Page Guid: " + _guid
					},
					_timesAppeared,
					new Label
					{
						Text = "Click the button a couple times, switch to the second tab, and then back to the first. The Appearing event (which increase the counter) should only occur for the visible first tab."
					},
					new Button
					{
						Text = "Push new page",
						AutomationId = "PushNewPage",
						Command = new Command(() => Navigation.PushAsync(new Bugzilla52419Page1()))
					}
				}
		};
		Appearing += OnAppearing;
	}

	void OnAppearing(object sender, EventArgs e)
	{
		_count++;
		_timesAppeared.Text = "Times Appeared: " + _count.ToString();
	}
}

class Bugzilla52419Page2 : ContentPage
{
	public Bugzilla52419Page2()
	{
		Title = "Tab Page 2";
		Content = new StackLayout
		{
			Children =
			{
				new Label
				{
					Text = "Other content"
				}
			}
		};
	}
}