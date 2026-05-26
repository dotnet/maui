using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample;
public class EditorViewModel : INotifyPropertyChanged
{
	private string _text = "Test Editor";
	private Color _textColor = Colors.Black;
	private string _placeholder = "Enter text here";
	private Color _placeholderColor = Colors.Gray;
	private double _fontSize = 14;
	private double _heightrequest = -1;
	private TextAlignment _horizontalTextAlignment = TextAlignment.Start;
	private TextAlignment _verticalTextAlignment = TextAlignment.End;
	private double _characterSpacing = 0;
	private ReturnType _returnType = ReturnType.Default;
	private int _maxLength = -1;
	private int _cursorPosition = 0;
	private int _selectionLength = 0;
	private bool _isReadOnly = false;
	private bool _isTextPredictionEnabled = false;
	private bool _isSpellCheckEnabled = false;
	private Keyboard _keyboard = Keyboard.Default;
	private string _fontFamily = null;
	private bool isVisible = true;
	private bool _isEnabled = true;
	private FlowDirection _flowDirection = FlowDirection.LeftToRight;
	private bool _hasShadow = false;
	private Shadow _editorShadow = null;
	private TextTransform _transform = TextTransform.Default;
	private FontAttributes _fontAttributes = FontAttributes.None;
	private EditorAutoSizeOption _autoSizeOption = EditorAutoSizeOption.Disabled;
	private string _textChangedText = "TextChanged: Not triggered";
	private string _completedText = "Completed: Not triggered";
	private string _focusedText = "Focused: Not triggered";
	private string _unfocusedText = "Unfocused: Not triggered";

	public event PropertyChangedEventHandler PropertyChanged;

	public ICommand ReturnCommand { get; set; }
	public EditorViewModel()
	{
		ReturnCommand = new Command<string>(
			execute: (entryText) =>
			{
				if (entryText == "Test")
				{
					Text = "Command Executed with Parameter";
				}
			}
		);
	}
	public string Text
	{
		get => _text;
		set { _text = value; OnPropertyChanged(); }
	}

	public Color TextColor
	{
		get => _textColor;
		set { _textColor = value; OnPropertyChanged(); }
	}

	public string Placeholder
	{
		get => _placeholder;
		set { _placeholder = value; OnPropertyChanged(); }
	}
	public Color PlaceholderColor
	{
		get => _placeholderColor;
		set { _placeholderColor = value; OnPropertyChanged(); }
	}

	public double FontSize
	{
		get => _fontSize;
		set { _fontSize = value; OnPropertyChanged(); }
	}

	public double HeightRequest
	{
		get => _heightrequest;
		set { _heightrequest = value; OnPropertyChanged(); }
	}

	public TextAlignment HorizontalTextAlignment
	{
		get => _horizontalTextAlignment;
		set { _horizontalTextAlignment = value; OnPropertyChanged(); }
	}

	public TextAlignment VerticalTextAlignment
	{
		get => _verticalTextAlignment;
		set { _verticalTextAlignment = value; OnPropertyChanged(); }
	}

	public double CharacterSpacing
	{
		get => _characterSpacing;
		set { _characterSpacing = value; OnPropertyChanged(); }
	}

	public ReturnType ReturnType
	{
		get => _returnType;
		set { _returnType = value; OnPropertyChanged(); }
	}

	public int MaxLength
	{
		get => _maxLength;
		set { _maxLength = value; OnPropertyChanged(); }
	}
	public int CursorPosition
	{
		get => _cursorPosition;
		set { _cursorPosition = value; OnPropertyChanged(); }
	}
	public int SelectionLength
	{
		get => _selectionLength;
		set { _selectionLength = value; OnPropertyChanged(); }
	}
	public bool IsReadOnly
	{
		get => _isReadOnly;
		set { _isReadOnly = value; OnPropertyChanged(); }
	}
	public bool IsTextPredictionEnabled
	{
		get => _isTextPredictionEnabled;
		set { _isTextPredictionEnabled = value; OnPropertyChanged(); }
	}
	public bool IsSpellCheckEnabled
	{
		get => _isSpellCheckEnabled;
		set { _isSpellCheckEnabled = value; OnPropertyChanged(); }
	}

	public bool IsVisible
	{
		get => isVisible;
		set { isVisible = value; OnPropertyChanged(); }
	}

	public bool IsEnabled
	{
		get => _isEnabled;
		set { _isEnabled = value; OnPropertyChanged(); }
	}

	public Keyboard Keyboard
	{
		get => _keyboard;
		set { _keyboard = value; OnPropertyChanged(); }
	}

	public FlowDirection FlowDirection
	{
		get => _flowDirection;
		set { _flowDirection = value; OnPropertyChanged(); }
	}

	public string FontFamily
	{
		get => _fontFamily;
		set { _fontFamily = value; OnPropertyChanged(); }
	}

	public string TextChangedText
	{
		get => _textChangedText;
		set { _textChangedText = value; OnPropertyChanged(); }
	}

	public string CompletedText
	{
		get => _completedText;
		set { _completedText = value; OnPropertyChanged(); }
	}

	public string FocusedText
	{
		get => _focusedText;
		set { _focusedText = value; OnPropertyChanged(); }
	}

	public string UnfocusedText
	{
		get => _unfocusedText;
		set { _unfocusedText = value; OnPropertyChanged(); }
	}

	public bool HasShadow
	{
		get => _hasShadow;
		set
		{
			if (_hasShadow != value)
			{
				_hasShadow = value;
				EditorShadow = value
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

	public Shadow EditorShadow
	{
		get => _editorShadow;
		private set
		{
			if (_editorShadow != value)
			{
				_editorShadow = value;
				OnPropertyChanged(nameof(EditorShadow));
			}
		}
	}

	public TextTransform TextTransform
	{
		get => _transform;
		set
		{
			if (_transform != value)
			{
				_transform = value;
				OnPropertyChanged(nameof(TextTransform));
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
				OnPropertyChanged(nameof(FontAttributes));
			}
		}
	}

	public EditorAutoSizeOption AutoSizeOption
	{
		get => _autoSizeOption;
		set
		{
			if (_autoSizeOption != value)
			{
				_autoSizeOption = value;
				OnPropertyChanged(nameof(AutoSizeOption));
			}
		}
	}

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}