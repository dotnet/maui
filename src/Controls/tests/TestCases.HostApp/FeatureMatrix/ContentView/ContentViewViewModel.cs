using System.ComponentModel;
namespace Maui.Controls.Sample;

public class ContentViewViewModel : INotifyPropertyChanged
{
	private string _defaultLabelText = "This is Default Page";
	private bool _useControlTemplate;
	private string _contentLabel = "Default";
	private int _heightRequest = -1;
	private int _widthRequest = -1;
	private Color _backgroundColor = Colors.LightGray;
	private Color _cardColor = Colors.SkyBlue;
	private string _cardTitle = "First ContentView Page";
	private ImageSource _iconImageSource = "dotnet_bot.png";
	private bool _isEnabled = true;
	private bool _isVisible = true;
	private FlowDirection _flowDirection = FlowDirection.LeftToRight;
	private bool _hasShadow = false;
	private Shadow _contentViewShadow = null;
	private string _controlTemplateKeyFirst = "DefaultFirstTemplate";
	private string _controlTemplateKeySecond = "DefaultSecondTemplate";
	public string DefaultLabelText
	{
		get => _defaultLabelText;
		set
		{
			if (_defaultLabelText != value)
			{
				_defaultLabelText = value;
				OnPropertyChanged(nameof(DefaultLabelText));
			}
		}
	}

	public string ControlTemplateKeyFirst
	{
		get => _controlTemplateKeyFirst;
		set
		{
			if (_controlTemplateKeyFirst != value)
			{
				_controlTemplateKeyFirst = value;
				OnPropertyChanged(nameof(ControlTemplateKeyFirst));
			}
		}
	}

	public string ControlTemplateKeySecond
	{
		get => _controlTemplateKeySecond;
		set
		{
			if (_controlTemplateKeySecond != value)
			{
				_controlTemplateKeySecond = value;
				OnPropertyChanged(nameof(ControlTemplateKeySecond));
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
				OnPropertyChanged(nameof(IsVisible));
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
				OnPropertyChanged(nameof(IsEnabled));
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
				OnPropertyChanged(nameof(BackgroundColor));
			}
		}
	}

	public Color CardColor
	{
		get => _cardColor;
		set
		{
			if (_cardColor != value)
			{
				_cardColor = value;
				OnPropertyChanged(nameof(CardColor));
			}
		}
	}

	public ImageSource IconImageSource
	{
		get => _iconImageSource;
		set
		{
			if (_iconImageSource != value)
			{
				_iconImageSource = value;
				OnPropertyChanged(nameof(IconImageSource));
			}
		}
	}

	public string CardTitle
	{
		get => _cardTitle;
		set
		{
			if (_cardTitle != value)
			{
				_cardTitle = value;
				OnPropertyChanged(nameof(CardTitle));
			}
		}
	}
	public int WidthRequest
	{
		get => _widthRequest;
		set
		{
			if (_widthRequest != value)
			{
				_widthRequest = value;
				OnPropertyChanged(nameof(WidthRequest));
			}
		}
	}
	public int HeightRequest
	{
		get => _heightRequest;
		set
		{
			if (_heightRequest != value)
			{
				_heightRequest = value;
				OnPropertyChanged(nameof(HeightRequest));
			}
		}
	}
	public bool UseControlTemplate
	{
		get => _useControlTemplate;
		set
		{
			if (_useControlTemplate != value)
			{
				_useControlTemplate = value;
				OnPropertyChanged(nameof(UseControlTemplate));
			}
		}
	}
	public string ContentLabel
	{
		get => _contentLabel;
		set
		{
			if (_contentLabel != value)
			{
				_contentLabel = value;
				OnPropertyChanged(nameof(ContentLabel));
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
				OnPropertyChanged(nameof(FlowDirection));
			}
		}
	}
	public bool HasShadow
	{
		get => _hasShadow;
		set
		{
			if (_hasShadow != value)
			{
				_hasShadow = value;
				ContentViewShadow = value
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

	public Shadow ContentViewShadow
	{
		get => _contentViewShadow;
		set
		{
			if (_contentViewShadow != value)
			{
				_contentViewShadow = value;
				OnPropertyChanged(nameof(ContentViewShadow));
			}
		}
	}


	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}