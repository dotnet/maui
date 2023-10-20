using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.None, 018071, "Validate Border using different shapes",
		PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.UWP)]
	public partial class BorderWithDifferentShapes : ContentPage
	{
		public BorderWithDifferentShapes()
		{
			InitializeComponent();
		}
	}
}