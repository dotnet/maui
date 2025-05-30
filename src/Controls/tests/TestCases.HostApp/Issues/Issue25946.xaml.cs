namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25946, "App crashes on iOS 18 when placing html label in carousel view with > 2 elements", PlatformAffected.iOS)]
	public partial class Issue25946 : ContentPage
	{
		public Issue25946()
		{
			InitializeComponent();
		}

		void ButtonClicked(object sender, EventArgs e)
		{
			collectionView.ScrollTo(1, animate: true);
		}
	}
}