using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
namespace Maui.Controls.Sample;

public class ImageClipViewModel : INotifyPropertyChanged
{
	private Aspect _aspect;
	private double _size = 40;
	private Color _color = Colors.Blue;
	private Geometry _clip = null;
	private ImageSource _source = ImageSource.FromFile("animated_heart.gif");

	public Aspect Aspect
	{
		get => _aspect;
		set { if (_aspect != value) { _aspect = value; OnPropertyChanged(); } }
	}
	public ImageSource Source
	{
		get => _source!;
		set { if (_source != value) { _source = value; OnPropertyChanged(); } }
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

	private void UpdateFontImageSource()
	{
		if (Source is FontImageSource fontImage)
		{
			fontImage.Color = Color;
			fontImage.Size = Size;
			OnPropertyChanged(nameof(Source));
		}
	}

	public Geometry Clip
	{
		get => _clip;
		set
		{
			if (_clip != value)
			{
				_clip = value;
				OnPropertyChanged();
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}