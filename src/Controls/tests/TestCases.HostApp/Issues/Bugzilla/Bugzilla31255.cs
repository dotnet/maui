namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 31255, "Flyout's page Icon cause memory leak after FlyoutPage is popped out by holding on page")]
	public class Bugzilla31255 : NavigationPage
	{
		public Bugzilla31255() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				var stack = new StackLayout() { VerticalOptions = LayoutOptions.Center };

				stack.Children.Add(new Label()
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalTextAlignment = TextAlignment.Center,
					Text = "Page 1",
					AutomationId = "MauiLabel"
				});
				SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
				Content = stack;

			}

			WeakReference _page2Tracker;

			protected override async void OnAppearing()
			{
				base.OnAppearing();

				if (_page2Tracker == null)
				{
					var page2 = new Page2();

					_page2Tracker = new WeakReference(page2, false);

					await Task.Delay(1000);
					await Navigation.PushModalAsync(page2);

					StartTrackPage2();
				}
			}

			async void StartTrackPage2()
			{
				while (true)
				{
					((Label)((StackLayout)Content).Children[0]).Text = _page2Tracker.IsAlive ? "Failed" : "Success";
					await Task.Delay(1000);
					GarbageCollectionHelper.Collect();
				}
			}


			public class Page2 : FlyoutPage
			{
				public Page2()
				{
					Flyout = new ContentPage
					{
						Title = "Flyout",
						IconImageSource = "Icon.png"
					};
					Detail = new ContentPage() { Title = "Detail" };
				}

				protected override async void OnAppearing()
				{
					base.OnAppearing();

					await Task.Delay(1000);
					await Navigation.PopModalAsync();
				}
				protected override void OnDisappearing()
				{
					base.OnDisappearing();
				}
			}
		}
	}
}