using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class SliderViewModel : INotifyPropertyChanged
	{
		private double _minimum = 0;
		private double _maximum = 1;
		private double _value = 0;
		private Color _thumbColor = null;
		private Color _minTrackColor = null;
		private Color _maxTrackColor = null;
		private Color _backgroundColor = null;
		private string _flowDirection = "LTR";
		private bool _isEnabled = true;
		private bool _isVisible = true;
		private string _thumbImageSource;
		private string _dragStartStatus;
		private string _dragCompletedStatus;

		public event PropertyChangedEventHandler PropertyChanged;

		public SliderViewModel()
		{
			DragStartedCommand = new Command(OnDragStarted);
			DragCompletedCommand = new Command(OnDragCompleted);
		}

		public double Minimum
		{
			get => _minimum;
			set { _minimum = value; OnPropertyChanged(); }
		}

		public double Maximum
		{
			get => _maximum;
			set { _maximum = value; OnPropertyChanged(); }
		}

		public double Value
		{
			get => _value;
			set { _value = value; OnPropertyChanged(); }
		}

		public Color ThumbColor
		{
			get => _thumbColor;
			set { _thumbColor = value; OnPropertyChanged(); }
		}

		public Color MinTrackColor
		{
			get => _minTrackColor;
			set { _minTrackColor = value; OnPropertyChanged(); }
		}

		public Color MaxTrackColor
		{
			get => _maxTrackColor;
			set { _maxTrackColor = value; OnPropertyChanged(); }
		}

		public Color BackgroundColor
		{
			get => _backgroundColor;
			set { _backgroundColor = value; OnPropertyChanged(); }
		}

		public string FlowDirection
		{
			get => _flowDirection;
			set { _flowDirection = value; OnPropertyChanged(); }
		}

		public bool IsEnabled
		{
			get => _isEnabled;
			set { _isEnabled = value; OnPropertyChanged(); }
		}

		public bool IsVisible
		{
			get => _isVisible;
			set { _isVisible = value; OnPropertyChanged(); }
		}

		public string ThumbImageSource
		{
			get => _thumbImageSource;
			set { _thumbImageSource = value; OnPropertyChanged(); }
		}

		public string DragStartStatus
		{
			get => _dragStartStatus;
			set { _dragStartStatus = value; OnPropertyChanged(); }
		}

		public string DragCompletedStatus
		{
			get => _dragCompletedStatus;
			set { _dragCompletedStatus = value; OnPropertyChanged(); }
		}

	 
		public ICommand DragStartedCommand { get; }
		public ICommand DragCompletedCommand { get; }


		private void OnDragStarted()
		{
			DragStartStatus = "Drag Started";
		}

		private void OnDragCompleted()
		{
			DragCompletedStatus = "Drag Completed";
		}


		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
