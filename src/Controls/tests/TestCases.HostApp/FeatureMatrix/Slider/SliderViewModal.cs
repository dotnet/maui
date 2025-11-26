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
		private Color _thumbColor;
		private Color _minTrackColor;
		private Color _maxTrackColor;
		private Color _backgroundColor;
		private FlowDirection _flowDirection = FlowDirection.LeftToRight;
		private bool _isEnabled = true;
		private bool _isVisible = true;
		private string _thumbImageSource;
		private string _dragStartStatus;
		private string _dragCompletedStatus;
		private bool _isEventStatusLabelVisible = false;

		public event PropertyChangedEventHandler PropertyChanged;

		public SliderViewModel()
		{
			DragStartedCommand = new Command(OnDragStarted);
			DragCompletedCommand = new Command(OnDragCompleted);
		}

		public double Minimum
		{
			get => _minimum;
			set
			{
				if (_minimum != value)
				{
					_minimum = value;
					OnPropertyChanged();
				}
			}
		}

		public double Maximum
		{
			get => _maximum;
			set
			{
				if (_maximum != value)
				{
					_maximum = value;
					OnPropertyChanged();
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
					_value = value;
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

		public Color MinTrackColor
		{
			get => _minTrackColor;
			set
			{
				if (_minTrackColor != value)
				{
					_minTrackColor = value;
					OnPropertyChanged();
				}
			}
		}

		public Color MaxTrackColor
		{
			get => _maxTrackColor;
			set
			{
				if (_maxTrackColor != value)
				{
					_maxTrackColor = value;
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

		public string ThumbImageSource
		{
			get => _thumbImageSource;
			set
			{
				if (_thumbImageSource != value)
				{
					_thumbImageSource = value;
					OnPropertyChanged();
				}
			}
		}

		public string DragStartStatus
		{
			get => _dragStartStatus;
			set
			{
				if (_dragStartStatus != value)
				{
					if (!string.IsNullOrEmpty(value))
					{
						IsEventStatusLabelVisible = true;
					}
					_dragStartStatus = value;
					OnPropertyChanged();
				}
			}
		}

		public string DragCompletedStatus
		{
			get => _dragCompletedStatus;
			set
			{
				if (_dragCompletedStatus != value)
				{
					if (!string.IsNullOrEmpty(value))
					{
						IsEventStatusLabelVisible = true;
					}
					_dragCompletedStatus = value;
					OnPropertyChanged();
				}
			}
		}

		public bool IsEventStatusLabelVisible
		{
			get => _isEventStatusLabelVisible;
			set
			{
				if (_isEventStatusLabelVisible != value)
				{
					_isEventStatusLabelVisible = value;
					OnPropertyChanged();
				}
			}
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