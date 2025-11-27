using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public class ShellViewModel : INotifyPropertyChanged
{
    private Color _tabBarBackgroundColor;
    private Color _tabBarDisabledColor;
    private Color _tabBarForegroundColor;
    private Color _tabBarTitleColor;
    private Color _tabBarUnselectedColor;
    private bool _tabBarIsVisible = true;
    private bool _isEnabled = true;
    private bool _isVisible = true;
    private FlowDirection _flowDirection = FlowDirection.LeftToRight;

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
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
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
    private string _currentItemTitle = "Tabs";
    public string CurrentItemTitle
    {
        get => _currentItemTitle;
        set
        {
            if (_currentItemTitle != value)
            {
                _currentItemTitle = value;
                OnPropertyChanged(nameof(CurrentItemTitle));
            }
        }
    }
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string name = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}