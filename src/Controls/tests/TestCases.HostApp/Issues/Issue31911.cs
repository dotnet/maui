namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31911, "CollectionView Header Footer not removed when set to null on Android", PlatformAffected.Android)]
public class Issue31911 : ContentPage
{
    public Issue31911()
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
        CollectionView collectionView = new CollectionView
        {
            AutomationId = "CollectionView",
            ItemsSource = System.Array.Empty<string>(),
        };
        collectionView.EmptyView = new Label
        {
            BackgroundColor = Color.FromArgb("#FFE40606"),
            Text = "EmptyView: This should show when no data.",
        };

        Label headerContent = new Label
        {
            BackgroundColor = Colors.LightBlue,
            Text = "Header: This is the header content.",
        };
        collectionView.Header = headerContent;

        Label footerContent = new Label
        {
            BackgroundColor = Colors.LightCoral,
            Text = "Footer: This is the footer content.",
        };
        collectionView.Footer = footerContent;

        var buttonGrid = new Grid
        {
            ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star }
                },
            Margin = new Thickness(0, 10)
        };

        var toggleHeaderButton = new Button
        {
            AutomationId = "ToggleHeaderButton",
            Text = "Remove Header",
        };
        toggleHeaderButton.Clicked += (s, e) =>
        {
            collectionView.Header = null;
            toggleHeaderButton.Text = "Add Header";
        };
        Grid.SetColumn(toggleHeaderButton, 0);
        buttonGrid.Children.Add(toggleHeaderButton);

        var toggleFooterButton = new Button
        {
            AutomationId = "ToggleFooterButton",
            Text = "Remove Footer",
        };
        toggleFooterButton.Clicked += (s, e) =>
        {
            collectionView.Footer = null;
            toggleFooterButton.Text = "Add Footer";
        };
        Grid.SetColumn(toggleFooterButton, 1);
        buttonGrid.Children.Add(toggleFooterButton);

        Grid.SetRow(buttonGrid, 0);
        grid.Children.Add(buttonGrid);
        Grid.SetRow(collectionView, 1);
        grid.Children.Add(collectionView);
        Content = grid;
    }
}