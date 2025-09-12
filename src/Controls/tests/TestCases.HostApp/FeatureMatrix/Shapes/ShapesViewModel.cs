using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample;

public enum ShapeType
{
	Line,
	Rectangle,
	Polygon,
	Polyline,
	Ellipse,
	Path
}
public class ShapesViewModel : INotifyPropertyChanged
{
	private ShapeType _selectedShapeType = ShapeType.Rectangle;
	private Color _fillColor = null;
	private bool _hasFillColor = false;
	private Color _strokeColor = Colors.Black;
	private double _strokeThickness = 1.0;
	private double _width = 300;
	private double _height = 150;
	private FlowDirection _flowDirection = FlowDirection.LeftToRight;
	private bool _hasShadow = false;
	private Shadow _boxShadow = null;
	private PathGeometryConverter _pathGeometryConverter = new PathGeometryConverter();

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

	public ShapeType SelectedShapeType
	{
		get => _selectedShapeType;
		set => SetProperty(ref _selectedShapeType, value);
	}

	public Color FillColor
	{
		get => _fillColor;
		set => SetProperty(ref _fillColor, value);
	}

	public bool HasFillColor
	{
		get => _hasFillColor;
		set
		{
			if (SetProperty(ref _hasFillColor, value))
			{
				if (!value)
				{
					_fillColor = null;
				}
			}
		}
	}

	public Color StrokeColor
	{
		get => _strokeColor;
		set => SetProperty(ref _strokeColor, value);
	}

	public double StrokeThickness
	{
		get => _strokeThickness;
		set => SetProperty(ref _strokeThickness, value);
	}

	public double Width
	{
		get => _width;
		set => SetProperty(ref _width, value);
	}

	public double Height
	{
		get => _height;
		set => SetProperty(ref _height, value);
	}

	private string _strokeDashArray = "0,0";
	public string StrokeDashArray
	{
		get => _strokeDashArray;
		set
		{
			if (SetProperty(ref _strokeDashArray, value))
			{
				OnPropertyChanged(nameof(StrokeDashCollection));
			}
		}
	}

	private double _strokeDashOffset = 0;
	public double StrokeDashOffset
	{
		get => _strokeDashOffset;
		set => SetProperty(ref _strokeDashOffset, value);
	}

	private double _radiusX = 0;
	public double RadiusX
	{
		get => _radiusX;
		set => SetProperty(ref _radiusX, value);
	}

	private double _radiusY = 0;
	public double RadiusY
	{
		get => _radiusY;
		set => SetProperty(ref _radiusY, value);
	}

	private double _x1 = 0;
	public double X1
	{
		get => _x1;
		set => SetProperty(ref _x1, value);
	}

	private double _y1 = 0;
	public double Y1
	{
		get => _y1;
		set => SetProperty(ref _y1, value);
	}

	private double _x2 = 0;
	public double X2
	{
		get => _x2;
		set => SetProperty(ref _x2, value);
	}

	private double _y2 = 0;
	public double Y2
	{
		get => _y2;
		set => SetProperty(ref _y2, value);
	}

	// Polygon/Polyline Points
	private string _points = "100,50 150,100 100,150 50,100";
	public string Points
	{
		get => _points;
		set
		{
			if (SetProperty(ref _points, value))
			{
				OnPropertyChanged(nameof(PolygonPointCollection));
			}
		}
	}

	// Separate points for Polyline (zigzag pattern)
	private string _polylinePoints = "50,100 100,50 150,100 200,50 250,100";
	public string PolylinePoints
	{
		get => _polylinePoints;
		set
		{
			if (SetProperty(ref _polylinePoints, value))
			{
				OnPropertyChanged(nameof(PolylinePointCollection));
			}
		}
	}

	// Path Data
	private string _pathData = "M 10,100 L 100,100 100,50Z";

	public string PathData
	{
		get => _pathData;
		set
		{
			if (SetProperty(ref _pathData, value))
			{
				OnPropertyChanged(nameof(PathGeometry));
			}
		}
	}

	// Computed properties for proper data types
	public PointCollection PolygonPointCollection
	{
		get
		{
			var points = new PointCollection();
			if (!string.IsNullOrWhiteSpace(_points))
			{
				var pointPairs = _points.Split(' ');
				foreach (var pointPair in pointPairs)
				{
					var coords = pointPair.Split(',');
					if (coords.Length == 2 &&
						double.TryParse(coords[0], out double x) &&
						double.TryParse(coords[1], out double y))
					{
						points.Add(new Point(x, y));
					}
				}
			}
			return points;
		}
	}

	public PointCollection PolylinePointCollection
	{
		get
		{
			var points = new PointCollection();
			if (!string.IsNullOrWhiteSpace(_polylinePoints))
			{
				var pointPairs = _polylinePoints.Split(' ');
				foreach (var pointPair in pointPairs)
				{
					var coords = pointPair.Split(',');
					if (coords.Length == 2 &&
						double.TryParse(coords[0], out double x) &&
						double.TryParse(coords[1], out double y))
					{
						points.Add(new Point(x, y));
					}
				}
			}
			return points;
		}
	}

	public DoubleCollection StrokeDashCollection
	{
		get
		{
			var collection = new DoubleCollection();
			if (!string.IsNullOrWhiteSpace(_strokeDashArray))
			{
				var values = _strokeDashArray.Split(',');
				foreach (var val in values)
				{
					if (double.TryParse(val.Trim(), out double dashValue))
					{
						collection.Add(dashValue);
					}
				}
			}
			return collection;
		}
	}

	public Geometry PathGeometry
	{
		get
		{
			try
			{
				if (!string.IsNullOrWhiteSpace(_pathData))
				{
					return (Geometry)_pathGeometryConverter.ConvertFromInvariantString(_pathData);
				}
			}
			catch
			{
				// Return a simple default path if parsing fails
			}
			return (Geometry)_pathGeometryConverter.ConvertFromInvariantString("M 10,100 L 100,100");
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string name = null) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

	protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = null)
	{
		if (Equals(field, value))
			return false;
		field = value;
		OnPropertyChanged(name);
		return true;
	}
}
