using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32435, "Rotating the Simulator causes the text on the collection view to disappear", PlatformAffected.iOS)]

public class Issue32435 : ContentPage
{
    readonly ObservableCollection<string> items = new();
    readonly CollectionView2 collectionView;

    public Issue32435()
    {
        var rootGrid = new Grid
        {
            Margin = 20,
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            }
        };

        var topStack = new StackLayout();
        topStack.Add(new Label { Text = "CollectionView text should appear after rotating the device", AutomationId = "InstructionLabel" });
        rootGrid.Add(topStack);
        Grid.SetRow(topStack, 0);

        var addButton = new Button
        {
            Text = "Add",
			AutomationId = "AddButton",
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start
        };
        addButton.Clicked += ButtonAdd_Clicked;
        rootGrid.Add(addButton);
        Grid.SetRow(addButton, 1);

        var innerGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            }
        };

        collectionView = new CollectionView2
        {
            BackgroundColor = Colors.LightSalmon,
            HeightRequest = 50,
            ItemsLayout = new GridItemsLayout(ItemsLayoutOrientation.Horizontal)
        };

        innerGrid.Add(collectionView);
        Grid.SetRow(collectionView, 0);

        rootGrid.Add(innerGrid);
        Grid.SetRow(innerGrid, 2);

        items.Add("item: " + items.Count);
        collectionView.ItemsSource = items;

        Content = rootGrid;
    }

    void ButtonAdd_Clicked(object sender, System.EventArgs e)
    {
        items.Add("item: " + items.Count);
    }
}
