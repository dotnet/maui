using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample;

public class LocationItem : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	public int Id { get; set; }
	private string _name;
	public string Name
	{
		get => _name;
		set
		{
			if (_name != value)
			{
				_name = value;
				OnPropertyChanged();
			}
		}
	}

	private bool _isSelected;
	public bool IsSelected
	{
		get => _isSelected;
		set
		{
			if (_isSelected != value)
			{
				_isSelected = value;
				OnPropertyChanged();
			}
		}
	}

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

public class MenuBarItemViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	#region MenuBarItem Properties

	// File Menu
	private string _fileMenuText = "FileMenuBar";
	public string FileMenuText
	{
		get => _fileMenuText;
		set => SetProperty(ref _fileMenuText, value);
	}

	private bool _fileMenuEnabled = true;
	public bool FileMenuEnabled
	{
		get => _fileMenuEnabled;
		set => SetProperty(ref _fileMenuEnabled, value);
	}

	// Locations Menu
	private string _locationsMenuText = "LocationsMenuBar";
	public string LocationsMenuText
	{
		get => _locationsMenuText;
		set => SetProperty(ref _locationsMenuText, value);
	}

	private bool _locationsMenuEnabled = true;
	public bool LocationsMenuEnabled
	{
		get => _locationsMenuEnabled;
		set => SetProperty(ref _locationsMenuEnabled, value);
	}

	// View Menu
	private string _viewMenuText = "ViewMenuBar";
	public string ViewMenuText
	{
		get => _viewMenuText;
		set => SetProperty(ref _viewMenuText, value);
	}

	private bool _viewMenuEnabled = true;
	public bool ViewMenuEnabled
	{
		get => _viewMenuEnabled;
		set => SetProperty(ref _viewMenuEnabled, value);
	}

	#endregion

	#region MenuFlyoutItem Properties

	// Exit Item (File Menu)
	private string _exitText = "ExitMenuBarFlyoutItem";
	public string ExitText
	{
		get => _exitText;
		set => SetProperty(ref _exitText, value);
	}

	private bool _exitEnabled = true;
	public bool ExitEnabled
	{
		get => _exitEnabled;
		set => SetProperty(ref _exitEnabled, value);
	}

	private string _exitIcon = "dotnet_bot_resized.png";
	public string ExitIcon
	{
		get => _exitIcon;
		set => SetProperty(ref _exitIcon, value);
	}

	// Change Location SubItem (Locations Menu)
	private string _changeLocationText = "Change Location";
	public string ChangeLocationText
	{
		get => _changeLocationText;
		set => SetProperty(ref _changeLocationText, value);
	}

	private bool _changeLocationEnabled = true;
	public bool ChangeLocationEnabled
	{
		get => _changeLocationEnabled;
		set => SetProperty(ref _changeLocationEnabled, value);
	}

	// Add Location Item (Locations Menu)
	private string _addLocationText = "Add Location";
	public string AddLocationText
	{
		get => _addLocationText;
		set => SetProperty(ref _addLocationText, value);
	}

	private bool _addLocationEnabled = true;
	public bool AddLocationEnabled
	{
		get => _addLocationEnabled;
		set => SetProperty(ref _addLocationEnabled, value);
	}

	// Edit Location Item (Locations Menu)
	private string _editLocationText = "Edit Location";
	public string EditLocationText
	{
		get => _editLocationText;
		set => SetProperty(ref _editLocationText, value);
	}

	private bool _editLocationEnabled = true;
	public bool EditLocationEnabled
	{
		get => _editLocationEnabled;
		set => SetProperty(ref _editLocationEnabled, value);
	}

	// Remove Location Item (Locations Menu)
	private string _removeLocationText = "Remove Location";
	public string RemoveLocationText
	{
		get => _removeLocationText;
		set => SetProperty(ref _removeLocationText, value);
	}

	private bool _removeLocationEnabled = true;
	public bool RemoveLocationEnabled
	{
		get => _removeLocationEnabled;
		set => SetProperty(ref _removeLocationEnabled, value);
	}

	// Refresh Item (View Menu)
	private string _refreshText = "RefreshMenuBarFlyoutItem";
	public string RefreshText
	{
		get => _refreshText;
		set => SetProperty(ref _refreshText, value);
	}

	private bool _refreshEnabled = true;
	public bool RefreshEnabled
	{
		get => _refreshEnabled;
		set => SetProperty(ref _refreshEnabled, value);
	}

	private string _refreshIcon = "dotnet_bot_resized.png";
	public string RefreshIcon
	{
		get => _refreshIcon;
		set => SetProperty(ref _refreshIcon, value);
	}

	#endregion

	#region Display Properties

	private string _currentLocation = "Not set";
	public string CurrentLocation
	{
		get => _currentLocation;
		set => SetProperty(ref _currentLocation, value);
	}

	private string _statusMessage = "Welcome! Use the menu bar to interact with the application.";
	public string StatusMessage
	{
		get => _statusMessage;
		set => SetProperty(ref _statusMessage, value);
	}

	private bool _isEntryVisible = false;
	public bool IsEntryVisible
	{
		get => _isEntryVisible;
		set => SetProperty(ref _isEntryVisible, value);
	}

	private string _entryText = string.Empty;
	public string EntryText
	{
		get => _entryText;
		set => SetProperty(ref _entryText, value);
	}

	private string _entryPlaceholder = "Enter location name";
	public string EntryPlaceholder
	{
		get => _entryPlaceholder;
		set => SetProperty(ref _entryPlaceholder, value);
	}

	private string _entryMode = string.Empty; // "add" or "edit"
	private int _editingLocationIndex = -1;

	private int _selectedLocationIndex = -1;
	public int SelectedLocationIndex
	{
		get => _selectedLocationIndex;
		set => SetProperty(ref _selectedLocationIndex, value);
	}

	public ObservableCollection<LocationItem> Locations { get; set; }

	#endregion

	#region Commands

	public ICommand ExitCommand { get; }
	public ICommand ChangeLocationCommand { get; }
	public ICommand AddLocationCommand { get; }
	public ICommand EditLocationCommand { get; }
	public ICommand RemoveLocationCommand { get; }
	public ICommand RefreshCommand { get; }
	public ICommand ResetCommand { get; }
	public ICommand ConfirmEntryCommand { get; }
	public ICommand CancelEntryCommand { get; }

	#endregion

	public MenuBarItemViewModel()
	{
		// Initialize locations collection
		Locations = new ObservableCollection<LocationItem>
		{
			new LocationItem { Id = 0, Name = "Redmond, USA", IsSelected = false },
			new LocationItem { Id = 1, Name = "London, UK", IsSelected = false },
			new LocationItem { Id = 2, Name = "Berlin, DE", IsSelected = false }
		};

		// Initialize commands
		ExitCommand = new Command(OnExit, () => FileMenuEnabled);
		ChangeLocationCommand = new Command<string>(OnChangeLocation, _ => LocationsMenuEnabled);
		AddLocationCommand = new Command(OnAddLocation, () => LocationsMenuEnabled);
		EditLocationCommand = new Command(OnEditLocation, () => LocationsMenuEnabled);
		RemoveLocationCommand = new Command(OnRemoveLocation, () => LocationsMenuEnabled);
		RefreshCommand = new Command(OnRefresh, () => ViewMenuEnabled);
		ResetCommand = new Command(OnReset);
		ConfirmEntryCommand = new Command(OnConfirmEntry, () => IsEntryVisible);
		CancelEntryCommand = new Command(OnCancelEntry, () => IsEntryVisible);

		// Listen to property changes to update CanExecute
		PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == nameof(FileMenuEnabled))
			{
				((Command)ExitCommand).ChangeCanExecute();
			}
			else if (e.PropertyName == nameof(LocationsMenuEnabled))
			{
				((Command)ChangeLocationCommand).ChangeCanExecute();
				((Command)AddLocationCommand).ChangeCanExecute();
				((Command)EditLocationCommand).ChangeCanExecute();
				((Command)RemoveLocationCommand).ChangeCanExecute();
			}
			else if (e.PropertyName == nameof(ViewMenuEnabled))
			{
				((Command)RefreshCommand).ChangeCanExecute();
			}
			else if (e.PropertyName == nameof(IsEntryVisible))
			{
				((Command)ConfirmEntryCommand).ChangeCanExecute();
				((Command)CancelEntryCommand).ChangeCanExecute();
			}
		};
	}

	#region Command Implementations

	private void OnExit()
	{
		StatusMessage = "Exit command executed - Application closing...";
		// Delay to show message before exit
		Application.Current?.Dispatcher.Dispatch(() =>
		{
			Application.Current?.Quit();
		});
	}

	private void OnChangeLocation(object parameter)
	{
		var location = parameter as string;
		if (string.IsNullOrWhiteSpace(location))
		{
			StatusMessage = "Invalid location provided";
			return;
		}

		CurrentLocation = location;
		StatusMessage = $"Location changed to: {location}";
	}

	private void OnAddLocation()
	{
		_entryMode = "add";
		_editingLocationIndex = -1;
		EntryText = string.Empty;
		EntryPlaceholder = "e.g., Tokyo, Japan";
		IsEntryVisible = true;
		StatusMessage = "Enter location name in the field below and click Confirm";
	}

	private void OnEditLocation()
	{
		if (Locations.Count == 0)
		{
			StatusMessage = "No locations available to edit";
			return;
		}

		if (SelectedLocationIndex < 0 || SelectedLocationIndex >= Locations.Count)
		{
			StatusMessage = "Please select a location to edit by checking its checkbox";
			return;
		}

		var location = Locations[SelectedLocationIndex];
		_entryMode = "edit";
		_editingLocationIndex = SelectedLocationIndex;
		EntryText = location.Name;
		EntryPlaceholder = "Enter new location name";
		IsEntryVisible = true;
		StatusMessage = $"Edit '{location.Name}' in the field below and click Confirm";
	}

	private void OnRemoveLocation()
	{
		if (Locations.Count == 0)
		{
			StatusMessage = "No locations available to remove";
			return;
		}

		if (SelectedLocationIndex < 0 || SelectedLocationIndex >= Locations.Count)
		{
			StatusMessage = "Please select a location to remove by checking its checkbox";
			return;
		}

		var location = Locations[SelectedLocationIndex];
		Locations.RemoveAt(SelectedLocationIndex);
		SelectedLocationIndex = -1; // Clear selection after removal
		StatusMessage = $"Removed location: {location.Name}";
	}

	private void OnRefresh()
	{
		StatusMessage = $"Refreshed";
	}

	private void OnConfirmEntry()
	{
		if (string.IsNullOrWhiteSpace(EntryText))
		{
			StatusMessage = "Location name cannot be empty";
			return;
		}

		if (_entryMode == "add")
		{
			var newId = Locations.Count > 0 ? Locations.Max(l => l.Id) + 1 : 0;
			Locations.Add(new LocationItem { Id = newId, Name = EntryText, IsSelected = false });
			StatusMessage = $"Added location: {EntryText}";
		}
		else if (_entryMode == "edit" && _editingLocationIndex >= 0)
		{
			var oldName = Locations[_editingLocationIndex].Name;
			Locations[_editingLocationIndex].Name = EntryText;
			StatusMessage = $"Updated '{oldName}' to '{EntryText}'";
		}

		// Reset entry state
		IsEntryVisible = false;
		EntryText = string.Empty;
		_entryMode = string.Empty;
		_editingLocationIndex = -1;
	}

	private void OnCancelEntry()
	{
		IsEntryVisible = false;
		EntryText = string.Empty;
		_entryMode = string.Empty;
		_editingLocationIndex = -1;
		StatusMessage = "Operation cancelled";
	}

	private void OnReset()
	{
		CurrentLocation = "Not set";
		StatusMessage = "Application state has been reset.";

		// Reset menu enabled states
		FileMenuEnabled = true;
		LocationsMenuEnabled = true;
		ViewMenuEnabled = true;

		// Reset locations to initial state
		Locations.Clear();
		Locations.Add(new LocationItem { Id = 0, Name = "Redmond, USA", IsSelected = false });
		Locations.Add(new LocationItem { Id = 1, Name = "London, UK", IsSelected = false });
		Locations.Add(new LocationItem { Id = 2, Name = "Berlin, DE", IsSelected = false });

		// Hide entry if visible
		IsEntryVisible = false;
		EntryText = string.Empty;
		_entryMode = string.Empty;
		_editingLocationIndex = -1;
		SelectedLocationIndex = -1;
	}

	#endregion

	#region INotifyPropertyChanged Implementation

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value))
			return false;

		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}

	#endregion
}