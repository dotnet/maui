using System.ComponentModel;
using System.Globalization;

namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 28208, "[Windows] The Slider and Stepper control does not work in One-Way binding mode with a MultiBinding Converter", PlatformAffected.UWP)]
public partial class Issue28208 : ContentPage
{
	Issue28208ViewModel _vm;
	public Issue28208()
	{
		InitializeComponent();
		this.BindingContext = _vm = new Issue28208ViewModel();
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
		if (_vm is not null)
		{
			_vm.Price++;
		}
	}
}

public class Issue28208ViewModel : INotifyPropertyChanged
{
	private double _price = 2;

	public double Price
	{
		get => _price;
		set
		{
			_price = value;
			OnPropertyChanged(nameof(Price));
		}
	}
	public event PropertyChangedEventHandler PropertyChanged;

	private void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

public class Issue28208OneWayMultiBindingValueConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values[0] is double price)
		{
			return price;
		}

		return 0.0;
	}
	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}