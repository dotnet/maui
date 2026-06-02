using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34247, "CollectionView with HeaderTemplate and SelectionMode.Single crashes on selection", PlatformAffected.Android)]
public class Issue34247 : ContentPage
{
	public Issue34247()
	{
		var layout = new StackLayout();

		var resultLabel = new Label()
		{
			Text = "Select an item to test",
			AutomationId = "ResultLabel"
		};

		var items = new ObservableCollection<string> { "Item 1", "Item 2", "Item 3" };

		var collectionView = new CollectionView()
		{
			SelectionMode = SelectionMode.Single,
			AutomationId = "TestCollectionView",
			HeightRequest = 300
		};

		collectionView.HeaderTemplate = new DataTemplate(() =>
		{
			return new Label
			{
				Text = "Header",
				FontSize = 18,
				FontAttributes = FontAttributes.Bold,
				Margin = new Thickness(10)
			};
		});

		collectionView.FooterTemplate = new DataTemplate(() =>
		{
			return new Label
			{
				Text = "Footer",
				FontSize = 18,
				FontAttributes = FontAttributes.Bold,
				Margin = new Thickness(10)
			};
		});

		collectionView.ItemTemplate = new DataTemplate(() =>
		{
			var label = new Label();
			label.SetBinding(Label.TextProperty, ".");
			label.Margin = new Thickness(10);
			label.FontSize = 16;
			return label;
		});

		collectionView.ItemsSource = items;
		collectionView.SelectionChanged += (s, e) =>
		{
			resultLabel.Text = "Success";
		};

		layout.Children.Add(resultLabel);
		layout.Children.Add(collectionView);

		Content = layout;
	}
}
