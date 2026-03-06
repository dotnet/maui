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
    private string _currentState = "Not Set";
    private string _currentPage = "Not Set";
    private string _currentItem = "Not Set";
    private string _shellCurrent = "Not Set";
    private string _navigatingCurrent = string.Empty;
    private string _navigatingSource = string.Empty;
    private string _navigatingTarget = string.Empty;
    private string _navigatingCanCancel = string.Empty;
    private string _navigatingCancelled = string.Empty;
    private string _navigatedCurrent = string.Empty;
    private string _navigatedPrevious = string.Empty;
    private string _navigatedSource = string.Empty;
    private string _routeStatus = string.Empty;
    private bool _cancelNavigation;
    private bool _enableDeferral;
    private string _deferralStatus = string.Empty;
    private string _overrideNavigatingStatus = string.Empty;
    private string _overrideNavigatedStatus = string.Empty;
    private string _tabStackInfo = string.Empty;
    readonly Command<object> _command;
    public ShellViewModel()
    {
        _command = new Command<object>(
            execute: param =>
            {
                CommandExecuted = param is string s && !string.IsNullOrEmpty(s)
                    ? $"Executed: {s}"
                    : "Executed";
                Shell.Current?.GoToAsync("..");
            },
            canExecute: _ => _isEnabled);
    }
    public event PropertyChangedEventHandler PropertyChanged;
    public ICommand Command => _command;
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
        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEnabledText));
                _command.ChangeCanExecute();
            }
        }
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
    public string NavigatingCurrent
    {
        get => _navigatingCurrent;
        set { if (_navigatingCurrent != value) { _navigatingCurrent = value; OnPropertyChanged(); } }
    }
    public string NavigatingSource
    {
        get => _navigatingSource;
        set { if (_navigatingSource != value) { _navigatingSource = value; OnPropertyChanged(); } }
    }
    public string NavigatingTarget
    {
        get => _navigatingTarget;
        set { if (_navigatingTarget != value) { _navigatingTarget = value; OnPropertyChanged(); } }
    }
    public string NavigatingCanCancel
    {
        get => _navigatingCanCancel;
        set { if (_navigatingCanCancel != value) { _navigatingCanCancel = value; OnPropertyChanged(); } }
    }
    public string NavigatingCancelled
    {
        get => _navigatingCancelled;
        set { if (_navigatingCancelled != value) { _navigatingCancelled = value; OnPropertyChanged(); } }
    }
    public string NavigatedCurrent
    {
        get => _navigatedCurrent;
        set { if (_navigatedCurrent != value) { _navigatedCurrent = value; OnPropertyChanged(); } }
    }
    public string NavigatedPrevious
    {
        get => _navigatedPrevious;
        set { if (_navigatedPrevious != value) { _navigatedPrevious = value; OnPropertyChanged(); } }
    }
    public string NavigatedSource
    {
        get => _navigatedSource;
        set { if (_navigatedSource != value) { _navigatedSource = value; OnPropertyChanged(); } }
    }
    public string RouteStatus
    {
        get => _routeStatus;
        set { if (_routeStatus != value) { _routeStatus = value; OnPropertyChanged(); } }
    }
    public bool CancelNavigation
    {
        get => _cancelNavigation;
        set { if (_cancelNavigation != value) { _cancelNavigation = value; OnPropertyChanged(); OnPropertyChanged(nameof(CancelNavigationText)); } }
    }
    public string CancelNavigationText => $"CancelNav: {_cancelNavigation}";
    public bool EnableDeferral
    {
        get => _enableDeferral;
        set { if (_enableDeferral != value) { _enableDeferral = value; OnPropertyChanged(); OnPropertyChanged(nameof(EnableDeferralText)); } }
    }
    public string EnableDeferralText => $"Deferral: {_enableDeferral}";
    public string DeferralStatus
    {
        get => _deferralStatus;
        set { if (_deferralStatus != value) { _deferralStatus = value; OnPropertyChanged(); } }
    }
    public string OverrideNavigatingStatus
    {
        get => _overrideNavigatingStatus;
        set { if (_overrideNavigatingStatus != value) { _overrideNavigatingStatus = value; OnPropertyChanged(); } }
    }
    public string OverrideNavigatedStatus
    {
        get => _overrideNavigatedStatus;
        set { if (_overrideNavigatedStatus != value) { _overrideNavigatedStatus = value; OnPropertyChanged(); } }
    }
    public string TabStackInfo
    {
        get => _tabStackInfo;
        set { if (_tabStackInfo != value) { _tabStackInfo = value; OnPropertyChanged(); } }
    }
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
