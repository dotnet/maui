using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Windows.Input;

namespace Maui.Controls.Sample;

public class ShellViewModel : INotifyPropertyChanged
{
    private FlyoutBehavior _flyoutBehavior = FlyoutBehavior.Flyout;
    private FlyoutHeaderBehavior _flyoutHeaderBehavior = FlyoutHeaderBehavior.Default;
    private ScrollMode _flyoutVerticalScrollMode = ScrollMode.Enabled;
    private bool _flyoutIsPresented;
    private const double DefaultDimension = -1; // -1 = use Shell's default
    private double _flyoutWidth = DefaultDimension;
    private double _flyoutHeight = DefaultDimension;
    private Color _flyoutBackgroundColor;
    private Aspect _flyoutBackgroundImageAspect = Aspect.AspectFill;
    private string _flyoutIcon;
    private string _flyoutBackgroundImage;
    private object _flyoutHeader;
    private object _flyoutFooter;
    private DataTemplate _flyoutHeaderTemplate;
    private DataTemplate _flyoutFooterTemplate;
    private object _flyoutContent;
    private DataTemplate _flyoutContentTemplate;
    private FlyoutDisplayOptions _flyoutDisplayOptions = FlyoutDisplayOptions.AsSingleItem;
    private bool _flyoutItemIsVisible = true;
    private string _selectedFlyoutTemplate;
    private Brush _flyoutBackdrop = Brush.Default;
    private FlowDirection _flowDirection = FlowDirection.LeftToRight;
    private DataTemplate _itemTemplate;
    private Color _tabBarBackgroundColor;
    private Color _tabBarDisabledColor;
    private Color _tabBarForegroundColor;
    private Color _tabBarTitleColor;
    private Color _tabBarUnselectedColor;
    private bool _tabBarIsVisible = true;
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
	public ICommand Command { get; }


    public FlyoutDisplayOptions FlyoutDisplayOptions
    {
        get => _flyoutDisplayOptions;
        set { _flyoutDisplayOptions = value; OnPropertyChanged(); }
    }
    public bool FlyoutItemIsVisible
    {
        get => _flyoutItemIsVisible;
        set { _flyoutItemIsVisible = value; OnPropertyChanged(); }
    }
    public bool FlyoutIsPresented
    {
        get => _flyoutIsPresented;
        set { _flyoutIsPresented = value; OnPropertyChanged(); }
    }
    public object FlyoutHeader
    {
        get => _flyoutHeader;
        set { _flyoutHeader = value; OnPropertyChanged(); }
    }
    public object FlyoutFooter
    {
        get => _flyoutFooter;
        set { _flyoutFooter = value; OnPropertyChanged(); }
    }
    public DataTemplate FlyoutHeaderTemplate
    {
        get => _flyoutHeaderTemplate;
        set { _flyoutHeaderTemplate = value; OnPropertyChanged(); }
    }
    public DataTemplate FlyoutFooterTemplate
    {
        get => _flyoutFooterTemplate;
        set { _flyoutFooterTemplate = value; OnPropertyChanged(); }
    }
    public object FlyoutContent
    {
        get => _flyoutContent;
        set { _flyoutContent = value; OnPropertyChanged(); }
    }
    public DataTemplate FlyoutContentTemplate
    {
        get => _flyoutContentTemplate;
        set { _flyoutContentTemplate = value; OnPropertyChanged(); }
    }
    public FlyoutBehavior FlyoutBehavior
    {
        get => _flyoutBehavior;
        set { _flyoutBehavior = value; OnPropertyChanged(); }
    }
    public FlyoutHeaderBehavior FlyoutHeaderBehavior
    {
        get => _flyoutHeaderBehavior;
        set { _flyoutHeaderBehavior = value; OnPropertyChanged(); }
    }
    public ScrollMode FlyoutVerticalScrollMode
    {
        get => _flyoutVerticalScrollMode;
        set { _flyoutVerticalScrollMode = value; OnPropertyChanged(); }
    }
    public double FlyoutWidth
    {
        get => _flyoutWidth;
        set { _flyoutWidth = value; OnPropertyChanged(); }
    }
    public double FlyoutHeight
    {
        get => _flyoutHeight;
        set { _flyoutHeight = value; OnPropertyChanged(); }
    }
    public Color FlyoutBackgroundColor
    {
        get => _flyoutBackgroundColor;
        set { _flyoutBackgroundColor = value; OnPropertyChanged(); }
    }
    public Aspect FlyoutBackgroundImageAspect
    {
        get => _flyoutBackgroundImageAspect;
        set { _flyoutBackgroundImageAspect = value; OnPropertyChanged(); }
    }
    public string FlyoutIcon
    {
        get => _flyoutIcon;
        set { _flyoutIcon = value; OnPropertyChanged(); }
    }
    public string FlyoutBackgroundImage
    {
        get => _flyoutBackgroundImage;
        set { _flyoutBackgroundImage = value; OnPropertyChanged(); }
    }
    public Brush FlyoutBackdrop
    {
        get => _flyoutBackdrop;
        set { _flyoutBackdrop = value; OnPropertyChanged(); }
    }
    public string SelectedFlyoutTemplate
    {
        get => _selectedFlyoutTemplate;
        set
        {
            if (_selectedFlyoutTemplate != value)
            {
                _selectedFlyoutTemplate = value;
                OnPropertyChanged();
            }
        }
    }
    public DataTemplate ItemTemplate
    {
        get => _itemTemplate;
        set { _itemTemplate = value; OnPropertyChanged(); }
    }

