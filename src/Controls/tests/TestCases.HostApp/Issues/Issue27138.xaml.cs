namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 27138, "[MAUI]I5_EmptyView - When input some string in the filtering bar and click the \"search\" during project debugging, app will hang", PlatformAffected.iOS)]
	public partial class Issue27138 : TestContentPage
	{
		public Issue27138()
		{
			InitializeComponent();
		}

		protected override void Init()
		{
			BindingContext = new MonkeysViewModel();
		}
	}
}