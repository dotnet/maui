using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class GraphicsViewViewModel : INotifyPropertyChanged
	{
		private string _drawableType = "Square";
		private Color _backgroundColor = Colors.Transparent;
		private FlowDirection _flowDirection = FlowDirection.LeftToRight;
		private bool _isEnabled = true;
		private bool _isVisible = true;
		private string _eventStatus;
		private bool _isEventStatusLabelVisible = false;

		public event PropertyChangedEventHandler PropertyChanged;

		public string DrawableType
		{
			get => _drawableType;
			set
			{
				if (_drawableType != value)
				{
					_drawableType = value;
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

		public string EventStatus
		{
			get => _eventStatus;
			set
			{
				if (_eventStatus != value)
				{
					_eventStatus = value;
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

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
