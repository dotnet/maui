using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31465, "CollectionView with null ItemsSource and Header can be dragged down creating extra space", PlatformAffected.iOS)]
public class Issue31465 : TestContentPage
{
	public string Text { get; set; }

	protected override void Init()
	{
		Title = "Header and footer (Null)";

		var grid = new Grid
		{
			Margin = new Thickness(20),
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		var instructionsStack = new StackLayout();
		var instructionsLabel = new Label
		{
			Text = "The test passes if the header and emptyview content displayed correctly and are not overlapped when itemsource is null."
		};
		instructionsStack.Children.Add(instructionsLabel);
		grid.Children.Add(instructionsStack);
		Grid.SetRow(instructionsStack, 0);

		var collectionView = new CollectionView2
		{
			AutomationId = "TestCollectionView"
		};

		collectionView.SetBinding(CollectionView2.ItemsSourceProperty, "Text");

		collectionView.Header = new Label
		{
			Text = "Header: This should show for all cases.",
			TextColor = Color.FromArgb("#512BD4"),
			AutomationId = "HeaderLabel"
		};

		collectionView.EmptyView = new Label
		{
			Text = "EmptyView: This should show when no data.",
			TextColor = Color.FromArgb("#512BD4"),
			AutomationId = "EmptyViewLabel"
		};

		collectionView.ItemTemplate = new DataTemplate(() =>
		{
			var label = new Label();
			label.SetBinding(Label.TextProperty, ".");
			return label;
		});

		grid.Children.Add(collectionView);
		Grid.SetRow(collectionView, 1);

		Content = grid;

		// Set BindingContext to null to reproduce the original issue
		BindingContext = null;
	}
}