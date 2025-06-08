using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample;

public class SwitchViewModel : INotifyPropertyChanged
{
    private Color _backgroundColor = Colors.White;
    private FlowDirection _flowDirection = default;
    private bool _isEnabled = true;
    private bool _isVisible = true;
    private bool _isToggled = default;
    private Color _onColor = Colors.SkyBlue;
    private float _shadowOpacity = 0f;
    private Color _thumbColor = Colors.DarkBlue;

    public Color BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            if (_backgroundColor != value)
            {
                _backgroundColor = value;
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

    public bool IsToggled
    {
        get => _isToggled;
        set
        {
            if (_isToggled != value)
            {
                _isToggled = value;
                OnPropertyChanged();
            }
        }
    }

    public Color OnColor
    {
        get => _onColor;
        set
        {
            if (_onColor != value)
            {
                _onColor = value;
                OnPropertyChanged();
            }
        }
    }

    public float ShadowOpacity
    {
        get => _shadowOpacity;
        set
        {
            if (_shadowOpacity != value)
            {
                _shadowOpacity = value;
                OnPropertyChanged();
            }
        }
    }

    public Color ThumbColor
    {
        get => _thumbColor;
        set
        {
            if (_thumbColor != value)
            {
                _thumbColor = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
