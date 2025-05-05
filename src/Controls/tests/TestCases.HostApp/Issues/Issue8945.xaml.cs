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

			IsOpenPicker.Opened += IsOpenPickerOpened;
			IsOpenPicker.Closed += IsOpenPickerClosed;
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			IsOpenPicker.Opened -= IsOpenPickerOpened;
			IsOpenPicker.Closed -= IsOpenPickerClosed;
		}

		void OnOpenClicked(object sender, EventArgs e)
		{
			IsOpenPicker.IsOpen = true;
		}

		void OnCloseClicked(object sender, EventArgs e)
		{
			IsOpenPicker.IsOpen = false;
		}

		void IsOpenPickerOpened(object sender, PickerOpenedEventArgs e)
		{
			StatusLabel.Text = "Picker Opened";
		}

		void IsOpenPickerClosed(object sender, PickerClosedEventArgs e)
		{
			StatusLabel.Text = "Picker Closed";
		}
	}
}