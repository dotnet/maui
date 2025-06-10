using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class TwoPaneViewViewModel : INotifyPropertyChanged
	{
		private double _minTallModeHeight = 300;
		private double _minWideModeWidth = 500;
		private GridLength _pane1Length = new GridLength(1, GridUnitType.Star);
		private GridLength _pane2Length = new GridLength(1, GridUnitType.Star);

		public event PropertyChangedEventHandler PropertyChanged;

		public double MinTallModeHeight
		{
			get => _minTallModeHeight;
			set
			{
				if (_minTallModeHeight != value)
				{
					_minTallModeHeight = value;
					OnPropertyChanged();
				}
			}
		}

		public double MinWideModeWidth
		{
			get => _minWideModeWidth;
			set
			{
				if (_minWideModeWidth != value)
				{
					_minWideModeWidth = value;
					OnPropertyChanged();
				}
			}
		}

		public GridLength Pane1Length
		{
			get => _pane1Length;
			set
			{
				if (_pane1Length != value)
				{
					_pane1Length = value;
					OnPropertyChanged();
				}
			}
		}

		public GridLength Pane2Length
		{
			get => _pane2Length;
			set
			{
				if (_pane2Length != value)
				{
					_pane2Length = value;
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