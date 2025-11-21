using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
namespace Maui.Controls.Sample;

public class ShellViewModel : INotifyPropertyChanged
{
    private FlyoutBehavior _flyoutBehavior = FlyoutBehavior.Flyout;
    private FlyoutHeaderBehavior _flyoutHeaderBehavior = FlyoutHeaderBehavior.Default;
    private ScrollMode _flyoutVerticalScrollMode = ScrollMode.Enabled;
    private bool _flyoutIsPresented;
    private double _flyoutWidth = -1;
    private double _flyoutHeight = -1;
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
    }
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string name = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
