using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31465, "The page can be dragged down, and it would cause an extra space between Header and EmptyView text.", PlatformAffected.iOS)]
public class Issue31465 : ContentPage
{
	public Issue31465()
	{
		var grid = new Grid
        {
            Margin = new Thickness(20),
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            }
        };

        // Header Label
        var headerLabel = new Label
		{
			Text = "Test for CollectionView empty view positioning",
			AutomationId = "HeaderLabel",
        };
        Grid.SetRow(headerLabel, 0);
        grid.Children.Add(headerLabel);

        // CollectionView
        var collectionView = new CollectionView
        {
			AutomationId = "CollectionView",
            ItemsSource = Array.Empty<string>(), // empty array to trigger EmptyView
            ItemTemplate = new DataTemplate(() =>
            {
                return new Label
                {
                    TextColor = Colors.Black,
                    FontSize = 16,
                    // This binding works with string items
                    BindingContext = "{Binding .}" 
                };
            })
        };

        // EmptyView
        collectionView.EmptyView = new Label
        {
            BackgroundColor = Color.FromArgb("#FFE40606"),
            Text = "EmptyView: This should show when no data.",
            TextColor = Color.FromArgb("#512BD4")
        };

        collectionView.Header = new Button
        {
            AutomationId = "CollectionViewHeader",
            BackgroundColor = Colors.LightBlue,
            Text = "Click me to verify the EmptyView position",
            TextColor = Color.FromArgb("#512BD4")
        };

        Grid.SetRow(collectionView, 1);
        grid.Children.Add(collectionView);

        // Set ContentPage content
        Content = grid;
	}
}