using System.Collections.ObjectModel;

namespace Maui.Controls.Sample;

/// <summary>
/// Custom selection page to replace DisplayActionSheet for better automation support on Mac/Catalyst
/// </summary>
public class LocationSelectionPage : ContentPage
{
    public LocationSelectionPage(string title, List<string> locations, Action<string> onSelected)
    {
        Title = title;

        var collectionView = new CollectionView
        {
            AutomationId = "LocationSelectionCollectionView",
            SelectionMode = SelectionMode.Single,
            ItemsSource = locations,
            ItemTemplate = new DataTemplate(() =>
            {
                var label = new Label
                {
                    Padding = new Thickness(20, 15),
                    FontSize = 14,
                    VerticalTextAlignment = TextAlignment.Center
                };
                label.SetBinding(Label.TextProperty, ".");
                label.SetBinding(AutomationIdProperty, new Binding(".", BindingMode.OneWay, new LocationToAutomationIdConverter()));

                var frame = new Frame
                {
                    Padding = 0,
                    HasShadow = false,
                    BorderColor = Colors.LightGray,
                    Content = label
                };

                return frame;
            })
        };

        collectionView.SelectionChanged += async (s, e) =>
        {
            if (e.CurrentSelection.FirstOrDefault() is string selectedLocation)
            {
                onSelected?.Invoke(selectedLocation);
                await Navigation.PopModalAsync();
            }
        };

        var cancelButton = new Button
        {
            Text = "Cancel",
            AutomationId = "CancelSelectionButton",
            Margin = new Thickness(10)
        };

        cancelButton.Clicked += async (s, e) =>
        {
            await Navigation.PopModalAsync();
        };

        Content = new VerticalStackLayout
        {
            Children =
            {
                collectionView,
                cancelButton
            }
        };
    }

    private class LocationToAutomationIdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string location)
            {
                // Create a valid AutomationId from the location name
                // Remove special characters and spaces
                return "Location_" + new string(location.Where(c => char.IsLetterOrDigit(c)).ToArray());
            }
            return "Location_Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
