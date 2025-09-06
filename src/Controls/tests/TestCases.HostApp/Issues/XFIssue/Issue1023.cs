namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.None, 1023, "Automate GC checks of picker disposals", PlatformAffected.iOS)]
public class Bugzilla1023 : TestNavigationPage
{
	protected override void Init()
	{
		PushAsync(new LandingPage1023());
	}
}


public class LandingPage1023 : ContentPage
{
	public static int Counter;
	public Label Label;

	public LandingPage1023()
	{
		Label = new Label
		{
			Text = "Counter: " + Counter,
			AutomationId = "Counter",
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
						await Navigation.PushAsync(new ContentPage1023());
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


public class ContentPage1023 : ContentPage
{
	public ContentPage1023()
	{
		Interlocked.Increment(ref LandingPage1023.Counter);
		System.Diagnostics.Debug.WriteLine("Page: " + LandingPage1023.Counter);

		Content = new ListView
		{
			HasUnevenRows = true,
			ItemsSource = new List<string> { "DatePicker", "Picker", "TimePicker" },
			ItemTemplate = new DataTemplateSelector1023(),
			AutomationId = "ListView"
		};
	}

	~ContentPage1023()
	{
		Interlocked.Decrement(ref LandingPage1023.Counter);
		System.Diagnostics.Debug.WriteLine("Page: " + LandingPage1023.Counter);
	}
}


public class DataTemplateSelector1023 : DataTemplateSelector
{
	public DataTemplate DatePickerTemplate { get; set; }
	public DataTemplate PickerTemplate { get; set; }
	public DataTemplate TimePickerTemplate { get; set; }

	public DataTemplateSelector1023()
	{
		DatePickerTemplate = new DataTemplate(() => new ViewCell { View = new DatePicker() });
		PickerTemplate = new DataTemplate(() => new ViewCell { View = new Picker() });
		TimePickerTemplate = new DataTemplate(() => new ViewCell { View = new TimePicker() });
	}

	protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
	{
		switch (item as string)
		{
			case "DatePicker":
				return DatePickerTemplate;
			case "Picker":
				return PickerTemplate;
			case "TimePicker":
				return TimePickerTemplate;
		}

		return null;
	}
}