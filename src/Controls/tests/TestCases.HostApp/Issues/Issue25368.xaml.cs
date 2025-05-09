namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25368, "Android RefreshView in a grid can break the grid layout", PlatformAffected.Android)]
	public partial class Issue25368 : ContentPage
	{
		public Issue25368()
		{
			InitializeComponent();
			cv.ItemsSource = Enumerable.Range(1, 100);
		}
	}
}