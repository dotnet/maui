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
		private string _lastNavigatedToParameters;
		private string _lastNavigatedFromParameters;
		private string _lastNavigatingFromParameters;
		// Simplified event status flags
		private bool _navigatedToRaised;
		private bool _navigatedFromRaised;
		private bool _navigatingFromRaised;
		// Track which page raised each event
		private string _lastNavigatedToPage;
		private string _lastNavigatedFromPage;
		private string _lastNavigatingFromPage;

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

		/// <summary>
		/// Reset all bindable test properties and navigation event tracking back to initial defaults.
		/// This is invoked by the sample "Reset" button so automated UI tests can guarantee a clean state.
		/// </summary>
		public void Reset()
		{
			// Direct bar properties
#if ANDROID
			BarBackgroundColor = Color.FromRgba(33, 150, 243, 255);
			BarTextColor = Colors.White;
#elif IOS || MACCATALYST
			BarBackgroundColor = Color.FromRgba(0, 122, 255, 255);
			BarTextColor = Colors.White;
#else
			BarBackgroundColor = null;
			BarTextColor = null;
#endif
			BarBackground = null;

			// Attached properties / state
			HasNavigationBar = true;
			HasBackButton = true;
			BackButtonTitle = "Back";
			IconColor = null;
			TitleIconImageSource = null;
			TitleView = null;
			Title = "Sample Page";

			// Navigation event tracking
			LastNavigationEvent = null;
			NavigatedToCount = 0;
			NavigatedFromCount = 0;
			NavigatingFromCount = 0;
			LastNavigationParameters = null;
			LastNavigatedToParameters = null;
			LastNavigatedFromParameters = null;
			LastNavigatingFromParameters = null;
			NavigatedToRaised = false;
			NavigatedFromRaised = false;
			NavigatingFromRaised = false;
			LastNavigatedToPage = null;
			LastNavigatedFromPage = null;
			LastNavigatingFromPage = null;
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

		public string LastNavigatedToParameters
		{
			get => _lastNavigatedToParameters;
			set
			{
				if (_lastNavigatedToParameters != value)
				{
					_lastNavigatedToParameters = value;
					OnPropertyChanged();
				}
			}
		}

		public string LastNavigatedFromParameters
		{
			get => _lastNavigatedFromParameters;
			set
			{
				if (_lastNavigatedFromParameters != value)
				{
					_lastNavigatedFromParameters = value;
					OnPropertyChanged();
				}
			}
		}

		public string LastNavigatingFromParameters
		{
			get => _lastNavigatingFromParameters;
			set
			{
				if (_lastNavigatingFromParameters != value)
				{
					_lastNavigatingFromParameters = value;
					OnPropertyChanged();
				}
			}
		}


		// Event raised status flags (bind to simple labels)
		public bool NavigatedToRaised
		{
			get => _navigatedToRaised;
			set
			{
				if (_navigatedToRaised != value)
				{
					_navigatedToRaised = value;
					OnPropertyChanged();
				}
			}
		}

		public bool NavigatedFromRaised
		{
			get => _navigatedFromRaised;
			set
			{
				if (_navigatedFromRaised != value)
				{
					_navigatedFromRaised = value;
					OnPropertyChanged();
				}
			}
		}

		public bool NavigatingFromRaised
		{
			get => _navigatingFromRaised;
			set
			{
				if (_navigatingFromRaised != value)
				{
					_navigatingFromRaised = value;
					OnPropertyChanged();
				}
			}
		}

		// Page names indicating which page raised each event
		public string LastNavigatedToPage
		{
			get => _lastNavigatedToPage;
			set
			{
				if (_lastNavigatedToPage != value)
				{
					_lastNavigatedToPage = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(NavigatedToStatusText));
				}
			}
		}

		public string LastNavigatedFromPage
		{
			get => _lastNavigatedFromPage;
			set
			{
				if (_lastNavigatedFromPage != value)
				{
					_lastNavigatedFromPage = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(NavigatedFromStatusText));
				}
			}
		}

		public string LastNavigatingFromPage
		{
			get => _lastNavigatingFromPage;
			set
			{
				if (_lastNavigatingFromPage != value)
				{
					_lastNavigatingFromPage = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(NavigatingFromStatusText));
				}
			}
		}

		public string NavigatedToStatusText => _navigatedToRaised && !string.IsNullOrEmpty(_lastNavigatedToPage) ? $"Raised on '{_lastNavigatedToPage}'" : "Not yet";
		public string NavigatedFromStatusText => _navigatedFromRaised && !string.IsNullOrEmpty(_lastNavigatedFromPage) ? $"Raised on '{_lastNavigatedFromPage}'" : "Not yet";
		public string NavigatingFromStatusText => _navigatingFromRaised && !string.IsNullOrEmpty(_lastNavigatingFromPage) ? $"Raised on '{_lastNavigatingFromPage}'" : "Not yet";

		public string EventCountsText => $"Counts: To={_navigatedToCount}, From={_navigatedFromCount}, PreFrom={_navigatingFromCount}";

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
