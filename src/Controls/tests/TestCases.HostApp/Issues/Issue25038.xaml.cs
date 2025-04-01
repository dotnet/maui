namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 25038, "MAUI Entry in Windows always shows ClearButton if initially hidden and shown even if ClearButtonVisibility set to Never", PlatformAffected.UWP)]
	public partial class Issue25038 : ContentPage
	{
		public Issue25038()
		{
			InitializeComponent();
		}

		private void OnShowEntriesClicked(object sender, EventArgs e)
		{
			// Show the first Grid
			firstEntryGrid.IsVisible = true;

			// Show the second Grid
			secondEntryGrid.IsVisible = true;

			// Change ClearButtonVisibility of the second Entry to Never
			dynamicClearButtonEntry.ClearButtonVisibility = ClearButtonVisibility.Never;
		}
	}
}