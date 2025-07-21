using System.Collections.Specialized;
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

		// Handle Pins collection changes to sync with the map's Pins collection
		_viewModel.Pins.CollectionChanged += OnPinsCollectionChanged;

		// Bind ItemsSource, ItemTemplate, and ItemTemplateSelector programmatically to ensure proper binding
		TestMap.SetBinding(Microsoft.Maui.Controls.Maps.Map.ItemsSourceProperty, new Binding("ItemsSource"));
		TestMap.SetBinding(Microsoft.Maui.Controls.Maps.Map.ItemTemplateProperty, new Binding("ItemTemplate"));
		TestMap.SetBinding(Microsoft.Maui.Controls.Maps.Map.ItemTemplateSelectorProperty, new Binding("ItemTemplateSelector"));

		// Set initial map region to Pearl City
		TestMap.MoveToRegion(_viewModel.InitialRegion);
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

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		// Use the existing view model instead of creating a new one
		await Navigation.PushAsync(new MapOptionsPage(_viewModel));
	}
}