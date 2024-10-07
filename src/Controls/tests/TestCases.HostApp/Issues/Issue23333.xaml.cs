using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 23333, "Frame offsets inner content view by 1pt", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.macOS)]
	public partial class Issue23333 : ContentPage
	{
		public Issue23333()
		{
			InitializeComponent();
		}
	}
}