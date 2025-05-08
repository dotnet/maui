namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 8945, "Add Open/Close API to picker controls", PlatformAffected.All)]
	public partial class Issue8945 : TestContentPage
	{
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

		void IsOpenDatePickerOpened(object sender, PickerOpenedEventArgs e)
		{
			DatePickerStatusLabel.Text = "DatePicker Opened";
		}

		void IsOpenDatePickerClosed(object sender, PickerClosedEventArgs e)
		{
			DatePickerStatusLabel.Text = "DatePicker Closed";
		}

		void IsOpenTimePickerOpened(object sender, PickerOpenedEventArgs e)
		{
			TimePickerStatusLabel.Text = "TimePicker Opened";
		}

		void IsOpenTimePickerClosed(object sender, PickerClosedEventArgs e)
		{
			TimePickerStatusLabel.Text = "TimePicker Closed";
		}

		void IsOpenPickerOpened(object sender, PickerOpenedEventArgs e)
		{
			PickerStatusLabel.Text = "Picker Opened";
		}

		void IsOpenPickerClosed(object sender, PickerClosedEventArgs e)
		{
			PickerStatusLabel.Text = "Picker Closed";
		}
	}
}