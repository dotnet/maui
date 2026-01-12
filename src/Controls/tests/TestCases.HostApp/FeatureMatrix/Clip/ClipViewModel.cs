using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Shapes;
namespace Maui.Controls.Sample;

public class ClipViewModel : INotifyPropertyChanged
{
	private Color _color = Colors.Red;
	private Geometry _clip = null;
	private string _selectedControl = "Image";
	private string _selectedClip = "null";
	private IShape _strokeShape = new Rectangle();
	private Shadow _shadow = null;
	private Brush _stroke = Brush.Red;
	private double _strokeThickness = 5;


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
	public string SelectedControl
	{
		get => _selectedControl;
		set
		{
			if (_selectedControl != value)
			{
				_selectedControl = value;
				OnPropertyChanged();
			}
		}
	}

	public string SelectedClip
	{
		get => _selectedClip;
		set
		{
			if (_selectedClip != value)
			{
				_selectedClip = value;
				OnPropertyChanged();
			}
		}
	}

	public IShape StrokeShape
	{
		get => _strokeShape;
		set
		{
			if (_strokeShape != value)
			{
				_strokeShape = value;
				OnPropertyChanged(nameof(StrokeShape));
			}
		}
	}

	public Shadow Shadow
	{
		get => _shadow;
		set
		{
			if (_shadow != value)
			{
				_shadow = value;
				OnPropertyChanged();
			}
		}
	}

	public Brush Stroke
	{
		get => _stroke;
		set
		{
			if (_stroke != value)
			{
				_stroke = value;
				OnPropertyChanged();
			}
		}
	}
	public double StrokeThickness
	{
		get => _strokeThickness;
		set
		{
			if (_strokeThickness != value)
			{
				_strokeThickness = value;
				OnPropertyChanged();
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}