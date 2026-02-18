using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Shapes;
namespace Maui.Controls.Sample;

public class ClipViewModel : INotifyPropertyChanged
{
	private Color _color = Colors.Red;
	private CornerRadius _cornerRadius;
	private Geometry _clip = null;
	private FormattedString _formattedText;
	private double _fontSize = 28;
	private string _imageSource = null;
	private string _text = "Button";
	private string _selectedControl = "Image";
	private string _selectedClip = "null";
	private IShape _strokeShape = new Rectangle();
	private Shadow _shadow = null;
	private Brush _stroke = Brush.Red;
	private double _strokeThickness = 5;


	public Color Color
	{
		get => _color;
		set
		{
			if (_color != value)
			{
				_color = value;
				OnPropertyChanged();
			}
		}
	}

	public CornerRadius CornerRadius
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
	public Geometry Clip
	{
		get => _clip;
		set
		{
			if (_clip != value)
			{
				_clip = value;
				OnPropertyChanged();
			}
		}
	}

	public FormattedString FormattedText
	{
		get => _formattedText;
		set
		{
			if (_formattedText != value)
			{
				_formattedText = value;
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

	public string ImageSource
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
	public string SelectedControl
	{
		get => _selectedControl;
		set
		{
			if (_selectedControl != value)
			{
				_selectedControl = value;
				OnPropertyChanged();
			}
		}
	}

	public string SelectedClip
	{
		get => _selectedClip;
		set
		{
			if (_selectedClip != value)
			{
				_selectedClip = value;
				OnPropertyChanged();
			}
		}
	}

	public IShape StrokeShape
	{
		get => _strokeShape;
		set
		{
			if (_strokeShape != value)
			{
				_strokeShape = value;
				OnPropertyChanged(nameof(StrokeShape));
			}
		}
	}

	public Shadow Shadow
	{
		get => _shadow;
		set
		{
			if (_shadow != value)
			{
				_shadow = value;
				OnPropertyChanged();
			}
		}
	}

	public Brush Stroke
	{
		get => _stroke;
		set
		{
			if (_stroke != value)
			{
				_stroke = value;
				OnPropertyChanged();
			}
		}
	}
	public double StrokeThickness
	{
		get => _strokeThickness;
		set
		{
			if (_strokeThickness != value)
			{
				_strokeThickness = value;
				OnPropertyChanged();
			}
		}
	}

	private double _rotation;
	public double ControlRotation
	{
		get => _rotation;
		set
		{
			if (_rotation != value)
			{
				_rotation = value;
				OnPropertyChanged();
			}
		}
	}

	private double _scaleX = 1.0;
	public double ControlScaleX
	{
		get => _scaleX;
		set
		{
			if (_scaleX != value)
			{
				_scaleX = value;
				OnPropertyChanged();
			}
		}
	}

	private double _scaleY = 1.0;
	public double ControlScaleY
	{
		get => _scaleY;
		set
		{
			if (_scaleY != value)
			{
				_scaleY = value;
				OnPropertyChanged();
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}