using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample;

public class BoxViewViewModel : INotifyPropertyChanged
{
	Color _color = Colors.Blue;
	Brush _fill = null;
	LinearGradientBrush _linearGradientBrush = new LinearGradientBrush()
	{
		StartPoint = new Point(0, 0),
		EndPoint = new Point(1, 1),
		GradientStops =
		[
			new GradientStop { Color = Colors.Purple, Offset = 0.0f },
			new GradientStop { Color = Colors.Pink, Offset = 0.3f },
			new GradientStop { Color = Colors.Orange, Offset = 0.7f },
			new GradientStop { Color = Colors.Red, Offset = 1.0f }
		]
	};

	RadialGradientBrush _radialGradientBrush = new RadialGradientBrush()
	{
		Center = new Point(0.5, 0.5),
		Radius = 0.5,
		GradientStops =
		[
			new GradientStop { Color = Colors.Yellow, Offset = 0.0f },
			new GradientStop { Color = Colors.Green, Offset = 1.0f }
		]
	};

	double _width = 200;
	double _height = 100;
	bool _isVisible = true;
	double _opacity = 1.0;
	CornerRadius _cornerRadius;
	bool _isRedChecked = false;
	bool _isBlueChecked = true;
	bool _isSolidChecked = false;
	bool _isLinearChecked = false;
	bool _isRadialChecked = false;
	FlowDirection _flowDirection = FlowDirection.LeftToRight;


	string _cornerRadiusEntryText = null;
	string _opacityEntryText = null;

	public string CornerRadiusEntryText
	{
		get => _cornerRadiusEntryText;
		set
		{
			if (_cornerRadiusEntryText != value)
			{
				_cornerRadiusEntryText = value;
				if (double.TryParse(value, out double radius))
				{
					CornerRadius = radius;
				}

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
				{
					Opacity = opacity;
				}

				OnPropertyChanged();
			}
		}
	}

	public bool IsSolidChecked
	{
		get => _isSolidChecked;
		set
		{
			if (_isSolidChecked != value)
			{
				_isSolidChecked = value;
				if (value)
				{
					Fill = Colors.Red.AsPaint();
				}

				OnPropertyChanged();
			}
		}
	}

	public bool IsLinearChecked
	{
		get => _isLinearChecked;
		set
		{
			if (_isLinearChecked != value)
			{
				_isLinearChecked = value;
				if (value)
				{
					Fill = _linearGradientBrush;
				}

				OnPropertyChanged();
			}
		}
	}

	public bool IsRadialChecked
	{
		get => _isRadialChecked;
		set
		{
			if (_isRadialChecked != value)
			{
				_isRadialChecked = value;
				if (value)
				{
					Fill = _radialGradientBrush;
				}

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
				{
					Color = Colors.Red;
				}

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
				{
					Color = Colors.Blue;
				}

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
	bool _hasShadow = false;
	Shadow _boxShadow = null;

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
		set
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

	public Brush Fill
	{
		get => _fill;
		set { _fill = value; OnPropertyChanged(); }
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