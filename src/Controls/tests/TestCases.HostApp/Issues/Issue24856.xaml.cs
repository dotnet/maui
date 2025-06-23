namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 24856, "Android ImageButton Aspect=AspectFit not display correctly", PlatformAffected.Android)]
	public partial class Issue24856 : TestContentPage
	{
		public Issue24856()
		{
			InitializeComponent();
		}

		protected override void Init()
		{

		}

		void OnButtonClicked(object sender, EventArgs e)
		{
			TestImageButton.Aspect = Aspect.AspectFit;
		}
	}
}