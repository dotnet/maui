namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 2, "UI theme change during the runtime", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.UWP)]
	public partial class ThemeChange : ContentPage
	{
		public ThemeChange()
		{
			InitializeComponent();
		}
	}
}