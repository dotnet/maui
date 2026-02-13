using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample;

public class ShellViewModel : INotifyPropertyChanged
{
	private string _textOverride = string.Empty;
	private string _iconOverride = string.Empty;
	private bool _isEnabled = true;
	private bool _isVisible = true;
	private string _commandParameter = string.Empty;
	private string _commandExecuted = string.Empty;
	private string _navigating = string.Empty;
	private string _navigated = string.Empty;
	private string _currentState = "Not Set";
	private string _currentPage = "Not Set";
	private string _currentItem = "Not Set";
	private string _shellCurrent = "Not Set";

	public ShellViewModel()
	{
		Command = new Command<object>(param =>
		{
			CommandExecuted = param is string s && !string.IsNullOrEmpty(s)
	? $"Executed: {s}"
	: "Executed";
			Shell.Current?.GoToAsync("..");
		});
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public ICommand Command { get; }

	public string TextOverride
	{
		get => _textOverride;
		set { if (_textOverride != value) { _textOverride = value; OnPropertyChanged(); } }
	}

	public string IconOverride
	{
		get => _iconOverride;
		set { if (_iconOverride != value) { _iconOverride = value; OnPropertyChanged(); } }
	}

	public bool IsEnabled
	{
		get => _isEnabled;
		set { if (_isEnabled != value) { _isEnabled = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsEnabledText)); } }
	}

	public string IsEnabledText => $"IsEnabled: {_isEnabled}";

	public bool IsVisible
	{
		get => _isVisible;
		set { if (_isVisible != value) { _isVisible = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsVisibleText)); } }
	}

	public string IsVisibleText => $"IsVisible: {_isVisible}";

	public string CommandParameter
	{
		get => _commandParameter;
		set { if (_commandParameter != value) { _commandParameter = value; OnPropertyChanged(); } }
	}

	public string CommandExecuted
	{
		get => _commandExecuted;
		set { if (_commandExecuted != value) { _commandExecuted = value; OnPropertyChanged(); } }
	}

	public string Navigating
	{
		get => _navigating;
		set { if (_navigating != value) { _navigating = value; OnPropertyChanged(); } }
	}

	public string Navigated
	{
		get => _navigated;
		set { if (_navigated != value) { _navigated = value; OnPropertyChanged(); } }
	}

	public string CurrentState
	{
		get => _currentState;
		set { if (_currentState != value) { _currentState = value; OnPropertyChanged(); } }
	}

	public string CurrentPage
	{
		get => _currentPage;
		set { if (_currentPage != value) { _currentPage = value; OnPropertyChanged(); } }
	}

	public string CurrentItem
	{
		get => _currentItem;
		set { if (_currentItem != value) { _currentItem = value; OnPropertyChanged(); } }
	}

	public string ShellCurrent
	{
		get => _shellCurrent;
		set { if (_shellCurrent != value) { _shellCurrent = value; OnPropertyChanged(); } }
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
