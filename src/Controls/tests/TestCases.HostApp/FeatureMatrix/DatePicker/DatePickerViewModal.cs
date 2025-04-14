using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample;

public class DatePickerViewModal : INotifyPropertyChanged
{
    private double _characterSpacing = default;
    private DateTime _date = DateTime.ParseExact("01/01/2025", "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
    private FontAttributes _fontAttributes = default;
    private string _fontFamily = default;
    private double _fontSize = default;
    private string _format = "MM/dd/yyyy";
    private DateTime _minDate = DateTime.ParseExact("01/01/2025", "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
    private DateTime _maxDate = DateTime.ParseExact("01/01/2027", "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
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
