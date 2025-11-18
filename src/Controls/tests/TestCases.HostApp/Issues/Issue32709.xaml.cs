using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 32709, "App crashes when TimePicker has a TimeSelected event defined and the Time value is null", PlatformAffected.iOS)]
	public partial class Issue32709 : ContentPage
	{
		public Issue32709()
		{
			InitializeComponent();
		}

		private void OnTimeSelected(object sender, TimeChangedEventArgs e)
		{
			StatusLabel.Text = $"TimeSelected fired: OldTime={e.OldTime}, NewTime={e.NewTime}";
		}

		private void OnTestButtonClicked(object sender, EventArgs e)
		{
			// Test setting null time after initialization
			TimePicker1.Time = null;
			
			// Test setting a time value on the picker that had null
			TimePicker2.Time = new TimeSpan(14, 30, 0);
			
			StatusLabel.Text = "Button clicked - times changed";
		}
	}
}
