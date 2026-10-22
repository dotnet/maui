using System;
using System.Collections.Generic;
using System.Linq;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 39534, "CollectionView dates truncate after existing item properties change with MeasureFirstItem", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue39534 : ContentPage
{
	private readonly Issue39534MainPageViewModel _viewModel;
	private bool _toggle;

	public Issue39534()
	{
		_viewModel = new Issue39534MainPageViewModel
		{
			Items = BuildItems()
		};

		BindingContext = _viewModel;

		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			}
		};

		var swapButton = new Button
		{
			Text = "Swap item properties",
			Margin = 10,
			AutomationId = "Issue39534SwapButton"
		};

		swapButton.Clicked += OnSwapClicked;
		grid.Add(swapButton, 0, 0);

		var collectionView = new CollectionView
		{
			AutomationId = "IssueCollectionView",
			ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem,
			ItemsSource = _viewModel.Items,
			ItemTemplate = new DataTemplate(() =>
			{
				var itemGrid = new Grid
				{
					Padding = 10,
					HeightRequest = 80,
					ColumnDefinitions =
					{
						new ColumnDefinition(GridLength.Star),
						new ColumnDefinition(new GridLength(115))
					}
				};

				var titleLabel = new Label
				{
					TextColor = Color.FromArgb("#17191C")
				};
				titleLabel.SetBinding(
					Label.TextProperty, nameof(IssueItem.Title));

				itemGrid.Add(titleLabel, 0, 0);

				var detailsLayout = new VerticalStackLayout
				{
					HorizontalOptions = LayoutOptions.End
				};

				detailsLayout.Children.Add(new Label
				{
					Text = "Last Issued",
					FontSize = 10,
					TextColor = Color.FromArgb("#4A4E55")
				});

				var dateLabel = new Label
				{
					AutomationId = "DateLabel",
					HorizontalTextAlignment = TextAlignment.End,
					TextColor = Color.FromArgb("#17191C")
				};
				dateLabel.SetBinding(
					Label.TextProperty, nameof(IssueItem.DateText));

				detailsLayout.Children.Add(dateLabel);
				itemGrid.Add(detailsLayout, 1, 0);

				return itemGrid;
			})
		};

		grid.Add(collectionView, 0, 1);

		Content = grid;
	}

	private void OnSwapClicked(object sender, EventArgs e)
	{
		_toggle = !_toggle;

		var newItems = _toggle
			? BuildItems().OrderBy(x =>
				DateTime.Parse(x.DateText)).ToList()
			: BuildItems().OrderByDescending(x =>
				DateTime.Parse(x.DateText)).ToList();

		for (int i = 0; i < newItems.Count; i++)
		{
			_viewModel.Items[i].Title = newItems[i].Title;
			_viewModel.Items[i].DateText = newItems[i].DateText;
		}
	}

	private static List<IssueItem> BuildItems() =>
	[
		new("Quarterly planning", "30-APR-2026"),
		new("Design review", "15-MAY-2026"),
		new("Release candidate", "28-MAY-2026"),
		new("Documentation", "05-JUN-2026"),
		new("Retrospective", "12-JUN-2026"),
		new("Backlog review", "19-JUN-2026"),
		new("Customer preview", "26-JUN-2026"),
		new("General availability", "03-JUL-2026")
	];
}

public sealed class Issue39534MainPageViewModel : BindableObject
{
	public List<IssueItem> Items { get; set; } = [];
}

public class IssueItem : BindableObject
{
	public IssueItem(string title, string dateText)
	{
		Title = title;
		DateText = dateText;
	}

	private string _title = string.Empty;

	public string Title
	{
		get => _title;
		set
		{
			if (_title == value)
				return;

			_title = value;
			OnPropertyChanged();
		}
	}

	private string _dateText = string.Empty;

	public string DateText
	{
		get => _dateText;
		set
		{
			if (_dateText == value)
				return;

			_dateText = value;
			OnPropertyChanged();
		}
	}
}
