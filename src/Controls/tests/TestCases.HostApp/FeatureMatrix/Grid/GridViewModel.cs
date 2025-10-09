using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public class GridViewModel : INotifyPropertyChanged
{
    private double _rowSpacing = 0;
    private double _columnSpacing = 0;
    private Thickness _padding;
    private Color _backgroundColor = Colors.White;
    private bool _isVisible = true;
    private LayoutOptions _horizontalOptions = LayoutOptions.Fill;
    private LayoutOptions _verticalOptions = LayoutOptions.Fill;
    private FlowDirection _flowDirection = FlowDirection.LeftToRight;
    private string _rowDefinitionType = "Star";
    private string _columnDefinitionType = "Star";
    private string _rowDefinitionValue = "1";
    private string _columnDefinitionValue = "1";
    private int _row = 0;
    private int _column = 0;
    private int _mainContentRowSpan = 1;
    private int _mainContentColumnSpan = 1;

    public GridViewModel()
    {
        Row = 2;
        Column = 2;
        RowSpacing = 2;
        ColumnSpacing = 2;
        Padding = new Thickness(2);
    }
    public int MainContentRowSpan
    {
        get => _mainContentRowSpan;
        set
        {
            if (_mainContentRowSpan != value)
            {
                _mainContentRowSpan = value;
                OnPropertyChanged();
            }
        }
    }

    public int MainContentColumnSpan
    {
        get => _mainContentColumnSpan;
        set
        {
            if (_mainContentColumnSpan != value)
            {
                _mainContentColumnSpan = value;
                OnPropertyChanged();
            }
        }
    }

    public double RowSpacing
    {
        get => _rowSpacing;
        set
        {
            if (_rowSpacing != value)
            {
                _rowSpacing = value;
                OnPropertyChanged();
            }
        }
    }

    public double ColumnSpacing
    {
        get => _columnSpacing;
        set
        {
            if (_columnSpacing != value)
            {
                _columnSpacing = value;
                OnPropertyChanged();
            }
        }
    }

    public Thickness Padding
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

    public LayoutOptions HorizontalOptions
    {
        get => _horizontalOptions;
        set
        {
            if (_horizontalOptions != value)
            {
                _horizontalOptions = value;
                OnPropertyChanged();
            }
        }
    }

    public LayoutOptions VerticalOptions
    {
        get => _verticalOptions;
        set
        {
            if (_verticalOptions != value)
            {
                _verticalOptions = value;
                OnPropertyChanged();
            }
        }
    }

    public int Row
    {
        get => _row;
        set
        {
            if (_row != value)
            {
                _row = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RowDefinitions));
            }
        }
    }

    private bool _showNestedGrid;
    public bool ShowNestedGrid
    {
        get => _showNestedGrid;
        set
        {
            if (_showNestedGrid != value)
            {
                _showNestedGrid = value;
                OnPropertyChanged(nameof(ShowNestedGrid));
            }
        }
    }

    public int Column
    {
        get => _column;
        set
        {
            if (_column != value)
            {
                _column = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ColumnDefinitions));
            }
        }
    }

    public string RowDefinitionType
    {
        get => _rowDefinitionType;
        set
        {
            if (_rowDefinitionType != value)
            {
                _rowDefinitionType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RowDefinitions));
            }
        }
    }

    public string ColumnDefinitionType
    {
        get => _columnDefinitionType;
        set
        {
            if (_columnDefinitionType != value)
            {
                _columnDefinitionType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ColumnDefinitions));
            }
        }
    }

    public string RowDefinitionValue
    {
        get => _rowDefinitionValue;
        set
        {
            if (_rowDefinitionValue != value)
            {
                _rowDefinitionValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RowDefinitions));
            }
        }
    }
    
    public string ColumnDefinitionValue
    {
        get => _columnDefinitionValue;
        set
        {
            if (_columnDefinitionValue != value)
            {
                _columnDefinitionValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ColumnDefinitions));
            }
        }
    }

    public RowDefinitionCollection RowDefinitions
    {
        get
        {
            var defs = new RowDefinitionCollection();
            double value = TryParseOrDefault(RowDefinitionValue, 1);

            for (int i = 0; i < Row; i++)
            {
                switch (RowDefinitionType)
                {
                    case "Absolute":
                        defs.Add(new RowDefinition { Height = new GridLength(value, GridUnitType.Absolute) });
                        break;
                    case "Auto":
                        defs.Add(new RowDefinition { Height = GridLength.Auto });
                        break;
                    default:
                        defs.Add(new RowDefinition { Height = new GridLength(value, GridUnitType.Star) });
                        break;
                }
            }
            return defs;
        }
    }

    public ColumnDefinitionCollection ColumnDefinitions
    {
        get
        {
            var defs = new ColumnDefinitionCollection();
            double value = TryParseOrDefault(ColumnDefinitionValue, 1);

            for (int i = 0; i < Column; i++)
            {
                switch (ColumnDefinitionType)
                {
                    case "Absolute":
                        defs.Add(new ColumnDefinition { Width = new GridLength(value, GridUnitType.Absolute) });
                        break;
                    case "Auto":
                        defs.Add(new ColumnDefinition { Width = GridLength.Auto });
                        break;
                    default:
                        defs.Add(new ColumnDefinition { Width = new GridLength(value, GridUnitType.Star) });
                        break;
                }
            }
            return defs;
        }
    }

    private double TryParseOrDefault(string input, double defaultValue)
        => double.TryParse(input, out var result) ? result : defaultValue;

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}