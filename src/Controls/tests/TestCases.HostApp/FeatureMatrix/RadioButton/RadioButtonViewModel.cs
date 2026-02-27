using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class RadioButtonViewModel : INotifyPropertyChanged
{
	private Color _borderColor = Colors.Transparent;
	private double _borderWidth = -1d;
	private double _characterSpacing = 0.0d;
	private object _content = "Dark Mode";
	private int _cornerRadius = -1;
	private FlowDirection _flowDirection = FlowDirection.LeftToRight;
	private FontAttributes _fontAttributes = FontAttributes.None;
	private bool _fontAutoScalingEnabled = true;
	private string _fontFamily = null;
	private double _fontSize = 14d;
	private string _groupName = "Theme";
	private bool _isChecked = false;
	private bool _isEnabled = true;
	private bool _isVisible = true;
	private Color _textColor = Colors.Black;
	private TextTransform _textTransform = TextTransform.Default;
	private object _selectedValue = null;
	private object _value = "One";

	public event PropertyChangedEventHandler PropertyChanged;

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

	public object Content
	{
		get => _content;
		set
		{
			if (_content != value)
			{
				_content = value;
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

	public bool FontAutoScalingEnabled
	{
		get => _fontAutoScalingEnabled;
		set
		{
			if (_fontAutoScalingEnabled != value)
			{
				_fontAutoScalingEnabled = value;
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

	public string GroupName
	{
		get => _groupName;
		set
		{
			if (_groupName != value)
			{
				_groupName = value;
				OnPropertyChanged();
			}
		}
	}

	public bool IsChecked
	{
		get => _isChecked;
		set
		{
			if (_isChecked != value)
			{
				_isChecked = value;
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
	public object Value
	{
		get => _value;
		set
		{
			if (_value != value)
			{
				_value = value;
				OnPropertyChanged();
			}
		}
	}

	public object SelectedValue
	{
		get => _selectedValue;
		set
		{
			if (_selectedValue != value)
			{
				_selectedValue = value;
				OnPropertyChanged();
			}
		}
	}

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}