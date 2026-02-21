using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 22120, "CollectionView.Header is not scrollable in Android platform",
        PlatformAffected.Android)]
    public class Issue22120 : ContentPage
    {
        public Issue22120()
        {
            Title = "Issue 22120";

            // Create header items for the ListView inside ScrollView
            var headerItems = new List<string>();
            for (int i = 1; i <= 15; i++)
            {
                headerItems.Add($"Header Item {i}");
            }

            // Create collection items
            var collectionItems = new List<string>
            {
                "Pink", "Green", "Blue", "Yellow", "Orange", "Purple", "SkyBlue", "PaleGreen"
            };

            // Create the ListView for the header content with ItemTemplate
            var headerListView = new ListView
            {
                AutomationId = "Issue22120HeaderListView",
                HorizontalOptions = LayoutOptions.Center,
                ItemsSource = headerItems,
                HeightRequest = 400,
                ItemTemplate = new DataTemplate(() =>
                {
                    var label = new Label
                    {
                        Padding = new Thickness(10),
                        BackgroundColor = Colors.LightBlue
                    };
                    label.SetBinding(Label.TextProperty, ".");

                    return new ViewCell
                    {
                        View = label
                    };
                })
            };

            // Create the ScrollView for the header containing the ListView
            var headerScrollView = new ScrollView
            {
                AutomationId = "Issue22120HeaderScrollView",
                HeightRequest = 200,
                Content = headerListView
            };

            // Create the main CollectionView
            var collectionView = new CollectionView
            {
                AutomationId = "Issue22120CollectionView",
                Margin = new Thickness(20),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                ItemsLayout = new GridItemsLayout(3, ItemsLayoutOrientation.Vertical),
                Header = headerScrollView,
                ItemsSource = collectionItems,
                ItemTemplate = new DataTemplate(() =>
                {
                    var button = new Button
                    {
                        Margin = new Thickness(5),
                        Padding = new Thickness(0),
                        HeightRequest = 60,
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Center
                    };
                    button.SetBinding(Button.TextProperty, ".");
                    return button;
                })
            };

            Content = collectionView;
        }
    }
}