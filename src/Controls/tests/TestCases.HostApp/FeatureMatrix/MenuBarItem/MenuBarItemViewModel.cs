using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample;

public class MenuBarItemViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	#region MenuBarItem Properties

	// File Menu
	private string _fileMenuText = "File";
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
	private string _locationsMenuText = "Locations";
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
	private string _viewMenuText = "View";
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
	private string _exitText = "Exit";
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

	private string _exitIcon = "exit.png";
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
	private string _refreshText = "Refresh";
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

	private string _refreshIcon = "refresh.png";
	public string RefreshIcon
	{
		get => _refreshIcon;
		set => SetProperty(ref _refreshIcon, value);
	}

	// Change Theme Item (View Menu)
	private string _changeThemeText = "Change Theme";
	public string ChangeThemeText
	{
		get => _changeThemeText;
		set => SetProperty(ref _changeThemeText, value);
	}

	private bool _changeThemeEnabled = true;
	public bool ChangeThemeEnabled
	{
		get => _changeThemeEnabled;
		set => SetProperty(ref _changeThemeEnabled, value);
	}

	private string _changeThemeIcon = "theme.png";
	public string ChangeThemeIcon
	{
		get => _changeThemeIcon;
		set => SetProperty(ref _changeThemeIcon, value);
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

	public ObservableCollection<string> Locations { get; set; }

	#endregion

	#region Commands

	public ICommand ExitCommand { get; }
	public ICommand ChangeLocationCommand { get; }
	public ICommand AddLocationCommand { get; }
	public ICommand EditLocationCommand { get; }
	public ICommand RemoveLocationCommand { get; }
	public ICommand RefreshCommand { get; }
	public ICommand ChangeThemeCommand { get; }
	public ICommand UpdateMenuTextsCommand { get; }

	#endregion

	public MenuBarItemViewModel()
	{
		// Initialize locations collection
		Locations = new ObservableCollection<string>
		{
			"Redmond, USA",
			"London, UK",
			"Berlin, DE"
		};

		// Initialize commands
		ExitCommand = new Command(OnExit, () => FileMenuEnabled);
		ChangeLocationCommand = new Command<string>(OnChangeLocation, _ => LocationsMenuEnabled);
		AddLocationCommand = new Command(OnAddLocation, () => LocationsMenuEnabled);
		EditLocationCommand = new Command(OnEditLocation, () => LocationsMenuEnabled);
		RemoveLocationCommand = new Command(OnRemoveLocation, () => LocationsMenuEnabled);
		RefreshCommand = new Command(OnRefresh, () => ViewMenuEnabled);
		ChangeThemeCommand = new Command(OnChangeTheme, () => ViewMenuEnabled);
		UpdateMenuTextsCommand = new Command(OnUpdateMenuTexts);

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
				((Command)ChangeThemeCommand).ChangeCanExecute();
			}
		};
	}

	#region Command Implementations

	private void OnExit()
	{
		StatusMessage = "Exit command executed - Application closing...";
		// Delay to show message before exit
		Task.Run(async () =>
		{
			await Task.Delay(1000);
			Application.Current?.Quit();
		});
	}

	private void OnChangeLocation(string location)
	{
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
		var newLocation = $"New Location {Locations.Count + 1}";
		Locations.Add(newLocation);
		StatusMessage = $"Added location: {newLocation}";
	}

	private void OnEditLocation()
	{
		if (Locations.Count > 0)
		{
			var oldLocation = Locations[0];
			Locations[0] = $"Updated Location ({DateTime.Now:HH:mm:ss})";
			StatusMessage = $"Updated '{oldLocation}' to '{Locations[0]}'";
		}
		else
		{
			StatusMessage = "No locations available to edit";
		}
	}

	private void OnRemoveLocation()
	{
		if (Locations.Count > 0)
		{
			var removed = Locations[Locations.Count - 1];
			Locations.RemoveAt(Locations.Count - 1);
			StatusMessage = $"Removed location: {removed}";
		}
		else
		{
			StatusMessage = "No locations available to remove";
		}
	}

	private void OnRefresh()
	{
		StatusMessage = $"Refreshed at {DateTime.Now:HH:mm:ss}";
	}

	private void OnChangeTheme()
	{
		StatusMessage = $"Theme change requested at {DateTime.Now:HH:mm:ss}";
	}

	private void OnUpdateMenuTexts()
	{
		FileMenuText = $"File ({DateTime.Now:ss}s)";
		LocationsMenuText = $"Locations ({Locations.Count})";
		ViewMenuText = $"View (Updated)";
		StatusMessage = "Menu texts updated dynamically!";
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