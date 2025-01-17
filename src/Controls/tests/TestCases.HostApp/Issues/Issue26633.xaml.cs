namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26633, "Label height in Grid with ColumnSpacing > 0 incorrect in certain cases", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.macOS)]
	public partial class Issue26633 : ContentPage
	{
		public Issue26633()
		{
			InitializeComponent();
		}
	}
}