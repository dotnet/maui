using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample;

public class FlexLayoutViewModel : INotifyPropertyChanged
{
	private FlexAlignContent _alignContent = FlexAlignContent.Stretch;
	private FlexAlignItems _alignItems = FlexAlignItems.Stretch;
	private FlexDirection _direction = FlexDirection.Row;
	private double _heightRequest = 70;
	private double _specificHeightRequest = 70;
	private double _widthRequest = 70;
	private FlexJustify _justifyContent = FlexJustify.Start;
	private FlexWrap _wrap = FlexWrap.NoWrap;
	private FlexAlignSelf _child1AlignSelf = FlexAlignSelf.Auto;
	private float _child1Grow = 0;
	private int _child1Order = 0;
	private float _childShrink = 1;
	private string _child1BasisMode = "Auto";
	private FlexPosition _child1Position = FlexPosition.Relative;

	public FlexAlignContent AlignContent
	{
		get => _alignContent;
		set { _alignContent = value; OnPropertyChanged(); }
	}
	public FlexAlignItems AlignItems
	{
		get => _alignItems;
		set { _alignItems = value; OnPropertyChanged(); }
	}
	public FlexDirection Direction
	{
		get => _direction;
		set { _direction = value; OnPropertyChanged(); }
	}
	public double HeightRequest
	{
		get => _heightRequest;
		set { _heightRequest = value; OnPropertyChanged(); }
	}
	public double SpecificHeightRequest
	{
		get => _specificHeightRequest;
		set { _specificHeightRequest = value; OnPropertyChanged(); }
	}
	public double WidthRequest
	{
		get => _widthRequest;
		set { _widthRequest = value; OnPropertyChanged(); }
	}
	public FlexJustify JustifyContent
	{
		get => _justifyContent;
		set { _justifyContent = value; OnPropertyChanged(); }
	}
	public FlexWrap Wrap
	{
		get => _wrap;
		set { _wrap = value; OnPropertyChanged(); }
	}

	public FlexAlignSelf Child1AlignSelf
	{
		get => _child1AlignSelf;
		set { _child1AlignSelf = value; OnPropertyChanged(); }
	}
	public float Child1Grow
	{
		get => _child1Grow;
		set { _child1Grow = value; OnPropertyChanged(); }
	}
	public int Child1Order
	{
		get => _child1Order;
		set { _child1Order = value; OnPropertyChanged(); }
	}
	public float ChildShrink
	{
		get => _childShrink;
		set { _childShrink = value; OnPropertyChanged(); }
	}

	public string Child1BasisMode
	{
		get => _child1BasisMode;
		set { _child1BasisMode = value; OnPropertyChanged(); OnPropertyChanged(nameof(Child1Basis)); }
	}

	public FlexBasis Child1Basis
	{
		get
		{
			return _child1BasisMode switch
			{
				"Fixed100" => new FlexBasis(100),
				"Percent50" => new FlexBasis(0.5f, true),
				_ => FlexBasis.Auto
			};
		}
	}

	public FlexPosition Child1Position
	{
		get => _child1Position;
		set { _child1Position = value; OnPropertyChanged(); }
	}

	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
