using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Maui.Controls.UITests;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 18111, "Setting MaximumTrackColor on Slider has no effect", PlatformAffected.macOS | PlatformAffected.iOS)]
	public partial class Issue18111 : ContentPage
	{
		public Issue18111()
		{
			InitializeComponent();
		}
	}
}