namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, "8945TimePicker", "Add Open/Close API to picker controls (TimePicker)", PlatformAffected.All)]
	public partial class Issue8945TimePicker : TestContentPage
	{	
		bool _timePickerOpened;
		
		public Issue8945TimePicker()
		{
			InitializeComponent();
		}

		protected override void Init()
		{

		}
		
		protected override void OnAppearing()
		{
			base.OnAppearing();
			
			IsOpenTimePicker.Opened += IsOpenPickerOpened;
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			IsOpenTimePicker.Opened -= IsOpenPickerOpened;
		}	
		
		void IsOpenPickerOpened(object sender, PickerOpenedEventArgs e)
		{
			_timePickerOpened = true;
			UpdateDatePickerStatus();
		}
		
		void UpdateDatePickerStatus()
		{
			if (_timePickerOpened)
			{
				TimePickerStatusLabel.Text = "Passed";
			}
		}
	}
}