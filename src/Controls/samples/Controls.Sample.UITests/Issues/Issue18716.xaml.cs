using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 18716, "Touch events are not working on WebView when a PDF is displayed", PlatformAffected.iOS)]
	public partial class Issue18716 : ContentPage
	{
		public Issue18716()
		{
			InitializeComponent();
		}
	}
}