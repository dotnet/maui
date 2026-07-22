using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample;

public enum ShapeType
{
	Line,
	Rectangle,
	RoundRectangle,
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
	private bool _hasShadow = false;
	private Shadow _boxShadow = null;
	private readonly PathGeometryConverter _pathGeometryConverter = new PathGeometryConverter();

	public void ResetToDefaults()
	{
		SelectedShapeType = ShapeType.Rectangle;
		FillColor = null;
		HasFillColor = false;
		StrokeColor = Colors.Black;
		StrokeThickness = 1.0;
		Width = 300;
		Height = 150;
		HasShadow = false;
		StrokeDashArray = "0,0";
		StrokeDashOffset = 0;
		Aspect = Stretch.None;
		StrokeLineCap = PenLineCap.Flat;
		StrokeLineJoin = PenLineJoin.Miter;
		FillRule = FillRule.EvenOdd;
		CornerRadius = 0;
		RadiusX = 0;
		RadiusY = 0;
		X1 = 0;
		Y1 = 0;
		X2 = 280;
		Y2 = 0;
		Points = null;
		PolylinePoints = null;
		PathData = null;
	}
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
					FillColor = null;
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

	private Stretch _aspect = Stretch.None;
	public Stretch Aspect
	{
		get => _aspect;
		set => SetProperty(ref _aspect, value);
	}

	private FillRule _fillRule = FillRule.EvenOdd;
	public FillRule FillRule
	{
		get => _fillRule;
		set => SetProperty(ref _fillRule, value);
	}

	private PenLineCap _strokeLineCap = PenLineCap.Flat;
	public PenLineCap StrokeLineCap
	{
		get => _strokeLineCap;
		set => SetProperty(ref _strokeLineCap, value);
	}

	private PenLineJoin _strokeLineJoin = PenLineJoin.Miter;
	public PenLineJoin StrokeLineJoin
	{
		get => _strokeLineJoin;
		set => SetProperty(ref _strokeLineJoin, value);
	}

	private double _cornerRadius = 0;
	public double CornerRadius
	{
		get => _cornerRadius;
		set => SetProperty(ref _cornerRadius, value);
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

	private double _x2 = 280;
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
	private string _points = "100,20 170,75 100,130 30,75";
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
	private string _pathData = "M 10,84 C 10,84 40,15 100,55 C 160,15 190,84 190,84 C 190,84 100,135 100,135 C 100,135 10,84 10,84 Z";

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
	public PointCollection PolygonPointCollection => ParsePointCollection(_points);

	public PointCollection PolylinePointCollection => ParsePointCollection(_polylinePoints);

	static PointCollection ParsePointCollection(string input)
	{
		var points = new PointCollection();
		if (string.IsNullOrWhiteSpace(input))
			return points;
		foreach (var pair in input.Split(' '))
		{
			var coords = pair.Split(',');
			if (coords.Length == 2 &&
				double.TryParse(coords[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double x) &&
				double.TryParse(coords[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double y))
			{
				points.Add(new Point(x, y));
			}
		}
		return points;
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
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"PathGeometry parse failed: {ex.Message}");
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
