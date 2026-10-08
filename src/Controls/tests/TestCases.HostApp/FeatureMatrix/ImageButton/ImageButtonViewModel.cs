using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public class ImageButtonViewModel : INotifyPropertyChanged
{
	private Aspect _aspect = Aspect.AspectFit;
	private bool _isOpaque = false;
	private ImageSource _source = ImageSource.FromFile("dotnet_bot.png");
	private Color _borderColor = Colors.Black;
	private double _borderWidth = 1;
	private int _cornerRadius;
	private bool _isEnabled = true;
	private Thickness _padding = new Thickness(10);
	private bool _isVisible = true;
	private Shadow _shadow = null;
	private FlowDirection _flowDirection = FlowDirection.LeftToRight;
	private Color _backgroundColor;
	private double _imageButtonOpacity = 1.0;
	private bool _inputTransparent = false;
	private double _rotation = 0;
	private double _scale = 1.0;
	public ICommand ImageCommand { get; }
	private int _clickTotal;
	private int _pressedTotal;
	private int _releasedTotal;
	private bool _isButtonClicked;
	private string _commandResult = "Tap the ImageButton to see the result.";
	public object CommandParameter { get; set; }


	public Aspect Aspect
	{
		get => _aspect;
		set { if (_aspect != value) { _aspect = value; OnPropertyChanged(); } }
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

	public Color BorderColor
	{
		get => _borderColor;
		set { if (_borderColor != value) { _borderColor = value; OnPropertyChanged(); } }
	}

	public double BorderWidth
	{
		get => _borderWidth;
		set { if (_borderWidth != value) { _borderWidth = value; OnPropertyChanged(); } }
	}

	public int CornerRadius
	{
		get => _cornerRadius;
		set { if (_cornerRadius != value) { _cornerRadius = value; OnPropertyChanged(); } }
	}

	public bool IsEnabled
	{
		get => _isEnabled;
		set { if (_isEnabled != value) { _isEnabled = value; OnPropertyChanged(); } }
	}

	public Thickness Padding
	{
		get => _padding;
		set { if (_padding != value) { _padding = value; OnPropertyChanged(); } }
	}

	public bool IsVisible
	{
		get => _isVisible;
		set { if (_isVisible != value) { _isVisible = value; OnPropertyChanged(); } }
	}

	public Shadow Shadow
	{
		get => _shadow;
		set { if (_shadow != value) { _shadow = value; OnPropertyChanged(); } }
	}

	public FlowDirection FlowDirection
	{
		get => _flowDirection;
		set { if (_flowDirection != value) { _flowDirection = value; OnPropertyChanged(); } }
	}

	public Color BackgroundColor
	{
		get => _backgroundColor;
		set { if (_backgroundColor != value) { _backgroundColor = value; OnPropertyChanged(); } }
	}

	public double ImageButtonOpacity
	{
		get => _imageButtonOpacity;
		set { if (_imageButtonOpacity != value) { _imageButtonOpacity = value; OnPropertyChanged(); } }
	}

	public bool InputTransparent
	{
		get => _inputTransparent;
		set { if (_inputTransparent != value) { _inputTransparent = value; OnPropertyChanged(); } }
	}

	public double ImageButtonRotation
	{
		get => _rotation;
		set { if (_rotation != value) { _rotation = value; OnPropertyChanged(); } }
	}

	public double ImageButtonScale
	{
		get => _scale;
		set { if (_scale != value) { _scale = value; OnPropertyChanged(); } }
	}

	public string CommandResult
	{
		get => _commandResult;
		set { _commandResult = value; OnPropertyChanged(); }
	}

	public int ClickTotal
	{
		get => _clickTotal;
		set { _clickTotal = value; OnPropertyChanged(); }
	}

	public int PressedTotal
	{
		get => _pressedTotal;
		set { _pressedTotal = value; OnPropertyChanged(); }
	}

	public int ReleasedTotal
	{
		get => _releasedTotal;
		set { _releasedTotal = value; OnPropertyChanged(); }
	}


	public bool IsButtonClicked
	{
		get => _isButtonClicked;
		set
		{
			if (_isButtonClicked != value)
			{
				_isButtonClicked = value;
				OnPropertyChanged();
			}
		}
	}

	public ImageButtonViewModel()
	{
		CommandParameter = "CommandParameter";
		ImageCommand = new Command<object>(OnImageCommandExecuted);
	}

	private void OnImageCommandExecuted(object parameter)
	{
		CommandResult = $"ImageButton: {parameter}";
		IsButtonClicked = true;
	}


	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

}