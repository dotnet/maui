namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 43941, "Memory leak with ListView's RecycleElement on iOS", PlatformAffected.iOS)]
public class Bugzilla43941 : TestNavigationPage
{
	protected override void Init()
	{
		PushAsync(new LandingPage43941());
	}
}


public class ContentPage43941 : ContentPage
{
	public ContentPage43941()
	{
		Interlocked.Increment(ref LandingPage43941.Counter);
		System.Diagnostics.Debug.WriteLine("Page: " + LandingPage43941.Counter);

		var list = new List<int>();
		for (var i = 0; i < 30; i++)
		{
			list.Add(i);
		}

		Title = "ContentPage43941";
		Content = new ListView
		{
			HasUnevenRows = true,
			ItemsSource = list,
			AutomationId = "ListView"
		};
	}

	~ContentPage43941()
	{
		Interlocked.Decrement(ref LandingPage43941.Counter);
		System.Diagnostics.Debug.WriteLine("Page: " + LandingPage43941.Counter);
	}
}


public class LandingPage43941 : ContentPage
{
	public static int Counter;
	public Label Label;

	public LandingPage43941()
	{
		Label = new Label
		{
			Text = "Counter: " + Counter,
			AutomationId = "counterlabel",
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center
		};

		Content = new StackLayout
		{
			Orientation = StackOrientation.Vertical,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			Spacing = 15,
			Children =
			{
				new Label
				{
					Text = "Click Push to show a ListView. When you hit the Back button, Counter will show the number of pages that have not been finalized yet."
					+ " If you click GC, the counter should be 0."
				},
				Label,
				new Button
				{
					Text = "GC",
					AutomationId = "GC",
					Command = new Command(o =>
					{
						GarbageCollectionHelper.Collect();
						Label.Text = "Counter: " + Counter;
					})
				},
				new Button
				{
					Text = "Push",
					AutomationId = "Push",
					Command = new Command(async o =>
					{
						await Navigation.PushAsync(new ContentPage43941());
					})
				}
			}
		};
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		Label?.Text = "Counter: " + Counter;
	}
}