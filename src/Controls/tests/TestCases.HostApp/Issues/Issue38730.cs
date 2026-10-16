using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38730, "Vertical Item Spacing page rendering issue on Mac 27", PlatformAffected.macOS)]
public class Issue38730 : NavigationPage
{
	public Issue38730()
		: base(new Issue38730Page())
	{
	}
}

public class Issue38730Page : ContentPage
{
	public Issue38730Page()
	{
		var button = new Button
		{
			Text = "Open Vertical Item Spacing Page",
			AutomationId = "NavigateButton"
		};

		button.Clicked += OnButtonClicked;

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Children =
			{
				button
			}
		};
	}

	async void OnButtonClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new Issue38730VerticalPage());
	}
}

public class Issue38730VerticalPage : ContentPage
{
	public Issue38730VerticalPage()
	{
		var instructions = new Label
		{
			AutomationId = "TestInstructions",
			Text = "1. The test passes if the grouped CollectionView has no extra space on iOS and Mac Catalyst."
		};

		var instructionLayout = new VerticalStackLayout
		{
			Children =
			{
				instructions
			}
		};

		var collectionView = new CollectionView
		{
			AutomationId = "GroupedCollectionView",
			ItemsSource = new Items[]
			{
				new Items
				{
					Name = "Small",
					Children = { "Rat", "Mouse", "Bird", "Fish", "Carrot" }
				},
				new Items
				{
					Name = "Big",
					Children = { "Cat", "Dog", "Rabbit", "Car", "Jet" }
				},
				new Items
				{
					Name = "Awesome",
					Children = { "Code", "School", "Not doing drugs" }
				}
			},
			Margin = new Thickness(10, 80, 10, 0),
			SelectionMode = SelectionMode.None,
			IsGrouped = true
		};

		collectionView.ItemsLayout = new GridItemsLayout(
			ItemsLayoutOrientation.Vertical)
		{
			Span = DeviceInfo.Idiom == DeviceIdiom.Phone ? 2 : 3,
			VerticalItemSpacing = 10
		};

		collectionView.ItemTemplate = new DataTemplate(() =>
		{
			var label = new Label();

			label.SetBinding(
				Label.TextProperty,
				".");

			return label;
		});

		collectionView.GroupHeaderTemplate = new DataTemplate(() =>
		{
			var label = new Label
			{
				FontSize = 32,
				TextColor = Colors.Blue
			};

			label.SetBinding(
				Label.TextProperty,
				"Name");

			return label;
		});

		var grid = new Grid
		{
			Margin = 20,
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			}
		};

		grid.Add(instructionLayout, 0, 0);
		grid.Add(collectionView, 0, 0);

		Content = grid;
	}
}

public class Items : List<string>
{
	public string Name { get; set; } = string.Empty;

	public List<string> Children => this;
}