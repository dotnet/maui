using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.ComponentModel;
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

             var placeInfo = new PlaceInfo
            {
                Name = "Current Location",
                Description = "Pearl City, Hawaii (Current Location)",
                Location = MapViewModel.PearlCityLocation,
                Type = PlaceType.Tourist
            };

            _viewModel.Pins.Add(currentLocationPin);
            _viewModel.Places.Add(placeInfo); 
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
            // Pin 1: Pearl City, Hawaii
            new { Lat = 21.3933, Lng = -157.9751, Name = "Pearl City", Description = "Suburb, shopping, residential, schools, parks" , Type = PlaceType.Tourist },
            // Pin 2: Waipahu, Hawaii
            new { Lat = 21.3986, Lng = -158.0097, Name = "Waipahu", Description = "Historic sugar town, diverse community" , Type = PlaceType.Restaurant },
            // Pin 3: Aiea, Hawaii
            new { Lat = 21.3649, Lng = -157.9634, Name = "Aiea", Description = "Residential, Pearl Harbor views, mall" , Type = PlaceType.Restaurant },
            // Pin 4: Waikele, Hawaii
            new { Lat = 21.4513, Lng = -158.0147, Name = "Waikele", Description = "Outlet shopping, golf, residential area" , Type = PlaceType.Tourist },
            // Pin 5: Halawa, Hawaii
            new { Lat = 21.3847, Lng = -157.9261, Name = "Halawa", Description = "Aloha Stadium, neighborhoods, valley, events", Type = PlaceType.Restaurant },
            // Pin 6: Mililani, Hawaii
            new { Lat = 21.4644, Lng = -158.0411, Name = "Mililani", Description = "Planned community, schools, parks, shopping" , Type = PlaceType.Tourist },
            // Pin 7: Ewa Beach, Hawaii
            new { Lat = 21.3408, Lng = -158.0061, Name = "Ewa Beach", Description = "Coastal, golf, growing, residential, beaches" , Type = PlaceType.Restaurant },
            // Pin 8: Waimalu, Hawaii
            new { Lat = 21.3247, Lng = -157.9772, Name = "Waimalu", Description = "Small community, shopping, residential, eateries", Type = PlaceType.Tourist },
            // Pin 9: Kapolei, Hawaii
            new { Lat = 21.3142, Lng = -158.0397, Name = "Kapolei", Description = "Second city, business, shopping, growth", Type = PlaceType.Restaurant },
            // Pin 10: Makakilo, Hawaii
            new { Lat = 21.3350, Lng = -158.0550, Name = "Makakilo", Description = "Hillside, residential, views, breezy, quiet", Type = PlaceType.Tourist },
        };

        // Get the next pin location based on numbered pins count
        var pinData = pinLocations[numberedPinsCount];

        var pin = new Pin
        {
            Label = pinData.Name,
            Address = $"{pinData.Description}",
            Type = PinType.Place,
            Location = new Location(pinData.Lat, pinData.Lng)
        };

        var placeInfo = new PlaceInfo
        {
            Name = pinData.Name,
            Description = "Pin Added from ItemsSource",
            Location = new Location(pinData.Lat, pinData.Lng),
            Type = pinData.Type
        };

        _viewModel.Places.Add(placeInfo);
        _viewModel.Pins.Add(pin);
        _viewModel.UserAddedPinCount++;
    }

    // Shape selection for Map Elements
    private string _selectedShape = "Polyline"; // Default

    private void ShapeRadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value && sender is RadioButton rb && rb.Content is string content)
        {
            _selectedShape = content;

            // If Polygon is selected, render the polygon immediately
            if (_selectedShape == "Polygon")
            {
                // Remove any existing polygon
                var polygons = _viewModel.MapElements.Where(el => el is Polygon).ToList();
                foreach (var poly in polygons)
                    _viewModel.MapElements.Remove(poly);

                // Add the polygon connecting all 9 locations
                var destinations = new[]
                {
                    new Location(21.3649, -157.9634), // Pin3
                    new Location(21.3986, -158.0097), // Pin2
                    
                    new Location(21.4513, -158.0147), // Pin4
                    new Location(21.3847, -157.9261), // Pin5
                };
                var polygon = new Polygon
                {
                    StrokeColor = Colors.Blue,
                    StrokeWidth = 3,
                    FillColor = Color.FromArgb("#330000FF")
                };
                foreach (var loc in destinations)
                    polygon.Geopath.Add(loc);
                _viewModel.MapElements.Add(polygon);
            }
            else
            {
                // Remove any existing polygon if another shape is selected
                var polygons = _viewModel.MapElements.Where(el => el is Polygon).ToList();
                foreach (var poly in polygons)
                    _viewModel.MapElements.Remove(poly);
            }
        }
    }

    private void AddElementButton_Clicked(object sender, EventArgs e)
    {
        // Use the same destinations for all shapes
        var destinations = new[]
        {
            new { Name = "Pin2", Location = new Location(21.3986, -158.0097), Color = Colors.Red, Type = PlaceType.Restaurant },
            new { Name = "Pin3", Location = new Location(21.3649, -157.9634), Color = Colors.Green, Type = PlaceType.Tourist },
            new { Name = "Pin4", Location = new Location(21.4513, -158.0147), Color = Colors.Orange, Type = PlaceType.Restaurant },
            new { Name = "Pin5", Location = new Location(21.3847, -157.9261), Color = Colors.Purple, Type = PlaceType.Tourist },
            new { Name = "Pin6", Location = new Location(21.4644, -158.0411), Color = Colors.Brown, Type = PlaceType.Restaurant },
            new { Name = "Pin7", Location = new Location(21.3408, -158.0061), Color = Colors.Pink, Type = PlaceType.Tourist },
            new { Name = "Pin8", Location = new Location(21.3247, -157.9772), Color = Colors.Navy, Type = PlaceType.Restaurant },
            new { Name = "Pin9", Location = new Location(21.3142, -158.0397), Color = Colors.Teal, Type = PlaceType.Tourist },
            new { Name = "Pin10", Location = new Location(21.3350, -158.0550), Color = Colors.Maroon, Type = PlaceType.Restaurant }
        };

        if (_selectedShape == "Polyline")
        {
            // Add up to 9 polylines, one per click
            int polylineCount = _viewModel.MapElements.Count(e => e is Polyline);
            if (polylineCount >= 9)
                return;
            var destination = destinations[polylineCount];
            var polyline = new Polyline
            {
                StrokeColor = destination.Color,
                StrokeWidth = 4
            };
            polyline.Geopath.Add(MapViewModel.PearlCityLocation);
            var startLat = MapViewModel.PearlCityLocation.Latitude;
            var startLng = MapViewModel.PearlCityLocation.Longitude;
            var endLat = destination.Location.Latitude;
            var endLng = destination.Location.Longitude;
            var midLat1 = startLat + (endLat - startLat) * 0.33;
            var midLng1 = startLng + (endLng - startLng) * 0.33;
            polyline.Geopath.Add(new Location(midLat1, midLng1));
            var midLat2 = startLat + (endLat - startLat) * 0.67;
            var midLng2 = startLng + (endLng - startLng) * 0.67;
            polyline.Geopath.Add(new Location(midLat2, midLng2));
            polyline.Geopath.Add(destination.Location);
            _viewModel.MapElements.Add(polyline);
        }
        else if (_selectedShape == "Circle")
        {
            // Add up to 9 circles, one per click
            int circleCount = _viewModel.MapElements.Count(e => e is Circle && ((Circle)e).Radius.Kilometers == 2);
            if (circleCount >= 9)
                return;
            var destination = destinations[circleCount];
            var circle = new Circle
            {
                Center = destination.Location,
                Radius = Distance.FromKilometers(2),
                StrokeColor = destination.Color,
                StrokeWidth = 2,
                FillColor = Color.FromArgb("#3300FF00")
            };
            _viewModel.MapElements.Add(circle);
        }
        else if (_selectedShape == "Polygon")
        {
            bool polygonExists = _viewModel.MapElements.Any(e => e is Polygon);
            if (polygonExists)
                return;

            var polygon = new Polygon
            {
                StrokeColor = Colors.Blue,
                StrokeWidth = 3,
                FillColor = Color.FromArgb("#330000FF")
            };
            polygon.Geopath.Add(MapViewModel.PearlCityLocation);
            polygon.Geopath.Add(destinations[0].Location);
            polygon.Geopath.Add(destinations[1].Location);
            polygon.Geopath.Add(destinations[3].Location);

            _viewModel.MapElements.Add(polygon);
        }
    }
    private void SetItemsSourceButton_Clicked(object sender, EventArgs e)
    {
        _viewModel.Pins.Clear(); // Clear any existing pins
        _viewModel.ItemsSource = _viewModel.Places;
    }

    private void ClearItemsSourceButton_Clicked(object sender, EventArgs e)
    {
        // Clear the ItemsSource to disable data templating
        // This will revert to manual pin management
        _viewModel.ItemsSource = null;
    }

    private void SetItemTemplateButton_Clicked(object sender, EventArgs e)
    {
        // Create a sample DataTemplate using a Pin to render place info
        var pinTemplate = new DataTemplate(() =>
        {
            var pin = new Pin
            {
                Type = PinType.SearchResult
            };
            pin.SetBinding(Pin.LocationProperty, new Binding("Location"));
            pin.SetBinding(Pin.LabelProperty, new Binding("Name"));
            pin.SetBinding(Pin.AddressProperty, new Binding("Description"));

            return pin;
        });

        _viewModel.ItemTemplate = pinTemplate;
    }

    private void ClearItemTemplateButton_Clicked(object sender, EventArgs e)
    {
        // Clear the ItemTemplate
        _viewModel.ItemTemplate = null;
    }

    private void SetTemplateSelectorButton_Clicked(object sender, EventArgs e)
    {
        // Create sample templates for the selector using Grid with red background
        var restaurantTemplate = new DataTemplate(() =>
        {
            var pin = new Pin
            {
                Type = PinType.Place,
                Address ="Restaurant Pin",
            };
            pin.SetBinding(Pin.LocationProperty, new Binding("Location"));
            pin.SetBinding(Pin.LabelProperty, new Binding("Name"));
            return pin;
        });

        var touristTemplate = new DataTemplate(() =>
        {
            var pin = new Pin
            {
                Type = PinType.SearchResult,
                Address = "Tourist Pin"
            };
            pin.SetBinding(Pin.LocationProperty, new Binding("Location"));
            pin.SetBinding(Pin.LabelProperty, new Binding("Name"));

            return pin;
        });

        // Setup ItemTemplateSelector
        var templateSelector = new SamplePinTemplateSelector
        {
            RestaurantTemplate = restaurantTemplate,
            TouristTemplate = touristTemplate
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

        // Reset Map Element radio button to Polyline
        _selectedShape = "Polyline";

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

        // Also reset the clicked location pin and label on the map page
        var nav = this.Navigation;
        if (nav != null && nav.NavigationStack.Count > 0)
        {
            // Find the MapControlMainPage in the navigation stack
            foreach (var page in nav.NavigationStack)
            {
                if (page is Maui.Controls.Sample.MapControlMainPage mainPage)
                {
                    mainPage.ResetClickedLocationPinAndLabel();
                    break;
                }
            }
        }
    }
}