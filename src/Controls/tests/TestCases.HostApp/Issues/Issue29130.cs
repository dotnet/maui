using System.Collections.ObjectModel;
using Maui.Controls.Sample;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29130, "CollectionView2 ItemSizingStrategy should work for MeasureFirstItem", PlatformAffected.iOS)]
public class Issue29130 : ContentPage
{
	CollectionView2 collectionView;
	bool isUsingAlternateTemplate = false;
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

		var changeTemplateButton = new Button
		{
			Text = "Change Template",
			AutomationId = "29130ChangeTemplateButton"
		};
		changeTemplateButton.Clicked += OnChangeTemplateButtonClicked;

		var grid = new Grid
		{
			AutomationId = "29130Grid",
			ColumnDefinitions = new ColumnDefinitionCollection
			{
				new ColumnDefinition { Width = GridLength.Star },
				new ColumnDefinition { Width = GridLength.Star },
				new ColumnDefinition { Width = GridLength.Star }
			}
		};

		// Add buttons to the grid with correct column assignments
		Grid.SetColumn(measureFirstItemButton, 0);
		Grid.SetColumn(measureAllItemsButton, 1);
		Grid.SetColumn(changeTemplateButton, 2);

		grid.Children.Add(measureFirstItemButton);
		grid.Children.Add(measureAllItemsButton);
		grid.Children.Add(changeTemplateButton);

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

	void OnChangeTemplateButtonClicked(object sender, EventArgs e)
	{
		isUsingAlternateTemplate = !isUsingAlternateTemplate;

		if (isUsingAlternateTemplate)
		{
			// Switch to alternate template with different styling
			collectionView.ItemTemplate = new DataTemplate(() =>
			{
				var border = new Border
				{
					BackgroundColor = Colors.LightBlue,
					Stroke = Colors.Blue,
					StrokeThickness = 2,
					Padding = new Thickness(10),
					Margin = new Thickness(5)
				};

				var stackLayout = new StackLayout { Orientation = StackOrientation.Horizontal };

				var label = new Label
				{
					VerticalOptions = LayoutOptions.Center
				};
				label.SetBinding(Label.TextProperty, "Text");
				label.SetBinding(Label.FontSizeProperty, "FontSize");
				label.SetBinding(Label.AutomationIdProperty, "AutomationId");

				var sizeLabel = new Label
				{
					Text = "SIZE",
					FontSize = 20,
					VerticalOptions = LayoutOptions.Center
				};

				stackLayout.Children.Add(sizeLabel);
				stackLayout.Children.Add(label);
				border.Content = stackLayout;

				return border;
			});
		}
		else
		{
			// Switch back to original simple template
			collectionView.ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, "Text");
				label.SetBinding(Label.FontSizeProperty, "FontSize");
				label.SetBinding(Label.AutomationIdProperty, "AutomationId");
				return label;
			});
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
