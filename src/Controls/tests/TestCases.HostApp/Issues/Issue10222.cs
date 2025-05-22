namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 10222, "[CollectionView] ObjectDisposedException if the page is closed during scrolling", PlatformAffected.iOS)]
	public class Issue10222 : NavigationPage
	{
		public Issue10222() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				// Initialize ui here instead of ctor
				Navigation.PushAsync(new ContentPage
				{
					Content = new Button
					{
						AutomationId = "goTo",
						Text = "Go",
						Command = new Command(async () => await Navigation.PushAsync(new CarouselViewTestPage()))
					}
				});
			}

			class CarouselViewTestPage : ContentPage
			{
				CollectionView cv;
				List<string> items;
				public CarouselViewTestPage()
				{
					cv = new CollectionView
					{
						AutomationId = "collectionView",
						Margin = new Thickness(0, 40),
						ItemTemplate = new DataTemplate(() =>
						{
							var label = new Label
							{
								HorizontalTextAlignment = TextAlignment.Center,
								Margin = new Thickness(0, 100)
							};
							label.SetBinding(Label.TextProperty, new Binding("."));
							label.SetBinding(Label.AutomationIdProperty, new Binding("."));
							var tapGestureRecognizer = new TapGestureRecognizer();
							tapGestureRecognizer.Tapped += (sender, e) => LabelTapped();
							label.GestureRecognizers.Add(tapGestureRecognizer);
							return label;
						})
					};

					items = new List<string>();
					for (int i = 0; i < 10; i++)
					{
						items.Add($"items{i}");
					}
					cv.ItemsSource = items;
					Content = cv;
				}

				// Scrolls to last item then pops the page
				async void LabelTapped()
				{

					cv.ScrollTo(items.Count - 1);

					await Task.Delay(200);

					await Navigation.PopAsync(false);

				}
			}
		}
	}
}