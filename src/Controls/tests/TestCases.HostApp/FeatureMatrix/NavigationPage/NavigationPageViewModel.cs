using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class NavigationPageViewModel : INotifyPropertyChanged
	{
		private Color _barBackgroundColor;
		private Brush _barBackground;
		private Color _barTextColor;
		private bool _hasNavigationBar = true;
		private bool _hasBackButton = true;
		private string _backButtonTitle = "Back";
		private Color _iconColor;
		private ImageSource _titleIconImageSource;
		private View _titleView;
		private string _title = "Sample Page";
		private string _lastNavigationEvent;
		private int _navigatedToCount;
		private int _navigatedFromCount;
		private int _navigatingFromCount;
		private string _lastNavigationParameters;
		private string _lastNavigatedFromEvent;
		private string _lastNavigatingFromEvent;

		public event PropertyChangedEventHandler PropertyChanged;

		public NavigationPageViewModel()
		{
			// Set platform-specific default colors
#if ANDROID
			BarBackgroundColor = Color.FromRgba(33, 150, 243, 255); // Material Blue
			BarTextColor = Colors.White;
#elif IOS || MACCATALYST
			BarBackgroundColor = Color.FromRgba(0, 122, 255, 255); // iOS Blue
			BarTextColor = Colors.White;
#else
			BarBackgroundColor = null;
			BarTextColor = null;
#endif
		}

		public Color BarBackgroundColor
		{
			get => _barBackgroundColor;
			set
			{
				if (_barBackgroundColor != value)
				{
					_barBackgroundColor = value;
					OnPropertyChanged();
				}
			}
		}

		public Brush BarBackground
		{
			get => _barBackground;
			set
			{
				if (_barBackground != value)
				{
					_barBackground = value;
					OnPropertyChanged();
				}
			}
		}

		public Color BarTextColor
		{
			get => _barTextColor;
			set
			{
				if (_barTextColor != value)
				{
					_barTextColor = value;
					OnPropertyChanged();
				}
			}
		}

		public bool HasNavigationBar
		{
			get => _hasNavigationBar;
			set
			{
				if (_hasNavigationBar != value)
				{
					_hasNavigationBar = value;
					OnPropertyChanged();
				}
			}
		}

		public bool HasBackButton
		{
			get => _hasBackButton;
			set
			{
				if (_hasBackButton != value)
				{
					_hasBackButton = value;
					OnPropertyChanged();
				}
			}
		}

		public string BackButtonTitle
		{
			get => _backButtonTitle;
			set
			{
				if (_backButtonTitle != value)
				{
					_backButtonTitle = value;
					OnPropertyChanged();
				}
			}
		}

		public Color IconColor
		{
			get => _iconColor;
			set
			{
				if (_iconColor != value)
				{
					_iconColor = value;
					OnPropertyChanged();
				}
			}
		}

		public ImageSource TitleIconImageSource
		{
			get => _titleIconImageSource;
			set
			{
				if (_titleIconImageSource != value)
				{
					_titleIconImageSource = value;
					OnPropertyChanged();
				}
			}
		}

		public View TitleView
		{
			get => _titleView;
			set
			{
				if (_titleView != value)
				{
					_titleView = value;
					OnPropertyChanged();
				}
			}
		}

		public string Title
		{
			get => _title;
			set
			{
				if (_title != value)
				{
					_title = value;
					OnPropertyChanged();
				}
			}
		}

		public string LastNavigationEvent
		{
			get => _lastNavigationEvent;
			set
			{
				if (_lastNavigationEvent != value)
				{
					_lastNavigationEvent = value;
					OnPropertyChanged();
				}
			}
		}

		public int NavigatedToCount
		{
			get => _navigatedToCount;
			set
			{
				if (_navigatedToCount != value)
				{
					_navigatedToCount = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(EventCountsText));
				}
			}
		}

		public int NavigatedFromCount
		{
			get => _navigatedFromCount;
			set
			{
				if (_navigatedFromCount != value)
				{
					_navigatedFromCount = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(EventCountsText));
				}
			}
		}

		public int NavigatingFromCount
		{
			get => _navigatingFromCount;
			set
			{
				if (_navigatingFromCount != value)
				{
					_navigatingFromCount = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(EventCountsText));
				}
			}
		}

		public string LastNavigationParameters
		{
			get => _lastNavigationParameters;
			set
			{
				if (_lastNavigationParameters != value)
				{
					_lastNavigationParameters = value;
					OnPropertyChanged();
				}
			}
		}

		public string LastNavigatedFromEvent
		{
			get => _lastNavigatedFromEvent;
			set
			{
				if (_lastNavigatedFromEvent != value)
				{
					_lastNavigatedFromEvent = value;
					OnPropertyChanged();
				}
			}
		}

		public string LastNavigatingFromEvent
		{
			get => _lastNavigatingFromEvent;
			set
			{
				if (_lastNavigatingFromEvent != value)
				{
					_lastNavigatingFromEvent = value;
					OnPropertyChanged();
				}
			}
		}

		public string EventCountsText => $"Counts: To={_navigatedToCount}, From={_navigatedFromCount}, PreFrom={_navigatingFromCount}";

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
