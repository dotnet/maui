using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.Maui.Controls.Maps;

namespace Maui.Controls.Sample;

public class MapControlPage : NavigationPage
{
	private MapViewModel _viewModel;
	public MapControlPage()
	{
		_viewModel = new MapViewModel();
		PushAsync(new MapControlMainPage(_viewModel));
	}
}

public partial class MapControlMainPage : ContentPage
{
	private MapViewModel _viewModel;
	public MapControlMainPage(MapViewModel viewModel)
	{

		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;

		// Set initial label text to 'Not clicked'
		var mapLabel = this.FindByName<Microsoft.Maui.Controls.Label>("MapClickedLabel");
		if (mapLabel != null)
			mapLabel.Text = "Map Clicked: Not clicked";
		var markerLabel = this.FindByName<Microsoft.Maui.Controls.Label>("MarkerClickedLabel");
		if (markerLabel != null)
			markerLabel.Text = "Marker Clicked: Not clicked";

		// Handle MapElements collection changes manually since it's not directly bindable
		_viewModel.MapElements.CollectionChanged += OnMapElementsCollectionChanged;

		// Note: Pins collection changes are handled through ItemsSource binding
		// Manual Pins handling is only needed when ItemsSource is null
		_viewModel.Pins.CollectionChanged += OnPinsCollectionChanged;
		_viewModel.PropertyChanged += OnViewModelPropertyChanged;

		// ItemsSource, ItemTemplate, and ItemTemplateSelector are already bound in XAML
		// No need for programmatic binding as it can cause conflicts

		// Set initial map region to Pearl City
		TestMap.MoveToRegion(_viewModel.InitialRegion);

		// Ensure ItemTemplate and ItemTemplateSelector are synchronized with ViewModel
		if (_viewModel.ItemTemplate != null)
		{
			TestMap.ItemTemplate = _viewModel.ItemTemplate;
		}
		if (_viewModel.ItemTemplateSelector != null)
		{
			TestMap.ItemTemplateSelector = _viewModel.ItemTemplateSelector;
		}

		// Subscribe to map events to sync VisibleRegion back to ViewModel
		TestMap.PropertyChanged += OnMapPropertyChanged;

		// Subscribe to MessagingCenter for label updates
		Microsoft.Maui.Controls.MessagingCenter.Subscribe<object, string>(this, "MapClickedLabelUpdate", (sender, value) =>
		{
			var label = this.FindByName<Microsoft.Maui.Controls.Label>("MapClickedLabel");
			if (label != null)
				label.Text = value;
		});
		Microsoft.Maui.Controls.MessagingCenter.Subscribe<object, string>(this, "MarkerClickedLabelUpdate", (sender, value) =>
		{
			var label = this.FindByName<Microsoft.Maui.Controls.Label>("MarkerClickedLabel");
			if (label != null)
				label.Text = value;
		});

		TestMap.MapClicked += OnMapClicked;

	}

