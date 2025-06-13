using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample;

public class DatePickerViewModal : INotifyPropertyChanged
{
    private double _characterSpacing = 0.0d;
    private DateTime _date = DateTime.ParseExact("12/01/2025", "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
    private FlowDirection _flowDirection = FlowDirection.LeftToRight;
    private FontAttributes _fontAttributes = FontAttributes.None;
    private string _fontFamily = null;
    private double _fontSize = 0d;
    private string _format = "MM/dd/yyyy";
    private bool _isEnabled = true;
    private bool _isVisible = true;
    private DateTime _minDate = DateTime.ParseExact("12/01/2025", "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
    private DateTime _maxDate = DateTime.ParseExact("12/01/2027", "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
    private float _shadowOpacity = 0f;
    private Color _textColor = Colors.Black;

    public double CharacterSpacing
    {
        get => _characterSpacing;
        set
        {
            if (_characterSpacing != value)
            {
                _characterSpacing = value;
                OnPropertyChanged();
            }
        }
    }

    public DateTime Date
    {
        get => _date;
        set
        {
            if (_date != value)
            {
                _date = value;
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

    public FontAttributes FontAttributes
    {
        get => _fontAttributes;
        set
        {
            if (_fontAttributes != value)
            {
                _fontAttributes = value;
                OnPropertyChanged();
            }
        }
    }

    public string FontFamily
    {
        get => _fontFamily;
        set
        {
            if (_fontFamily != value)
            {
                _fontFamily = value;
                OnPropertyChanged();
            }
        }
    }

    public double FontSize
    {
        get => _fontSize;
        set
        {
            if (_fontSize != value)
            {
                _fontSize = value;
                OnPropertyChanged();
            }
        }
    }

    public string Format
    {
        get => _format;
        set
        {
            if (_format != value)
            {
                _format = value;
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

    public DateTime MaximumDate
    {
        get => _maxDate;
        set
        {
            if (_maxDate != value)
            {
                _maxDate = value;
                OnPropertyChanged();
            }
        }
    }

    public DateTime MinimumDate
    {
        get => _minDate;
        set
        {
            if (_minDate != value)
            {
                _minDate = value;
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
    public Color TextColor
    {
        get => _textColor;
        set
        {
            if (_textColor != value)
            {
                _textColor = value;
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
