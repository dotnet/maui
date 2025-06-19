using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample;

public class BoxViewViewModel : INotifyPropertyChanged
{
	private Color _color = Colors.Blue;
	private double _width = 200;
	private double _height = 100;
	private bool _isVisible = true;
	private double _opacity = 1.0;
	private CornerRadius _cornerRadius;
	private bool _isRedChecked = false;
	private bool _isBlueChecked = true;
	private FlowDirection _flowDirection = FlowDirection.LeftToRight;


	private string _cornerRadiusEntryText = null;
	private string _opacityEntryText = null;

	public string CornerRadiusEntryText
	{
		get => _cornerRadiusEntryText;
		set
		{
			if (_cornerRadiusEntryText != value)
			{
				_cornerRadiusEntryText = value;
				if (double.TryParse(value, out double radius))
					CornerRadius = radius;
				OnPropertyChanged();
			}
		}
	}

	public string OpacityEntryText
	{
		get => _opacityEntryText;
		set
		{
			if (_opacityEntryText != value)
			{
				_opacityEntryText = value;
				if (double.TryParse(value, out double opacity))
					Opacity = opacity;
				OnPropertyChanged();
			}
		}
	}

	public bool IsRedChecked
	{
		get => _isRedChecked;
		set
		{
			if (_isRedChecked != value)
			{
				_isRedChecked = value;
				if (value)
					Color = Colors.Red;
				OnPropertyChanged();
			}
		}
	}

	public bool IsBlueChecked
	{
		get => _isBlueChecked;
		set
		{
			if (_isBlueChecked != value)
			{
				_isBlueChecked = value;
				if (value)
					Color = Colors.Blue;
				OnPropertyChanged();
			}
		}
	}
	public double Opacity
	{
		get => _opacity;
		set
		{
			if (_opacity != value)
			{
				_opacity = value;
				OnPropertyChanged(nameof(Opacity));
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
				OnPropertyChanged(nameof(CornerRadius));
			}
		}
	}
	public bool IsVisible
	{
		get => _isVisible;
		set
		{
			_isVisible = value;
			OnPropertyChanged();
		}
	}
	private bool _hasShadow = false;
	private Shadow _boxShadow = null;

	public bool HasShadow
	{
		get => _hasShadow;
		set
		{
			if (_hasShadow != value)
			{
				_hasShadow = value;
				BoxShadow = value
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

	public Shadow BoxShadow
	{
		get => _boxShadow;
		private set
		{
			if (_boxShadow != value)
			{
				_boxShadow = value;
				OnPropertyChanged(nameof(BoxShadow));
			}
		}
	}

	public Color Color
	{
		get => _color;
		set { _color = value; OnPropertyChanged(); }
	}

	public double Width
	{
		get => _width;
		set { _width = value; OnPropertyChanged(); }
	}

	public double Height
	{
		get => _height;
		set { _height = value; OnPropertyChanged(); }
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
	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}