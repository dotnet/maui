using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample;

public class StepperViewModel : INotifyPropertyChanged
{
	private double _minimum = 0;
	private double _maximum = 10;
	private double _increment = 1;
	private double _value = 0;
	private bool _isEnabled = true;
	private bool _isVisible = true;
	private FlowDirection _flowDirection = FlowDirection.LeftToRight;

	public double Minimum
	{
		get => _minimum;
		set => SetProperty(ref _minimum, value);
	}

	public double Maximum
	{
		get => _maximum;
		set => SetProperty(ref _maximum, value);
	}

	public double Increment
	{
		get => _increment;
		set => SetProperty(ref _increment, value);
	}

	public double Value
	{
		get => _value;
		set => SetProperty(ref _value, value);
	}

	public bool IsEnabled
	{
		get => _isEnabled;
		set => SetProperty(ref _isEnabled, value);
	}

	public bool IsVisible
	{
		get => _isVisible;
		set => SetProperty(ref _isVisible, value);
	}

	public FlowDirection FlowDirection
	{
		get => _flowDirection;
		set => SetProperty(ref _flowDirection, value);
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
	{
		if (EqualityComparer<T>.Default.Equals(backingStore, value))
			return;

		backingStore = value;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}