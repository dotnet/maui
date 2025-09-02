namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 31117,
		"DatePicker initially shows year in 4 digits, but after changing the year it displays only 2 digits in net 10.0",
		PlatformAffected.iOS)]
	public partial class Issue31117 : ContentPage
	{
		public Issue31117()
		{
			InitializeComponent();
			MyDatePicker.Date = new DateTime(2024, 12, 24);
			UpdateDisplayLabel();
		}

		void OnSetDateClicked(object sender, EventArgs e)
		{
			if (DateTime.TryParse(DateEntry.Text, out DateTime newDate))
			{
				MyDatePicker.Date = newDate;
				UpdateDisplayLabel();
			}
		}

		void UpdateDisplayLabel()
		{
			DisplayLabel.Text = $"Current Date: {MyDatePicker.Date:d}";
		}
	}
}