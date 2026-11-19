using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class ProgressBarViewModel : INotifyPropertyChanged
	{
		const string DefaultProgressText = "0.50";
		const string DefaultProgressToText = "0";

		private double _progress = 0.5;
		private string _progressText = DefaultProgressText;
		private string _progressToText = DefaultProgressToText;
		private Color _progressColor;
		private Color _backgroundColor;
		private bool _isEnabled = true;
		private bool _isVisible = true;
		private FlowDirection _flowDirection = FlowDirection.LeftToRight;
		private Shadow _shadow;

		public ProgressBarViewModel()
		{
			ProgressColorCommand = new Command<Color>(color => ProgressColor = color);
			BackgroundColorCommand = new Command<Color>(color => BackgroundColor = color);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public ICommand ProgressColorCommand { get; }

		public ICommand BackgroundColorCommand { get; }

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

		public string ProgressText
		{
			get => _progressText;
			set
			{
				if (_progressText != value)
				{
					_progressText = value;
					OnPropertyChanged();

					if (double.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out double progress))
					{
						Progress = progress;
					}
				}
			}
		}

		public string ProgressToText
		{
			get => _progressToText;
			set
			{
				if (_progressToText != value)
				{
					_progressToText = value;
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
					OnPropertyChanged(nameof(HasShadow));
				}
			}
		}

		public bool HasShadow
		{
			get => _shadow is not null;
			set
			{
				if (HasShadow != value)
				{
					Shadow = value
						? new Shadow { Brush = Colors.Violet, Radius = 20, Offset = new Point(0, 0), Opacity = 1f }
						: null;
				}
			}
		}

		public bool TryGetProgressToValue(out double value) =>
			double.TryParse(ProgressToText, NumberStyles.Float, CultureInfo.CurrentCulture, out value);

		public void Reset()
		{
			ProgressText = DefaultProgressText;
			Progress = 0.5;
			ProgressToText = DefaultProgressToText;
			ProgressColor = null;
			// Android does not clear an existing native background when BackgroundColor becomes null.
			BackgroundColor = Colors.Transparent;
			IsEnabled = true;
			IsVisible = true;
			FlowDirection = FlowDirection.LeftToRight;
			Shadow = null;
		}

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
