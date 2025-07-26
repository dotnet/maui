namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 8945, "Add Open/Close API to picker controls", PlatformAffected.All)]
	public partial class Issue8945 : TestContentPage
	{
		bool _datePickerOpened;
		bool _datePickerClosed;
		bool _timePickerOpened;
		bool _timePickerClosed;
		bool _pickerOpened;
		bool _pickerClosed;

		public Issue8945()
		{
			InitializeComponent();
		}

		protected override void Init()
		{

		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			IsOpenDatePicker.Opened += IsOpenDatePickerOpened;
			IsOpenDatePicker.Closed += IsOpenDatePickerClosed;

			IsOpenTimePicker.Opened += IsOpenTimePickerOpened;
			IsOpenTimePicker.Closed += IsOpenTimePickerClosed;

			IsOpenPicker.Opened += IsOpenPickerOpened;
			IsOpenPicker.Closed += IsOpenPickerClosed;
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			IsOpenDatePicker.Opened -= IsOpenDatePickerOpened;
			IsOpenDatePicker.Closed -= IsOpenDatePickerClosed;

			IsOpenTimePicker.Opened -= IsOpenTimePickerOpened;
			IsOpenTimePicker.Closed -= IsOpenTimePickerClosed;

			IsOpenPicker.Opened -= IsOpenPickerOpened;
			IsOpenPicker.Closed -= IsOpenPickerClosed;
		}

		void OnOpenDatePickerClicked(object sender, EventArgs e)
		{
			IsOpenDatePicker.IsOpen = true;
		}

		void OnCloseDatePickerClicked(object sender, EventArgs e)
		{
			IsOpenDatePicker.IsOpen = false;
		}

		void OnOpenTimePickerClicked(object sender, EventArgs e)
		{
			IsOpenTimePicker.IsOpen = true;
		}

		void OnCloseTimePickerClicked(object sender, EventArgs e)
		{
			IsOpenTimePicker.IsOpen = false;
		}

		void OnOpenPickerClicked(object sender, EventArgs e)
		{
			IsOpenPicker.IsOpen = true;
		}

		void OnClosePickerClicked(object sender, EventArgs e)
		{
			IsOpenPicker.IsOpen = false;
		}

		void IsOpenDatePickerOpened(object sender, DatePickerOpenedEventArgs e)
		{
			_datePickerOpened = true;
			UpdateDatePickerStatus();
		}

		void IsOpenDatePickerClosed(object sender, DatePickerClosedEventArgs e)
		{
			_datePickerClosed = true;
			UpdateDatePickerStatus();
		}

		void IsOpenTimePickerOpened(object sender, TimePickerOpenedEventArgs e)
		{
			_timePickerOpened = true;
			UpdateTimePickerStatus();
		}

		void IsOpenTimePickerClosed(object sender, TimePickerClosedEventArgs e)
		{
			_timePickerClosed = true;
			UpdateTimePickerStatus();
		}

		void IsOpenPickerOpened(object sender, PickerOpenedEventArgs e)
		{
			_pickerOpened = true;
			UpdatePickerStatus();
		}

		void IsOpenPickerClosed(object sender, PickerClosedEventArgs e)
		{
			_pickerClosed = true;
			UpdatePickerStatus();
		}

		void UpdateDatePickerStatus()
		{
			if (_datePickerOpened && _datePickerClosed)
			{
				DatePickerStatusLabel.Text = "Passed";
			}
		}

		void UpdateTimePickerStatus()
		{
			if (_timePickerOpened && _timePickerClosed)
			{
				TimePickerStatusLabel.Text = "Passed";
			}
		}

		void UpdatePickerStatus()
		{
			if (_pickerOpened && _pickerClosed)
			{
				PickerStatusLabel.Text = "Passed";
			}
		}
	}
}