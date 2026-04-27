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
	private bool _isRTL = false;


	private bool _isGreenChecked = false;

	private string _cornerRadiusEntryText = string.Empty;
	private string _opacityEntryText = "1";
	private string _widthEntryText = "200";
	private string _heightEntryText = "100";

	public void Reset()
	{
		Color = Colors.Blue;
		Width = 200;
		Height = 100;
		IsVisible = true;
		Opacity = 1.0;
		CornerRadius = default;
		IsRedChecked = false;
		IsBlueChecked = true;
		IsGreenChecked = false;
		IsRTL = false;
		HasShadow = false;
		CornerRadiusEntryText = string.Empty;
		OpacityEntryText = "1";
		WidthEntryText = "200";
		HeightEntryText = "100";
	}

	public string CornerRadiusEntryText
	{
		get => _cornerRadiusEntryText;
		set
		{
			if (_cornerRadiusEntryText != value)
			{
				_cornerRadiusEntryText = value;
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
				OnPropertyChanged();
			}
		}
	}

	public string WidthEntryText
	{
		get => _widthEntryText;
		set
		{
			if (_widthEntryText != value)
			{
				_widthEntryText = value;
				OnPropertyChanged();
			}
		}
	}

	public string HeightEntryText
	{
		get => _heightEntryText;
		set
		{
			if (_heightEntryText != value)
			{
				_heightEntryText = value;
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
					SelectColor(Colors.Red, ref _isBlueChecked, nameof(IsBlueChecked), ref _isGreenChecked, nameof(IsGreenChecked));
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
					SelectColor(Colors.Blue, ref _isRedChecked, nameof(IsRedChecked), ref _isGreenChecked, nameof(IsGreenChecked));
				OnPropertyChanged();
			}
		}
	}

	public bool IsGreenChecked
	{
		get => _isGreenChecked;
		set
		{
			if (_isGreenChecked != value)
			{
				_isGreenChecked = value;
				if (value)
					SelectColor(Colors.Green, ref _isRedChecked, nameof(IsRedChecked), ref _isBlueChecked, nameof(IsBlueChecked));
				OnPropertyChanged();
			}
		}
	}

	private void SelectColor(Color color, ref bool other1, string other1Name, ref bool other2, string other2Name)
	{
		Color = color;
		other1 = false;
		OnPropertyChanged(other1Name);
		other2 = false;
		OnPropertyChanged(other2Name);
	}
	public double Opacity
	{
		get => _opacity;
		set
		{
			if (_opacity != value)
			{
				_opacity = value;
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
				OnPropertyChanged();
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
				OnPropertyChanged();
			}
		}
	}

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

	public double Width
	{
		get => _width;
		set
		{
			if (_width != value)
			{
				_width = value;
				OnPropertyChanged();
			}
		}
	}

	public double Height
	{
		get => _height;
		set
		{
			if (_height != value)
			{
				_height = value;
				OnPropertyChanged();
			}
		}
	}

	public bool IsRTL
	{
		get => _isRTL;
		set
		{
			if (_isRTL != value)
			{
				_isRTL = value;
				FlowDirection = value ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
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
	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}