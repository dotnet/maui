﻿namespace Maui.Controls.Sample.Issues
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
							return label;
						})
					};
					Content = cv;
					InitCV();
				}

				async void InitCV()
				{
					var items = new List<string>();
					for (int i = 0; i < 10; i++)
					{
						items.Add($"items{i}");
					}

					cv.ItemsSource = items;

					//give the cv time to draw the items
					await Task.Delay(1000);

					cv.ScrollTo(items.Count - 1);

					//give the cv time to scroll
					var rand = new Random();
					await Task.Delay(rand.Next(10, 200));

					await Navigation.PopAsync(false);

				}
			}
		}
	}
}