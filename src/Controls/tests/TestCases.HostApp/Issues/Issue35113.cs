using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35113, "CV2 header/footer view width is not expanded to its content width on iOS/macOS", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue35113 : ContentPage
{
	public Issue35113()
	{
		var items = new ObservableCollection<string>();
		for (int i = 0; i < 2; i++)
			items.Add($"Item {i}");

		// Header label with no explicit WidthRequest so it must self-size to its content width.
		// With the bug: ScrollDirection defaults to Vertical on the supplementary TemplatedCell2.
		// GetMeasureConstraints() constrains width to preferredAttributes.Size.Width (~30pt from
		// LayoutFactory2's estimated value). The label text is clipped/invisible at 30pt and
		// is absent from the accessibility tree — WaitForElement times out.
		// With the fix: ScrollDirection = Horizontal is set, width is unconstrained, the label
		// expands to its full content width and becomes accessible in the UI tree.
		Content = new CollectionView2
		{
			ItemsLayout = new GridItemsLayout(3, ItemsLayoutOrientation.Horizontal),
			Header = new Label
			{
				Text = "This Is A Header",
				AutomationId = "Issue35113Header",
				BackgroundColor = Colors.LightBlue,
				FontSize = 24,
				Padding = new Thickness(8),
			},
			Footer = new Label
			{
				Text = "This Is A Footer",
				AutomationId = "Issue35113Footer",
				BackgroundColor = Colors.LightGreen,
				FontSize = 24,
				Padding = new Thickness(8),
			},
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label { Padding = 8 };
				label.SetBinding(Label.TextProperty, ".");
				return new Border { Content = label, Padding = 4 };
			}),
			ItemsSource = items,
			AutomationId = "Issue35113CollectionView",
		};
	}
}
