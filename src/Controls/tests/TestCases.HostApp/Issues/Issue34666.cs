namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 34666, "The C6 page cannot scroll on Windows and Android platforms", PlatformAffected.All)]
	public class Issue34666 : ContentPage
	{
		public Issue34666()
		{
			Title = "C6";

			var collectionView = new CollectionView
			{
				AutomationId = "CollectionView",
				VerticalScrollBarVisibility = ScrollBarVisibility.Always,
				ItemTemplate = new DataTemplate(() =>
				{
					var itemGrid = new Grid { Padding = new Thickness(10) };
					itemGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
					itemGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
					itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
					itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

					var image = new Image
					{
						Aspect = Aspect.AspectFill,
						HeightRequest = 60,
						WidthRequest = 60
					};
					image.SetBinding(Image.SourceProperty, nameof(Monkey.ImageUrl));
					Grid.SetRowSpan(image, 2);

					var nameLabel = new Label { FontAttributes = FontAttributes.Bold };
					nameLabel.SetBinding(Label.TextProperty, nameof(Monkey.Name));
					nameLabel.SetBinding(Label.AutomationIdProperty, nameof(Monkey.Name));
					Grid.SetColumn(nameLabel, 1);

					var locationLabel = new Label
					{
						FontAttributes = FontAttributes.Italic,
						VerticalOptions = LayoutOptions.End
					};
					locationLabel.SetBinding(Label.TextProperty, nameof(Monkey.Location));
					Grid.SetRow(locationLabel, 1);
					Grid.SetColumn(locationLabel, 1);

					itemGrid.Add(image);
					itemGrid.Add(nameLabel);
					itemGrid.Add(locationLabel);

					return itemGrid;
				})
			};

			collectionView.SetBinding(CollectionView.ItemsSourceProperty, nameof(MonkeysViewModel.Monkeys));

			var refreshView = new RefreshView
			{
				IsEnabled = false,
				Content = collectionView
			};

			var mainGrid = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				}
			};
			mainGrid.Add(new Label { Text = "1. Pull down to initiate a refresh." }, 0, 0);
			mainGrid.Add(new Label { Text = "2. The test passes if the progress spinner does not appear." }, 0, 1);
			mainGrid.Add(refreshView, 0, 2);

			Content = mainGrid;
			BindingContext = new MonkeysViewModel();
		}
	}
}
