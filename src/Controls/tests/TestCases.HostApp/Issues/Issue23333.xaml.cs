namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 23333, "Frame offsets inner content view by 1pt", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.macOS)]
	public partial class Issue23333 : ContentPage
	{
		public Issue23333()
		{
			InitializeComponent();
		}
	}
}