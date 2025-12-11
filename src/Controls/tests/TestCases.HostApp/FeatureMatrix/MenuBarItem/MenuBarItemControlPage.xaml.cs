using System.Collections.Specialized;

namespace Maui.Controls.Sample;

public partial class MenuBarItemControlPage : Shell
{
	private MenuBarItemViewModel _viewModel;

	public MenuBarItemControlPage()
	{
		InitializeComponent();
		this.SetAppThemeColor(BackgroundColorProperty, Colors.White, Colors.Black);
		_viewModel = new MenuBarItemViewModel();
		BindingContext = _viewModel;

		// Subscribe to collection changes
		_viewModel.Locations.CollectionChanged += OnLocationsCollectionChanged;

		// Initial population of menu items
		PopulateLocationMenuItems();
	}

	private void OnLocationsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
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
				Text = location,
				Command = _viewModel.ChangeLocationCommand,
				CommandParameter = location
			};
			ChangeLocationSubItem.Add(menuItem);
		}
	}
}