using System.ComponentModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25943, "[Android] DatePicker Graphical Bug", PlatformAffected.Android)]
	public partial class Issue25943 : ContentPage
	{
		public Issue25943()
		{
			BindingContext = new _25943MainPageViewModel();
			InitializeComponent();
		}
	}

	public class _25943MainPageViewModel : INotifyPropertyChanged
	{
		private DateTime _date = new DateTime(2024, 1, 1);
		public DateTime Date
		{
			get => _date;
			set
			{
				if (_date != value)
				{
					_date = value;
					OnPropertyChanged(nameof(Date));
				}
			}
		}

		private DateTime _maxDate = new DateTime(2024, 2, 29);
		public DateTime MaxDate
		{
			get => _maxDate;
			set
			{
				if (_maxDate != value)
				{
					_maxDate = value;
					OnPropertyChanged(nameof(MaxDate));
				}
			}
		}

		private DateTime _minDate = new DateTime(2023, 11, 1);
		public DateTime MinDate
		{
			get => _minDate;
			set
			{
				if (_minDate != value)
				{
					_minDate = value;
					OnPropertyChanged(nameof(MinDate));
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
