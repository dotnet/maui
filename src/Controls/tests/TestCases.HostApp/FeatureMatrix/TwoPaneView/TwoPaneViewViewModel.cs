using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Foldable;
namespace Maui.Controls.Sample;

public class TwoPaneViewViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    private GridLength _pane1Length = new GridLength(1, GridUnitType.Star);
    private GridLength _pane2Length = new GridLength(1, GridUnitType.Star);
    private double _minTallModeHeight = 500;
    private double _minWideModeWidth = 700;
    private bool _isShadowEnabled = false;
    private bool _isVisible = true;
    private FlowDirection _flowDirection = FlowDirection.LeftToRight;
    private TwoPaneViewTallModeConfiguration _tallModeConfiguration = TwoPaneViewTallModeConfiguration.TopBottom;
    private TwoPaneViewWideModeConfiguration _wideModeConfiguration = TwoPaneViewWideModeConfiguration.LeftRight;
    private TwoPaneViewPriority _panePriority = TwoPaneViewPriority.Pane1;

    public bool IsShadowEnabled
    {
        get => _isShadowEnabled;
        set
        {
            if (_isShadowEnabled != value)
            {
                _isShadowEnabled = value;
                OnPropertyChanged();
            }
        }
    }
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible != value)
            {
                _isVisible = value;
                OnPropertyChanged();
            }
        }
    }

    public FlowDirection FlowDirection
    {
        get => _flowDirection;
        set
        {
            if (_flowDirection != value)
            {
                _flowDirection = value;
                OnPropertyChanged();
            }
        }
    }

    public GridLength Pane1Length
    {
        get => _pane1Length;
        set
        {
            _pane1Length = value;
            OnPropertyChanged();
        }
    }
    public GridLength Pane2Length
    {
        get => _pane2Length;
        set
        {
            _pane2Length = value;
            OnPropertyChanged();
        }
    }
    public double MinTallModeHeight
    {
        get => _minTallModeHeight;
        set
        {
            _minTallModeHeight = value;
            OnPropertyChanged();
        }
    }
    public double MinWideModeWidth
    {
        get => _minWideModeWidth;
        set
        {
            _minWideModeWidth = value;
            OnPropertyChanged();
        }
    }
    public TwoPaneViewTallModeConfiguration TallModeConfiguration
    {
        get => _tallModeConfiguration;
        set
        {
            if (_tallModeConfiguration != value)
            {
                _tallModeConfiguration = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsTallModeTopBottom));
                OnPropertyChanged(nameof(IsTallModeSinglePane));
            }
        }
    }

    public TwoPaneViewWideModeConfiguration WideModeConfiguration
    {
        get => _wideModeConfiguration;
        set
        {
            if (_wideModeConfiguration != value)
            {
                _wideModeConfiguration = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsWideModeLeftRight));
                OnPropertyChanged(nameof(IsWideModeSinglePane));
            }
        }
    }

    public TwoPaneViewPriority PanePriority
    {
        get => _panePriority;
        set
        {
            if (_panePriority != value)
            {
                _panePriority = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPanePriorityPane1));
                OnPropertyChanged(nameof(IsPanePriorityPane2));
            }
        }
    }

    // Computed properties for radio button bindings
    public bool IsTallModeTopBottom
    {
        get => TallModeConfiguration == TwoPaneViewTallModeConfiguration.TopBottom;
        set { if (value) TallModeConfiguration = TwoPaneViewTallModeConfiguration.TopBottom; }
    }
    public bool IsTallModeSinglePane
    {
        get => TallModeConfiguration == TwoPaneViewTallModeConfiguration.SinglePane;
        set { if (value) TallModeConfiguration = TwoPaneViewTallModeConfiguration.SinglePane; }
    }

    public bool IsWideModeLeftRight
    {
        get => WideModeConfiguration == TwoPaneViewWideModeConfiguration.LeftRight;
        set { if (value) WideModeConfiguration = TwoPaneViewWideModeConfiguration.LeftRight; }
    }
    public bool IsWideModeSinglePane
    {
        get => WideModeConfiguration == TwoPaneViewWideModeConfiguration.SinglePane;
        set { if (value) WideModeConfiguration = TwoPaneViewWideModeConfiguration.SinglePane; }
    }

    public bool IsPanePriorityPane1
    {
        get => PanePriority == TwoPaneViewPriority.Pane1;
        set { if (value) PanePriority = TwoPaneViewPriority.Pane1; }
    }
    public bool IsPanePriorityPane2
    {
        get => PanePriority == TwoPaneViewPriority.Pane2;
        set { if (value) PanePriority = TwoPaneViewPriority.Pane2; }
    }

    // Property to track if the current mode is Wide mode
    // This would typically be set by the TwoPaneView control itself
    private bool _isWideMode = false;
    public bool IsWideMode
    {
        get => _isWideMode;
        set
        {
            if (_isWideMode != value)
            {
                _isWideMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentModeText));
            }
        }
    }

    public string CurrentModeText => IsWideMode ? "Wide Mode" : "Tall Mode";

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
