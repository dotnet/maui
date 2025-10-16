using System.Collections.ObjectModel;
using Maui.Controls.Sample;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29130, "CollectionView2 ItemSizingStrategy should work for MeasureFirstItem", PlatformAffected.iOS)]
public class Issue29130 : ContentPage
{
	CollectionView2 collectionView;
	public Issue29130()
	{
		var viewModel = new Issue29130ViewModel();
		this.BindingContext = viewModel;

		var measureFirstItemButton = new Button
		{
			Text = "MeasureFirstItem",
			AutomationId = "29130MeasureFirstItemButton"
		};
		measureFirstItemButton.Clicked += OnMeasureItemButtonClicked;

		var measureAllItemsButton = new Button
		{
			Text = "MeasureAllItems",
			AutomationId = "29130MeasureAllItemsButton"

		};
		measureAllItemsButton.Clicked += OnMeasureItemButtonClicked;

		var grid = new Grid
		{
			AutomationId = "29130Grid",
			ColumnDefinitions = new ColumnDefinitionCollection
			{
				new ColumnDefinition { Width = GridLength.Auto },
				new ColumnDefinition { Width = GridLength.Auto }
			}
		};

		// Add buttons to the grid with correct column assignments
		Grid.SetColumn(measureFirstItemButton, 0);
		Grid.SetColumn(measureAllItemsButton, 1);

		grid.Children.Add(measureFirstItemButton);
		grid.Children.Add(measureAllItemsButton);

		collectionView = new CollectionView2
		{
			Margin = new Thickness(0, 20, 0, 0),
			AutomationId = "29130CollectionView",
			ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, "Text");
				label.SetBinding(Label.FontSizeProperty, "FontSize");
				label.SetBinding(Label.AutomationIdProperty, "AutomationId");
				return label;
			})
		};
		collectionView.SetBinding(ItemsView.ItemsSourceProperty, "Items");

		var stackLayout = new StackLayout
		{
			Padding = 10,
			Children =
			{
				grid,
				collectionView
			}
		};

		Content = stackLayout;
	}

	void OnMeasureItemButtonClicked(object sender, EventArgs e)
	{
		var button = (Button)sender;
		if (button.Text == "MeasureFirstItem")
		{
			collectionView.ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem;
		}
		else
		{
			collectionView.ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems;
		}
	}
}
public class Issue29130ViewModel

{
	public ObservableCollection<Issue29130ItemModel> Items { get; } = new()
	{
		new Issue29130ItemModel("Short Text", 14, "29130Item1"),
		new Issue29130ItemModel("Short Text expanded to test sizing. Just a few more words added.", 18, "29130Item2"),
		new Issue29130ItemModel("This is a bit longer text that continues with additional words to create a slightly larger visual footprint.This is a bit longer text that continues with additional words to create a slightly larger visual footprint.This is a bit longer text that continues with additional words to create a slightly larger visual footprint.", 24, "29130Item3"),
	};
}

public class Issue29130ItemModel
{
	public string Text { get; }
	public double FontSize { get; }

	public string AutomationId { get; set; }

	public Issue29130ItemModel(string text, double fontSize, string automationId = null)
	{
		AutomationId = automationId;
		Text = text;
		FontSize = fontSize;
	}

}