using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Controls.Maps;
using System.Windows.Input;

namespace Maui.Controls.Sample;

public class SamplePinTemplateSelector : DataTemplateSelector
{
    public DataTemplate RestaurantTemplate { get; set; }
    public DataTemplate TouristTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if (item is PlaceInfo placeInfo)
        {
            return placeInfo.Type == PlaceType.Tourist ? TouristTemplate : RestaurantTemplate;
        }
        return RestaurantTemplate;
    }
}

public class MapViewModel : INotifyPropertyChanged
{
    public static readonly Location PearlCityLocation = new Location(21.3933, -157.9751);
    private bool _isShowingUser = false;
    private bool _isScrollEnabled = false;
    private bool _isTrafficEnabled;
    private bool _isZoomEnabled = false;
    private bool _isVisible = true;
    private MapType _mapType = MapType.Street;
    private ObservableCollection<Pin> _pins = new();
    private ObservableCollection<MapElement> _mapElements = new();
    private IEnumerable _itemsSource;
    private DataTemplate _itemTemplate;
    private DataTemplateSelector _itemTemplateSelector;
    private MapSpan _initialRegion;
    private MapSpan _visibleRegion;
    private ICommand _mapClickedCommand;
    private int _userAddedPinCount = 0;

    public ObservableCollection<PlaceInfo> Places { get; set; } = new();

    public MapViewModel()
    {

        _initialRegion = MapSpan.FromCenterAndRadius(
            PearlCityLocation,
            Distance.FromMiles(5)
        );

        // Initialize visible region to the same as initial region
        _visibleRegion = _initialRegion;

        // Start with ItemsSource cleared (null) - user must click "Set ItemsSource" button to enable data templating
        // This allows manual pin management by default, and data templating only when explicitly requested
        _itemsSource = null;

        // Note: IsShowingUser is set to false by default
        // No pins are added initially - pins will only appear when user clicks "Add Pin"
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

    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            _isVisible = value;
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

    public int UserAddedPinCount
    {
        get => _userAddedPinCount;
        set
        {
            _userAddedPinCount = value;
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

    public IEnumerable ItemsSource
    {
        get => _itemsSource;
        set
        {
            _itemsSource = value;
            OnPropertyChanged();
        }
    }

    public DataTemplate ItemTemplate
    {
        get => _itemTemplate;
        set
        {
            _itemTemplate = value;
            OnPropertyChanged();
        }
    }

    public DataTemplateSelector ItemTemplateSelector
    {
        get => _itemTemplateSelector;
        set
        {
            _itemTemplateSelector = value;
            OnPropertyChanged();
        }
    }

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

    public MapSpan VisibleRegion
    {
        get => _visibleRegion;
        set
        {
            _visibleRegion = value;
            OnPropertyChanged();
        }
    }

    public ICommand MapClickedCommand
    {

        get => _mapClickedCommand;
        set
        {
            _mapClickedCommand = value;
            OnPropertyChanged();
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class PlaceInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Location Location { get; set; } = new Location();
    public PlaceType Type { get; set; }
}

public enum PlaceType
{
    Restaurant,
    Tourist,
}