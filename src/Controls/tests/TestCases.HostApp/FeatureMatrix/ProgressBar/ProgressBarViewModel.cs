using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class ProgressBarViewModel : INotifyPropertyChanged
	{
		private double _progress = 0.5;
		private Color _progressColor;
		private Color _backgroundColor;
		private bool _isVisible = true;
		private FlowDirection _flowDirection = FlowDirection.LeftToRight;
		private Shadow _shadow;

		public event PropertyChangedEventHandler PropertyChanged;

		public double Progress
		{
			get => _progress;
			set
			{
				if (_progress != value)
				{
					_progress = value;
					OnPropertyChanged();
				}
			}
		}

		public Color ProgressColor
		{
			get => _progressColor;
			set
			{
				if (_progressColor != value)
				{
					_progressColor = value;
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

		public Shadow Shadow
		{
			get => _shadow;
			set
			{
				if (_shadow != value)
				{
					_shadow = value;
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
