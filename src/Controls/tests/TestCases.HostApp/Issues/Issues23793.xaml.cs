namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 23793, "When the date is changed, the DatePicker (Format='D') text didn't keep format", PlatformAffected.iOS)]
	public partial class Issue23793 : ContentPage
	{
		public Issue23793()
		{
			InitializeComponent();
			datePicker.Date = new DateTime(2024, 08, 18);
		}

		void ButtonClicked(object sender, EventArgs e)
		{
			datePicker.Date = new DateTime(2024, 08, 19);
		}
	}
}