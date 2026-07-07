using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

public class LabelViewModel : INotifyPropertyChanged
{
	// Dynamic span that can be modified at runtime
	public Span DynamicSpan { get; }

	public LabelViewModel()
	{
		DynamicSpan = new Span { Text = "Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco", TextColor = Colors.Red, FontSize = 20 };

		FormattedText = new FormattedString
		{
			Spans =
		   {
			   new Span { Text = "This is a Basic Label"},
		   }
		};

		TapCommand = new Command(() =>
		{
			TapResult = "Tapped";
		}, () => LabelHeightRequest == -1);

		ChangeSpanTextCommand = new Command(() =>
		{
			EnsureDynamicSpan();
			DynamicSpan.Text = DynamicSpan.Text == "Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco" ? "Tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat" : "Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco";
		});

		ChangeSpanTextColorCommand = new Command(() =>
		{
			EnsureDynamicSpan();
			DynamicSpan.TextColor = DynamicSpan.TextColor == Colors.Red ? Colors.Green : Colors.Red;
		});

		ChangeSpanFontSizeCommand = new Command(() =>
		{
			EnsureDynamicSpan();
			DynamicSpan.FontSize = DynamicSpan.FontSize == 20 ? 30 : 20;
		});

		ChangeSpanFontAttributesCommand = new Command(() =>
		{
			EnsureDynamicSpan();
			DynamicSpan.FontAttributes = DynamicSpan.FontAttributes == FontAttributes.None
				? FontAttributes.Bold | FontAttributes.Italic
				: FontAttributes.None;
		});

		ChangeSpanTextDecorationsCommand = new Command(() =>
		{
			EnsureDynamicSpan();
			DynamicSpan.TextDecorations = DynamicSpan.TextDecorations == TextDecorations.None
				? TextDecorations.Underline | TextDecorations.Strikethrough
				: TextDecorations.None;
		});

		ChangeSpanBackgroundColorCommand = new Command(() =>
		{
			EnsureDynamicSpan();
			DynamicSpan.BackgroundColor = DynamicSpan.BackgroundColor == Colors.Transparent || DynamicSpan.BackgroundColor == null
				? Colors.Yellow
				: Colors.Transparent;
		});

		ChangeSpanCharacterSpacingCommand = new Command(() =>
		{
			EnsureDynamicSpan();
			DynamicSpan.CharacterSpacing = DynamicSpan.CharacterSpacing == 0 ? 5 : 0;
		});

		ChangeSpanTextTransformCommand = new Command(() =>
		{
			EnsureDynamicSpan();
			DynamicSpan.TextTransform = DynamicSpan.TextTransform != TextTransform.Uppercase
				? TextTransform.Uppercase
				: TextTransform.None;
		});

		ChangeSpanFontFamilyCommand = new Command(() =>
		{
			EnsureDynamicSpan();
			DynamicSpan.FontFamily = DynamicSpan.FontFamily is null ? "Dokdo" : null;
		});

		ChangeSpanLineHeightCommand = new Command(() =>
		{
			EnsureDynamicSpan();
			DynamicSpan.LineHeight = DynamicSpan.LineHeight != 2.0 ? 2.0 : 1.0;
		});

		ChangeAllSpanPropertiesCommand = new Command(() =>
		{
			EnsureDynamicSpan();
			_allSpanToggled = !_allSpanToggled;
			DynamicSpan.Text = _allSpanToggled ? "Tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat" : "Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco";
			DynamicSpan.TextColor = _allSpanToggled ? Colors.Green : Colors.Red;
			DynamicSpan.FontSize = _allSpanToggled ? 30 : 20;
			DynamicSpan.FontAttributes = _allSpanToggled ? FontAttributes.Bold | FontAttributes.Italic : FontAttributes.None;
			DynamicSpan.TextDecorations = _allSpanToggled ? TextDecorations.Underline | TextDecorations.Strikethrough : TextDecorations.None;
			DynamicSpan.BackgroundColor = _allSpanToggled ? Colors.Yellow : Colors.Transparent;
			DynamicSpan.CharacterSpacing = _allSpanToggled ? 5 : 0;
			DynamicSpan.TextTransform = _allSpanToggled ? TextTransform.Uppercase : TextTransform.None;
			DynamicSpan.FontFamily = _allSpanToggled ? "OpenSansRegular" : null;
			DynamicSpan.LineHeight = _allSpanToggled ? 2.0 : 1.0;
		});

		ResetSpanCommand = new Command(() =>
		{
			_allSpanToggled = false;
			DynamicSpan.Text = "Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco";
			DynamicSpan.TextColor = Colors.Red;
			DynamicSpan.FontSize = 20;
			DynamicSpan.FontAttributes = FontAttributes.None;
			DynamicSpan.TextDecorations = TextDecorations.None;
			DynamicSpan.BackgroundColor = Colors.Transparent;
			DynamicSpan.CharacterSpacing = 0;
			DynamicSpan.TextTransform = TextTransform.None;
			DynamicSpan.FontFamily = null;
			DynamicSpan.LineHeight = 1.0;
		});
	}

