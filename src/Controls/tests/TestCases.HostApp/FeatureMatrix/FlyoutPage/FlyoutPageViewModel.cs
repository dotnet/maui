using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class FlyoutPageViewModel : INotifyPropertyChanged
{
	private string _title = "FlyoutPage";
	private bool _isPresented = false;
	private bool _isGestureEnabled = true;
	private FlyoutLayoutBehavior _flyoutLayoutBehavior = FlyoutLayoutBehavior.Default;
	private FlowDirection _flowDirection = FlowDirection.LeftToRight;
	private Color _backgroundColor = Colors.Lavender;
	private bool _isEnabled = true;
	private bool _isVisible = true;
	private ImageSource _iconImageSource = "coffee.png";

	public string Title
	{
		get => _title;
		set
		{
			if (_title != value)
			{
				_title = value;
				OnPropertyChanged();
			}
		}
	}

	public bool IsPresented
	{
		get => _isPresented;
		set
		{
			if (_isPresented != value)
			{
				_isPresented = value;
				OnPropertyChanged();
			}
		}
	}

	public ImageSource IconImageSource
	{
		get => _iconImageSource;
		set
		{
			if (_iconImageSource != value)
			{
				_iconImageSource = value;
				OnPropertyChanged();
			}
		}
	}


	public bool IsGestureEnabled
	{
		get => _isGestureEnabled;
		set
		{
			if (_isGestureEnabled != value)
			{
				_isGestureEnabled = value;
				OnPropertyChanged();
			}
		}
	}

	public FlyoutLayoutBehavior FlyoutLayoutBehavior
	{
		get => _flyoutLayoutBehavior;
		set
		{
			if (_flyoutLayoutBehavior != value)
			{
				_flyoutLayoutBehavior = value;
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

	public bool IsEnabled
	{
		get => _isEnabled;
		set
		{
			if (_isEnabled != value)
			{
				_isEnabled = value;
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

	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
