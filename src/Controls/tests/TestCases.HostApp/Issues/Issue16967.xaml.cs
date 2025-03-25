using System;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 16967, "Existing TextDecorations applied to a Label are not removed when a new TextDecoration value is set after the Label's Text has been modified", PlatformAffected.iOS | PlatformAffected.macOS)]
	public partial class Issue16967 : ContentPage
	{
		int count = 0;
		public Issue16967()
		{
			InitializeComponent();
		}

		private void OnButtonClicked(object sender, EventArgs e)
		{
			count++;
			if (count == 1)
				label.Text = $"Clicked {count} time";
			else
				label.Text = $"Clicked {count} times";
		}

		private void OnCheckBoxChanged(object sender, CheckedChangedEventArgs e)
		{
			label.TextDecorations = checkBox.IsChecked ? TextDecorations.Strikethrough : TextDecorations.Underline;
		}
	}
}