	bool _allSpanToggled;

	void EnsureDynamicSpan()
	{
		if (!FormattedText.Spans.Contains(DynamicSpan))
		{
			FormattedText = new FormattedString
			{
				Spans =
				{
					new Span { Text = "Hello ", TextColor = Colors.Red },
					new Span { Text = "World", TextColor = Colors.Blue },
					DynamicSpan
				}
			};
			Text = null;
			LabelHeightRequest = -1;
			AreSpanButtonsVisible = true;
		}
	}

	public void EnableDynamicSpan()
	{
		EnsureDynamicSpan();
	}

	public void DisableDynamicSpan()
	{
		FormattedText = new FormattedString
		{
			Spans =
			{
				new Span { Text = "This is a Basic Label" },
			}
		};
		AreSpanButtonsVisible = false;
	}

	public Command ChangeSpanTextCommand { get; }
	public Command ChangeSpanTextColorCommand { get; }
	public Command ChangeSpanFontSizeCommand { get; }
	public Command ChangeSpanFontAttributesCommand { get; }
	public Command ChangeSpanTextDecorationsCommand { get; }
	public Command ChangeSpanBackgroundColorCommand { get; }
	public Command ChangeSpanCharacterSpacingCommand { get; }
	public Command ChangeSpanTextTransformCommand { get; }
	public Command ChangeSpanFontFamilyCommand { get; }
	public Command ChangeSpanLineHeightCommand { get; }
	public Command ChangeAllSpanPropertiesCommand { get; }
	public Command ResetSpanCommand { get; }
	private string _text;
	public string Text
	{
		get => _text;
		set
		{
			if (_text == value)
				return;
			_text = value;
			OnPropertyChanged(nameof(Text));
		}
	}

	private FormattedString _formattedText;
	public FormattedString FormattedText
	{
		get => _formattedText;
		set
		{
			if (_formattedText == value)
				return;
			_formattedText = value;
			OnPropertyChanged(nameof(FormattedText));
		}
	}


	private string fontFamily;
	public string FontFamily
	{
		get => fontFamily;
		set { fontFamily = value; OnPropertyChanged(); }
	}


	private double characterSpacing;
	public double CharacterSpacing
	{
		get => characterSpacing;
		set { characterSpacing = value; OnPropertyChanged(); }
	}

	private double lineHeight;
	public double LineHeight
	{
		get => lineHeight;
		set { lineHeight = value; OnPropertyChanged(); }
	}

	private int maxLines;
	public int MaxLines
	{
		get => maxLines;
		set { maxLines = value; OnPropertyChanged(); }
	}

	private Thickness padding;
	public Thickness Padding
	{
		get => padding;
		set { padding = value; OnPropertyChanged(); }
	}

	private TextType textType = TextType.Text;
	public TextType TextType
	{
		get => textType;
		set { textType = value; OnPropertyChanged(); }
	}


	private Color textColor = Colors.Black;
	public Color TextColor
	{
		get => textColor;
		set { textColor = value; OnPropertyChanged(); }
	}

