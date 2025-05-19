using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample;

public class ButtonViewModal : INotifyPropertyChanged
{
    private Color _borderColor = default;
    private double _borderWidth = default;
    private double _characterSpacing = default;
    private ICommand _command = default;
    private int _cornerRadius = default;
    private FontAttributes _fontAttributes = default;
    private string _fontFamily = default;
    private double _fontSize = default;
    private LineBreakMode _lineBreakMode = default;
    private double _padding = default;
    private string _text = default;
    private Color _textColor = Colors.White;
    private TextTransform _textTransform = default;

    public Color BorderColor
    {
        get => _borderColor;
        set
        {
            if (_borderColor != value)
            {
                _borderColor = value;
                OnPropertyChanged();
            }
        }
    }
    public double BorderWidth
    {
        get => _borderWidth;
        set
        {
            if (_borderWidth != value)
            {
                _borderWidth = value;
                OnPropertyChanged();
            }
        }
    }
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
    public ICommand Command
    {
        get => _command;
        set
        {
            if (_command != value)
            {
                _command = value;
                OnPropertyChanged();
            }
        }
    }
    public int CornerRadius
    {
        get => _cornerRadius;
        set
        {
            if (_cornerRadius != value)
            {
                _cornerRadius = value;
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
    public LineBreakMode LineBreakMode
    {
        get => _lineBreakMode;
        set
        {
            if (_lineBreakMode != value)
            {
                _lineBreakMode = value;
                OnPropertyChanged();
            }
        }
    }
    public double Padding
    {
        get => _padding;
        set
        {
            if (_padding != value)
            {
                _padding = value;
                OnPropertyChanged();
            }
        }
    }
    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
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
    public TextTransform TextTransform
    {
        get => _textTransform;
        set
        {
            if (_textTransform != value)
            {
                _textTransform = value;
                OnPropertyChanged();
            }
        }
    }

    public ButtonViewModal()
    {
        Command = new Command<string>(
            execute: (buttonText) =>
            {
                if (buttonText == "Command Text")
                {
                    Text = "Command Executed";
                }
            },
            canExecute: (buttonText) => buttonText == "Command Text"
        );
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
