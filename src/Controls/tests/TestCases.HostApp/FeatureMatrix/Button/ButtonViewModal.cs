using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample;

public class ButtonViewModal : INotifyPropertyChanged
{
	private Color _borderColor = Colors.White;
	private double _borderWidth = 0d;
	private double _characterSpacing = 0.0d;
	private ICommand _command = null;
	private int _cornerRadius = 0;
	private FlowDirection _flowDirection = FlowDirection.MatchParent;
	private FontAttributes _fontAttributes = FontAttributes.None;
	private string _fontFamily = null;
	private double _fontSize = 0d;
	private bool _isEnabled = true;
	private bool _isVisible = true;
	private LineBreakMode _lineBreakMode = LineBreakMode.NoWrap;
	private double _padding = 0d;
	private string _text = null;
	private Color _textColor = Colors.Black;
	private float _shadowOpacity = 0f;
	private TextTransform _textTransform = TextTransform.Default;

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
				if (buttonText == "Command with Parameter")
				{
					Text = "Command Executed with Parameter";
				}
				else
				{
					Text = "Command Executed";
				}
			}
		);
	}

	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
