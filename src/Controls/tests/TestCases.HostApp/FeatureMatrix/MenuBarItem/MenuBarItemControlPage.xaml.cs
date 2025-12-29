using System.Collections.Specialized;

namespace Maui.Controls.Sample;

public partial class MenuBarItemControlPage : Shell
{
	private MenuBarItemViewModel _viewModel;

	public MenuBarItemControlPage()
	{
		InitializeComponent();
		_viewModel = new MenuBarItemViewModel();
		BindingContext = _viewModel;

		// Subscribe to collection changes
		_viewModel.Locations.CollectionChanged += OnLocationsCollectionChanged;

		// Subscribe to property changes on each location item
		foreach (var location in _viewModel.Locations)
		{
			location.PropertyChanged += OnLocationPropertyChanged;
		}

		// Initial population of menu items
		PopulateLocationMenuItems();
	}

	private void OnLocationPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(LocationItem.IsSelected) && sender is LocationItem changedLocation)
		{
			if (changedLocation.IsSelected)
			{
				// Uncheck all other locations
				foreach (var location in _viewModel.Locations)
				{
					if (location != changedLocation && location.IsSelected)
					{
						location.IsSelected = false;
					}
				}

				// Update selected index
				_viewModel.SelectedLocationIndex = _viewModel.Locations.IndexOf(changedLocation);
			}
			else
			{
				// If this was the selected item, clear selection
				var index = _viewModel.Locations.IndexOf(changedLocation);
				if (index == _viewModel.SelectedLocationIndex)
				{
					_viewModel.SelectedLocationIndex = -1;
				}
			}
		}
	}

	private void OnLocationsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		// Subscribe to new items
		if (e.NewItems != null)
		{
			foreach (LocationItem location in e.NewItems)
			{
				location.PropertyChanged += OnLocationPropertyChanged;
			}
		}

		// Unsubscribe from old items
		if (e.OldItems != null)
		{
			foreach (LocationItem location in e.OldItems)
			{
				location.PropertyChanged -= OnLocationPropertyChanged;
			}
		}

		PopulateLocationMenuItems();
	}

	private void PopulateLocationMenuItems()
	{
		// Clear existing items
		ChangeLocationSubItem.Clear();

		// Add menu items for each location
		foreach (var location in _viewModel.Locations)
		{
			var menuItem = new MenuFlyoutItem
			{
				Text = location.Name,
				Command = _viewModel.ChangeLocationCommand,
				CommandParameter = location.Name
			};
			ChangeLocationSubItem.Add(menuItem);
		}
	}
}