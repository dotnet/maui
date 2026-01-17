using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 33375, "SwipeGestureRecognizer triggers while scrolling CollectionView horizontally on iOS", PlatformAffected.iOS)]
	public class Issue33375 : TestContentPage
	{
		private int swipeCount = 0;
		private Label _statusLabel;

		protected override void Init()
		{
			var items = new ObservableCollection<Issue33375_ItemData>
			{
				new Issue33375_ItemData { Name = "Item 1", Description = "Scroll right" },
				new Issue33375_ItemData { Name = "Item 2", Description = "Keep scrolling" },
				new Issue33375_ItemData { Name = "Item 3", Description = "More items" },
				new Issue33375_ItemData { Name = "Item 4", Description = "Even more" },
				new Issue33375_ItemData { Name = "Item 5", Description = "Last item" },
				new Issue33375_ItemData { Name = "Item 6", Description = "Extra item" },
				new Issue33375_ItemData { Name = "Item 7", Description = "Another one" },
				new Issue33375_ItemData { Name = "Item 8", Description = "Keep going" }
			};

			var collectionView = new CollectionView
			{
				AutomationId = "TestCollectionView",
				HeightRequest = 200,
				ItemsSource = items,
				ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
				{
					ItemSpacing = 10
				},
				ItemTemplate = new DataTemplate(() =>
				{
					var border = new Border
					{
						Background = Colors.DodgerBlue,
						Padding = 20,
						WidthRequest = 150,
						HeightRequest = 180,
						StrokeThickness = 0
					};

					var stack = new VerticalStackLayout
					{
						VerticalOptions = LayoutOptions.Center
					};

					var nameLabel = new Label
					{
						FontSize = 20,
						FontAttributes = FontAttributes.Bold,
						HorizontalOptions = LayoutOptions.Center
					};
					nameLabel.SetBinding(Label.TextProperty, nameof(Issue33375_ItemData.Name));

					var descLabel = new Label
					{
						FontSize = 14,
						HorizontalOptions = LayoutOptions.Center,
						Margin = new Thickness(0, 10, 0, 0)
					};
					descLabel.SetBinding(Label.TextProperty, nameof(Issue33375_ItemData.Description));

					stack.Children.Add(nameLabel);
					stack.Children.Add(descLabel);
					border.Content = stack;

					return border;
				})
			};

			_statusLabel = new Label
			{
				AutomationId = "StatusLabel",
				Text = "No swipe detected",
				FontSize = 18,
				FontAttributes = FontAttributes.Bold,
				HorizontalOptions = LayoutOptions.Center,
				Margin = new Thickness(0, 20)
			};

			var border = new Border
			{
				AutomationId = "TestBorder",
				Background = Colors.LightGray,
				Padding = 20,
				Content = new VerticalStackLayout
				{
					Spacing = 20,
					Children =
					{
						_statusLabel,
						collectionView,
					}
				}
			};

			var swipeGesture = new SwipeGestureRecognizer
			{
				Direction = SwipeDirection.Right,
				Threshold = 200,
				Command = new Command(() =>
				{
					swipeCount++;
					_statusLabel.Text = $"Swipe triggered! Count: {swipeCount}";
					_statusLabel.TextColor = Colors.Red;
				})
			};
			border.GestureRecognizers.Add(swipeGesture);
			Content = border;
		}

		class  Issue33375_ItemData
		{
			public string Name { get; set; } = string.Empty;
			public string Description { get; set; } = string.Empty;
		}
	}
}
