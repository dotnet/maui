using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample;

public class DatePickerViewModal : INotifyPropertyChanged
{
    private double _characterSpacing = default;
    private string _date = DateTime.Today.ToString("d");
    private FontAttributes _fontAttributes = default;
    private string _fontFamily = default;
    private double _fontSize = default;
    private string _format = "d";
    private string _maxDate = DateTime.Today.AddYears(1).ToString("d");
    private string _minDate = DateTime.Today.AddYears(-1).ToString("d");
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
    public string Date
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
    public string MaximumDate
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
    public string MinimumDate
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
