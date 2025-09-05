namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 30483, "Flyout Menu CollectionView First Item Misaligned", PlatformAffected.iOS)]
	public partial class Issue30483 : Shell
	{
		public Issue30483()
		{
			InitializeComponent();
			FlyoutIsPresented = true;
		}
	}
}