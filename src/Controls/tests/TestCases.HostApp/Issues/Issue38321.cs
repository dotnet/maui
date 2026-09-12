using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38321, "Grouped CollectionView with GridItemsLayout throws ArgumentOutOfRangeException after an item is removed", PlatformAffected.Android)]
public class Issue38321 : ContentPage
{
	readonly Label _statusLabel;
	int _removalStep;

	public ObservableCollection<Issue38321ItemGroup> Groups { get; } = new ObservableCollection<Issue38321ItemGroup>();

	public Issue38321()
	{
		Title = "Issue 38321";

		for (var groupIndex = 1; groupIndex <= 4; groupIndex++)
		{
			var items = new List<Issue38321SampleItem>();

			for (var itemIndex = 1; itemIndex <= 8; itemIndex++)
			{
				items.Add(new Issue38321SampleItem($"Item {groupIndex}.{itemIndex}"));
			}

			Groups.Add(new Issue38321ItemGroup($"Group {groupIndex}", items));
		}

		var titleLabel = new Label
		{
			Text = "Grouped grid removal",
			FontAttributes = FontAttributes.Bold,
			FontSize = 20
		};

		var descriptionLabel = new Label
		{
			Text = "Remove the first visible item on Android",
			FontSize = 13
		};

		var headerLayout = new VerticalStackLayout
		{
			Spacing = 2,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				titleLabel,
				descriptionLabel
			}
		};

		var removeButton = new Button
		{
			AutomationId = "RemoveItemButton",
			Text = "Remove item"
		};

		_statusLabel = new Label
		{
			AutomationId = "ItemsRemainingLabel",
			Text = "Items remaining: 8"
		};

		removeButton.Clicked += OnRemoveItemClicked;

		var headerGrid = new Grid
		{
			ColumnDefinitions =
			{
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Auto)
			},
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto)
			},
			ColumnSpacing = 12
		};

		headerGrid.Add(headerLayout);
		headerGrid.Add(removeButton, 1, 0);
		headerGrid.Add(_statusLabel, 0, 1);

		var collectionView = new CollectionView
		{
			AutomationId = "GroupedCollectionView",
			IsGrouped = true,
			ItemsSource = Groups,
			ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical)
			{
				HorizontalItemSpacing = 4,
				VerticalItemSpacing = 4
			}
		};

		collectionView.GroupHeaderTemplate = new DataTemplate(() =>
		{
			var label = new Label
			{
				Padding = 8,
				Background = Color.FromArgb("#E7EEF8"),
				FontAttributes = FontAttributes.Bold,
				HeightRequest = 40,
				TextColor = Color.FromArgb("#172033")
			};

			label.SetBinding(Label.TextProperty, "Name");

			return label;
		});

		collectionView.GroupFooterTemplate = new DataTemplate(() =>
		{
			var label = new Label
			{
				Padding = 8,
				Background = Color.FromArgb("#F1F3F6"),
				HeightRequest = 40,
				TextColor = Color.FromArgb("#303846")
			};

			label.SetBinding(
				Label.TextProperty,
				"Count",
				stringFormat: "Items remaining: {0}");

			return label;
		});

		collectionView.ItemTemplate = new DataTemplate(() =>
		{
			var label = new Label
			{
				TextColor = Color.FromArgb("#172033")
			};

			label.SetBinding(Label.TextProperty, "Name");

			return new Border
			{
				Margin = 4,
				Padding = 12,
				Background = Colors.White,
				Stroke = Color.FromArgb("#8792A5"),
				StrokeShape = new RoundRectangle
				{
					CornerRadius = 6
				},
				Content = label
			};
		});

		var grid = new Grid
		{
			Padding = 16,
			RowSpacing = 12,
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			}
		};

		grid.Add(headerGrid);
		grid.Add(collectionView, 0, 1);

		Content = grid;
	}

	async void OnRemoveItemClicked(object sender, EventArgs e)
	{
		await Task.Delay(250);

		if (Groups.Count == 0 || Groups[0].Count == 0)
			return;

		var group = Groups[0];
		var (positionName, index) = _removalStep switch
		{
			0 => ("first", 0),
			1 => ("middle", group.Count / 2),
			_ => ("last", group.Count - 1)
		};

		group.RemoveAt(index);
		_removalStep++;
		_statusLabel.Text = $"Removed {positionName}; items remaining: {group.Count}";
	}
}

public sealed record Issue38321SampleItem(string Name);

public sealed class Issue38321ItemGroup(
	string name,
	IEnumerable<Issue38321SampleItem> items)
	: ObservableCollection<Issue38321SampleItem>(items)
{
	public string Name { get; } = name;
}