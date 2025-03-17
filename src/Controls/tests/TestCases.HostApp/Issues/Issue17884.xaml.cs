
namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 17884, "[Android] Entire words omitted & letters truncated from Label display", PlatformAffected.Android)]
	public partial class Issue17884 : ContentPage
	{
		public Issue17884()
		{
			InitializeComponent();
		}

		private void OnStubLabelTapped(object sender, EventArgs e)
		{
			++StubLabel.FontSize;
		}
	}
}
