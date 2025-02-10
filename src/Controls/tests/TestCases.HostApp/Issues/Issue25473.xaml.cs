namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 25473, "MAUI Entry in Windows always shows ClearButton despite ClearButtonVisibility set to 'Never'", PlatformAffected.UWP)]
	public partial class Issue25473 : ContentPage
	{
		public Issue25473()
		{
			InitializeComponent();
		}

		private void OnToggleClearButtonVisibilityClicked(object sender, EventArgs e)
		{
			if (mainEntryField.ClearButtonVisibility == ClearButtonVisibility.Never)
				mainEntryField.ClearButtonVisibility = ClearButtonVisibility.WhileEditing;
			else
				mainEntryField.ClearButtonVisibility = ClearButtonVisibility.Never;
		}
	}
}