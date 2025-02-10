using System;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 23951, "[Android] Frame disappears when assigning GradientStops to LinearGradientBrush inside this Frame", PlatformAffected.Android)]
	public partial class Issue23951 : ContentPage
	{
		public Issue23951()
		{
			InitializeComponent();
			BindingContext = new Issue23951ViewModel();
		}

	}

	public partial class Issue23951ViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		private GradientStopCollection _gradientStops = new GradientStopCollection();
		public GradientStopCollection GradientStops
		{
			get => _gradientStops;
			private set
			{
				_gradientStops = value;
				OnPropertyChanged(nameof(GradientStops));
			}
		}
		public void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		public ICommand ClickCommand { get; private set; }
		public Issue23951ViewModel()
		{
			UpdateGradientStops();
			ClickCommand = new Command(Click);
		}

		private void Click(object obj)
		{
			UpdateGradientStops();
		}

		private void UpdateGradientStops()
		{
			GradientStops = new GradientStopCollection();
			GradientStops.Add(new GradientStop(Color.Parse("Red"), 0f));
			GradientStops.Add(new GradientStop(Color.Parse("Blue"), 1f));
		}
	}
}