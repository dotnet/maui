using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

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
    private Color _backgroundColor;
    private Color _foregroundColor;
    private Color _titleColor;
    private Color _disabledColor;
    private Color _unselectedColor;
    private bool _navBarVisible = true;
    private bool _navBarHasShadow = true;
    private bool _navBarVisibilityAnimationEnabled = true;
    private PresentationMode _presentationMode = PresentationMode.Animated;
    private Color _cancelButtonColor;
    private Color _textColor;
    private Color _placeholderColor;
    private double _characterSpacing = 0d;
    private FontAttributes _fontAttributes = FontAttributes.None;
    private bool _fontAutoScalingEnabled = true;
    private string _fontFamily;
    private double _fontSize = 16d;
    private TextAlignment _horizontalTextAlignment = TextAlignment.Start;
    private TextAlignment _verticalTextAlignment = TextAlignment.Center;
    private TextTransform _textTransform = TextTransform.Default;
    private Keyboard _keyboard = Keyboard.Default;
    private string _placeholder = "Search fruits or birds";
    private bool _isSearchEnabled = true;
    private bool _showsResults = false;
    private SearchBoxVisibility _searchBoxVisibility = SearchBoxVisibility.Expanded;
    private string _query = string.Empty;
    private object _selectedItem;
    private string _itemsSourceMode = "Query";
    private ImageSource _clearIcon;
    private ImageSource _queryIcon;
    private bool _clearPlaceholderEnabled = false;
    private ImageSource _clearPlaceholderIcon;
    private string _clearPlaceholderCommandParameter = string.Empty;
    private DataTemplate _searchItemTemplate;

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

    public Color BackgroundColor
    {
        get => _backgroundColor;
        set { _backgroundColor = value; OnPropertyChanged(); }
    }

    public Color ForegroundColor
    {
        get => _foregroundColor;
        set { _foregroundColor = value; OnPropertyChanged(); }
    }

    public Color TitleColor
    {
        get => _titleColor;
        set { _titleColor = value; OnPropertyChanged(); }
    }

    public Color DisabledColor
    {
        get => _disabledColor;
        set { _disabledColor = value; OnPropertyChanged(); }
    }

    public Color UnselectedColor
    {
        get => _unselectedColor;
        set { _unselectedColor = value; OnPropertyChanged(); }
    }

    public bool NavBarIsVisible
    {
        get => _navBarVisible;
        set { _navBarVisible = value; OnPropertyChanged(); }
    }

    public bool NavBarHasShadow
    {
        get => _navBarHasShadow;
        set { _navBarHasShadow = value; OnPropertyChanged(); }
    }

    public bool NavBarVisibilityAnimationEnabled
    {
        get => _navBarVisibilityAnimationEnabled;
        set { _navBarVisibilityAnimationEnabled = value; OnPropertyChanged(); }
    }

    public PresentationMode PresentationMode
    {
        get => _presentationMode;
        set { _presentationMode = value; OnPropertyChanged(); }
    }

    public void Reset()
    {
        BackgroundColor = null;
        CancelButtonColor = null;
        TextColor = null;
        PlaceholderColor = null;
        CharacterSpacing = 0d;
        FontAttributes = FontAttributes.None;
        FontAutoScalingEnabled = true;
        FontFamily = null;
        FontSize = 16d;
        HorizontalTextAlignment = TextAlignment.Start;
        VerticalTextAlignment = TextAlignment.Center;
        TextTransform = TextTransform.Default;
        Keyboard = Keyboard.Default;
        Placeholder = "Search fruits or birds";
        IsSearchEnabled = true;
        ShowsResults = false;
        SearchBoxVisibility = SearchBoxVisibility.Expanded;
        Query = string.Empty;
        SelectedItem = null;
        ItemTemplate = BuildSimpleTemplate();
        SearchItemTemplate = BuildSimpleTemplate();
        ItemsSourceMode = "Query";
        ClearIcon = null;
        QueryIcon = null;
        ClearPlaceholderEnabled = false;
        ClearPlaceholderIcon = null;
        ClearPlaceholderCommandParameter = string.Empty;
        CommandParameter = string.Empty;
        QueryChangedLog = string.Empty;
        FocusStatus = string.Empty;
        IsFocused = false;
        CommandFired = string.Empty;
        ClearPlaceholderCommandFired = string.Empty;
    }
    public Color CancelButtonColor
    {
        get => _cancelButtonColor;
        set { _cancelButtonColor = value; OnPropertyChanged(); }
    }
    public Color TextColor
    {
        get => _textColor;
        set { _textColor = value; OnPropertyChanged(); }
    }
    public Color PlaceholderColor
    {
        get => _placeholderColor;
        set { _placeholderColor = value; OnPropertyChanged(); }
    }
    public double CharacterSpacing
    {
        get => _characterSpacing;
        set { _characterSpacing = value; OnPropertyChanged(); }
    }
    public FontAttributes FontAttributes
    {
        get => _fontAttributes;
        set { _fontAttributes = value; OnPropertyChanged(); }
    }
    public bool FontAutoScalingEnabled
    {
        get => _fontAutoScalingEnabled;
        set { _fontAutoScalingEnabled = value; OnPropertyChanged(); }
    }
    public string FontFamily
    {
        get => _fontFamily;
        set { _fontFamily = value; OnPropertyChanged(); }
    }
    public double FontSize
    {
        get => _fontSize;
        set { _fontSize = value; OnPropertyChanged(); }
    }
    public TextAlignment HorizontalTextAlignment
    {
        get => _horizontalTextAlignment;
        set { _horizontalTextAlignment = value; OnPropertyChanged(); }
    }
    public TextAlignment VerticalTextAlignment
    {
        get => _verticalTextAlignment;
        set { _verticalTextAlignment = value; OnPropertyChanged(); }
    }
    public TextTransform TextTransform
    {
        get => _textTransform;
        set { _textTransform = value; OnPropertyChanged(); }
    }

    public Keyboard Keyboard
    {
        get => _keyboard;
        set { _keyboard = value; OnPropertyChanged(); }
    }
    public string Placeholder
    {
        get => _placeholder;
        set { _placeholder = value; OnPropertyChanged(); }
    }

    public bool IsSearchEnabled
    {
        get => _isSearchEnabled;
        set
        {
            _isSearchEnabled = value;
            OnPropertyChanged();
            (SearchCommand as Command)?.ChangeCanExecute();
        }
    }
    public bool ShowsResults
    {
        get => _showsResults;
        set { _showsResults = value; OnPropertyChanged(); }
    }
    public SearchBoxVisibility SearchBoxVisibility
    {
        get => _searchBoxVisibility;
        set { _searchBoxVisibility = value; OnPropertyChanged(); }
    }
    public string Query
    {
        get => _query;
        set { _query = value; OnPropertyChanged(); }
    }
    public object SelectedItem
    {
        get => _selectedItem;
        set { _selectedItem = value; OnPropertyChanged(); }
    }

    public string ItemsSourceMode
    {
        get => _itemsSourceMode;
        set { _itemsSourceMode = value; OnPropertyChanged(); }
    }

    public static DataTemplate BuildSimpleTemplate()
    {
        return new DataTemplate(() =>
        {
            var label = new Label
            {
                AutomationId = "SearchResultName",
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(10, 5),
                FontSize = 20,
            };
            label.SetBinding(Label.TextProperty, static (string s) => s);
            return label;
        });
    }

    public static DataTemplate BuildCustomTemplate()
    {
        return new DataTemplate(() =>
        {
            var image = new Image
            {
                Source = ImageSource.FromFile("dotnet_bot.png"),
                WidthRequest = 24,
                HeightRequest = 24,
                VerticalOptions = LayoutOptions.Center,
            };

            var label = new Label
            {
                AutomationId = "SearchResultName",
                VerticalOptions = LayoutOptions.Center,
                TextColor = Colors.MediumVioletRed,
                FontAttributes = FontAttributes.Bold,
            };
            label.SetBinding(Label.TextProperty, static (string s) => s);

            return new HorizontalStackLayout
            {
                Spacing = 10,
                Padding = new Thickness(10, 5),
                Children =
                {
                    image,
                    label
                }
            };
        });
    }

    public ImageSource ClearIcon
    {
        get => _clearIcon;
        set { _clearIcon = value; OnPropertyChanged(); }
    }

    public ImageSource QueryIcon
    {
        get => _queryIcon;
        set { _queryIcon = value; OnPropertyChanged(); }
    }

    public bool ClearPlaceholderEnabled
    {
        get => _clearPlaceholderEnabled;
        set
        {
            _clearPlaceholderEnabled = value;
            OnPropertyChanged();
            ((Command<object>)ClearPlaceholderCommand)?.ChangeCanExecute();
        }
    }
    public ImageSource ClearPlaceholderIcon
    {
        get => _clearPlaceholderIcon;
        set { _clearPlaceholderIcon = value; OnPropertyChanged(); }
    }
    public string ClearPlaceholderCommandParameter
    {
        get => _clearPlaceholderCommandParameter;
        set { _clearPlaceholderCommandParameter = value; OnPropertyChanged(); }
    }
    public ICommand SearchCommand { get; }
    public ICommand ClearPlaceholderCommand { get; }
    private string _commandFired = string.Empty;
    public string CommandFired
    {
        get => _commandFired;
        set { _commandFired = value; OnPropertyChanged(); }
    }
    private string _clearPlaceholderCommandFired = string.Empty;
    public string ClearPlaceholderCommandFired
    {
        get => _clearPlaceholderCommandFired;
        set { _clearPlaceholderCommandFired = value; OnPropertyChanged(); }
    }
    private string _queryChangedLog = string.Empty;
    public string QueryChangedLog
    {
        get => _queryChangedLog;
        set { _queryChangedLog = value; OnPropertyChanged(); }
    }
    private string _focusStatus = string.Empty;
    public string FocusStatus
    {
        get => _focusStatus;
        set { _focusStatus = value; OnPropertyChanged(); }
    }
    private bool _isFocused;
    public bool IsFocused
    {
        get => _isFocused;
        set { _isFocused = value; OnPropertyChanged(); }
    }

    public DataTemplate SearchItemTemplate
    {
        get => _searchItemTemplate;
        set { _searchItemTemplate = value; OnPropertyChanged(); }
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

        _command = new Command<object>(
            execute: param =>
            {
                CommandExecuted = param is string s && !string.IsNullOrEmpty(s)
                    ? $"Executed: {s}"
                    : "Executed";
                Shell.Current?.GoToAsync("..");
            },
            canExecute: _ => _isEnabled);

        SearchItemTemplate = BuildSimpleTemplate();

        SearchCommand = new Command<object>(p =>
        {
            CommandFired = $"QueryConfirmed:{_query}|Param:{p}";
        },
        _ => IsSearchEnabled);

        ClearPlaceholderCommand = new Command<object>(p =>
        {
            ClearPlaceholderCommandFired = $"ClearPlaceholder:{p}";
        },
        _ => ClearPlaceholderEnabled);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}