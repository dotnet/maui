using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35889, "Empty CollectionView (CV2) expands to fill available space on iOS instead of collapsing to zero height", PlatformAffected.iOS)]
public class Issue35889 : ContentPage
{
	readonly ObservableCollection<string> _items = new ObservableCollection<string>();

	public Issue35889()
	{
		Label statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			Text = "Empty"
		};

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(16),
			Spacing = 0,
			Children =
			{
				new Label
				{
					Text = "Before CV",
					AutomationId = "BeforeCVLabel"
				},
				new CollectionView
				{
					AutomationId = "CollectionView35889",
					VerticalOptions = LayoutOptions.Start,
					BackgroundColor = Colors.LightGray,
					ItemsSource = _items,
					ItemTemplate = new DataTemplate(() =>
					{
						var label = new Label { Padding = new Thickness(8) };
						label.SetBinding(Label.TextProperty, static (string s) => s);
						return label;
					})
				},
				new Label
				{
					Text = "After CV",
					AutomationId = "AfterCVLabel"
				},
				statusLabel,
				new Button
				{
					Text = "Add Items",
					AutomationId = "AddItemsButton",
					Command = new Command(() =>
					{
						_items.Add("Item 1");
						_items.Add("Item 2");
						_items.Add("Item 3");
						statusLabel.Text = "HasItems";
					})
				}
			}
		};
	}
}
