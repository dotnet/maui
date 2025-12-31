using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;
public class ShellViewModel : INotifyPropertyChanged
{
    Color _backgroundColor ;
    Color _foregroundColor;
    Color _titleColor ;
    Color _disabledColor ;
    Color _unselectedColor ;
    bool _navBarVisible = true;
    bool _navBarHasShadow = true;
    bool _navBarVisibilityAnimationEnabled = true;
    PresentationMode _presentationMode = PresentationMode.Animated;
    public Color ShellBackgroundColor
    {
        get => _backgroundColor;
        set { _backgroundColor = value; OnPropertyChanged(); }
    }

    public Color ShellForegroundColor
    {
        get => _foregroundColor;
        set { _foregroundColor = value; OnPropertyChanged(); }
    }

    public Color ShellTitleColor
    {
        get => _titleColor;
        set { _titleColor = value; OnPropertyChanged(); }
    }

    public Color ShellDisabledColor
    {
        get => _disabledColor;
        set { _disabledColor = value; OnPropertyChanged(); }
    }

    public Color ShellUnselectedColor
    {
        get => _unselectedColor;
        set { _unselectedColor = value; OnPropertyChanged(); }
    }

    public bool ShellNavBarIsVisible
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

    

    public event PropertyChangedEventHandler PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
