using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29411, "[Android] CarouselView.Loop = false causes crash on Android when changed at runtime", PlatformAffected.Android)]
public partial class Issue29411 : ContentPage
{
	ObservableCollection<string> _items;

    public Issue29411()
    {
        InitializeComponent();

        _items = new ObservableCollection<string>
        {
            "Item 1",
            "Item 2",
            "Item 3",
            "Item 4",
            "Item 5",
        };

        carouselView.ItemsSource = _items;

        carouselView.ItemTemplate = new DataTemplate(() =>
        {
            var label = new Label
            {
                FontSize = 18,
                Margin = new Thickness(10),
                BackgroundColor = Colors.LightGray
            };
            label.SetBinding(Label.TextProperty, "."); // Proper binding

            return label;
        });
    }

    void OnDisabledButtonClicked(object sender, EventArgs e)
    {
        carouselView.Loop = !carouselView.Loop;
    }

    void OnPositionChangeClicked(object sender, EventArgs e)
    {
        carouselView.Position = 2;
    }
}