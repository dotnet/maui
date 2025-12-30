using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30868, "CollectionView selection visual states", PlatformAffected.UWP)]
public class Issue30868 : ContentPage
{
	public ObservableCollection<string> Items { get; set; }

	public Issue30868()
	{
		Items = new ObservableCollection<string>
		{
			"Item 1",
			"Item 2",
			"Item 3"
		};

		var collectionView = new CollectionView
		{
			ItemsSource = Items,
			SelectionMode = SelectionMode.Multiple,
			HeightRequest = 40,
			AutomationId = "collectionViewSelectionMode",
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
			{
				ItemSpacing = 4
			},
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					FontSize = 16,
					VerticalOptions = LayoutOptions.Center,
					VerticalTextAlignment = TextAlignment.Center,
					Padding = new Thickness(10, 0, 10, 10),
					TextColor = Colors.Purple
				};
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};

		Content = collectionView;
	}
}