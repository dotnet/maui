using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{
	public class ShadowViewModel : BindableObject
	{
		Color _color;
		double _offsetX;
		double _offsetY;
		Point _offset;
		double _radius;
		double _opacity;
		FlowDirection _flowDirection;
		bool _isEnabled;
		bool _isVisible;

		public ShadowViewModel()
		{
			Reset();
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

		public double OffsetX
		{
			get => _offsetX;
			set
			{
				if (_offsetX != value)
				{
					_offsetX = value;
					UpdateOffset();
					OnPropertyChanged();
				}
			}
		}

		public double OffsetY
		{
			get => _offsetY;
			set
			{
				if (_offsetY != value)
				{
					_offsetY = value;
					UpdateOffset();
					OnPropertyChanged();
				}
			}
		}

		public Point Offset
		{
			get => _offset;
			set
			{
				if (_offset != value)
				{
					_offset = value;
					OnPropertyChanged();
				}
			}
		}

		public double Radius
		{
			get => _radius;
			set
			{
				if (_radius != value)
				{
					_radius = value;
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

		public ICommand ResetCommand => new Command(Reset);

		void UpdateOffset()
		{
			Offset = new Point(OffsetX, OffsetY);
		}

		void Reset()
		{
			Color = Colors.Black;
			OffsetX = 10;
			OffsetY = 10;
			Offset = new Point(OffsetX, OffsetY);
			Radius = 12;
			Opacity = 1;
			FlowDirection = FlowDirection.LeftToRight;
			IsEnabled = true;
			IsVisible = true;
		}
	}
}