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

		// Subscribe to map events to sync VisibleRegion back to ViewModel
		TestMap.PropertyChanged += OnMapPropertyChanged;
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
			// When ItemsSource changes, clear manual pins and let binding handle it
			if (_viewModel.ItemsSource != null)
			{
				// Clear manual pins when ItemsSource is set
				TestMap.Pins.Clear();
			}
			else
			{
				// Re-add pins manually when ItemsSource is cleared
				TestMap.Pins.Clear();
				foreach (var pin in _viewModel.Pins)
				{
					TestMap.Pins.Add(pin);
				}
			}
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

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		// Use the existing view model instead of creating a new one
		await Navigation.PushAsync(new MapOptionsPage(_viewModel));
	}
}