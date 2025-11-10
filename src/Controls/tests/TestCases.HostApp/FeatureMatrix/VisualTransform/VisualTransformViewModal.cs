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
	private bool _isVisible = true;

	// Additional Related Properties
	private double _translationX = 0.0;
	private double _translationY = 0.0;
	private double _anchorX = 0.5;
	private double _anchorY = 0.5;

	// Core Transformation Properties
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
	private bool _hasShadow = false;
	private Shadow _boxShadow = null;
	public bool HasShadow
	{
		get => _hasShadow;
		set
		{
			if (_hasShadow != value)
			{
				_hasShadow = value;
				// Minimal shadow that shouldn't interfere with layout
				BoxShadow = value
					? new Shadow
					{
						Radius = 2,
						Opacity = 0.15f,
						Brush = new SolidColorBrush(Colors.Gray),
						Offset = new Point(0, 0) // No offset to prevent positioning issues
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
		TranslationX = 0.0;
		TranslationY = 0.0;
		AnchorX = 0.5;
		AnchorY = 0.5;
		// Force property change notification for UI state
		_isVisible = false; // Set to opposite first
		IsVisible = true;   // Then set to desired value to trigger change
		HasShadow = false;
	}
	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}