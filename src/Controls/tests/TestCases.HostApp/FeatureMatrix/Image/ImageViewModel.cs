using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
namespace Maui.Controls.Sample;

public class ImageViewModel : INotifyPropertyChanged
{
	private Aspect _aspect;
	private bool _isAnimationPlaying;
	private bool _isOpaque;
	private ImageSource _source = ImageSource.FromFile("animated_heart.gif");
	private string _sourcePath = "animated_heart.gif";


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

	// In your ViewModel
	private Color _color = Colors.Blue;
	public Color Color
	{
		get => _color;
		set { if (_color != value) { _color = value; OnPropertyChanged(); UpdateFontImageSource(); } }
	}

	private double _size = 40;
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

	private double _containerWidth = 350; // initial value as in XAML
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
	public string SourcePath
	{
		get => _sourcePath!;
		set
		{
			if (_sourcePath != value)
			{
				_sourcePath = value;
				if (!string.IsNullOrWhiteSpace(_sourcePath))
				{
					if (_sourcePath.StartsWith("http", System.StringComparison.OrdinalIgnoreCase))
						Source = ImageSource.FromUri(new System.Uri(_sourcePath));
					else
						Source = ImageSource.FromFile(_sourcePath);
				}
				OnPropertyChanged();
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}