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
    private string _flyoutIcon ;
    private string _flyoutBackgroundImage;
    private object _flyoutHeader;
    private object _flyoutFooter;
    private DataTemplate _flyoutHeaderTemplate;
    private DataTemplate _flyoutFooterTemplate;
    private FlyoutDisplayOptions _flyoutDisplayOptions = FlyoutDisplayOptions.AsSingleItem;
    private bool _flyoutItemIsVisible = true;
    private string _selectedFlyoutTemplate;
    private Brush _flyoutBackdrop = Brush.Default;
    private FlowDirection _flowDirection = FlowDirection.LeftToRight;
    private bool _isEnabled = true;
    private DataTemplate _flyoutItemTemplate;
 
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
    public DataTemplate FlyoutItemTemplate
    {
        get => _flyoutItemTemplate;
        set { _flyoutItemTemplate = value; OnPropertyChanged(); }
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

    public bool IsEnabled
    {
        get => _isEnabled;
        set { _isEnabled = value; OnPropertyChanged(); }
    }

    private string _currentItemTitle = "Home";
    public string CurrentItemTitle
    {
        get => _currentItemTitle;
        set { _currentItemTitle = value; OnPropertyChanged(); }
    }

    public ShellViewModel()
    {
        FlyoutItemTemplate = new DataTemplate(() =>
        {
            var grid = new Grid 
            { 
                Padding = new Thickness(10),
                Margin = new Thickness(5, 2),  
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            var label = new Label
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,  
                TextColor = Colors.DarkBlue,
                FontSize = 16,

            };
            label.SetBinding(Label.TextProperty, "Title");
            grid.Children.Add(label);
            Grid.SetColumn(label, 0);
            Grid.SetRow(label, 0);
            return grid;
        });

        MenuItemTemplate = new DataTemplate(() =>
        {
            var grid = new Grid 
            { 
                Padding = new Thickness(10),
                Margin = new Thickness(5, 2),  
                
                BackgroundColor = Colors.Transparent
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            var label = new Label 
            { 
                VerticalOptions = LayoutOptions.Center, 
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Colors.DarkRed,
                FontSize = 16,
                
            };
            label.SetBinding(Label.TextProperty, "Text");
            grid.Children.Add(label);
            Grid.SetColumn(label, 0);
            Grid.SetRow(label, 0);
            return grid;
        });
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string name = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
