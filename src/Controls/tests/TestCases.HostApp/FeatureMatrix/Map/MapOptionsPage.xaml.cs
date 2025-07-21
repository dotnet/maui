using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
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
		SetupSampleItemTemplate();

		// Initialize radio buttons based on current MapType
		UpdateRadioButtonsFromMapType();
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

	private void SetupSampleItemTemplate()
	{
		// Create a sample DataTemplate for pins
		var pinTemplate = new DataTemplate(() =>
		{
			var pin = new Pin();
			pin.SetBinding(Pin.LabelProperty, "Label");
			pin.SetBinding(Pin.AddressProperty, "Address");
			pin.SetBinding(Pin.LocationProperty, "Location");
			pin.SetBinding(Pin.TypeProperty, "Type");
			return pin;
		});

		_viewModel.ItemTemplate = pinTemplate;

		// Create sample templates for the selector
		var genericTemplate = new DataTemplate(() =>
		{
			var pin = new Pin { Type = PinType.Generic };
			pin.SetBinding(Pin.LabelProperty, "Label");
			pin.SetBinding(Pin.AddressProperty, "Address");
			pin.SetBinding(Pin.LocationProperty, "Location");
			return pin;
		});

		var placeTemplate = new DataTemplate(() =>
		{
			var pin = new Pin { Type = PinType.Place };
			pin.SetBinding(Pin.LabelProperty, "Label");
			pin.SetBinding(Pin.AddressProperty, "Address");
			pin.SetBinding(Pin.LocationProperty, "Location");
			return pin;
		});

		// Setup ItemTemplateSelector
		var templateSelector = new SamplePinTemplateSelector
		{
			GenericTemplate = genericTemplate,
			PlaceTemplate = placeTemplate
		};

		_viewModel.ItemTemplateSelector = templateSelector;
	}

	private void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}

	private void AddPinButton_Clicked(object sender, EventArgs e)
	{
		// Use Pearl City as the current location for adding pins
		var location = MapViewModel.PearlCityLocation;

		// Add some randomization to avoid overlapping pins
		var random = new Random();
		var offsetLat = (random.NextDouble() - 0.5) * 0.01; // ±0.005 degrees
		var offsetLng = (random.NextDouble() - 0.5) * 0.01; // ±0.005 degrees

		var randomLocation = new Location(
			location.Latitude + offsetLat,
			location.Longitude + offsetLng
		);

		var pin = new Pin
		{
			Label = $"Pin {_viewModel.Pins.Count + 1}",
			Address = "Near Pearl City, Hawaii",
			Type = PinType.Generic,
			Location = randomLocation
		};
		_viewModel.Pins.Add(pin);
	}

	private void ClearPinsButton_Clicked(object sender, EventArgs e)
	{
		_viewModel.Pins.Clear();
	}

	private void AddElementButton_Clicked(object sender, EventArgs e)
	{
		// Define different destinations around Oahu, Hawaii
		var destinations = new[]
		{
			new { Name = "Honolulu Airport", Location = new Location(21.3099, -157.8581), Color = Colors.Blue },
			new { Name = "Waikiki Beach", Location = new Location(21.2761, -157.8294), Color = Colors.Red },
			new { Name = "Diamond Head", Location = new Location(21.2616, -157.8055), Color = Colors.Green },
			new { Name = "North Shore (Haleiwa)", Location = new Location(21.5934, -158.1064), Color = Colors.Orange },
			new { Name = "Kailua Beach", Location = new Location(21.4022, -157.7394), Color = Colors.Purple },
			new { Name = "Hanauma Bay", Location = new Location(21.2693, -157.6946), Color = Colors.Brown },
			new { Name = "Polynesian Cultural Center", Location = new Location(21.6401, -157.9220), Color = Colors.Pink },
			new { Name = "USS Arizona Memorial", Location = new Location(21.3649, -157.9502), Color = Colors.Navy }
		};

		// Get the current count to cycle through destinations
		var count = _viewModel.MapElements.Count;
		var destination = destinations[count % destinations.Length];

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

		// Add a pin at the destination for reference
		var destinationPin = new Pin
		{
			Label = destination.Name,
			Address = $"{destination.Name}, Oahu, Hawaii",
			Type = PinType.Place,
			Location = destination.Location
		};
		_viewModel.Pins.Add(destinationPin);
	}

	private void ClearElementsButton_Clicked(object sender, EventArgs e)
	{
		_viewModel.MapElements.Clear();
	}

	private void SetItemsSourceButton_Clicked(object sender, EventArgs e)
	{
		// Set the ItemsSource to the pins collection
		_viewModel.ItemsSource = _viewModel.Pins;
	}

	private void ClearItemsSourceButton_Clicked(object sender, EventArgs e)
	{
		// Clear the ItemsSource
		_viewModel.ItemsSource = null;
	}
}