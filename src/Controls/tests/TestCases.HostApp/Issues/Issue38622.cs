namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38622, "CollectionView layout changes after device rotation with RTL", PlatformAffected.iOS)]
public class Issue38622 : Shell
{
	public Issue38622()
	{
		Shell.SetNavBarIsVisible(this, false);
		Items.Add(new ShellContent
		{
			Route = "MainPage",
			ContentTemplate = new DataTemplate(typeof(Issue38622Page)),
		});
	}
}

public class Issue38622Page : ContentPage
{
	public Issue38622Page()
	{
		FlowDirection = FlowDirection.RightToLeft;
		BackgroundColor = Colors.White;

		var collectionView = new CollectionView2
		{
			AutomationId = "TestCollectionView",
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
			{
				ItemSpacing = 4,
			},
			SelectionMode = SelectionMode.None,
			ItemTemplate = new DataTemplate(() =>
			{
				var grid = new Grid
				{
					ColumnDefinitions =
					{
						new ColumnDefinition(GridLength.Auto),
						new ColumnDefinition(GridLength.Star),
						new ColumnDefinition(GridLength.Auto),
					},
					ColumnSpacing = 12,
					Padding = new Thickness(12, 8),
					Margin = new Thickness(8, 2),
					BackgroundColor = Colors.LightGray,
				};

				var numberLabel = new Label
				{
					FontSize = 16,
					FontAttributes = FontAttributes.Bold,
					VerticalOptions = LayoutOptions.Center,
				};
				numberLabel.SetBinding(Label.TextProperty, "Number");

				var contentLayout = new VerticalStackLayout
				{
					Spacing = 4,
				};

				var titleLabel = new Label
				{
					FontSize = 16,
					FontAttributes = FontAttributes.Bold,
				};
				titleLabel.SetBinding(Label.TextProperty, "Title");

				var descriptionLabel = new Label
				{
					FontSize = 13,
				};
				descriptionLabel.SetBinding(Label.TextProperty, "Description");

				contentLayout.Add(titleLabel);
				contentLayout.Add(descriptionLabel);

				var arrowLabel = new Label
				{
					Text = "→",
					FontSize = 20,
					VerticalOptions = LayoutOptions.Center,
				};

				grid.Add(numberLabel, 0, 0);
				grid.Add(contentLayout, 1, 0);
				grid.Add(arrowLabel, 2, 0);

				return grid;
			}),
		};

		var items = new List<Issue38622Model>();

		for (var i = 1; i <= 100; i++)
		{
			items.Add(new Issue38622Model
			{
				Number = i,
				Title = $"Item {i}",
				Description = (i % 3) switch
				{
					0 => $"This is item {i} with a short description.",
					1 => $"This is item {i} with a medium length description to exercise CollectionView measurement and layout.",
					_ => $"This is item {i} with a longer description. This intentionally creates a different item height so that the CollectionView has to measure and arrange different cells during scrolling and rotation.",
				},
			});
		}

		collectionView.ItemsSource = items;

		Content = collectionView;
	}
}

public class Issue38622Model
{
	public int Number { get; set; }

	public string Title { get; set; }

	public string Description { get; set; }
}
