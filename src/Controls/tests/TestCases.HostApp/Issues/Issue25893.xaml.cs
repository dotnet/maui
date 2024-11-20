namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 25893, "Setting MenuFlyoutSubItem IconImageSource throws a NullReferenceException", PlatformAffected.UWP)]
	public partial class Issue25893 : TestContentPage
	{
		public Issue25893()
		{
			InitializeComponent();	
		}

		protected override void Init()
		{
		
		}
	}
}