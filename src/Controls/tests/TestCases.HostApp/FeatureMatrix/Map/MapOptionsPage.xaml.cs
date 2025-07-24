using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.Linq;
using System.ComponentModel;
using System;
namespace Maui.Controls.Sample;

public partial class MapOptionsPage : ContentPage
{
    private MapViewModel _viewModel;

    public MapOptionsPage(MapViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        // Set up a basic ItemTemplate for demonstration
        // Templates are now set via UI controls - SetItemTemplateButton_Clicked and SetTemplateSelectorButton_Clicked

        // Initialize radio buttons based on current MapType
        UpdateRadioButtonsFromMapType();

        // Subscribe to checkbox events
        IsShowingUserCheckBox.CheckedChanged += OnIsShowingUserCheckBoxChanged;

        // Subscribe to VisibleRegion property changes to update the UI
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        // Initialize the VisibleRegion display
        UpdateVisibleRegionDisplay();
    }

    private void UpdateRadioButtonsFromMapType()
    {
        switch (_viewModel.MapType)
        {
            case MapType.Street:
                StreetRadioButton.IsChecked = true;
                break;
            case MapType.Satellite:
                SatelliteRadioButton.IsChecked = true;
                break;
            case MapType.Hybrid:
                HybridRadioButton.IsChecked = true;
                break;
        }
    }

