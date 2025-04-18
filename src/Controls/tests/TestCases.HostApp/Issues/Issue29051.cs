namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 29051, "I8_Headers_and_Footers displays the footer 2019 in the middle of the header", PlatformAffected.iOS)]
public class Issue29051 : ContentPage
{
	public Issue29051()
	{
		var mainGrid = new Grid
		{
			Margin = new Thickness(20),
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		var instructionsStack = new StackLayout
		{
			Children =
			{
				new Label { Text = "1. The test passes if the view is displayed in Vertical list layout." },
				new Label { Text = "2. The test passes if the header text display 'Monkeys'." },
				new Label { Text = "3. The test passes if the footer text display '2019'." }
			}
		};

		Grid.SetRow(instructionsStack, 0);
		mainGrid.Children.Add(instructionsStack);

		var collectionView = new CollectionView1
		{
			Header = "Monkeys",
			Footer = "2019",
			ItemTemplate = new DataTemplate(() =>
			{
				var itemGrid = new Grid
				{
					Padding = new Thickness(10),
					RowDefinitions =
					{
						new RowDefinition { Height = GridLength.Auto },
						new RowDefinition { Height = GridLength.Auto }
					},
					ColumnDefinitions =
					{
						new ColumnDefinition { Width = GridLength.Auto },
						new ColumnDefinition { Width = GridLength.Star }
					}
				};

				var nameLabel = new Label
				{
					FontAttributes = FontAttributes.Bold
				};
				nameLabel.SetBinding(Label.TextProperty, "Name");
				nameLabel.SetBinding(Label.AutomationIdProperty, "AutomationId");

				Grid.SetColumn(nameLabel, 1);
				itemGrid.Children.Add(nameLabel);

				return itemGrid;
			}),
			ItemsSource = Enumerable.Range(0, 5).Select(i => new { Name = $"Item {i}", AutomationId = $"Item{i}" }).ToList(),
		};

		Grid.SetRow(collectionView, 1);
		mainGrid.Children.Add(collectionView);

		Content = mainGrid;
	}
}