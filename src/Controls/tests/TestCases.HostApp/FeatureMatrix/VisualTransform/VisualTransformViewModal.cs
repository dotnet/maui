using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample;

public class VisualTransformViewModal : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;
	// Core Transformation Properties
	private double _rotation = 0.0;
	private double _rotationX = 0.0;
	private double _rotationY = 0.0;
	private double _scale = 1.0;
	private double _scaleX = 1.0;
	private double _scaleY = 1.0;
	private string _transform = null;

	// Additional Related Properties
	private double _translationX = 0.0;
	private double _translationY = 0.0;
	private double _anchorX = 0.5;
	private double _anchorY = 0.5;

	// Core Transformation Properties
	public double Rotation
	{
		get => _rotation;
		set
		{
			if (_rotation != value)
			{
				_rotation = value;
				OnPropertyChanged();
			}
		}
	}

	public double RotationX
	{
		get => _rotationX;
		set
		{
			if (_rotationX != value)
			{
				_rotationX = value;
				OnPropertyChanged();
			}
		}
	}

	public double RotationY
	{
		get => _rotationY;
		set
		{
			if (_rotationY != value)
			{
				_rotationY = value;
				OnPropertyChanged();
			}
		}
	}

	public double Scale
	{
		get => _scale;
		set
		{
			if (_scale != value)
			{
				_scale = value;
				OnPropertyChanged();
			}
		}
	}

	public double ScaleX
	{
		get => _scaleX;
		set
		{
			if (_scaleX != value)
			{
				_scaleX = value;
				OnPropertyChanged();
			}
		}
	}

	public double ScaleY
	{
		get => _scaleY;
		set
		{
			if (_scaleY != value)
			{
				_scaleY = value;
				OnPropertyChanged();
			}
		}
	}

	public string Transform
	{
		get => _transform;
		set
		{
			if (_transform != value)
			{
				_transform = value;
				OnPropertyChanged();
			}
		}
	}

	// Additional Related Properties
	public double TranslationX
	{
		get => _translationX;
		set
		{
			if (_translationX != value)
			{
				_translationX = value;
				OnPropertyChanged();
			}
		}
	}

	public double TranslationY
	{
		get => _translationY;
		set
		{
			if (_translationY != value)
			{
				_translationY = value;
				OnPropertyChanged();
			}
		}
	}

	public double AnchorX
	{
		get => _anchorX;
		set
		{
			if (_anchorX != value)
			{
				_anchorX = value;
				OnPropertyChanged();
			}
		}
	}

	public double AnchorY
	{
		get => _anchorY;
		set
		{
			if (_anchorY != value)
			{
				_anchorY = value;
				OnPropertyChanged();
			}
		}
	}

	// Reset Command
	public ICommand ResetCommand => new Command(Reset);

	private void Reset()
	{
		Rotation = 0.0;
		RotationX = 0.0;
		RotationY = 0.0;
		Scale = 1.0;
		ScaleX = 1.0;
		ScaleY = 1.0;
		Transform = null;
		TranslationX = 0.0;
		TranslationY = 0.0;
		AnchorX = 0.5;
		AnchorY = 0.5;
	}
	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}