using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public class ShellViewModel : INotifyPropertyChanged
{
    private Color _backgroundColor;
    private Color _foregroundColor;
    private Color _titleColor;
    private Color _disabledColor;
    private Color _unselectedColor;
    private bool _navBarVisible = true;
    private bool _navBarHasShadow = true;
    private bool _navBarVisibilityAnimationEnabled = true;
    private PresentationMode _presentationMode = PresentationMode.Animated;
    private bool _isEnabled = true;
    private bool _isVisible = true;
    private FlowDirection _flowDirection = FlowDirection.LeftToRight;
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

    public bool IsEnabled
    {
        get => _isEnabled;
        set { _isEnabled = value; OnPropertyChanged(); }
    }

    public bool IsVisible
    {
        get => _isVisible;
        set { _isVisible = value; OnPropertyChanged(); }
    }

    public FlowDirection FlowDirection
    {
        get => _flowDirection;
        set { _flowDirection = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
