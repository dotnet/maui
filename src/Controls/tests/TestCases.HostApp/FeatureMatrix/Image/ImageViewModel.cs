using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
namespace Maui.Controls.Sample;

public class ImageViewModel : INotifyPropertyChanged
{
	private Aspect _aspect;
	private bool _isAnimationPlaying;
	private bool _isOpaque;
	private bool _isVisible = true;
	private double _size = 40;
	private Color _color = Colors.Blue;
	private bool _hasShadow;
	private Shadow _imageShadow;
	private FlowDirection _flowDirection = FlowDirection.LeftToRight;
	private ImageSource _source = ImageSource.FromFile("animated_heart.gif");

	public Aspect Aspect
	{
		get => _aspect;
		set { if (_aspect != value) { _aspect = value; OnPropertyChanged(); } }
	}

	public bool IsAnimationPlaying
	{
		get => _isAnimationPlaying;
		set { if (_isAnimationPlaying != value) { _isAnimationPlaying = value; OnPropertyChanged(); } }
	}

	public bool IsOpaque
	{
		get => _isOpaque;
		set { if (_isOpaque != value) { _isOpaque = value; OnPropertyChanged(); } }
	}

	public ImageSource Source
	{
		get => _source!;
		set { if (_source != value) { _source = value; OnPropertyChanged(); } }
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

	public Color Color
	{
		get => _color;
		set { if (_color != value) { _color = value; OnPropertyChanged(); UpdateFontImageSource(); } }
	}

	public double Size
	{
		get => _size;
		set { if (_size != value) { _size = value; OnPropertyChanged(); UpdateFontImageSource(); } }
	}

	private double _containerHeight = 550;
	public double ContainerHeight
	{
		get => _containerHeight;
		set
		{
			if (_containerHeight != value)
			{
				_containerHeight = value;
				OnPropertyChanged();
			}
		}
	}

	private double _containerWidth = 350;
	public double ContainerWidth
	{
		get => _containerWidth;
		set
		{
			if (_containerWidth != value)
			{
				_containerWidth = value;
				OnPropertyChanged();
			}
		}
	}

	private void UpdateFontImageSource()
	{
		if (Source is FontImageSource fontImage)
		{
			fontImage.Color = Color;
			fontImage.Size = Size;
			OnPropertyChanged(nameof(Source));
		}
	}

	public bool HasShadow
	{
		get => _hasShadow;
		set
		{
			if (_hasShadow != value)
			{
				_hasShadow = value;
				ImageShadow = value
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

	public Shadow ImageShadow
	{
		get => _imageShadow;
		private set
		{
			if (_imageShadow != value)
			{
				_imageShadow = value;
				OnPropertyChanged(nameof(ImageShadow));
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}