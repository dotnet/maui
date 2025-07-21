using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Controls.Maps;

namespace Maui.Controls.Sample;

public class SamplePinTemplateSelector : DataTemplateSelector
{
	public DataTemplate GenericTemplate { get; set; }
	public DataTemplate PlaceTemplate { get; set; }

	protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
	{
		if (item is Pin pin)
		{
			return pin.Type == PinType.Place ? PlaceTemplate : GenericTemplate;
		}
		return GenericTemplate;
	}
}

public class MapViewModel : INotifyPropertyChanged
{
	// Pearl City, Hawaii coordinates
	public static readonly Location PearlCityLocation = new Location(21.3933, -157.9751);

	private bool _isShowingUser = true; // Enable user location by default
	private bool _isScrollEnabled = true;
	private bool _isTrafficEnabled;
	private bool _isZoomEnabled = true;
	private MapType _mapType = MapType.Street;
	private ObservableCollection<Pin> _pins = new();
	private ObservableCollection<MapElement> _mapElements = new();
	private IEnumerable _itemsSource;
	private DataTemplate _itemTemplate;
	private DataTemplateSelector _itemTemplateSelector;
	private MapSpan _initialRegion;

	public MapViewModel()
	{
		// Set initial location to Pearl City, Hawaii
		_initialRegion = MapSpan.FromCenterAndRadius(
			PearlCityLocation, // Pearl City coordinates
			Distance.FromMiles(5)
		);

		// Add a sample pin for Pearl City as the main location
		Pins.Add(new Pin
		{
			Label = "Pearl City",
			Address = "Pearl City, Hawaii",
			Type = PinType.Place,
			Location = PearlCityLocation
		});

		// Note: IsShowingUser is set to true by default, 
		// which will show user location as Pearl City coordinates
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public bool IsShowingUser
	{
		get => _isShowingUser;
		set
		{
			_isShowingUser = value;
			OnPropertyChanged();
		}
	}

	public bool IsScrollEnabled
	{
		get => _isScrollEnabled;
		set
		{
			_isScrollEnabled = value;
			OnPropertyChanged();
		}
	}

	public bool IsTrafficEnabled
	{
		get => _isTrafficEnabled;
		set
		{
			_isTrafficEnabled = value;
			OnPropertyChanged();
		}
	}

	public bool IsZoomEnabled
	{
		get => _isZoomEnabled;
		set
		{
			_isZoomEnabled = value;
			OnPropertyChanged();
		}
	}

	public MapType MapType
	{
		get => _mapType;
		set
		{
			_mapType = value;
			OnPropertyChanged();
		}
	}

	public ObservableCollection<Pin> Pins
	{
		get => _pins;
		set
		{
			_pins = value;
			OnPropertyChanged();
		}
	}

	public ObservableCollection<MapElement> MapElements
	{
		get => _mapElements;
		set
		{
			_mapElements = value;
			OnPropertyChanged();
		}
	}

	// ItemsSource for data templating
	public IEnumerable ItemsSource
	{
		get => _itemsSource;
		set
		{
			_itemsSource = value;
			OnPropertyChanged();
		}
	}

	// ItemTemplate for pin data templating
	public DataTemplate ItemTemplate
	{
		get => _itemTemplate;
		set
		{
			_itemTemplate = value;
			OnPropertyChanged();
		}
	}

	// ItemTemplateSelector for dynamic template selection
	public DataTemplateSelector ItemTemplateSelector
	{
		get => _itemTemplateSelector;
		set
		{
			_itemTemplateSelector = value;
			OnPropertyChanged();
		}
	}

	// MapType options for picker
	public List<MapType> MapTypeOptions { get; } = new List<MapType>
	{
		MapType.Street,
		MapType.Satellite,
		MapType.Hybrid
	};

	public MapSpan InitialRegion
	{
		get => _initialRegion;
		set
		{
			_initialRegion = value;
			OnPropertyChanged();
		}
	}

	// Current location property for binding - set to Pearl City
	public Location CurrentLocation => PearlCityLocation;

	// Method to get simulated user location (Pearl City)
	public Location GetSimulatedUserLocation()
	{
		return PearlCityLocation;
	}

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}