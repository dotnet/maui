namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34861, "[Android] CollectionView EmptyView not displayed correctly with GridItemsLayout span > 1", PlatformAffected.Android)]
public class Issue34861 : ContentPage
{
	public Issue34861()
	{
		var collectionView = new CollectionView
		{
			AutomationId = "TestCollectionView",
			ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical),
			Header = new Label
			{
				Text = "Header",
				AutomationId = "HeaderLabel",
				FontSize = 20,
				FontAttributes = FontAttributes.Bold,
				BackgroundColor = Colors.LightBlue,
				Padding = new Thickness(10)
			},
			EmptyView = new Label
			{
				Text = "No items available",
				AutomationId = "EmptyViewLabel",
				FontSize = 16,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				BackgroundColor = Colors.LightYellow,
				Padding = new Thickness(10)
			},
		};

		Content = new VerticalStackLayout
		{
			Children =
			{
				new Label
				{
					Text = "The EmptyView should appear below the Header, spanning the full width.",
					AutomationId = "InstructionLabel",
					Padding = new Thickness(10)
				},
				collectionView
			}
		};
	}
}
