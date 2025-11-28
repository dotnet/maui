using System.ComponentModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 32903, "Slider Binding Initialization Order Causes Incorrect Value Assignment in XAML", PlatformAffected.All)]
	public partial class Issue32903 : ContentPage
	{
		public Issue32903()
		{
			InitializeComponent();
			
			var viewModel = new SliderViewModel();
			BindingContext = viewModel;
			
			// Log initial values for debugging
			Console.WriteLine($"[Issue32903] ViewModel initialized - Min: {viewModel.ValueMin}, Max: {viewModel.ValueMax}, Value: {viewModel.Value}");
			
			// Use Dispatcher to log actual slider values after bindings are applied
			Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
			{
				Console.WriteLine($"[Issue32903] After bindings - Slider.Minimum: {TestSlider.Minimum}, Slider.Maximum: {TestSlider.Maximum}, Slider.Value: {TestSlider.Value}");
				Console.WriteLine($"[Issue32903] ViewModel.Value: {viewModel.Value}");
			});
		}
	}
	
	public class SliderViewModel : INotifyPropertyChanged
	{
		private double _valueMin = 10;
		private double _valueMax = 100;
		private double _value = 50;
		
		public double ValueMin
		{
			get => _valueMin;
			set
			{
				if (_valueMin != value)
				{
					_valueMin = value;
					Console.WriteLine($"[SliderViewModel] ValueMin set to: {value}");
					OnPropertyChanged(nameof(ValueMin));
				}
			}
		}
		
		public double ValueMax
		{
			get => _valueMax;
			set
			{
				if (_valueMax != value)
				{
					_valueMax = value;
					Console.WriteLine($"[SliderViewModel] ValueMax set to: {value}");
					OnPropertyChanged(nameof(ValueMax));
				}
			}
		}
		
		public double Value
		{
			get => _value;
			set
			{
				if (_value != value)
				{
					Console.WriteLine($"[SliderViewModel] Value changing from {_value} to {value}");
					_value = value;
					OnPropertyChanged(nameof(Value));
				}
			}
		}
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
