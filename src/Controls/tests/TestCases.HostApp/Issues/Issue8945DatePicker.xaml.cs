namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, "8945DatePicker", "Add Open/Close API to picker controls (DatePicker)", PlatformAffected.All)]
	public partial class Issue8945DatePicker : TestContentPage
	{	
		bool _datePickerOpened;
		
		public Issue8945DatePicker()
		{
			InitializeComponent();
		}

		protected override void Init()
		{

		}
		
		protected override void OnAppearing()
		{
			base.OnAppearing();
			
			IsOpenDatePicker.Opened += IsOpenPickerOpened;
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			IsOpenDatePicker.Opened -= IsOpenPickerOpened;
		}	
		
		void IsOpenPickerOpened(object sender, PickerOpenedEventArgs e)
		{
			_datePickerOpened = true;
			UpdateDatePickerStatus();
		}
		
		void UpdateDatePickerStatus()
		{
			if (_datePickerOpened)
			{
				DatePickerStatusLabel.Text = "Passed";
			}
		}
	}
}