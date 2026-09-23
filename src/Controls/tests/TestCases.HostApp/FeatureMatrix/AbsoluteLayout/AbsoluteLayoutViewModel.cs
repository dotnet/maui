using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
namespace Maui.Controls.Sample;

public class AbsoluteLayoutViewModel : INotifyPropertyChanged
{
	double _x = 0, _y = 0;
	double _width = AbsoluteLayout.AutoSize;
	double _height = AbsoluteLayout.AutoSize;
	private bool _isVisible = true;
	private Color _backgroundColor = Colors.Lavender;
	private FlowDirection _flowDirection = FlowDirection.LeftToRight;
	AbsoluteLayoutFlags _layoutFlags = AbsoluteLayoutFlags.None;
	public Rect Bounds => new Rect(X, Y, Width, Height);

	public double X
	{
		get => _x;
		set { _x = value; OnPropertyChanged(); OnPropertyChanged(nameof(Bounds)); }
	}

	public double Y
	{
		get => _y;
		set { _y = value; OnPropertyChanged(); OnPropertyChanged(nameof(Bounds)); }
	}

	public double Width
	{
		get => _width;
		set { _width = value; OnPropertyChanged(); OnPropertyChanged(nameof(Bounds)); }
	}

	public double Height
	{
		get => _height;
		set { _height = value; OnPropertyChanged(); OnPropertyChanged(nameof(Bounds)); }
	}

	public AbsoluteLayoutFlags LayoutFlags
	{
		get => _layoutFlags;
		set { _layoutFlags = value; OnPropertyChanged(); }
	}

	public Color BackgroundColor
	{
		get => _backgroundColor;
		set
		{
			if (_backgroundColor != value)
			{
				_backgroundColor = value;
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

	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}