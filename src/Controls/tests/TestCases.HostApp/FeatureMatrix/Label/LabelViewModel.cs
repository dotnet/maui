using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

public class LabelViewModel : INotifyPropertyChanged
{

	public LabelViewModel()
	{

		FormattedText = new FormattedString
		{
			Spans =
		   {
			   new Span { Text = "This is a Basic Label"},
		   }
		};
	}
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

	public Command TapCommand => new Command(() =>
	{
		TapResult = "Tapped";
	}, () => LabelHeightRequest == -1);

	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}