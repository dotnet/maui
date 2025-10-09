namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.None, 1024, "Entry and Editor are leaking when used in ViewCell", PlatformAffected.iOS)]
public class Bugzilla1024 : TestNavigationPage
{
	protected override void Init()
	{
		PushAsync(new LandingPage1024());
	}
}


public class LandingPage1024 : ContentPage
{
	public static int Counter;
	public Label Label;

	public LandingPage1024()
	{
		Label = new Label
		{
			AutomationId = "Counter",
			Text = "Counter: " + Counter,
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
						await Navigation.PushAsync(new ContentPage1024());
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


public class ContentPage1024 : ContentPage
{
	public ContentPage1024()
	{
		Interlocked.Increment(ref LandingPage1024.Counter);
		System.Diagnostics.Debug.WriteLine("Page: " + LandingPage1024.Counter);

		Content = new ListView
		{
			HasUnevenRows = true,
			ItemsSource = new List<string> { "Entry", "Editor" },
			ItemTemplate = new InputViewDataTemplateSelector(),
			AutomationId = "ListView"
		};
	}

	~ContentPage1024()
	{
		Interlocked.Decrement(ref LandingPage1024.Counter);
		System.Diagnostics.Debug.WriteLine("Page: " + LandingPage1024.Counter);
	}
}


public class InputViewDataTemplateSelector : DataTemplateSelector
{
	public DataTemplate EntryTemplate { get; set; }
	public DataTemplate EditorTemplate { get; set; }

	public InputViewDataTemplateSelector()
	{
		EntryTemplate = new DataTemplate(() => new ViewCell { View = new Entry { BackgroundColor = Colors.DarkGoldenrod, Text = "Entry" } });
		EditorTemplate = new DataTemplate(() => new ViewCell { View = new Editor { BackgroundColor = Colors.Bisque, Text = "Editor" } });
	}

	protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
	{
		switch (item as string)
		{
			case "Entry":
				return EntryTemplate;
			case "Editor":
				return EditorTemplate;
		}

		return null;
	}
}