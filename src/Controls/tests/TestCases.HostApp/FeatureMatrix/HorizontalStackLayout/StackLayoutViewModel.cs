using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample;

public class StackLayoutViewModel : INotifyPropertyChanged
{
	public bool IsHorizontalVisible => IsHorizontal && IsVisible;
	public bool IsVerticalVisible => IsVertical && IsVisible;
	private bool _isVisible = true;
	public bool IsVisible
	{
		get => _isVisible;
		set
		{
			if (_isVisible != value)
			{
				_isVisible = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(IsHorizontalVisible));
				OnPropertyChanged(nameof(IsVerticalVisible));
			}
		}
	}

	private bool _isRtl = false;
	public bool IsRtl
	{
		get => _isRtl;
		set
		{
			if (_isRtl != value)
			{
				_isRtl = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(FlowDirection));
			}
		}
	}
	public FlowDirection FlowDirection => IsRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
	private bool _isHorizontal = true;
	public bool IsHorizontal
	{
		get => _isHorizontal;
		set
		{
			if (_isHorizontal != value)
			{
				_isHorizontal = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(IsVertical));
				OnPropertyChanged(nameof(IsHorizontalVisible));
				OnPropertyChanged(nameof(IsVerticalVisible));
			}
		}
	}
	public bool IsVertical
	{
		get => !_isHorizontal;
		set
		{
			if (_isHorizontal == value)
			{
				_isHorizontal = !value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(IsHorizontal));
				OnPropertyChanged(nameof(IsHorizontalVisible));
				OnPropertyChanged(nameof(IsVerticalVisible));
			}
		}
	}
	private double _spacing = 0;
	public double Spacing
	{
		get => _spacing;
		set
		{
			if (_spacing != value)
			{
				_spacing = value;
				OnPropertyChanged();
			}
		}
	}

	private double _rectHeight = 30;
	public double RectHeight
	{
		get => _rectHeight;
		set
		{
			if (_rectHeight != value)
			{
				_rectHeight = value;
				OnPropertyChanged();
			}
		}
	}

	private double _rectWidth = 30;
	public double RectWidth
	{
		get => _rectWidth;
		set
		{
			if (_rectWidth != value)
			{
				_rectWidth = value;
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