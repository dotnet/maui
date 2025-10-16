namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 32206, "ContextActions cause memory leak: Page is never destroyed", PlatformAffected.iOS)]
	public class Bugzilla32206 : TestNavigationPage
	{
		protected override void Init()
		{
			PushAsync(new LandingPage32206());
		}
	}


	public class LandingPage32206 : ContentPage
	{
		public static int Counter;
		public Label Label;

		public LandingPage32206()
		{
			Label = new Label
			{
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
							await Navigation.PushAsync(new ContentPage32206());
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


	public class ContentPage32206 : ContentPage
	{
		public ContentPage32206()
		{
			Interlocked.Increment(ref LandingPage32206.Counter);
			System.Diagnostics.Debug.WriteLine("Page: " + LandingPage32206.Counter);

			Content = new ListView
			{
				ItemsSource = new List<string> { "Apple", "Banana", "Cherry" },
				ItemTemplate = new DataTemplate(typeof(ViewCell32206)),
				AutomationId = "ListView"
			};
		}

		~ContentPage32206()
		{
			Interlocked.Decrement(ref LandingPage32206.Counter);
			System.Diagnostics.Debug.WriteLine("Page: " + LandingPage32206.Counter);
		}
	}


	public class ViewCell32206 : ViewCell
	{
		public ViewCell32206()
		{
			View = new Label();
			View.SetBinding(Label.TextProperty, ".");
			ContextActions.Add(new MenuItem { Text = "Delete" });
		}
	}
}