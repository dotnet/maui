using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample;

public class LayoutViewModel : INotifyPropertyChanged
{
	private LayoutOptions _horizontalOptions = LayoutOptions.Fill;
	private LayoutOptions _verticalOptions = LayoutOptions.Fill;
	private double _widthRequest = -1;
	private double _heightRequest = -1;
	private ScrollOrientation _orientation = ScrollOrientation.Vertical;
	private FlexDirection _flexDirection = FlexDirection.Column;
	private int _labelCount = 3;
	private int _rowCount = 2;
	private int _columnCount = 2;

	public ScrollOrientation Orientation
	{
		get => _orientation;
		set
		{
			if (_orientation != value)
			{
				_orientation = value;
				OnPropertyChanged(nameof(Orientation));
			}
		}
	}

	public LayoutOptions HorizontalOptions
	{
		get => _horizontalOptions;
		set
		{
			if (_horizontalOptions != value)
			{
				_horizontalOptions = value;
				OnPropertyChanged(nameof(HorizontalOptions));
			}
		}
	}

	public LayoutOptions VerticalOptions
	{
		get => _verticalOptions;
		set
		{
			if (_verticalOptions != value)
			{
				_verticalOptions = value;
				OnPropertyChanged(nameof(VerticalOptions));
			}
		}
	}

	public double WidthRequest
	{
		get => _widthRequest;
		set
		{
			if (_widthRequest != value)
			{
				_widthRequest = value;
				OnPropertyChanged(nameof(WidthRequest));
			}
		}
	}

	public double HeightRequest
	{
		get => _heightRequest;
		set
		{
			if (_heightRequest != value)
			{
				_heightRequest = value;
				OnPropertyChanged(nameof(HeightRequest));
			}
		}
	}

	public int LabelCount
	{
		get => _labelCount;
		set
		{
			if (_labelCount != value)
			{
				_labelCount = value;
				OnPropertyChanged(nameof(LabelCount));
			}
		}
	}

	public int RowCount
	{
		get => _rowCount;
		set
		{
			if (_rowCount != value)
			{
				_rowCount = value;
				OnPropertyChanged(nameof(RowCount));
			}
		}
	}

	public int ColumnCount
	{
		get => _columnCount;
		set
		{
			if (_columnCount != value)
			{
				_columnCount = value;
				OnPropertyChanged(nameof(ColumnCount));
			}
		}
	}

	public FlexDirection FlexDirection
	{
		get => _flexDirection;
		set
		{
			if (_flexDirection != value)
			{
				_flexDirection = value;
				OnPropertyChanged(nameof(FlexDirection));
			}
		}
	}
	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
		 PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}