	private double fontSize = 18;
	public double FontSize
	{
		get => fontSize;
		set { fontSize = value; OnPropertyChanged(); }
	}

	private FontAttributes fontAttributes = FontAttributes.None;
	public FontAttributes FontAttributes
	{
		get => fontAttributes;
		set { fontAttributes = value; OnPropertyChanged(); }
	}

	private bool fontAutoScalingEnabled;
	public bool FontAutoScalingEnabled
	{
		get => fontAutoScalingEnabled;
		set { fontAutoScalingEnabled = value; OnPropertyChanged(); }
	}

	private LineBreakMode lineBreakMode = LineBreakMode.WordWrap;
	public LineBreakMode LineBreakMode
	{
		get => lineBreakMode;
		set { lineBreakMode = value; OnPropertyChanged(); }
	}

	private TextAlignment horizontalTextAlignment = TextAlignment.Center;
	public TextAlignment HorizontalTextAlignment
	{
		get => horizontalTextAlignment;
		set { horizontalTextAlignment = value; OnPropertyChanged(); }
	}

	private TextAlignment verticalTextAlignment = TextAlignment.Center;
	public TextAlignment VerticalTextAlignment
	{
		get => verticalTextAlignment;
		set { verticalTextAlignment = value; OnPropertyChanged(); }
	}

	private TextDecorations textDecorations = TextDecorations.None;
	public TextDecorations TextDecorations
	{
		get => textDecorations;
		set { textDecorations = value; OnPropertyChanged(); }
	}

	private TextTransform textTransform = TextTransform.None;
	public TextTransform TextTransform
	{
		get => textTransform;
		set { textTransform = value; OnPropertyChanged(); }
	}

	private bool isEnabled = true;
	public bool IsEnabled
	{
		get => isEnabled;
		set
		{
			isEnabled = value;
			LabelHeightRequest = -1;
			OnPropertyChanged();
		}
	}

	private bool isVisible = true;
	public bool IsVisible
	{
		get => isVisible;
		set { isVisible = value; OnPropertyChanged(); }
	}

	private Color labelBackgroundColor = Colors.LightGray;
	public Color LabelBackgroundColor
	{
		get => labelBackgroundColor;
		set { labelBackgroundColor = value; OnPropertyChanged(); }
	}

	private bool hasShadow;
	public bool HasShadow
	{
		get => hasShadow;
		set
		{
			if (hasShadow != value)
			{
				hasShadow = value;
				if (value)
				{
					LabelShadow = new Shadow
					{
						Radius = 10,
						Opacity = 1.0f,
						Brush = Colors.Black.AsPaint(),
						Offset = new Point(5, 5)
					};
					LabelBackgroundColor = Colors.Transparent;
				}
				else
				{
					LabelShadow = null;
					LabelBackgroundColor = Colors.LightGray;
				}
				OnPropertyChanged(nameof(HasShadow));
			}
		}
	}

	private Shadow labelShadow;
	public Shadow LabelShadow
	{
		get => labelShadow;
		private set
		{
			if (labelShadow != value)
			{
				labelShadow = value;
				OnPropertyChanged(nameof(LabelShadow));
			}
		}
	}

	private FlowDirection flowDirection = FlowDirection.LeftToRight;
	public FlowDirection FlowDirection
	{
		get => flowDirection;
		set { flowDirection = value; OnPropertyChanged(); }
	}

	private double labelHeightRequest = 600;
	public double LabelHeightRequest
	{
		get => labelHeightRequest;
		set { labelHeightRequest = value; OnPropertyChanged(); }
	}

	private string tapResult;
	public string TapResult
	{
		get => tapResult;
		set { tapResult = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsTapResultVisible)); }
	}

	public bool IsTapResultVisible => !string.IsNullOrEmpty(TapResult);

	private bool areSpanButtonsVisible;
	public bool AreSpanButtonsVisible
	{
		get => areSpanButtonsVisible;
		set { areSpanButtonsVisible = value; OnPropertyChanged(); }
	}

	public Command TapCommand { get; }

	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}