namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, "8945Picker", "Add Open/Close API to picker controls (Picker)", PlatformAffected.All)]
	public partial class Issue8945Picker : TestContentPage
	{
		bool _pickerOpened;

		public Issue8945Picker()
		{
			InitializeComponent();

			IsOpenPicker.Opened += IsOpenPickerOpened;
		}

		protected override void Init()
		{

		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			IsOpenPicker.Opened -= IsOpenPickerOpened;
		}

		void IsOpenPickerOpened(object sender, PickerOpenedEventArgs e)
		{
			_pickerOpened = true;
			UpdatePickerStatus();
		}

		void UpdatePickerStatus()
		{
			if (_pickerOpened)
			{
				PickerStatusLabel.Text = "Passed";
			}
		}
	}
}