    private DataTemplate _menuItemTemplate;
    public DataTemplate MenuItemTemplate
    {
        get => _menuItemTemplate;
        set { _menuItemTemplate = value; OnPropertyChanged(); }
    }

    public FlowDirection FlowDirection
    {
        get => _flowDirection;
        set { _flowDirection = value; OnPropertyChanged(); }
    }
    private string _currentItemTitle = "Home";
    public string CurrentItemTitle
    {
        get => _currentItemTitle;
        set { _currentItemTitle = value; OnPropertyChanged(); }
    }
    public Color TabBarBackgroundColor
    {
        get => _tabBarBackgroundColor;
        set
        {
            if (_tabBarBackgroundColor != value)
            {
                _tabBarBackgroundColor = value;
                OnPropertyChanged();
            }
        }
    }
    public Color TabBarDisabledColor
    {
        get => _tabBarDisabledColor;
        set
        {
            if (_tabBarDisabledColor != value)
            {
                _tabBarDisabledColor = value;
                OnPropertyChanged();
            }
        }
    }
    public Color TabBarForegroundColor
    {
        get => _tabBarForegroundColor;
        set
        {
            if (_tabBarForegroundColor != value)
            {
                _tabBarForegroundColor = value;
                OnPropertyChanged();
            }
        }
    }
    public Color TabBarTitleColor
    {
        get => _tabBarTitleColor;
        set
        {
            if (_tabBarTitleColor != value)
            {
                _tabBarTitleColor = value;
                OnPropertyChanged();
            }
        }
    }
    public Color TabBarUnselectedColor
    {
        get => _tabBarUnselectedColor;
        set
        {
            if (_tabBarUnselectedColor != value)
            {
                _tabBarUnselectedColor = value;
                OnPropertyChanged();
            }
        }
    }
    public bool TabBarIsVisible
    {
        get => _tabBarIsVisible;
        set
        {
            if (_tabBarIsVisible != value)
            {
                _tabBarIsVisible = value;
                OnPropertyChanged();
            }
        }
    }
    public ShellViewModel()
    {
        ItemTemplate = new DataTemplate(() =>
        {
            var label = new Label
            {
                Padding = new Thickness(10),
            };
            label.SetBinding(Label.TextProperty, "Title");
            return label;
        });

        MenuItemTemplate = new DataTemplate(() =>
        {
            var label = new Label
            {
                Padding = new Thickness(10),
            };
            label.SetBinding(Label.TextProperty, "Text");
            return label;
        });

        Command = new Command<object>(param =>
		{
			CommandExecuted = param is string s && !string.IsNullOrEmpty(s)
	? $"Executed: {s}"
	: "Executed";
			Shell.Current?.GoToAsync("..");
		});
    }
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

    public event PropertyChangedEventHandler PropertyChanged;
	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
