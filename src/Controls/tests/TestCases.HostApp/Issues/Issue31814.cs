using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31814, "CollectionView RemainingItemsThreshold incorrectly including Header and Footer in counts", PlatformAffected.Android)]
public class Issue31814 : ContentPage
{
    public ObservableCollection<int> Items { get; set; } = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
    Label label;

    public Issue31814()
    {
        BindingContext = this;
        label = new Label
        {
            Text = "Initial Label",
            AutomationId = "Issue31814Label"
        };

        CollectionView collectionView = new CollectionView
        {
            ItemsSource = Items,
            AutomationId = "Issue31814CollectionView",
            RemainingItemsThreshold = 0,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
            {
                ItemSpacing = 4
            },
            Header = new BoxView
            {
                BackgroundColor = Colors.Green,
                HeightRequest = 120
            },
            Footer = new BoxView
            {
                BackgroundColor = Colors.Red,
                HeightRequest = 120
            },
            ItemTemplate = new DataTemplate(() =>
            {
                return new BoxView
                {
                    HeightRequest = 120,
                    BackgroundColor = Colors.Yellow
                };
            })
        };

        collectionView.RemainingItemsThresholdReached += CollectionView_RemainingItemsThresholdReached;
        Grid grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
        grid.Add(label, 0, 0);
        grid.Add(collectionView, 0, 1);
        Content = grid;
    }

    void CollectionView_RemainingItemsThresholdReached(object sender, EventArgs e)
    {
        Items.Add(0);
        label.Text = "RemainingItemsThresholdReached fired";
    }
}
