#if MAUI_UI_EVIDENCE
namespace Maui.Controls.Sample;

sealed class UiEvidenceCollectionViewPage : ContentPage
{
	public UiEvidenceCollectionViewPage()
	{
		Title = "UI Evidence - CollectionView";
		BackgroundColor = Color.FromArgb("#F5F7FA");

		var items = Enumerable.Range(1, 24)
			.Select(index => new UiEvidenceItem(index, $"Item {index:00}", $"Stable detail {index:00}"))
			.ToArray();

		var collection = new CollectionView
		{
			AutomationId = "UiEvidenceCollection",
			ItemsSource = items,
			SelectionMode = SelectionMode.None,
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
			{
				ItemSpacing = 4
			},
			ItemTemplate = new DataTemplate(() =>
			{
				var title = new Label
				{
					FontSize = 17,
					FontAttributes = FontAttributes.Bold,
					TextColor = Color.FromArgb("#172B4D")
				};
				title.SetBinding(Label.TextProperty, nameof(UiEvidenceItem.Title));

				var detail = new Label
				{
					FontSize = 13,
					TextColor = Color.FromArgb("#42526E")
				};
				detail.SetBinding(Label.TextProperty, nameof(UiEvidenceItem.Detail));

				return new Border
				{
					Margin = new Thickness(0, 2),
					Padding = new Thickness(14, 10),
					HeightRequest = 64,
					BackgroundColor = Colors.White,
					Stroke = Color.FromArgb("#DFE1E6"),
					StrokeThickness = 1,
					Content = new VerticalStackLayout
					{
						Spacing = 2,
						Children = { title, detail }
					}
				};
			})
		};

		var ready = new Label
		{
			Text = "Ready",
			AutomationId = "UiEvidenceReady",
			FontSize = 12,
			TextColor = Color.FromArgb("#006644"),
			HorizontalTextAlignment = TextAlignment.End
		};

		var root = new Grid
		{
			Padding = new Thickness(20),
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star),
				new RowDefinition(GridLength.Auto)
			},
			RowSpacing = 12,
			Children =
			{
				new Label
				{
					Text = "CollectionView evidence",
					AutomationId = "UiEvidenceCollectionHeader",
					FontSize = 24,
					FontAttributes = FontAttributes.Bold,
					TextColor = Color.FromArgb("#172B4D")
				},
				collection,
				ready
			}
		};

		Grid.SetRow(collection, 1);
		Grid.SetRow(ready, 2);
		Content = root;
	}

	sealed record UiEvidenceItem(int Id, string Title, string Detail);
}
#endif
