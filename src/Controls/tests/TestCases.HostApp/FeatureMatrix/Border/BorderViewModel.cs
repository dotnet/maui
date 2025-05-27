using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample;

public class BorderViewModel : INotifyPropertyChanged
{
	private View contentView;
	public View ContentView
	{
		get => contentView;
		set
		{
			if (contentView != value)
			{
				contentView = value;
				OnPropertyChanged();
			}
		}
	}

	public BorderViewModel()
	{
		SetLabelContent();
	}

	public void SetLabelContent()
	{
		ContentView = new Label
		{
			Text = "This is a bordered label!",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
	}

	public void SetButtonContent()
	{
		ContentView = new Button
		{
			Text = "Click Me",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
	}

	public void SetImageContent()
	{
		ContentView = new Image
		{
			Source = "dotnet_bot.png", // Make sure this image is added to your resources
			WidthRequest = 100,
			HeightRequest = 100,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
	}
	private Thickness _padding = 5;
	public Thickness Padding
	{
		get => _padding;
		set { _padding = value; OnPropertyChanged(nameof(Padding)); }
	}
	private double _strokeThickness = 8;
	public double StrokeThickness
	{
		get => _strokeThickness;
		set
		{
			if (_strokeThickness != value)
			{
				_strokeThickness = value;
				OnPropertyChanged(nameof(StrokeThickness));
			}
		}
	}
	public string StrokeDashArrayAsString => string.Join(",", StrokeDashArray);
	private DoubleCollection _strokeDashArray = new DoubleCollection { 5, 2 };
	public DoubleCollection StrokeDashArray
	{
		get => _strokeDashArray;
		set
		{
			if (_strokeDashArray != value)
			{
				_strokeDashArray = value;
				OnPropertyChanged(nameof(StrokeDashArray));
				OnPropertyChanged(nameof(StrokeDashArrayAsString));
			}
		}
	}
	private double _strokeDashOffset = 0;
	public double StrokeDashOffset
	{
		get => _strokeDashOffset;
		set { _strokeDashOffset = value; OnPropertyChanged(nameof(StrokeDashOffset)); }
	}
	private PenLineJoin _strokeLineJoin = PenLineJoin.Miter;
	public PenLineJoin StrokeLineJoin
	{
		get => _strokeLineJoin;
		set
		{
			if (_strokeLineJoin != value)
			{
				_strokeLineJoin = value;
				OnPropertyChanged();
			}
		}
	}

	private double _strokeMiterLimit = 10.0;
	public double StrokeMiterLimit
	{
		get => _strokeMiterLimit;
		set
		{
			if (_strokeMiterLimit != value)
			{
				_strokeMiterLimit = value;
				OnPropertyChanged();
			}
		}
	}

	private IShape _strokeShape = new Rectangle();
	public IShape StrokeShape
	{
		get => _strokeShape;
		set { _strokeShape = value; OnPropertyChanged(nameof(StrokeShape)); }
	}
	private PenLineCap _strokeLineCap = PenLineCap.Flat;
	public PenLineCap StrokeLineCap
	{
		get => _strokeLineCap;
		set
		{
			if (_strokeLineCap != value)
			{
				_strokeLineCap = value;
				OnPropertyChanged();
			}
		}
	}
	private Brush _stroke = Brush.Red;
	public Brush Stroke
	{
		get => _stroke;
		set { _stroke = value; OnPropertyChanged(nameof(Stroke)); }
	}
	private Shadow _shadow = new Shadow
	{
		Brush = Brush.Gray,
		Offset = new Point(10, 10),
		Radius = 20,
		Opacity = 0.8f
	};

	public Shadow Shadow
	{
		get => _shadow;
		set
		{
			if (_shadow != value)
			{
				_shadow = value;
				OnPropertyChanged(nameof(Shadow));
			}
		}
	}

	public void UpdateShadow(double offsetX, double offsetY, double radius, float opacity)
	{
		Shadow = new Shadow
		{
			Brush = new SolidColorBrush(Colors.Black),
			Offset = new Point(offsetX, offsetY),
			Radius = (float)radius,
			Opacity = opacity
		};
	}
	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}