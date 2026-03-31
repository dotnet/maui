using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample;

public class ButtonViewModel : INotifyPropertyChanged
{
	private Color _borderColor = null;
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
	private string _text = "Button";
	private Color _textColor = null;
	private float _shadowOpacity = 0f;
	private TextTransform _textTransform = TextTransform.Default;
	private Brush _background = null;
	private double _heightRequest = -1d;
	private double _widthRequest = -1d;
	private string _pressedEventLabelText = string.Empty;
	private string _releasedEventLabelText = string.Empty;
	private string _clickedEventLabelText = string.Empty;
	private ImageSource _imageSource = null;

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

	public Brush Background
	{
		get => _background;
		set
		{
			if (_background != value)
			{
				_background = value;
				OnPropertyChanged();
			}
		}
	}

	public double HeightRequest
	{
		get => _heightRequest;
		set
		{
			if (_heightRequest != value)
			{
				_heightRequest = value;
				OnPropertyChanged();
			}
		}
	}

	public double WidthRequest
	{
		get => _widthRequest;
		set
		{
			if (_widthRequest != value)
			{
				_widthRequest = value;
				OnPropertyChanged();
			}
		}
	}

	public string PressedEventLabelText
	{
		get => _pressedEventLabelText;
		set
		{
			if (_pressedEventLabelText != value)
			{
				_pressedEventLabelText = value;
				OnPropertyChanged();
			}
		}
	}

	public string ReleasedEventLabelText
	{
		get => _releasedEventLabelText;
		set
		{
			if (_releasedEventLabelText != value)
			{
				_releasedEventLabelText = value;
				OnPropertyChanged();
			}
		}
	}

	public string ClickedEventLabelText
	{
		get => _clickedEventLabelText;
		set
		{
			if (_clickedEventLabelText != value)
			{
				_clickedEventLabelText = value;
				OnPropertyChanged();
			}
		}
	}

	public ImageSource ImageSource
	{
		get => _imageSource;
		set
		{
			if (_imageSource != value)
			{
				_imageSource = value;
				OnPropertyChanged();
			}
		}
	}

	public ButtonViewModel()
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

	public void Reset()
	{
		BorderColor = Colors.White;
		BorderWidth = 0d;
		CharacterSpacing = 0.0d;
		Command = null;
		CornerRadius = 0;
		FlowDirection = FlowDirection.MatchParent;
		FontAttributes = FontAttributes.None;
		FontFamily = null;
		FontSize = 0d;
		IsEnabled = true;
		IsVisible = true;
		LineBreakMode = LineBreakMode.NoWrap;
		Padding = 0d;
		Text = "Button";
		TextColor = null;
		TextTransform = TextTransform.Default;
		Background = null;
		HeightRequest = -1d;
		WidthRequest = -1d;
		PressedEventLabelText = string.Empty;
		ReleasedEventLabelText = string.Empty;
		ClickedEventLabelText = string.Empty;
		ImageSource = null;
	}

	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
