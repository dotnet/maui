namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23708, "Grouped CollectionView has extra space before group headers", PlatformAffected.UWP)]
public class Issue23708 : ContentPage
{
	public Issue23708()
	{
		var collectionView = new CollectionView
		{
			AutomationId = "GroupedCollectionView",
			IsGrouped = true,
			Margin = new Thickness(10, 10, 10, 0),
			ItemsSource = new[]
			{
				new GroupedItems("Small", ["Rat", "Mouse", "Bird", "Fish", "Carrot"]),
				new GroupedItems("Big", ["Cat", "Dog", "Rabbit", "Car", "Jet"]),
				new GroupedItems("Awesome", ["Code", "School", "Not doing drugs"]),
			},
			ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical)
			{
				VerticalItemSpacing = 10,
			},
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(AutomationIdProperty, new Binding(".", stringFormat: "Item{0}"));
				return label;
			}),
			GroupHeaderTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					FontSize = 32,
					TextColor = Colors.Blue,
				};
				label.SetBinding(Label.TextProperty, nameof(GroupedItems.Name));
				label.SetBinding(AutomationIdProperty, new Binding(nameof(GroupedItems.Name), stringFormat: "GroupHeader{0}"));
				return label;
			}),
		};

		Content = collectionView;
	}

	sealed class GroupedItems(string name, IEnumerable<string> children) : List<string>(children)
	{
		public string Name { get; } = name;
	}
}
