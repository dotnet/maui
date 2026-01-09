using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample;

public partial class ClipOptionsPage : ContentPage
{
	private ClipViewModel _viewModel;
	public ClipOptionsPage(ClipViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}

	private void ClipRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value && BindingContext is ClipViewModel vm && sender is RadioButton rb)
		{
			vm.Clip = rb.Content?.ToString() switch
			{
				"None" => null,
				"Rectangle" => new RectangleGeometry(new Rect(0, 275, 150, 100)),
				"Ellipse" => new EllipseGeometry(new Point(175, 275), 100, 65),
				"RoundRectangle" => new RoundRectangleGeometry(new CornerRadius(50), new Rect(0, 275, 150, 100)),
				"GeometryGroup" => new GeometryGroup
				{
					Children = new GeometryCollection
					{
						new EllipseGeometry(new Point(100, 300), 80, 50),
						new RectangleGeometry(new Rect(200, 250, 100, 150))
					}
				},
				_ => null
			};

			vm.SelectedClip = rb.Content?.ToString() ?? "null";
		}
	}

	private void PathTypeRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value && BindingContext is ClipViewModel vm && sender is RadioButton rb)
		{
			string pathType = rb.Content?.ToString() ?? "LineSegment";
			vm.Clip = CreatePathGeometry(pathType);
		}
	}

	private PathGeometry CreatePathGeometry(string pathType)
	{
		return pathType switch
		{
			// LineSegment: Draws straight lines between points
			"Line" => new PathGeometry
			{
				Figures = new PathFigureCollection
				{
					new PathFigure
					{
						StartPoint = new Point(150, 20),
						IsClosed = true,
						Segments = new PathSegmentCollection
						{
							new LineSegment { Point = new Point(280, 180) },
							new LineSegment { Point = new Point(20, 180) }
						}
					}
				}
			},
			// ArcSegment: Draws an elliptical arc between two points
			"Arc" => new PathGeometry
			{
				Figures = new PathFigureCollection
				{
					new PathFigure
					{
						StartPoint = new Point(10, 100),
						IsClosed = false,
						Segments = new PathSegmentCollection
						{
							new ArcSegment
							{
								Point = new Point(290, 100),
								Size = new Size(140, 80),
								RotationAngle = 0,
								IsLargeArc = true,
								SweepDirection = SweepDirection.CounterClockwise
							}
						}
					}
				}
			},
			// BezierSegment: Draws a cubic Bezier curve (2 control points)
			"Bezier" => new PathGeometry
			{
				Figures = new PathFigureCollection
				{
					new PathFigure
					{
						StartPoint = new Point(10, 100),
						IsClosed = false,
						Segments = new PathSegmentCollection
						{
							new BezierSegment
							{
								Point1 = new Point(100, 0),
								Point2 = new Point(200, 200),
								Point3 = new Point(290, 100)
							}
						}
					}
				}
			},
			// QuadraticBezierSegment: Draws a quadratic Bezier curve (1 control point)
			"QuadraticBezier" => new PathGeometry
			{
				Figures = new PathFigureCollection
				{
					new PathFigure
					{
						StartPoint = new Point(10, 100),
						IsClosed = false,
						Segments = new PathSegmentCollection
						{
							new QuadraticBezierSegment
							{
								Point1 = new Point(150, 50),
								Point2 = new Point(290, 100)
							}
						}
					}
				}
			},
			// PolyLineSegment: Draws a series of connected straight lines
			"PolyLine" => new PathGeometry
			{
				Figures = new PathFigureCollection
				{
					new PathFigure
					{
						StartPoint = new Point(150, 10),
						IsClosed = true,
						Segments = new PathSegmentCollection
						{
							new PolyLineSegment
							{
								Points = new PointCollection
								{
									new Point(180, 80),
									new Point(260, 80),
									new Point(200, 130),
									new Point(230, 200),
									new Point(150, 160),
									new Point(70, 200),
									new Point(100, 130),
									new Point(40, 80),
									new Point(120, 80)
								}
							}
						}
					}
				}
			},
			// PolyBezierSegment: Draws a series of connected cubic Bezier curves
			"PolyBezier" => new PathGeometry
			{
				Figures = new PathFigureCollection
				{
					new PathFigure
					{
						StartPoint = new Point(10, 100),
						IsClosed = false,
						Segments = new PathSegmentCollection
						{
							new PolyBezierSegment
							{
								Points = new PointCollection
								{
									// First Bezier curve (3 points: control1, control2, end)
									new Point(100, 50),
									new Point(150, 50),
									new Point(200, 100),
									// Second Bezier curve
									new Point(250, 150),
									new Point(200, 150),
									new Point(290, 100)
								}
							}
						}
					}
				}
			},
			// PolyQuadraticBezierSegment: Draws a series of connected quadratic Bezier curves
			"PolyQuadraticBezier" => new PathGeometry
			{
				Figures = new PathFigureCollection
				{
					new PathFigure
					{
						StartPoint = new Point(10, 100),
						IsClosed = false,
						Segments = new PathSegmentCollection
						{
							new PolyQuadraticBezierSegment
							{
								Points = new PointCollection
								{
									// First quadratic curve (2 points: control, end)
									new Point(100, 50),
									new Point(150, 100),
									// Second quadratic curve
									new Point(200, 150),
									new Point(290, 100)
								}
							}
						}
					}
				}
			},
			_ => new PathGeometry()
		};
	}

}