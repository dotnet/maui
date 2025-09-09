using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31535, "[iOS] Crash occured on CarouselView2 when deleting last one remaining item with loop as false", PlatformAffected.iOS)]
public class Issue31535 : ContentPage
{
    ObservableCollection<string> _items = new();
    public ObservableCollection<string> Items
    {
        get => _items;
        set
        {
            _items = value;
            OnPropertyChanged();
        }
    }
    public Issue31535()
    {
        // Initialize items
        Items = new ObservableCollection<string>
        {
            "Item 1",
            "Item 2",
        };

        // Create CarouselView
        var carousel = new CarouselView2
        {
            Loop = false,
            HeightRequest = 200,
            ItemsSource = Items,
            AutomationId = "TestCarouselView",
            ItemTemplate = new DataTemplate(() =>
            {
                var border = new Border
                {
                    Margin = 10,
                    WidthRequest = 200,
                    BackgroundColor = Colors.Red
                };

                var label = new Label();
                label.SetBinding(Label.TextProperty, ".");

                border.Content = label;
                return border;
            })
        };

        // Create Button
        var button = new Button
        {
            AutomationId = "RemoveLastItemButton",
            Text = "Remove last item"
        };

        button.Clicked += (s, e) =>
        {
            if (Items.Count > 0)
                Items.RemoveAt(Items.Count - 1);
        };

        Content = new VerticalStackLayout
        {
            Children =
            {
                carousel,
                button
            }
        };
        BindingContext = this;
    }
}