	private void OnMapElementsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.Action == NotifyCollectionChangedAction.Add)
		{
			foreach (MapElement element in e.NewItems)
			{
				TestMap.MapElements.Add(element);
			}
		}
		else if (e.Action == NotifyCollectionChangedAction.Remove)
		{
			foreach (MapElement element in e.OldItems)
			{
				TestMap.MapElements.Remove(element);
			}
		}
		else if (e.Action == NotifyCollectionChangedAction.Reset)
		{
			TestMap.MapElements.Clear();
		}
	}

	private void OnPinsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		// Only handle pins manually when ItemsSource is null
		// When ItemsSource is set, the binding handles pin display automatically
		if (_viewModel.ItemsSource == null)
		{
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (Pin pin in e.NewItems)
				{
				   TestMap.Pins.Add(pin);
				}
			}
			else if (e.Action == NotifyCollectionChangedAction.Remove)
			{
				foreach (Pin pin in e.OldItems)
				{
					TestMap.Pins.Remove(pin);
				}
			}
			else if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				TestMap.Pins.Clear();
			}
		}
	}

	private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(_viewModel.ItemsSource))
		{
			TestMap.Pins.Clear(); // Clear existing pins if any
			TestMap.ItemsSource = _viewModel.ItemsSource;
		}
		else if (e.PropertyName == nameof(_viewModel.ItemTemplate))
		{
			// When ItemTemplate changes in ViewModel, update the actual Map
			TestMap.ItemTemplate = _viewModel.ItemTemplate;
		}
		else if (e.PropertyName == nameof(_viewModel.ItemTemplateSelector))
		{
			TestMap.ItemTemplate = null;
			// When ItemTemplateSelector changes in ViewModel, update the actual Map
			TestMap.ItemTemplateSelector = _viewModel.ItemTemplateSelector;
		}
		else if (e.PropertyName == nameof(_viewModel.VisibleRegion))
		{
			// When VisibleRegion changes in ViewModel, update the actual Map
			if (_viewModel.VisibleRegion != null)
			{
				TestMap.MoveToRegion(_viewModel.VisibleRegion);
			}
		}
	}

	private void OnMapPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		// Update ViewModel's VisibleRegion when the map's VisibleRegion changes
		if (e.PropertyName == "VisibleRegion" && TestMap.VisibleRegion != null)
		{
			// Only update if it's different to avoid infinite loops
			if (_viewModel.VisibleRegion != TestMap.VisibleRegion)
			{
				_viewModel.VisibleRegion = TestMap.VisibleRegion;
			}
		}
	}

	private Pin _lastClickPin;
	private void OnMapClicked(object sender, Microsoft.Maui.Controls.Maps.MapClickedEventArgs e)
	{
		var label = this.FindByName<Microsoft.Maui.Controls.Label>("MapClickedLabel");
		if (label != null)
			label.Text = $"Map Clicked: Latitude: {e.Location.Latitude:F6}, Longitude: {e.Location.Longitude:F6}";

		// Remove previous click marker if it exists
		if (_lastClickPin != null)
		{
			if (TestMap.Pins.Contains(_lastClickPin))
				TestMap.Pins.Remove(_lastClickPin);
			_lastClickPin = null;
		}

		// Add a new pin at the clicked location
		var clickPin = new Pin
		{
			Label = "Clicked Location",
			Location = e.Location,
			Address = $"Lat: {e.Location.Latitude:F6}, Lng: {e.Location.Longitude:F6}",
			Type = PinType.Place
		};
		// Attach MarkerClicked and InfoWindowClicked events
		clickPin.MarkerClicked += OnPinMarkerClicked;
		clickPin.InfoWindowClicked += OnPinInfoWindowClicked;
		TestMap.Pins.Add(clickPin);
		_lastClickPin = clickPin;
	}
	private void OnPinMarkerClicked(object sender, PinClickedEventArgs e)
	{
		var label = this.FindByName<Microsoft.Maui.Controls.Label>("MarkerClickedLabel");
		if (label != null)
			label.Text = "Marker Clicked: Pin tapped (info window will show)";
		e.HideInfoWindow = false;
	}

	private void OnPinInfoWindowClicked(object sender, PinClickedEventArgs e)
	{
		var label = this.FindByName<Microsoft.Maui.Controls.Label>("MarkerClickedLabel");
		if (label != null)
			label.Text = "Marker Clicked: Info window tapped (info window will hide)";
		e.HideInfoWindow = true;
	}

	public void ResetClickedLocationPinAndLabel()
	{
		// Remove the clicked location pin if it exists
		if (_lastClickPin != null)
		{
			if (TestMap.Pins.Contains(_lastClickPin))
				TestMap.Pins.Remove(_lastClickPin);
			_lastClickPin = null;
		}
		// Reset the labels to 'Not clicked'
		var mapLabel = this.FindByName<Microsoft.Maui.Controls.Label>("MapClickedLabel");
		if (mapLabel != null)
			mapLabel.Text = "Map Clicked: Not clicked";
		var markerLabel = this.FindByName<Microsoft.Maui.Controls.Label>("MarkerClickedLabel");
		if (markerLabel != null)
			markerLabel.Text = "Marker Clicked: Not clicked";
	}
	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		// Use the existing view model instead of creating a new one
		await Navigation.PushAsync(new MapOptionsPage(_viewModel));
	}
}