    private void MapTypeRadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value) // Only respond to checked events, not unchecked
        {
            if (sender == StreetRadioButton)
                _viewModel.MapType = MapType.Street;
            else if (sender == SatelliteRadioButton)
                _viewModel.MapType = MapType.Satellite;
            else if (sender == HybridRadioButton)
                _viewModel.MapType = MapType.Hybrid;
        }
    }

    private void OnIsShowingUserCheckBoxChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value)
        {
            // Add current location pin when IsShowingUser is checked
            AddCurrentLocationPin();
        }
        else
        {
            // Remove current location pin when IsShowingUser is unchecked
            RemoveCurrentLocationPin();
        }
    }

    private void AddCurrentLocationPin()
    {
        // Check if current location pin already exists
        var existingCurrentLocationPin = _viewModel.Pins.FirstOrDefault(p => p.Label == "Current Location");
        if (existingCurrentLocationPin == null)
        {
            var currentLocationPin = new Pin
            {
                Label = "Current Location",
                Address = "Pearl City, Hawaii (Current Location)",
                Type = PinType.Generic,
                Location = MapViewModel.PearlCityLocation
            };
            _viewModel.Pins.Add(currentLocationPin);
        }
    }

    private void RemoveCurrentLocationPin()
    {
        // Find and remove the current location pin
        var currentLocationPin = _viewModel.Pins.FirstOrDefault(p => p.Label == "Current Location");
        if (currentLocationPin != null)
        {
            _viewModel.Pins.Remove(currentLocationPin);
        }
    }

    private void ApplyButton_Clicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }

    private void AddPinButton_Clicked(object sender, EventArgs e)
    {
        // Count only numbered pins (Pin1, Pin2, etc.), exclude "Current Location" pin
        var numberedPinsCount = _viewModel.Pins.Count(p => p.Label != "Current Location");

        // Maximum of 10 numbered pins allowed
        if (numberedPinsCount >= 10)
        {
            return; // Don't add more than 10 numbered pins
        }

        // Predefined pin locations around Pearl City, Hawaii
        var pinLocations = new[]
        {
            new { Lat = 21.3933, Lng = -157.9751, Name = "Pin1" }, // Pin 1
			new { Lat = 21.3986, Lng = -158.0097, Name = "Pin2" }, // Pin 2
			new { Lat = 21.3649, Lng = -157.9634, Name = "Pin3" }, // Pin 3
			new { Lat = 21.4513, Lng = -158.0147, Name = "Pin4" }, // Pin 4
			new { Lat = 21.3847, Lng = -157.9261, Name = "Pin5" }, // Pin 5
			new { Lat = 21.4644, Lng = -158.0411, Name = "Pin6" }, // Pin 6
			new { Lat = 21.3408, Lng = -158.0061, Name = "Pin7" }, // Pin 7
			new { Lat = 21.3247, Lng = -157.9772, Name = "Pin8" }, // Pin 8
			new { Lat = 21.3142, Lng = -158.0397, Name = "Pin9" }, // Pin 9
			new { Lat = 21.3350, Lng = -158.0550, Name = "Pin10" } // Pin 10
		};

        // Get the next pin location based on numbered pins count
        var pinData = pinLocations[numberedPinsCount];

        var pin = new Pin
        {
            Label = pinData.Name,
            Address = $"{pinData.Name}, Hawaii",
            Type = PinType.Generic,
            Location = new Location(pinData.Lat, pinData.Lng)
        };
        _viewModel.Pins.Add(pin);
        _viewModel.UserAddedPinCount++;
    }

    private void AddElementButton_Clicked(object sender, EventArgs e)
    {
        // Maximum of 9 elements allowed
        if (_viewModel.MapElements.Count >= 9)
        {
            return; // Don't add more than 9 elements
        }

        // Use the same locations as pins for map elements, with different colors
        var destinations = new[]
        {
            new { Name = "Pin2", Location = new Location(21.3986, -158.0097), Color = Colors.Red },
            new { Name = "Pin3", Location = new Location(21.3649, -157.9634), Color = Colors.Green },
            new { Name = "Pin4", Location = new Location(21.4513, -158.0147), Color = Colors.Orange },
            new { Name = "Pin5", Location = new Location(21.3847, -157.9261), Color = Colors.Purple },
            new { Name = "Pin6", Location = new Location(21.4644, -158.0411), Color = Colors.Brown },
            new { Name = "Pin7", Location = new Location(21.3408, -158.0061), Color = Colors.Pink },
            new { Name = "Pin8", Location = new Location(21.3247, -157.9772), Color = Colors.Navy },
            new { Name = "Pin9", Location = new Location(21.3142, -158.0397), Color = Colors.Teal },
            new { Name = "Pin10", Location = new Location(21.3350, -158.0550), Color = Colors.Maroon }
        };

        // Get the destination based on current count (starts from 0, so first element goes to Pin2)
        var count = _viewModel.MapElements.Count;
        var destination = destinations[count];

        // Create a polyline from Pearl City to the destination
        var polyline = new Polyline
        {
            StrokeColor = destination.Color,
            StrokeWidth = 4
        };

        // Start from Pearl City
        polyline.Geopath.Add(MapViewModel.PearlCityLocation);

        // Add intermediate points for a more realistic route
        var startLat = MapViewModel.PearlCityLocation.Latitude;
        var startLng = MapViewModel.PearlCityLocation.Longitude;
        var endLat = destination.Location.Latitude;
        var endLng = destination.Location.Longitude;

        // Add 2-3 intermediate points to make the route more interesting
        var midLat1 = startLat + (endLat - startLat) * 0.33;
        var midLng1 = startLng + (endLng - startLng) * 0.33;
        polyline.Geopath.Add(new Location(midLat1, midLng1));

        var midLat2 = startLat + (endLat - startLat) * 0.67;
        var midLng2 = startLng + (endLng - startLng) * 0.67;
        polyline.Geopath.Add(new Location(midLat2, midLng2));

        // End at the destination
        polyline.Geopath.Add(destination.Location);

        _viewModel.MapElements.Add(polyline);
    }

    private void SetItemsSourceButton_Clicked(object sender, EventArgs e)
    {
        // Set the ItemsSource to the pins collection to enable data templating
        // This will cause the Map to use data binding instead of manual pin management
        _viewModel.ItemsSource = _viewModel.Pins;
    }

    private void ClearItemsSourceButton_Clicked(object sender, EventArgs e)
    {
        // Clear the ItemsSource to disable data templating
        // This will revert to manual pin management
        _viewModel.ItemsSource = null;
    }

    private void SetItemTemplateButton_Clicked(object sender, EventArgs e)
    {
        // Create a sample DataTemplate using a Grid to render pin info
        var pinTemplate = new DataTemplate(() =>
        {
            var grid = new Grid
            {
                Padding = new Thickness(8),
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            var label = new Label
            {
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black
            };
            label.SetBinding(Label.TextProperty, "Label");
            grid.Add(label, 0, 0);

            var address = new Label
            {
                FontSize = 12,
                TextColor = Colors.Gray
            };
            address.SetBinding(Label.TextProperty, "Address");
            grid.Add(address, 0, 1);

            var location = new Label
            {
                FontSize = 10,
                TextColor = Colors.DarkGray
            };
            location.SetBinding(Label.TextProperty, new Binding("Location", stringFormat: "Lat: {0.Latitude:F4}, Lng: {0.Longitude:F4}"));
            grid.Add(location, 1, 0);
            Grid.SetRowSpan(location, 2);

            return grid;
        });

        _viewModel.ItemTemplate = pinTemplate;

        // Note: ItemsSource must be set separately via "Set ItemsSource" button
        // Templates only work when ItemsSource is explicitly set by the user
    }

    private void ClearItemTemplateButton_Clicked(object sender, EventArgs e)
    {
        // Clear the ItemTemplate
        _viewModel.ItemTemplate = null;
    }

    private void SetTemplateSelectorButton_Clicked(object sender, EventArgs e)
    {
        // Create sample templates for the selector using Grid with red background
        var genericTemplate = new DataTemplate(() =>
        {
            var grid = new Grid
            {
                BackgroundColor = Colors.Red,
                Padding = new Thickness(8),
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            var label = new Label
            {
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                Text = "[Generic]"
            };
            label.SetBinding(Label.TextProperty, "Label");
            grid.Add(label, 0, 0);

            var address = new Label
            {
                FontSize = 12,
                TextColor = Colors.White
            };
            address.SetBinding(Label.TextProperty, "Address");
            grid.Add(address, 0, 1);

            var location = new Label
            {
                FontSize = 10,
                TextColor = Colors.White
            };
            location.SetBinding(Label.TextProperty, new Binding("Location", stringFormat: "Lat: {0.Latitude:F4}, Lng: {0.Longitude:F4}"));
            grid.Add(location, 1, 0);
            Grid.SetRowSpan(location, 2);

            return grid;
        });

        var placeTemplate = new DataTemplate(() =>
        {
            var grid = new Grid
            {
                BackgroundColor = Colors.DarkRed,
                Padding = new Thickness(8),
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            var label = new Label
            {
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Yellow,
                Text = "[Place]"
            };
            label.SetBinding(Label.TextProperty, "Label");
            grid.Add(label, 0, 0);

            var address = new Label
            {
                FontSize = 12,
                TextColor = Colors.Yellow
            };
            address.SetBinding(Label.TextProperty, "Address");
            grid.Add(address, 0, 1);

            var location = new Label
            {
                FontSize = 10,
                TextColor = Colors.Yellow
            };
            location.SetBinding(Label.TextProperty, new Binding("Location", stringFormat: "Lat: {0.Latitude:F4}, Lng: {0.Longitude:F4}"));
            grid.Add(location, 1, 0);
            Grid.SetRowSpan(location, 2);

            return grid;
        });

        // Setup ItemTemplateSelector
        var templateSelector = new SamplePinTemplateSelector
        {
            GenericTemplate = genericTemplate,
            PlaceTemplate = placeTemplate
        };

        _viewModel.ItemTemplateSelector = templateSelector;

        // Note: ItemsSource must be set separately via "Set ItemsSource" button
        // Template selector only works when ItemsSource is explicitly set by the user
    }

    private void ClearTemplateSelectorButton_Clicked(object sender, EventArgs e)
    {
        // Clear the ItemTemplateSelector
        _viewModel.ItemTemplateSelector = null;
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_viewModel.VisibleRegion))
        {
            UpdateVisibleRegionDisplay();
        }
    }

    private void UpdateVisibleRegionDisplay()
    {
        // RegionDetailsLabel is commented out in XAML, so no UI update needed
        // This method can be used for debugging or future implementation
        if (_viewModel.VisibleRegion != null)
        {
            var region = _viewModel.VisibleRegion;
            // Could log or store region info for debugging: 
            // $"Lat: {region.Center.Latitude:F4}, Lng: {region.Center.Longitude:F4}, LatDeg: {region.LatitudeDegrees:F4}, LngDeg: {region.LongitudeDegrees:F4}"
        }
    }

    private void ZoomInButton_Clicked(object sender, EventArgs e)
    {
        // Only allow zoom if IsZoomEnabled is checked
        if (!_viewModel.IsZoomEnabled)
        {
            return; // Don't zoom if zoom is disabled
        }

        // Zoom in by reducing the radius by half
        if (_viewModel.VisibleRegion != null)
        {
            var currentRegion = _viewModel.VisibleRegion;
            var newRadius = Distance.FromMeters(currentRegion.Radius.Meters / 2);
            _viewModel.VisibleRegion = MapSpan.FromCenterAndRadius(currentRegion.Center, newRadius);
        }
    }

    private void ZoomOutButton_Clicked(object sender, EventArgs e)
    {
        // Only allow zoom if IsZoomEnabled is checked
        if (!_viewModel.IsZoomEnabled)
        {
            return; // Don't zoom if zoom is disabled
        }

        // Zoom out by doubling the radius
        if (_viewModel.VisibleRegion != null)
        {
            var currentRegion = _viewModel.VisibleRegion;
            var newRadius = Distance.FromMeters(currentRegion.Radius.Meters * 2);
            _viewModel.VisibleRegion = MapSpan.FromCenterAndRadius(currentRegion.Center, newRadius);
        }
    }

    private void ShowAllPinsButton_Clicked(object sender, EventArgs e)
    {
        // Calculate a region that encompasses all pins
        if (_viewModel.Pins.Any())
        {
            var pins = _viewModel.Pins.ToList();

            // Find the bounds of all pins
            var minLat = pins.Min(p => p.Location.Latitude);
            var maxLat = pins.Max(p => p.Location.Latitude);
            var minLng = pins.Min(p => p.Location.Longitude);
            var maxLng = pins.Max(p => p.Location.Longitude);

            // Calculate center point
            var centerLat = (minLat + maxLat) / 2;
            var centerLng = (minLng + maxLng) / 2;
            var center = new Location(centerLat, centerLng);

            // Calculate the distance to encompass all pins with some padding
            var latDelta = Math.Abs(maxLat - minLat);
            var lngDelta = Math.Abs(maxLng - minLng);
            var maxDelta = Math.Max(latDelta, lngDelta);

            // Add 20% padding and ensure minimum radius
            var radiusKm = Math.Max(maxDelta * 111 * 0.6, 1); // 111 km per degree, 20% padding
            var radius = Distance.FromKilometers(radiusKm);

            _viewModel.VisibleRegion = MapSpan.FromCenterAndRadius(center, radius);
        }
        else
        {
            // If no pins, just reset to initial state
            ResetToInitialButton_Clicked(sender, e);
        }
    }

    private void ResetToInitialButton_Clicked(object sender, EventArgs e)
    {
        // Reset all properties to their original state

        // Reset boolean properties to their default values
        _viewModel.IsShowingUser = false;
        _viewModel.IsScrollEnabled = false;
        _viewModel.IsTrafficEnabled = false;
        _viewModel.IsZoomEnabled = false;
        _viewModel.IsVisible = true;

        // Reset MapType to default
        _viewModel.MapType = MapType.Street;

        // Clear all pins and reset user pin count (from ClearPinsButton functionality)
        _viewModel.Pins.Clear();
        _viewModel.UserAddedPinCount = 0;

        // Clear all map elements (from ClearElementsButton functionality)
        _viewModel.MapElements.Clear();

        // Reset template properties
        _viewModel.ItemsSource = null;
        _viewModel.ItemTemplate = null;
        _viewModel.ItemTemplateSelector = null;

        // Reset visible region to initial region
        _viewModel.VisibleRegion = _viewModel.InitialRegion;

        // Update radio buttons to reflect the reset MapType
        UpdateRadioButtonsFromMapType();
    }
}