using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample;

public class SwitchViewModel : INotifyPropertyChanged
{
	private FlowDirection _flowDirection = FlowDirection.LeftToRight;
	private bool _isEnabled = true;
	private bool _isVisible = true;
	private bool _isToggled = false;
	private Color _onColor;
	private Shadow _shadow;
	private Color _thumbColor;

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

	public bool IsToggled
	{
		get => _isToggled;
		set
		{
			if (_isToggled != value)
			{
				_isToggled = value;
				OnPropertyChanged();
			}
		}
	}

	public Color OnColor
	{
		get => _onColor;
		set
		{
			if (_onColor != value)
			{
				_onColor = value;
				OnPropertyChanged();
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

	public Color ThumbColor
	{
		get => _thumbColor;
		set
		{
			if (_thumbColor != value)
			{
				_thumbColor = value;
				OnPropertyChanged();
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
