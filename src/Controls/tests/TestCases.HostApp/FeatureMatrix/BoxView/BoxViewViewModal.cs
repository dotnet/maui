using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample;

public class BoxViewViewModal : INotifyPropertyChanged
{
    private Color _color = Colors.Blue;
    private double _width = 200;
    private double _height = 100;
    private bool _isVisible = true;
    private double _opacity = 1.0;
    public void ResetToDefaults()
    {
        Color = Colors.Blue;
        Opacity = 1.0;
        CornerRadius = 0;
        Width = 200;
        Height = 100;
        IsVisible = true;
         HasShadow = false;
    }
    public double Opacity
    {
        get => _opacity;
        set
        {
            if (_opacity != value)
            {
                _opacity = value;
                OnPropertyChanged(nameof(Opacity));
            }
        }
    }
    private CornerRadius _cornerRadius;
    public CornerRadius CornerRadius
    {
        get => _cornerRadius;
        set
        {
            if (_cornerRadius != value)
            {
                _cornerRadius = value;
                OnPropertyChanged(nameof(CornerRadius));
            }
        }
    }
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            _isVisible = value;
            OnPropertyChanged();
        }
    }
    private bool _hasShadow;
    private Shadow _boxShadow;

    public bool HasShadow
    {
        get => _hasShadow;
        set
        {
            if (_hasShadow != value)
            {
                _hasShadow = value;
                BoxShadow = value
                    ? new Shadow
                    {
                        Radius = 10,
                        Opacity = 1.0f,
                        Brush = Colors.Black.AsPaint(),
                        Offset = new Point(5, 5)
                    }
                    : null;
                OnPropertyChanged(nameof(HasShadow));
            }
        }
    }

    public Shadow BoxShadow
    {
        get => _boxShadow;
        private set
        {
            if (_boxShadow != value)
            {
                _boxShadow = value;
                OnPropertyChanged(nameof(BoxShadow));
            }
        }
    }

    public Color Color
    {
        get => _color;
        set { _color = value; OnPropertyChanged(); }
    }

    public double Width
    {
        get => _width;
        set { _width = value; OnPropertyChanged(); }
    }

    public double Height
    {
        get => _height;
        set { _height = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}